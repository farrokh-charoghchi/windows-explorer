using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using System.Diagnostics;
using windows_explorer.Core;
using windows_explorer.Models;

namespace windows_explorer.Controllers
{
    public class FileIconController : Controller
    {
        

        public FileIconController()
        {
        }

        public IActionResult Index(string type, int width = 100, int height = 100, double scale = 1)
        {
            var icon = FileIconModel.GetIconSvg(type, width, height, scale);
            string iconHash = icon.GetHashCode().ToString();

            if (Request.Headers["If-None-Match"].Any(x => x == iconHash))
            {
                Response.StatusCode = 304;
                return Content("");
            }

            Response.Headers.Add("Accept-Ranges", "bytes");
            Response.Headers.Add("ETag", iconHash);

            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(icon.Content);
            return File(bytes, "image/svg+xml");
        }

        public IActionResult test()
        {
            var hostName = System.Net.Dns.GetHostName();
            var ips = System.Net.Dns.GetHostAddresses(hostName).Where(ip=>ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
            return Content(string.Join("|", ips.Select(ip=>ip.ToString()).ToArray()));
        }

    }
}
