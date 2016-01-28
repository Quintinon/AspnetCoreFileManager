//TODO: verify write permission before creating directory
//TODO: add some kind of security to download action
//TODO: complete UploadFile

using AspnetCoreFileManager.Models;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.PlatformAbstractions;
using System;
using System.IO;

namespace AspnetCoreFileManager.Web.Controllers
{
    [Route("file-manager")]
    public class FileManagerController : Controller
    {
        private readonly IApplicationEnvironment _appEnvironment;

        public FileManagerController(IApplicationEnvironment appEnvironment)
        {
            _appEnvironment = appEnvironment;
        }

        public IActionResult Index(string dir)
        {
            var fileManager = new FileManager(dir, Request.Path.ToString(), _appEnvironment.ApplicationBasePath);

            var fileManagerViewMode = "Details";
            var fileManagerViewModeCookie = Request.Cookies["FileManagerViewMode"];
            if (!string.IsNullOrWhiteSpace(fileManagerViewModeCookie))
                fileManagerViewMode = fileManagerViewModeCookie;

            ViewBag.FileManagerViewMode = fileManagerViewMode;

            return View(fileManager);
        }

        [Route("bulk-delete")]
        [HttpPost]
        public IActionResult BulkDelete(string[] deletePaths)
        {
            // since all of the paths start from the same directory it should be safe to use the first path in the array to establish the current directory
            var fileManager = new FileManager(deletePaths[0], Request.Path.ToString(), _appEnvironment.ApplicationBasePath);

            foreach (string s in deletePaths)
            {
                var deletePath = fileManager.FileSystemPath + s;
                if ((System.IO.File.GetAttributes(deletePath) & FileAttributes.Directory) == FileAttributes.Directory)
                    Directory.Delete(deletePath, true);
                else
                    System.IO.File.Delete(deletePath);
            }

            TempData["MessageBus"] = "The files were deleted successfully.";
            var returnDir = deletePaths[0].Substring(0, deletePaths[0].LastIndexOf("/"));
            return RedirectToAction("Index", new { dir = returnDir });
        }

        [Route("create-directory")]
        [HttpPost]
        public IActionResult CreateDirectory(string dir, FileManagerDirectory directory)
        {
            if (!ModelState.IsValid)
                return View(directory);

            var fileManager = new FileManager(dir, Request.Path.ToString(), _appEnvironment.ApplicationBasePath);
            var newDirectoryName = directory.Name.Slugify();
            Directory.CreateDirectory(fileManager.FileSystemPath + fileManager.CurrentDirectory + "/" + newDirectoryName);

            TempData["MessageBus"] = "The Directory was created successfully.";
            return RedirectToAction("Index", new { dir = fileManager.CurrentDirectory + "/" + newDirectoryName });
        }

        [Route("delete-directory")]
        public IActionResult DeleteDirectory(string dir)
        {
            var fileManager = new FileManager(dir, Request.Path.ToString(), _appEnvironment.ApplicationBasePath);
            Directory.Delete(fileManager.FileSystemPath + dir, true);

            TempData["MessageBus"] = "The folder was deleted successfully.";
            return RedirectToAction("Index", new { dir = dir.Substring(0, dir.LastIndexOf("/")) });
        }

        [Route("delete-file")]
        public IActionResult DeleteFile(string file)
        {
            var fileManager = new FileManager(file, Request.Path.ToString(), _appEnvironment.ApplicationBasePath);

            System.IO.File.Delete(fileManager.FileSystemPath + file);

            TempData["MessageBus"] = "The file was deleted successfully.";
            return RedirectToAction("Index", new { dir = file.Substring(0, file.LastIndexOf("/")) });
        }

        [Route("download-file")]
        public FileResult DownloadFile(string file)
        {
            ////var fileManager = new FileManager(file, Request.Path.ToString(), _appEnvironment);
            ////return File(fileManager.CurrentDirectory, "text/plain");

            //var fileManager = new FileManager(file, Request.Path.ToString(), _appEnvironment);

            //var contentDisposition = new System.Net.Mime.ContentDisposition();
            //contentDisposition.FileName = Path.GetFileName(file);
            //contentDisposition.Inline = false;
            ////throw new System.Exception(contentDisposition.ToString());
            //Response.Headers.Append("Content-Disposition", contentDisposition.ToString());
            ////Response.Headers.Append("Content-Disposition", "attachment; filename=" + Path.GetFileName(file));

            ////byte[] fileBytes = System.IO.File.ReadAllBytes(fileManager.AppDirectory + file);
            ////return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet);

            //return File(fileManager.CurrentDirectory, System.Net.Mime.MediaTypeNames.Application.Octet);

            return null;
        }

        [Route("upload-file")]
        [HttpPost]
        public ActionResult UploadFile(string dir, string name, int? chunks, int? chunk, IFormFile file)
        {
            string appDirectory;
            string currentDirectory;

            if (!string.IsNullOrWhiteSpace(dir) && dir.StartsWith("/secure-files"))
            {
                appDirectory = _appEnvironment.ApplicationBasePath + "/Uploads";
                currentDirectory = dir;
            }
            else
            {
                appDirectory = _appEnvironment.ApplicationBasePath + "/wwwroot";
                currentDirectory = string.IsNullOrWhiteSpace(dir) ? "/media" : dir;
                if (!currentDirectory.StartsWith("/media"))
                    //throw new System.Web.HttpException("Access is denied on the upload directory.");
                    throw new Exception("Access is denied on the upload directory.");
            }

            //bool hasPermission = Utilities.VerifyWritePermission(uploadDirectory);
            //if (!hasPermission)
            //{
            //    context.Response.StatusCode = 500;
            //    context.Response.Write("{\"jsonrpc\" : \"2.0\", \"error\" : {\"code\": 400, \"message\": \"Permissions have not been set correctly on the currently selected folder.\"}, \"id\" : \"id\"}");
            //    return;
            //}


            //string fileName = string.IsNullOrWhiteSpace(name)?  string.Empty;
            //fileName = Path.GetFileNameWithoutExtension(fileName).Slugify() + Path.GetExtension(fileName).ToLower();

            chunks = chunks ?? 0;
            chunk = chunk ?? 0;
            bool isLastChunk = (chunk >= (chunks - 1)) ? true : false;

            //var l = new Logging.LogWriter("Plupload.txt");
            //l.WriteLine("Chunk " + (chunk + 1).ToString() + " of " + chunks.ToString() + ". IsLastChunk: " + isLastChunk.ToString());

            var savePath = appDirectory + currentDirectory + "/" + name;
            using (var fs = new FileStream(savePath, chunk == 0 ? FileMode.Create : FileMode.Append))
            {
                //var fileBytes = new byte[file.Length];
                using (var reader = new BinaryReader(file.OpenReadStream()))
                {
                    //var fileContent = reader.ReadToEnd();
                    //reader.read(fileBytes, 0, fileBytes.Length);
                    fs.Write(reader.ReadBytes(Convert.ToInt32(file.Length)), 0, Convert.ToInt32(file.Length));
                }
                //var buffer = new byte[file.Length];
                //file.OpenReadStream();
                //file.OpenReadStream.Read(buffer, 0, buffer.Length);
                //fs.Write(fileBytes, 0, fileBytes.Length);
            }

            //if (isLastChunk && ImageResizer.Configuration.Config.Current.Pipeline.IsAcceptedImageType(fileName))
            //{
            //    // check the querystring for image properties and validate them if they exist
            //    string widthQs = context.Request.QueryString["w"];
            //    string heightQs = context.Request.QueryString["h"];
            //    int width = 0;
            //    if (widthQs.IsNumeric())
            //        width = Convert.ToInt32(widthQs);
            //    if (width > 4000)
            //        width = 0;
            //    int height = 0;
            //    if (heightQs.IsNumeric())
            //        height = Convert.ToInt32(heightQs);
            //    if (height > 4000)
            //        height = 0;

            //    //l.WriteLine("Dimensions: " + width + "x" + height);

            //    if ((width > 0 || height > 0))
            //    {
            //        var sourceFilePath = Path.Combine(savePath, fileName);
            //        var newFilePath = Path.Combine(savePath, Guid.NewGuid().ToString());

            //        ImageResizer.ImageJob i = new ImageResizer.ImageJob();
            //        i.Source = sourceFilePath;
            //        i.Dest = newFilePath;
            //        i.Instructions = new ImageResizer.Instructions();
            //        i.Instructions.Mode = ImageResizer.FitMode.Max;
            //        if (width > 0)
            //            i.Instructions.Width = width;
            //        if (height > 0)
            //            i.Instructions.Height = height;
            //        i.Build();
            //        File.Delete(sourceFilePath);
            //        File.Move(newFilePath, sourceFilePath);
            //    }
            //}

            return Content("chunk uploaded", "text/plain");
        }
    }
}