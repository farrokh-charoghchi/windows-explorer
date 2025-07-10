using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace windows_explorer.Models.DevFileManagerModels
{
    public class KeyName
    {
        public string key { get; set; }
        public string name { get; set; }
    }

    public class PathInfo
    {
        public List<KeyName> pathInfo { get; set; } = new List<KeyName>();
        public List<List<KeyName>> pathInfoList { get; set; } = new List<List<KeyName>>();
    }

    public class PathInfoResult
    {
        public string key { get; set; }
        public string name { get; set; }
        public string dateModified { get; set; }
        public string thumbnail { get; set; }
        public bool isDirectory { get; set; }
        public bool hasSubDirectories { get; set; }
        public long size { get; set; }
    }

    public class DevFileManagerResult<T> where T : class, new()
    {
        //success = true,
        //errorCode = 0,
        //errorText = "",
        //result = res
        public bool success { get; set; } = true;
        public long errorCode { get; set; } = 0;
        public string errorText { get; set; } = "";
        public T result { get; set; }
    }


}
