using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace windows_explorer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {

                    var hostName = System.Net.Dns.GetHostName();
                    var ips = System.Net.Dns.GetHostAddresses(hostName).Where(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
                    var ipArr = ips.Select(ip => ip.ToString()).ToList();


                    var CustomHost = config.GetSection("CustomHost").Value;
                    if (!string.IsNullOrEmpty(CustomHost))
                    {
                        ipArr.Add(CustomHost);
                    }
                    
                    var CustomPort = config.GetSection("CustomPort").Value;
                    var urls = string.Join(";", ipArr.Select(ip => $@"http://{ip}:{CustomPort}").ToArray());

                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseUrls(urls);
                });
        }
    }
}