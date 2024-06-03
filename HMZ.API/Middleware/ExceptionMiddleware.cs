using System.Net;
using System.Text.Json;
namespace HMZ.API.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _requestDelegate;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ExceptionMiddleware(RequestDelegate requestDelegate, ILogger<ExceptionMiddleware> logger, IHostEnvironment hostEnvironment, IWebHostEnvironment webHostEnvironment)
        {
            _hostEnvironment = hostEnvironment;
            _requestDelegate = requestDelegate;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _requestDelegate(httpContext);
            }
            catch (Exception ex)
            {
                // Log error to file
                var logPath = Path.Combine(_webHostEnvironment.ContentRootPath, "Logs");
                if (!Directory.Exists(logPath))
                {
                    Directory.CreateDirectory(logPath);
                }
                var logFile = Path.Combine(logPath, $"ErrorLog_{DateTime.Now:yyyyMMdd}.txt");
                await File.AppendAllTextAsync(logFile, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {ex.Message}{Environment.NewLine}{ex.StackTrace}{Environment.NewLine}");

                // Log error to console
                _logger.LogError(ex, ex.Message);

                // Return error response
                httpContext.Response.ContentType = "application/json";
                httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                var response = _hostEnvironment.IsDevelopment()
                    ? new ApiException(httpContext.Response.StatusCode, ex.Message, ex.StackTrace?.ToString())
                    : new ApiException(httpContext.Response.StatusCode, "Internal Server Error");
                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                var json = JsonSerializer.Serialize(response, options);
                await httpContext.Response.WriteAsync(json);
            }
        }
    }
}
