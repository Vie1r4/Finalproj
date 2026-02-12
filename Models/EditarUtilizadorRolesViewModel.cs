namespace Finalproj.Models
{
    /// <summary>
    /// ViewModel para editar os cargos (roles) de um utilizador – painel admin (Class 8).
    /// </summary>
    public class EditarUtilizadorRolesViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Cargos disponíveis: selecionados = atribuídos ao utilizador.
        /// </summary>
        public List<RoleItemViewModel> Roles { get; set; } = new();
    }

    public class RoleItemViewModel
    {
        public string Nome { get; set; } = string.Empty;
        public bool Atribuido { get; set; }
    }
}
