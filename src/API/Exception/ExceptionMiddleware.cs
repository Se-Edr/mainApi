
using Microsoft.AspNetCore.Mvc;

namespace Avantime.API.Exception
{
    public class ExceptionMiddleware(/*ILogger<ExceptionMiddleware> _logger*/) : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next.Invoke(context);
            }
            catch(System.Exception ex)
            {

                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsJsonAsync(
                     new ProblemDetails
                     {
                         Type = ex.GetType().Name,
                         Title = "Some Error Occured ",
                         Detail = ex.Message
                     });
            }
        }
    }
}
