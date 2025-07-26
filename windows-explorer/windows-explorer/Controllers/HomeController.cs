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
                URL = (Request.IsHttps ? "https://" : "http://") + Request.Host.Value
            });
        }




        [Route("/home/file-manager-file-system")]
        [AcceptVerbs("get", "post")]
        public IActionResult fileManagerFileSystem(string command, string arguments, string _)
        {
            var pathArgs = System.Text.Json.JsonSerializer.Deserialize<PathInfo>(arguments);

            try
            {
                switch (command)
                {
                    case "GetDirContents":
                        return GetDirContents(arguments);
                        break;
                    case "Download":
                        return DownloadFile(arguments);
                        break;
                    case "CreateDir":
                        return CreateDir(arguments);
                        break;
                    case "Rename":
                        return Rename(arguments);
                        break;
                    case "Remove":
                        return Remove(arguments);
                        break;
                    case "Copy":
                        return Copy(arguments);
                        break;
                    case "Move":
                        return Move(arguments);
                        break;
                    case "UploadChunk":
                        return UploadChunk(arguments);
                        break;
                    default:
                        break;
                }
                throw new Exception("fileManagerFileSystem ERROR!");
            }
            catch (Exception ex)
            {
                return Json(new DevFileManagerResult<List<string>> 
                { 
                    success = false,
                    errorText = ex.Message,
                    result = [] 
                });
            }
        }

        private IActionResult UploadChunk(string arguments)
        {
            if (!System.IO.Directory.Exists(Program.UploadTempFolder))
            {
                System.IO.Directory.CreateDirectory(Program.UploadTempFolder);
            }

            var model = System.Text.Json.JsonSerializer.Deserialize<UploadChunkVM>(arguments);
            var dirItem = model.destinationPathInfo.LastOrDefault();
            if (dirItem == null)
            {
                throw new Exception("Upload ERROR: Location unknown!");
            }

            // proccess uploading chunks
            string tempChunkFileName = model.chunkMetadataModel?.UploadId + "__" + model.chunkMetadataModel.Index.ToString();

            var requestFileStream = Request.Form.Files[0].OpenReadStream();
            requestFileStream.Seek(0, System.IO.SeekOrigin.Begin);

            var tempChunkFileStream =  System.IO.File.OpenWrite(Program.UploadTempFolder + tempChunkFileName);
            requestFileStream.CopyTo(tempChunkFileStream);
            tempChunkFileStream.Close();
            tempChunkFileStream.Dispose();

            var uploadedChunks = System.IO.Directory.GetFiles(Program.UploadTempFolder, model.chunkMetadataModel?.UploadId + "*");

            if (uploadedChunks.Length >= model.chunkMetadataModel.TotalCount)
            {
                // All chunks uploaded, now create final file

                string finalFilePath = dirItem.key + "\\" + model.chunkMetadataModel.FileName;
                int fileExistsCount = 1;
                while (System.IO.File.Exists(finalFilePath))
                {
                    finalFilePath = dirItem.key + "\\" + model.chunkMetadataModel.FileName.Substring(0, model.chunkMetadataModel.FileName.LastIndexOf('.')) + "(" +(++fileExistsCount).ToString() + ")" + model.chunkMetadataModel.FileName.Substring(model.chunkMetadataModel.FileName.LastIndexOf('.'));
                }

                var finalFileStream = System.IO.File.OpenWrite(finalFilePath);

                for (var i = 0; i < model.chunkMetadataModel.TotalCount; i++)
                {
                    string chunkFileName = Program.UploadTempFolder + model.chunkMetadataModel?.UploadId + "__" + i.ToString();
                    var chunkFileStream = System.IO.File.OpenRead(chunkFileName);
                    chunkFileStream.CopyTo(finalFileStream);
                    chunkFileStream.Close();
                    chunkFileStream.Dispose();

                    System.IO.File.Delete(chunkFileName);
                }
                finalFileStream.Close();
                finalFileStream.Dispose();
            }


            return Json(new DevFileManagerResult<List<string>> { result = [] });
        }

        private IActionResult Move(string arguments)
        {
            var model = System.Text.Json.JsonSerializer.Deserialize<CopyMoveVM>(arguments);
            var srcItem = model.sourcePathInfo.LastOrDefault();
            var destItem = model.destinationPathInfo.LastOrDefault();


            if (srcItem != null && destItem != null)
            {
                if (model.sourceIsDirectory)
                {
                    System.IO.Directory.Move(srcItem.key, destItem.key + "\\" + srcItem.name);
                }
                else
                {
                    System.IO.File.Move(srcItem.key, destItem.key + "\\" + srcItem.name);
                }
            }
            else
            {
                if (srcItem == null)
                {
                    throw new Exception("Move ERROR: source not defined!"); 
                }
                else
                {
                    throw new Exception("Move ERROR: destination not defined!");
                }
            }


            return Json(new DevFileManagerResult<List<string>> { result = [] });
        }

        private IActionResult Copy(string arguments)
        {
            var model = System.Text.Json.JsonSerializer.Deserialize<CopyMoveVM>(arguments);
            var srcItem = model.sourcePathInfo.LastOrDefault();
            var destItem = model.destinationPathInfo.LastOrDefault();

            if (srcItem != null && destItem != null)
            {
                if (model.sourceIsDirectory)
                {
                    string sourcePath = srcItem.key;
                    string targetPath = destItem.key + "\\" + srcItem.name;

                    try
                    {
                        System.IO.Directory.CreateDirectory(targetPath);
                    }
                    catch (Exception ex) { }

                    //Now Create all of the directories
                    foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
                    {
                        System.IO.Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
                    }

                    //Copy all the files & Replaces any files with the same name
                    foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
                    {
                        System.IO.File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
                    }
                }
                else
                {
                    System.IO.File.Copy(srcItem.key, destItem.key + "\\" + srcItem.name);
                }
            }
            else
            {
                if (srcItem == null)
                {
                    throw new Exception("Copy ERROR: source not defined!");
                }
                else
                {
                    throw new Exception("Copy ERROR: destination not defined!");
                }
            }

            return Json(new DevFileManagerResult<List<string>> { result = [] });
        }

        private IActionResult Remove(string arguments)
        {
            var model = System.Text.Json.JsonSerializer.Deserialize<RemoveVM>(arguments);
            var dirItem = model.pathInfo.LastOrDefault();

            if (dirItem != null)
            {
                if (model.isDirectory)
                {
                    System.IO.Directory.Delete(dirItem.key, true);
                }
                else
                {
                    System.IO.File.Delete(dirItem.key);
                }
            }
            else
            {
                throw new Exception("Remove ERROR: Location unknown!");
            }

            return Json(new DevFileManagerResult<List<string>> { result = [] });
        }

        private IActionResult Rename(string arguments)
        {
            var model = System.Text.Json.JsonSerializer.Deserialize<RenameVM>(arguments);
            var dirItem = model.pathInfo.LastOrDefault();
            if (dirItem != null)
            {
                if (model.isDirectory)
                {
                    string NewFolderPath = dirItem.key.Substring(0, dirItem.key.LastIndexOf('\\'))+ "\\" + model.name;
                    System.IO.Directory.Move(dirItem.key, NewFolderPath);
                }
                else
                {
                    string NewFilePath = dirItem.key.Substring(0, dirItem.key.LastIndexOf('\\')) + "\\" + model.name;
                    System.IO.File.Move(dirItem.key, NewFilePath);
                }
            }
            else
            {
                throw new Exception("Rename ERROR: Location unknown!");
            }

            return Json(new DevFileManagerResult<List<string>> { result = [] });
        }

        private IActionResult CreateDir(string arguments)
        {
            var model = System.Text.Json.JsonSerializer.Deserialize<CreateDirVM>(arguments);
            var dirItem = model.pathInfo.LastOrDefault();
            if (dirItem != null)
            {
                System.IO.Directory.CreateDirectory(dirItem.key + "\\" + model.name);
            }
            else
            {
                throw new Exception("CreateDir ERROR: Location unknown!");
            }

            return Json(new DevFileManagerResult<List<string>> { result = [] });
        }

        private IActionResult GetDirContents(string arguments)
        {
            var pathArgs = System.Text.Json.JsonSerializer.Deserialize<PathInfo>(arguments);
            if (pathArgs.pathInfo.Count < 1)
            {
                return GetSystemDrives();
            }
            return GetDirectoryInfo(pathArgs);


        }

        private IActionResult DownloadFile(string arguments)
        {
            var pathArgs = System.Text.Json.JsonSerializer.Deserialize<PathInfo>(arguments);
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
                    throw new Exception("Download multiple file not supported yet!");
                }
                else
                {
                    throw new Exception("Download ERROR: no file selected to download!");
                }
            }
            throw new Exception("Download ERROR!");
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