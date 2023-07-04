using Microsoft.AspNetCore.StaticFiles;
using System.Collections.Concurrent;
using System.Globalization;
using windows_explorer.Core;

namespace windows_explorer.Models
{
    public class FileIconModel
    {
        public FileIconModel(string extention)
        {
            Extention = extention ?? "";
            Width = 100;
            Height = 100;
            Scale = 1;
        }

        public FileIconModel(string extention, int width = 100, int height = 100, double scale = 1) : this(extention)
        {
            Width = width;
            Height = height;
            Scale = scale;
        }

        


        public int Width { get; private set; }
        public int Height { get; private set; }
        public double Scale { get; private set; }
        public string Extention { get; private set; }
        public string Content 
        { 
            get
            {
                if (string.IsNullOrEmpty(_content))
                {
                    _content = $@"<?xml version='1.0' encoding='UTF-8'?>
<svg width='{Width * Scale}px' height='{Height * Scale}px' viewBox='0 0 {Width} {Height}' version='1.1' xmlns='http://www.w3.org/2000/svg' xmlns:xlink='http://www.w3.org/1999/xlink'>
    <title>file_doc</title>
    <desc>Created with Sketch.</desc>
    <defs></defs>
    <g stroke='none' stroke-width='1' fill='none' fill-rule='evenodd'>
        <g>
            <g transform='translate({wp(10)}, 0)' fill-rule='nonzero'>
                <polygon id='Shape' fill='#F0F0F0' points='{wp(57.48)} 0 {wp(0.04)} 0 {wp(0.04)} {hp(100.04)} {wp(79.96)} {hp(100.04)} {wp(79.96)} {hp(22.51)}'></polygon>
                <polygon id='Shape' fill='#E2E2E2' points='{wp(57.48)} {hp(22.51)} {wp(57.48)} 0 {wp(40)} 0 {wp(40)} {hp(100.04)} {wp(79.96)} {hp(100.04)} {wp(79.96)} {hp(22.51)}'></polygon>
            </g>
            <rect fill='#{_mainColorRGB}' fill-rule='nonzero' x='0' y='{hp(35)}' width='{wp(100)}' height='{hp(51)}'></rect>
            <rect fill='#00000020' fill-rule='nonzero' x='{wp(50)}' y='{hp(35)}' width='{wp(52)}' height='{hp(51)}'></rect>
            <text font-family='monospace,Tahoma' font-size='{wp(_fontSize)}' font-weight='bold' fill='#FFFFFF'>
                <tspan x='{wp(5)}' y='{hp(80)}'>{_text.ToUpper()}</tspan>
            </text>
        </g>
    </g>
</svg>";
                }
                return _content;
            }
        }



        private static ConcurrentDictionary<string, FileIconModel> IconContentsDic = new ConcurrentDictionary<string, FileIconModel>();
        private string _mainColorRGB
        {
            get
            {
                var filetype = GetFileTypeFromFileName("." + Extention);
                string mainColorRGB = "EBCDB8";

                switch (filetype)
                {
                    case FileType.image:
                        mainColorRGB = "3FC779";
                        break;
                    case FileType.video:
                        mainColorRGB = "444444";
                        break;
                    case FileType.audio:
                        mainColorRGB = "E24C96";
                        break;
                    case FileType.file:
                        mainColorRGB = "A2B2CB";
                        break;
                    default:
                        break;
                }

                if (Extention.ToLower() == "pdf")
                {
                    mainColorRGB = "E2574C";
                }
                else if (Extention.ToLower().StartsWith("doc"))
                {
                    mainColorRGB = "4A90E2";
                }

                return mainColorRGB;
            }
        }
        private double _fontSize
        {
            get
            {
                if (Extention.Length < 5)
                {
                    return 30;
                }
                return 20;
            }
        }
        private string _text => Extention.Length < 7 ? Extention : (Extention.Substring(0, 6) + "~");
        
        private string _content = "";



        private string wp(double p) => (p * Width / 100).ToString(CultureInfo.GetCultureInfo("en-US"));
        private string hp(double p) => (p * Height / 100).ToString(CultureInfo.GetCultureInfo("en-US"));
        //public string GeyUniqueKey()
        //{
        //    return GeyIconUniqueKey(Extention, Width, Height, Scale);
        //}
        public override int GetHashCode()
        {
            return Content.GetFixedHashCode();
        }


        private static string GetMimeMapping(string fileName)
        {
            var provider = new FileExtensionContentTypeProvider();
            provider.Mappings.TryGetValue(fileName, out var mimeType);
            return mimeType;
        }

        private static FileType GetFileTypeFromMime(string extention)
        {
            var mime = (extention ?? "").ToLower();
            if (mime.StartsWith("image"))
            {
                return FileType.image;
            }
            else if (mime.StartsWith("video"))
            {
                return FileType.video;
            }
            else if (mime.StartsWith("audio"))
            {
                return FileType.audio;
            }
            return FileType.file;
        }

        private static FileType GetFileTypeFromFileName(string extention)
        {
            var mimeType = GetMimeMapping(extention);
            return GetFileTypeFromMime(mimeType);
        }

        public static string GeyIconUniqueKey(string extention, int width = 100, int height = 100, double scale = 1)
        {
            return $@"{extention}_{width}_{height}_{scale}";
        }

        

        public static FileIconModel GetIconSvg(string extention, int width = 100, int height = 100, double scale = 1)
        {
            string key = GeyIconUniqueKey(extention, width, height, scale);
            if (!IconContentsDic.Keys.Contains(key))
            {
                var iconModel = new FileIconModel(extention, width, height, scale);
                IconContentsDic.TryAdd(key, iconModel);
            }
            return IconContentsDic[key];
        }

        //public static bool IsIconSvgGenerated(string extention, int width, int height, double scale) => IconContentsDic.Keys.Any(i => i == GeyIconUniqueKey(extention, width, height, scale));


    }
}
