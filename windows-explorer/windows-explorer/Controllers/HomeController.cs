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

        public IActionResult Open(string fileItem)
        {
            PathInfoResult fileinfo = System.Text.Json.JsonSerializer.Deserialize<PathInfoResult>(fileItem);
            if (!string.IsNullOrEmpty(fileinfo?.key))
            {
                return new ResumableFileStreamResult2(new System.IO.FileStream(fileinfo.key, System.IO.FileMode.Open, FileAccess.Read), mappings[fileinfo.key.Substring(fileinfo.key.LastIndexOf('.'))]);
            }
            throw new Exception("OpnFile ERROR!");
        }

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

        public IActionResult Test()
        {
            return Json(new
            {
                GetDisplayUrl = Request.GetDisplayUrl()
                    .TrimStart("https://".ToCharArray())
                    .TrimStart("http://".ToCharArray()),
                Request.Host,
                Request.Path,
                Request.PathBase,
                Request.Protocol,
                Request.Query,
                Request.QueryString,
                URL= (Request.IsHttps ? "https://" : "http://") + Request.Host.Value
             });
        }

        private IActionResult GetDirContents(PathInfo? pathArgs)
        {
            if (pathArgs.pathInfo.Count < 1)
            {
                return GetSystemDrives();
            }
            return GetDirectoryInfo(pathArgs);

            
        }

        private IActionResult GetDirectoryInfo(PathInfo pathArgs)
        {
            List<PathInfoResult> res = new List<PathInfoResult>();
            System.IO.DirectoryInfo dirInfo = new System.IO.DirectoryInfo(pathArgs.pathInfo.Last().key + "\\");
            res = dirInfo.GetDirectories().Select(dir => new PathInfoResult
            {
                name = dir.Name.TrimEnd('\\'),
                key = pathArgs.pathInfo.Last().key + "\\" + dir.Name.TrimEnd('\\'),
                size = 0,
                isDirectory = true,
                dateModified = dir.LastWriteTimeUtc.ToLocalTime().ToString("yyyy-MM-ddTHH:mm:ss.FFFFFFFZ") //"2025-07-02T16:01:56.4536709Z"
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


            string WebsiteRootUrlAddress = (Request.IsHttps ? "https://" : "http://") + Request.Host.Value;
            

            res.AddRange(dirInfo.GetFiles().Select(file => new PathInfoResult
            {
                name = file.Name.TrimEnd('\\'),
                key = pathArgs.pathInfo.Last().key + "\\" + file.Name.TrimEnd('\\'),
                size = file.Length,
                isDirectory = false,
                hasSubDirectories = false,
                thumbnail =  WebsiteRootUrlAddress + "/FileIcon?type=" + file.Name.Substring(file.Name.LastIndexOf('.') + 1),
                dateModified = file.LastWriteTimeUtc.ToLocalTime().ToString("yyyy-MM-ddTHH:mm:ss.FFFFFFFZ") //"2025-07-02T16:01:56.4536709Z"
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