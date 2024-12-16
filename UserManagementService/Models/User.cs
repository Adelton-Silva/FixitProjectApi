namespace UserManagementService.Models
{
    public class User
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        // Adicione outros campos conforme necessário
    }
}