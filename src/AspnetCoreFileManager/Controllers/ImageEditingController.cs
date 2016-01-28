using AspnetCoreFileManager.Models;
using ImageProcessor;
using ImageProcessor.Samplers;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using Microsoft.Net.Http.Headers;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace AspnetCoreFileManager.Web.Controllers
{
    [Route("image-editing")]
    public class ImageEditingController : Controller
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        public ImageEditingController(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        [Route("resize-image")]
        public IActionResult ResizeImage()
        {
            return View(new ResizeImageUpload());
        }

        [Route("resize-image")]
        [HttpPost]
        public async Task<IActionResult> ResizeImage(ResizeImageUpload resizeImageUpload)
        {
            if (!ModelState.IsValid || resizeImageUpload.FormFile.Length == 0)
                return View(resizeImageUpload);

            var uploadDir = Path.Combine(_hostingEnvironment.WebRootPath, "uploads");
            var fileName = ContentDispositionHeaderValue.Parse(resizeImageUpload.FormFile.ContentDisposition).FileName.Trim('"');
            fileName = Path.GetFileNameWithoutExtension(fileName).Slugify() + Path.GetExtension(fileName);
            var filePath = Path.Combine(uploadDir, fileName);
            await resizeImageUpload.FormFile.SaveAsAsync(filePath);

            using (var inputStream = System.IO.File.OpenRead(filePath))
            {
                var sw = new Stopwatch();
                sw.Start();

                var image = new Image(inputStream);
                using (var outputStream = System.IO.File.OpenWrite(filePath))
                {
                    image.Resize(resizeImageUpload.Width, resizeImageUpload.Height).Save(outputStream);
                }

                sw.Stop();
                ViewBag.ProcessingTime = sw.Elapsed.TotalSeconds.ToString();
            }

            ViewBag.UploadedFilePath = "/uploads/" + fileName;

            return View(resizeImageUpload);
        }
    }
}