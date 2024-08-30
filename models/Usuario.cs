namespace OpenConsult.Models
{
    public class Usuario
    {
        public string? NomeCompleto { get; set; } 
        public string? Login { get; set; }
        public string? Telefone { get; set; } 
        public List<string>? Grupos { get; set; } 
    }
}
