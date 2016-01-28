using System.ComponentModel.DataAnnotations;

namespace AspnetCoreFileManager.Models
{
    public class FileManagerDirectory
    {
        [Display(Name = "Name")]
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
    }
}