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
        public bool success { get; set; } = true;
        public long errorCode { get; set; } = 0;
        public string errorText { get; set; } = "";
        public T result { get; set; }
    }

    public class CreateDirVM
    {
        public List<KeyName> pathInfo { get; set; } = new List<KeyName>();
        public string name { get; set; }
    }

    public class RenameVM
    {
        public List<KeyName> pathInfo { get; set; } = new List<KeyName>();
        public bool isDirectory { get; set; }
        public string name { get; set; }
    }

    public class RemoveVM
    {
        public List<KeyName> pathInfo { get; set; } = new List<KeyName>();
        public bool isDirectory { get; set; }
    }

    public class CopyMoveVM
    {
        public List<KeyName> sourcePathInfo { get; set; } = new List<KeyName>();
        public List<KeyName> destinationPathInfo { get; set; } = new List<KeyName>();
        public bool sourceIsDirectory { get; set; }
    }

    public class UploadChunkVM
    {
        private ChunkMetadataVM _chunkMetadataModel = null;

        public List<KeyName> destinationPathInfo { get; set; } = new List<KeyName>();
        public string chunkMetadata { get; set; }
        public ChunkMetadataVM chunkMetadataModel 
        { 
            get 
            {
                if (_chunkMetadataModel == null)
                {
                    _chunkMetadataModel = System.Text.Json.JsonSerializer.Deserialize<ChunkMetadataVM>(chunkMetadata);
                }
                return _chunkMetadataModel;
            } 
        }
    }

    public class ChunkMetadataVM
    {
        public string UploadId { get; set; }
        public string FileName { get; set; }
        public int Index { get; set; }
        public int TotalCount { get; set; }
        public long FileSize { get; set; }
    }
}
