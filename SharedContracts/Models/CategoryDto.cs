namespace SharedContracts.Models
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public List<LauncherDto>? Launchers { get; set; }
    }
}
