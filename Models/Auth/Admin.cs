namespace MiTaller.Models.Auth
{
    // Perfil TPT mínimo para la única cuenta de administrador de plataforma.
    // Se crea exclusivamente vía el comando de consola `dotnet run -- seed-admin`
    // (ver Program.cs) - nunca a través de un endpoint HTTP.
    public class Admin : BaseIdentityUser
    {
        public string FullName { get; set; } = string.Empty;
    }
}
