using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using windows_explorer.Models;

namespace windows_explorer.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            ViewData["ver"] = Request.QueryString.Value?.Contains("ver") ?? false ? "1" : "0";
            return View();
        }

        [Route("/home/file-manager-file-system")]
        [AcceptVerbs("get","post")]
        public IActionResult fileManagerFileSystem()
        {
            List<object> res =
            [
                new
                {
                    key = "MyFile.jpg",
                    name = "MyFile.jpg",
                    dateModified = "2025-07-02T20:22:59.6911492Z",
                    isDirectory = false,
                    size = 1024,
                    hasSubDirectories = false
                    //thumbnail = "/fileicon?type=jpg",
                },
                new
                {
                    key = "MyFile2.mp4",
                    name = "MyFile2.mp4",
                    dateModified = "2025-07-02T20:22:59.6911492Z",
                    isDirectory = false,
                    size = 1024123456,
                    hasSubDirectories = false,
                    //thumbnail = "https://www.citypng.com/public/uploads/preview/yellow-computer-folder-icon-download-png-7017516950339154cbr0m5roh.png",
                },
                //{"key":"cldr-data","name":"cldr-data","dateModified":"2025-07-02T16:01:56.4536709Z","isDirectory":true,"size":0,"hasSubDirectories":true}
            ];
            return Json(new{
                success = true,
                errorCode = 0,
                errorText = "",
                result = res
            });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}