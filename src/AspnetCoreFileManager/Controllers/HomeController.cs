using Microsoft.AspNet.Mvc;

namespace AspnetCoreFileManager.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            //return RedirectToAction("", "FileManager");
            return Redirect("file-manager");
        }
    }
}
