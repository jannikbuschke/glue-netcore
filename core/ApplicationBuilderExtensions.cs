using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using Glow.Authentication.Aad;
using JannikB.Glue.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Glow.Core
{

    public static class ApplicationBuilderExtensions
    {
        public static void AddGlowErrorHandler(this IApplicationBuilder app, IWebHostEnvironment env, IConfiguration configuration)
        {
            app.UseExceptionHandler(CreateErrorHandler(env, configuration));
        }

        public static Action<IApplicationBuilder> CreateErrorHandler(IWebHostEnvironment env, IConfiguration configuration)
        {
            var withDetails = env.IsDevelopment() || "true".Equals(configuration["EnableExceptionDetails"], StringComparison.OrdinalIgnoreCase);

            void errorHandler(IApplicationBuilder options)
            {
                options.Run(async context =>
                {
                    IExceptionHandlerFeature errorFeature = context.Features.Get<IExceptionHandlerFeature>();
                    Exception exception = errorFeature.Error;

                    var errorDetail = exception.ToString();

                    ProblemDetails toProblemDetails(Exception ex)
                    {
                        var instance = $"urn:gertrud:error:{Guid.NewGuid()}";

                        if (ex is BadHttpRequestException badHttpRequestException)
                        {
                            return new ProblemDetails
                            {
                                Title = "Invalid request",
                                Status = (int) typeof(BadHttpRequestException).GetProperty("StatusCode",
                                BindingFlags.NonPublic | BindingFlags.Instance).GetValue(badHttpRequestException),
                                Detail = badHttpRequestException.Message,
                                Type = "kestrel_bad_request"
                            };
                        }
                        else if (ex is BadRequestException)
                        {
                            return new ProblemDetails
                            {
                                Title = "Bad request",
                                Status = (int) HttpStatusCode.BadRequest,
                                Detail = ex.Message,
                                Type = "bad_request"
                            };
                        }
                        else if (ex is ForbiddenException)
                        {
                            return new ProblemDetails
                            {
                                Title = "Forbidden",
                                Status = (int) HttpStatusCode.Forbidden,
                                Detail = ex.Message,
                                Type = "forbidden_request"
                            };
                        }
                        else if (ex is MissingConsentException mce)
                        {
                            var problemDetails = new ProblemDetails
                            {
                                Title = "Missing consent",
                                Status = (int) HttpStatusCode.Forbidden,
                                Detail = ex.Message,
                                Type = "missing_consent"
                            };
                            problemDetails.Extensions.Add("extensions", new Dictionary<string, object> { { "scope", mce.Scope } });
                            return problemDetails;
                        }
                        else if (ex is DbUpdateConcurrencyException dbc)
                        {
                            var problemDetails = new ProblemDetails
                            {
                                Title = "Conflict",
                                Status = (int) HttpStatusCode.Conflict,
                                Detail = "Datensatz wurde zwischenzeitlich bearbeitet. Bitte Seite neu laden und erneut probieren.",
                                Type = "db_conflict"
                            };
                            return problemDetails;
                        }
                        else
                        {
                            return new ProblemDetails
                            {
                                Title = "Internal error occurred.",
                                Status = 500,
                                Detail = withDetails ? exception.ToString() : null,
                            };
                        }
                    }

                    ProblemDetails problemDetails = toProblemDetails(exception);
                    if (problemDetails.Type != "bad_request")
                    {
                        ClaimsPrincipal user = context.User;
                        var id = user?.GetObjectId();
                        var name = user?.Name();
                        Log.Logger.Information("User is encountering an error {id} {user}", id, name);
                    }

                    context.Response.StatusCode = problemDetails.Status.Value;

                    //await context.Response.Write
                    await context.Response.WriteJson(problemDetails, "application/problem+json");
                });
            }

            return errorHandler;
        }


    }
}