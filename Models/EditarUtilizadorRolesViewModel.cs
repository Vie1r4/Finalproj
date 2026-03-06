namespace Finalproj.Models
{
    /// <summary>
    /// ViewModel para editar os cargos (roles) e a associação a funcionário – painel admin.
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

        /// <summary>
        /// Id do funcionário associado a este utilizador (opcional). Quando preenchido, este utilizador pode fazer login e fica ligado à ficha do funcionário.
        /// </summary>
        public int? FuncionarioId { get; set; }
    }

    public class RoleItemViewModel
    {
        public string Nome { get; set; } = string.Empty;
        public bool Atribuido { get; set; }
    }
}
