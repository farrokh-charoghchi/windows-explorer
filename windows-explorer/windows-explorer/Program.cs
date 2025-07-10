using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace windows_explorer
{
    
    public class Program
    {
        public static List<string> ipList = new List<string>();
        public static void Main(string[] args)
        {
            var hostBuilder = CreateHostBuilder(args).Build();

            // Get the WebRootPath after building the host
            var env = hostBuilder.Services.GetRequiredService<IWebHostEnvironment>();
            Console.WriteLine($"WebRootPath: {env.WebRootPath}");
            Console.WriteLine($"ContentRootPath: {env.ContentRootPath}");

            hostBuilder.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var config = new ConfigurationBuilder()
                .AddCommandLine(args)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            // Get the wwwrootpath from command line
            var wwwrootPath = config["wwwrootpath"];

            var host = Host.CreateDefaultBuilder(args);

            return host.ConfigureWebHostDefaults(webBuilder =>
                {

                    var hostName = System.Net.Dns.GetHostName();
                    var ips = System.Net.Dns.GetHostAddresses(hostName).Where(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
                    ipList = ips.Select(ip => ip.ToString()).ToList();

                    if (ipList.Contains("127.0.0.1"))
                    {
                        ipList.Remove("127.0.0.1");
                    }

                    var CustomHost = config.GetSection("CustomHost").Value;
                    if (!string.IsNullOrEmpty(CustomHost))
                    {
                        ipList.Add(CustomHost);
                    }

                    if (!ipList.Contains("localhost"))
                    {
                        ipList.Add("localhost");
                    }

                    var CustomPort = config.GetSection("CustomPort").Value;
                    var urls = string.Join(";", ipList.Select(ip => $@"http://{ip}:{CustomPort}").ToArray());

                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseUrls(urls);

                    // Set web root if path was provided
                    if (!string.IsNullOrEmpty(wwwrootPath))
                    {
                        if (!Path.IsPathRooted(wwwrootPath))
                        {
                            wwwrootPath = Path.GetFullPath(wwwrootPath, Directory.GetCurrentDirectory());
                        }
                        
                        webBuilder.UseWebRoot(wwwrootPath);
                    }
                });
        }
    }
}