namespace Finalproj.Models
{
    /// <summary>
    /// ViewModel para listar utilizadores e os seus cargos (roles) – painel admin (Class 8).
    /// </summary>
    public class UtilizadorComRolesViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public IList<string> Roles { get; set; } = new List<string>();
    }
}
