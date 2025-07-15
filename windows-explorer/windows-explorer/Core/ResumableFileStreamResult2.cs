using System.Net;
using Microsoft.Net.Http.Headers;
namespace Microsoft.AspNetCore.Mvc
{
    public class ResumableFileStreamResult2 : FileStreamResult
    {
        public ResumableFileStreamResult2(Stream fileStream, string contentType)
            : base(fileStream, contentType)
        {
            EnableRangeProcessing = true;
        }
    }

}