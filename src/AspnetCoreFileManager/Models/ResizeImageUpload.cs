using Microsoft.AspNet.Http;
using System.ComponentModel.DataAnnotations;

namespace AspnetCoreFileManager.Models
{
    public class ResizeImageUpload
    {
        [Required]
        [DataType(DataType.Text)]
        public int Width { get; set; }

        [Required]
        [DataType(DataType.Text)]
        public int Height { get; set; }

        [Required]
        public IFormFile FormFile { get; set; }

        public ResizeImageUpload()
        {
            Width = 250;
            Height = 250;
        }
    }
}