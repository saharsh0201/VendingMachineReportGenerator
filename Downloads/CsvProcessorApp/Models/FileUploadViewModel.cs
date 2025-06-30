using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace CsvProcessorApp.Models
{
    public class FileUploadViewModel
    {
        [Required]
        public IFormFile Emplist { get; set; }

        public List<IFormFile> Tlists { get; set; }
    }
}
