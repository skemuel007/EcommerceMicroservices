using Catalog.API.Utils;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;

namespace Catalog.API.Middlewares
{
    public class GlobalErrorHandler
    {
        private readonly RequestDelegate _next;

        /// <summary>
        /// Global error handler request method
        /// </summary>
        /// <param name="next"></param>
        public GlobalErrorHandler(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);

            }
            catch (Exception e)
            {
                if (e.InnerException != null)
                {
                    // TODO: write logs to Log service
                }
                else
                {
                    // TODO: write the logs to Log service
                }
                await HandleErrorAsync(context, e);
            }
        }

        public static Task HandleErrorAsync(HttpContext context, Exception exception)
        {
            switch (exception)
            {
                // checks for application error
                case AppException e:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;
                case ValidationException e:
                    context.Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
                    break;
                // not found error
                case KeyNotFoundException e:
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    break;
                // all unhandled error
                default:
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    break;
            }

            // create response
            var response = new { Status = false, Message = exception.InnerException != null ? exception.InnerException.Message : exception.Message, Data = new { } };
            var payload = JsonConvert.SerializeObject(response);
            context.Response.ContentType = "application/json";

            return context.Response.WriteAsync(payload);
        }
    }
}
