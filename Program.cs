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
    var jwtKey = builder.Configuration["Jwt:Key"];
    var jwtIssuer = builder.Configuration["Jwt:Issuer"];

    // EmailSender
    builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
    builder.Services.AddTransient<IEmailSender, EmailSenderService>();

    // Firebase Notification Service
    builder.Services.AddScoped<FirebaseNotificationService>();

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
    builder.Services.AddDbContext<DataContext>(options =>
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
            sqlServerOptionsAction: sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(maxRetryCount: 10, maxRetryDelay: TimeSpan.FromSeconds(5), errorNumbersToAdd: null);
            });
    });

    // QuestPDF licencia gratuita
    QuestPDF.Settings.License = LicenseType.Community;

    var app = builder.Build();


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

