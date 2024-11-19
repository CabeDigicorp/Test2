using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Text.Json;
using System.Text.Json.Serialization;
using Serilog;

namespace JoinApi.Service
{
    public class ErrorHandlerMiddleware : IMiddleware
    {
        private readonly Serilog.ILogger _logger;

        public ErrorHandlerMiddleware(Serilog.ILogger logger)
        {
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (Exception exception)
            {
                _logger./*ForContext("HttpContext", JsonSerializer.Serialize(context)).ForContext("RequestDelegate", JsonSerializer.Serialize(next)).*/
                    Fatal(exception, "Rilevata eccezione non gestita si è verificata mentre veniva eseguita la richista!");

                context.Response.Clear();
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            }
        }
    }
}
