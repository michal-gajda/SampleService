namespace CompanyName.SampleService.WebApi.Middlewares
{
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Microsoft.IO;

    [ExcludeFromCodeCoverage]
    internal sealed class RequestResponseLoggingMiddleware
    {
        private readonly ILogger logger;
        private readonly RequestDelegate next;
        private readonly RecyclableMemoryStreamManager recyclableMemoryStreamManager;

        public RequestResponseLoggingMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            this.logger = loggerFactory.CreateLogger<RequestResponseLoggingMiddleware>();
            this.next = next;
            this.recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
        }

        public async Task Invoke(HttpContext context)
        {
            await this.LogRequest(context).ConfigureAwait(true);
            await this.LogResponse(context).ConfigureAwait(true);
        }

        private async Task LogRequest(HttpContext context)
        {
            context.Request.EnableBuffering();
            await using var requestStream = this.recyclableMemoryStreamManager.GetStream();
            await context.Request.Body.CopyToAsync(requestStream).ConfigureAwait(true);
            var text = ReadStreamInChunks(requestStream);

            if (!string.IsNullOrEmpty(text))
            {
                this.logger.LogDebug(text);
            }

            context.Request.Body.Position = 0;
        }

        private async Task LogResponse(HttpContext context)
        {
            var originalBodyStream = context.Response.Body;
            await using var responseBody = this.recyclableMemoryStreamManager.GetStream();
            context.Response.Body = responseBody;
            await this.next(context);
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var text = await new StreamReader(context.Response.Body).ReadToEndAsync();
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            this.logger.LogDebug(text);
            await responseBody.CopyToAsync(originalBodyStream);
        }

        private static string ReadStreamInChunks(Stream stream)
        {
            const int readChunkBufferLength = 4096;
            stream.Seek(0, SeekOrigin.Begin);
            using var textWriter = new StringWriter();
            using var reader = new StreamReader(stream);
            var readChunk = new char[readChunkBufferLength];
            int readChunkLength;

            do
            {
                readChunkLength = reader.ReadBlock(readChunk, 0, readChunkBufferLength);
                textWriter.Write(readChunk, 0, readChunkLength);
            } while (readChunkLength > 0);

            return textWriter.ToString();
        }
    }
}
