using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Demo.MedTech.Api.Domain.Exceptions;
using Demo.MedTech.Api.Helpers;
using Demo.MedTech.DataModel.Exceptions;
using Demo.MedTech.Utility.Converter;
using Demo.MedTech.Utility.Helper;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Cosmos;
using Polly.CircuitBreaker;
using Polly.Timeout;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Demo.MedTech.Api.Application.Middleware
{
    /// <summary>
    /// Global exception handling for inbuilt as well as custom exceptions.
    /// All the exceptions thrown from anywhere after the application starts will be handled from here globally using HandleExceptionAsync().
    /// This file can be extended to handle further exceptions as required.
    /// </summary>
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext, IRequestPipe requestPipe, ICorrelationIdProvider correlationIdProvider)
        {
            var model = new ExceptionLogging();
            try
            {
                requestPipe.CorrelationId = correlationIdProvider.InitializeCorrelationId();
                //To read Request Body Enable Buffering
                httpContext.Request.EnableBuffering();
                var requestBody = await new StreamReader(httpContext.Request.Body)
                    .ReadToEndAsync();
                httpContext.Request.Body.Position = 0;
                try
                {
                    await _next(httpContext);
                }
                catch (Exception ex)
                {
                    model.RequestBody = requestBody;
                    model.RequestHeaders = JsonSerializer.Serialize(httpContext.Request.Headers.ToDictionary(header => header.Key, header => header.Value));
                    await HandleExceptionAsync(httpContext, ex, requestPipe,model);
                }
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(httpContext, ex, requestPipe, model);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception, IRequestPipe requestPipe, ExceptionLogging model)
        {
            var jsonSerializerOptions = new JsonSerializerOptions
            {
                Converters = { new SbsDateTimeConverter() },
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            // Here we can extend the implementation to add more type of exceptions if needed
            switch (exception)
            {
                case ValidationException validationException:
                    var validationsErrors = JsonSerializer.Serialize(validationException.Errors, jsonSerializerOptions);
                    context.Response.ContentType = "application/json";
                    context.Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
                    context.Response.Headers.Add("X-Status-Reason", "Validation failed");
                    await context.Response.WriteAsync(validationsErrors);
                    break;
                case HttpRequestException _:
                case BrokenCircuitException _:
                case TimeoutRejectedException _:
                    context.Response.ContentType = "application/json";
                    context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                    await context.Response.WriteAsync(new ErrorResult
                    {
                        StatusCode = context.Response.StatusCode,
                        Message = ResourceReader.ReadValue("ServiceUnavailable")
                    }.ToString());
                    break;
                case HeaderValidationException _:
                    model.Exception = exception.StackTrace;
                    context.Response.ContentType = "application/json";
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    await context.Response.WriteAsync(new ErrorResult
                    {
                        StatusCode = context.Response.StatusCode,
                        Message = ResourceReader.ReadValue("InsufficientHeaders")
                    }.ToString());
                    break;
                case TransientException transientException:
                    model.Exception = exception.StackTrace;
                    context.Response.ContentType = "application/json";
                    context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                    context.Response.Headers.Add("Retry-After", transientException.RetryAfterSeconds.ToString());
                    AddHeaders(context, transientException.HeaderDictionary);
                    await context.Response.WriteAsync(new ErrorResult
                    {
                        StatusCode = context.Response.StatusCode,
                        Message = ResourceReader.ReadValue("TransientMessage")
                    }.ToString());
                    break;
                case NonTransientException nonTransientException:
                    model.Exception = exception.ToString();
                    context.Response.ContentType = "application/json";
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    AddHeaders(context, nonTransientException.HeaderDictionary);
                    await context.Response.WriteAsync(new ErrorResult
                    {
                        StatusCode = context.Response.StatusCode,
                        Message = ResourceReader.ReadValue("NonTransientMessage")
                    }.ToString());
                    break;
                case RecordNotFoundException recordNotFoundException:
                    context.Response.ContentType = "application/json";
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    await context.Response.WriteAsync(new ErrorResult
                    {
                        StatusCode = context.Response.StatusCode,
                        Message = ResourceReader.ReadValue("RecordNotFound", recordNotFoundException.Key, recordNotFoundException.Value)
                    }.ToString());
                    break;
                case CosmosException { StatusCode: HttpStatusCode.PreconditionFailed }:
                    model.Exception = exception.StackTrace;
                    context.Response.ContentType = "application/json";
                    context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                    context.Response.Headers.Add("Retry-After", "3 second");
                    await context.Response.WriteAsync(new ErrorResult
                    {
                        StatusCode = context.Response.StatusCode,
                        Message = ResourceReader.ReadValue("TransientMessage")
                    }.ToString());
                    break;
                default:
                    if (exception.GetBaseException() is RuleEngineException || exception is RuleEngineException)
                    {
                        RuleEngineException objValidationResults = (exception is RuleEngineException ? exception : exception.GetBaseException()) as RuleEngineException;
                        objValidationResults.RuleValidationMessage.TimeStamp = DateTime.UtcNow;
                        objValidationResults.RuleValidationMessage.RequestId = requestPipe.CorrelationId;

                        string errors = JsonSerializer.Serialize(objValidationResults.RuleValidationMessage, jsonSerializerOptions);

                        if (objValidationResults.BaseException != null)
                        {
                            model.Exception = objValidationResults.BaseException.ToString();
                        }
                        else
                        {
                            model.Exception = errors;
                        }

                        context.Response.ContentType = "application/json";
                        context.Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
                        context.Response.Headers.Add("X-Status-Reason", "Validation failed");
                        await context.Response.WriteAsync(errors);
                    }
                    else
                    {
                        model.Exception = exception.ToString();
                        context.Response.ContentType = "application/json";
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        await context.Response.WriteAsync(new ErrorResult
                        {
                            StatusCode = context.Response.StatusCode,
                            Message = ResourceReader.ReadValue("NonTransientMessage")
                        }.ToString());
                    }
                    break;
            }
        }

        private string GetFullQualifiedNameSpace(Exception exception)
        {
            return exception.TargetSite?.DeclaringType?.FullName?.Replace("+<", ".").Split('>')[0];
        }

        /// <summary>
        /// Adds headers to the HttpContext from HeaderDictionary which are added by while code execution
        /// </summary>
        /// <param name="context"></param>
        /// <param name="headerDictionary"></param>
        private static void AddHeaders(HttpContext context, IReadOnlyDictionary<string, string> headerDictionary)
        {
            if (headerDictionary != null)
            {
                foreach (var (key, value) in headerDictionary)
                {
                    context.Response.Headers.Add(key, value);
                }
            }
        }
    }

    public class ExceptionLogging
    {
        public string RequestHeaders { get; set; }

        public string RequestBody { get; set; }

        public string Exception { get; set; }
    }
}