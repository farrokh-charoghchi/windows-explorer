using System.IO;
using System.Linq;
using System.Net;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.Mvc
{
    public class ResumableFileStreamResult : FileStreamResult
    {
        // default buffer size as defined in BufferedStream type
        private const int BufferSize = 0x1000;
        private string MultipartBoundary = $@"<{Guid.NewGuid().ToString().Replace("-", "").ToLower()}>";
        private string downloadFileName = null;


        public ResumableFileStreamResult(Stream fileStream, string contentType)
        : base(fileStream, contentType)
        {

        }

        public ResumableFileStreamResult(Stream fileStream, string contentType, string filename)
        : base(fileStream, contentType)
        {
            downloadFileName = filename;
        }

        private bool IsMultipartRequest(RangeHeaderValue range)
        {
            return range != null && range.Ranges != null && range.Ranges.Count > 1;
        }

        private bool IsRangeRequest(RangeHeaderValue range)
        {
            return range != null && range.Ranges != null && range.Ranges.Count > 0;
        }

        private async Task WriteResponseFileAsync(ActionContext context)
        {
            var length = FileStream.Length;

            var response = context.HttpContext.Response;

            //response.BufferOutput = false;

            //var range = context.HttpContext.GetRanges(length);
            var range = context.HttpContext.Request.GetTypedHeaders().Range;

            if (IsMultipartRequest(range))
            {
                response.ContentType = $"multipart/byteranges; boundary={MultipartBoundary}";
            }
            else
            {
                response.ContentType = ContentType.ToString();
            }

            response.Headers.Append("Accept-Ranges", "bytes");

            if (!string.IsNullOrEmpty(downloadFileName))
            {
                // With setting the file name,
                // in the saving dialog, user will see
                // the [strFileName] name instead of [download]!
                response.Headers.Append("Content-Disposition", "attachment; filename=" + downloadFileName);
            }

            if (IsRangeRequest(range))
            {
                response.StatusCode = (int)HttpStatusCode.PartialContent;

                if (!IsMultipartRequest(range))
                {
                    response.Headers.Append("Content-Range", $"bytes {range.Ranges.First().From}-{range.Ranges.First().To}/{length}");
                }

                foreach (var rangeValue in range.Ranges)
                {
                    // TODO: multipart should be tested
                    if (IsMultipartRequest(range)) 
                    {
                        await response.WriteAsync($"--{MultipartBoundary}"
                        + Environment.NewLine
                        + $"Content-type: {ContentType}"
                        + Environment.NewLine
                        + $"Content-Range: bytes {range.Ranges.First().From}-{range.Ranges.First().To}/{length}"
                        + Environment.NewLine);
                    }

                    await WriteDataToResponseBodyAsync(rangeValue, response);

                    if (IsMultipartRequest(range))
                    {
                        await response.WriteAsync(Environment.NewLine);
                    }
                }

                if (IsMultipartRequest(range))
                {
                    await response.WriteAsync($"--{MultipartBoundary}--" + Environment.NewLine);
                }
            }
            else
            {

                FileStream.Seek(0, SeekOrigin.Begin);
                response.Headers.Append("Content-Length", FileStream.Length.ToString());
                byte[] buffer = new byte[BufferSize];
                int readSize = 0;
                while ((readSize = FileStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    await response.Body.WriteAsync(buffer, 0, readSize);
                    await response.Body.FlushAsync();
                }
            }
        }

        private async Task WriteDataToResponseBodyAsync(RangeItemHeaderValue rangeValue, HttpResponse response)
        {
            var startIndex = rangeValue.From ?? 0;
            var endIndex = rangeValue.To ?? 0;

            byte[] buffer = new byte[BufferSize];
            long totalToSend = endIndex - startIndex;
            int count = 0;

            long bytesRemaining = totalToSend + 1;
            response.Headers.Append("Content-Length", bytesRemaining.ToString());

            FileStream.Seek(startIndex, SeekOrigin.Begin);

            while (bytesRemaining > 0)
            {
                try
                {
                    if (bytesRemaining <= buffer.Length)
                        count = await FileStream.ReadAsync(buffer, 0, (int)bytesRemaining);
                    else
                        count = await FileStream.ReadAsync(buffer, 0, buffer.Length);

                    if (count == 0)
                        return;

                    //if (!response.IsClientConnected)
                    //{
                    //    return;
                    //}
                    await response.Body.WriteAsync(buffer, 0, count);

                    bytesRemaining -= count;
                }
                catch (IndexOutOfRangeException)
                {
                    await response.Body.FlushAsync();
                    return;
                }
                finally
                {
                    await response.Body.FlushAsync();
                }
            }
        }

        public override async Task ExecuteResultAsync(ActionContext context)
        {
            // Handle range requests or other custom logic here
            await WriteResponseFileAsync(context);
        }
    }
}