using System.ComponentModel.DataAnnotations;

namespace CsvProcessorApp.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string? Username { get; set; }

        [Required]
        public string? Password { get; set; } // In production, store hashed passwords
    }
}
