namespace Finalproj.Models
{
    /// <summary>
    /// ViewModel para listar utilizadores e os seus cargos (roles) – painel admin.
    /// Inclui o nome do funcionário associado (se existir).
    /// </summary>
    public class UtilizadorComRolesViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public IList<string> Roles { get; set; } = new List<string>();
        /// <summary> Nome do funcionário ligado a esta conta (null se não houver). </summary>
        public string? FuncionarioAssociadoNome { get; set; }
    }
}
