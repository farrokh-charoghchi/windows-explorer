using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using windows_explorer.Models;
using windows_explorer.Models.DevFileManagerModels;

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
        [AcceptVerbs("get", "post")]
        public IActionResult fileManagerFileSystem(string command, string arguments, string _)
        {
            var pathArgs = System.Text.Json.JsonSerializer.Deserialize<PathInfo>(arguments);

            switch (command) 
            { 
                case "GetDirContents":
                    return GetDirContents(pathArgs);
                    break;
                case "Download":
                    return DownloadFile(pathArgs);
                    break;
            default:
                break;
            }
            throw new Exception("fileManagerFileSystem ERROR!");
        }

        public static IDictionary<string, string> mappings = new FileExtensionContentTypeProvider().Mappings;

        private IActionResult DownloadFile(PathInfo? pathArgs)
        {
            if (pathArgs.pathInfoList != null)
            {
                if (pathArgs.pathInfoList.Count == 1)
                {
                    var fileItem = pathArgs.pathInfoList[0].Last();
                    // download one file
                    return File(new System.IO.FileStream(fileItem.key, System.IO.FileMode.Open), mappings[fileItem.name.Substring(fileItem.name.LastIndexOf('.'))], fileItem.name);
                }
                else if (pathArgs.pathInfoList.Count > 1)
                {
                    // download multiple file as zip
                }
            }
            throw new Exception("Download ERROR!");
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

        private IActionResult GetDirContents(PathInfo? pathArgs)
        {
            if (pathArgs.pathInfo.Count < 1)
            {
                return GetSystemDrives();
            }
            return GetDirectoryInfo(pathArgs);

            //arguments: {"pathInfo":[{"key":"Folder1","name":"Folder1"}]}
            List<object> res = new List<object>();
            res.Add(new
            {
                key = "Folder1",
                name = "Folder1",
                dateModified = "2025-07-02T20:22:59.6911492Z",
                isDirectory = true,
                size = 2409765000,
                hasSubDirectories = true
                //thumbnail = "/fileicon?type=jpg",
            });
            res.Add(new
            {
                key = "MyFile.jpg",
                name = "MyFile.jpg",
                dateModified = "2025-07-02T20:22:59.6911492Z",
                isDirectory = false,
                size = 1024,
                hasSubDirectories = false
                //thumbnail = "/fileicon?type=jpg",
            });
            res.Add(new
            {
                key = "MyFile2.mp4",
                name = "MyFile2.mp4",
                dateModified = "2025-07-02T20:22:59.6911492Z",
                isDirectory = false,
                size = 1024123456,
                hasSubDirectories = false,
                //thumbnail = "https://www.citypng.com/public/uploads/preview/yellow-computer-folder-icon-download-png-7017516950339154cbr0m5roh.png",
            });
            //{"key":"cldr-data","name":"cldr-data","dateModified":"2025-07-02T16:01:56.4536709Z","isDirectory":true,"size":0,"hasSubDirectories":true}

            return Json(new
            {
                success = true,
                errorCode = 0,
                errorText = "",
                result = res
            });
        }

        private IActionResult GetDirectoryInfo(PathInfo pathArgs)
        {
            List<PathInfoResult> res = new List<PathInfoResult>();
            System.IO.DirectoryInfo dirInfo = new System.IO.DirectoryInfo(pathArgs.pathInfo.Last().key + "\\");
            res = dirInfo.GetDirectories().Select(d => new PathInfoResult
            {
                name = d.Name.TrimEnd('\\'),
                key = pathArgs.pathInfo.Last().key + "\\" + d.Name.TrimEnd('\\'),
                size = 0,
                isDirectory = true,
                //hasSubDirectories = new System.IO.DirectoryInfo(d.FullName).GetDirectories().Count() > 0,
                dateModified = "2025-07-02T16:01:56.4536709Z"
            }).ToList();


            res.ForEach(r =>
            {
                bool hasSubDir = false;
                try
                {
                    hasSubDir = new System.IO.DirectoryInfo(r.key + "\\").GetDirectories().Count() > 0;
                }
                catch (Exception ex)
                {
                }

                r.hasSubDirectories = hasSubDir;
            });


            res.AddRange(dirInfo.GetFiles().Select(f => new PathInfoResult
            {
                name = f.Name.TrimEnd('\\'),
                key = pathArgs.pathInfo.Last().key + "\\" + f.Name.TrimEnd('\\'),
                size = f.Length,
                isDirectory = false,
                hasSubDirectories = false,
                dateModified = "2025-07-02T16:01:56.4536709Z"
            }).ToList());

            return Json(new DevFileManagerResult<List<PathInfoResult>> { result = res });
        }

        private IActionResult GetSystemDrives()
        {
            List<PathInfoResult> res = new List<PathInfoResult>();
            var drives = System.IO.DriveInfo.GetDrives().ToList();
            res = drives.Where(d=>d.IsReady).Select(d => new PathInfoResult
            {
                name = d.Name.TrimEnd('\\'),
                key = d.Name.TrimEnd('\\'),
                size = d.TotalSize - d.TotalFreeSpace,
                isDirectory = true,
                hasSubDirectories = true,
                dateModified = "2025-07-02T16:01:56.4536709Z"
            }).ToList();
            
            return Json(new DevFileManagerResult<List<PathInfoResult>> { result = res });
        }
    }
}