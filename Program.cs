using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MiTaller.Data;
using MiTaller.Models.Auth;
using MiTaller.Models.Customer;
using MiTaller.Services;
using MiTaller.Services.Audit;
using QuestPDF.Infrastructure;
using System.Text;
using static System.Net.WebRequestMethods;

var builder = WebApplication.CreateBuilder(args);


try
{
    // Add services to the container.

    builder.Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "MiTaller API", Version = "v1" });

        // 🔹 Configurar autenticación JWT en Swagger
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "Autenticación con JWT usando el esquema Bearer. \n\n" +
                          "Ejemplo: 'Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...'",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new List<string>()
            }
        });

    });

    //CORS
    //var allowedOrigins = new[]
    //{
    //"http://localhost:49475",
    //"https://app.mitaller.com",
    //"https://plataforma.mitaller.io",
    //"https://mitaller.io"
    //};

    //builder.Services.AddCors(options =>
    //{
    //    options.AddPolicy("AllowSpecificOrigins", policy =>
    //    {
    //        policy
    //            .WithOrigins(allowedOrigins)  
    //            .AllowAnyHeader()
    //            .AllowAnyMethod()
    //            .AllowCredentials(); 
    //    });
    //});

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });

        // Política dedicada y restrictiva para /api/Admin - no esperamos a la limpieza
        // general de CORS (AllowAll) para proteger el endpoint de mayor valor si se filtra.
        var adminOrigins = (builder.Configuration["Admin:AllowedOrigins"] ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        options.AddPolicy("AdminPortal", policy =>
        {
            policy
                .WithOrigins(adminOrigins)
                .AllowAnyHeader()
                .WithMethods("GET", "POST", "PUT", "DELETE");
        });
    });

    // Identity
    builder.Services.AddIdentity<BaseIdentityUser, IdentityRole<Guid>>(options =>
    {
        options.User.RequireUniqueEmail = false;
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
    })
    .AddEntityFrameworkStores<DataContext>()
    .AddDefaultTokenProviders()
    .AddErrorDescriber<CustomIdentityErrorDescriber>();

    // Remover el validador de contraseña predeterminado
    var defaultPasswordValidator = builder.Services.FirstOrDefault(
        d => d.ServiceType == typeof(IPasswordValidator<BaseIdentityUser>) &&
             d.ImplementationType == typeof(PasswordValidator<BaseIdentityUser>)
    );
    if (defaultPasswordValidator != null)
    {
        builder.Services.Remove(defaultPasswordValidator);
    }

    // Registrar el validador de contraseña personalizado que retorna un solo error
    builder.Services.AddTransient<IPasswordValidator<BaseIdentityUser>, SingleErrorPasswordValidator<BaseIdentityUser>>();


    // JWT
    builder.Services.AddScoped<JwtService>();

    // Audit log (historial global de cambios) - necesita saber quién hizo la request.
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<AuditSaveChangesInterceptor>();

    // Autorización específica para el portal admin - claim propio, no roles de Identity
    // (ver plan: una sola cuenta fija, sin infraestructura de roles sin usar en el resto
    // del proyecto).
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("PlatformAdmin", policy =>
            policy.RequireClaim("IsPlatformAdmin", "true"));
    });
    var jwtKey = builder.Configuration["Jwt:Key"];
    var jwtIssuer = builder.Configuration["Jwt:Issuer"];

    // EmailSender
    builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
    builder.Services.AddTransient<IEmailSender, EmailSenderService>();

    // Firebase Notification Service
    builder.Services.AddScoped<FirebaseNotificationService>();

    // Notificaciones de citas (in-app + correo) y el job que las dispara
    // (recordatorio ~1 día antes, cancelación automática ~4h antes si sigue "Pendiente").
    builder.Services.AddScoped<AppointmentNotificationService>();
    builder.Services.AddHostedService<AppointmentSchedulerService>();

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });


    // DBContext
    builder.Services.AddDbContext<DataContext>((serviceProvider, options) =>
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
            sqlServerOptionsAction: sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(maxRetryCount: 10, maxRetryDelay: TimeSpan.FromSeconds(5), errorNumbersToAdd: null);
            });
        options.AddInterceptors(serviceProvider.GetRequiredService<AuditSaveChangesInterceptor>());
    });

    // QuestPDF licencia gratuita
    QuestPDF.Settings.License = LicenseType.Community;

    var app = builder.Build();

    // Comando de un solo uso para crear la cuenta de administrador de plataforma -
    // nunca un endpoint HTTP. Uso: dotnet run -- seed-admin correo@ejemplo.com ContraseñaSegura!
    if (args.Length > 0 && args[0] == "seed-admin")
    {
        await SeedAdminAsync(app.Services, args);
        return;
    }

    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseHttpsRedirection();
    app.UseCors("AllowAll");
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception)
{

	throw;
}

static async Task SeedAdminAsync(IServiceProvider services, string[] args)
{
    if (args.Length < 3)
    {
        Console.WriteLine("Uso: dotnet run -- seed-admin <correo> <contraseña>");
        return;
    }

    var email = args[1];
    var password = args[2];

    using var scope = services.CreateScope();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<BaseIdentityUser>>();

    var existing = await userManager.FindByEmailAsync(email);
    if (existing != null)
    {
        Console.WriteLine($"Ya existe una cuenta con ese correo (Id={existing.Id}, UserType={existing.UserType}). No se creó nada.");
        return;
    }

    var admin = new Admin
    {
        UserName = email,
        Email = email,
        FullName = "Administrador",
        UserType = UserType.Admin,
        EmailConfirmed = true,
    };

    var result = await userManager.CreateAsync(admin, password);
    if (!result.Succeeded)
    {
        Console.WriteLine("No se pudo crear la cuenta admin:");
        foreach (var error in result.Errors)
        {
            Console.WriteLine($" - {error.Description}");
        }
        return;
    }

    Console.WriteLine($"Cuenta admin creada correctamente. Id = {admin.Id}");
    Console.WriteLine("Copia ese Id al appsettings correspondiente como \"Admin:UserId\" para que el login de esta cuenta reciba el claim IsPlatformAdmin.");
}

