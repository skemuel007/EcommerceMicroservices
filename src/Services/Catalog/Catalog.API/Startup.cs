using Catalog.API.Data;
using Catalog.API.Middlewares;
using Catalog.API.Repositories;
using Catalog.API.Utils;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Catalog.API
{
    public class Startup
    {
        readonly string AllowedOriginSpecifications = "AllowOrigin";
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        [Obsolete]
        public void ConfigureServices(IServiceCollection services)
        {
            // configure cors
            services.AddCors(c =>
            {
                c.AddPolicy(name: AllowedOriginSpecifications, options => options
                .SetIsOriginAllowed(_ => true)
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod());
            });
            
            // add fluent validation support in controllers

            // add compression to response
            services.AddResponseCompression();

            // configure automapper
            services.AddAutoMapper(typeof(Startup));

            // enable api health check
            services.AddHealthChecks();
            services.AddHttpContextAccessor();

            services.AddMemoryCache(options =>
            {
                options.SizeLimit = 1024;
            }
            );

            services.AddResponseCaching(options =>
            {
                // Each response cannot be more than 1 KB 
                options.MaximumBodySize = 1024;

                // Case Sensitive Paths 
                // Responses to be returned only if case sensitive paths match
                options.UseCaseSensitivePaths = true;
            });

            // Database extension properties
            services.Configure<CatalogDatabaseSettings>(
                Configuration.GetSection(nameof(CatalogDatabaseSettings)));

            services.AddSingleton<ICatalogDatabaseSettings>(sp =>
                sp.GetRequiredService<IOptions<CatalogDatabaseSettings>>().Value);

            services.AddControllers()
                .AddFluentValidation(v =>
                {
                    // Note: it is possible to use data annotation and fluent validation at the same time
                    v.RegisterValidatorsFromAssemblyContaining<Startup>();
                })
                .AddNewtonsoftJson(options => {
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                    options.UseMemberCasing();
                });


            // Configure Api version
            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = ApiVersionReader.Combine(
                    new QueryStringApiVersionReader("api-version"),
                    new HeaderApiVersionReader("api-version")
                   );
            });

            // versioning explorer
            services.AddVersionedApiExplorer(options =>
            {
                // add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
                // note: the specified format code will format the version as "'v'major[.minor][-status]"
                options.GroupNameFormat = "'v'VVV";

                // note: this option is only necessary when versioning by url segment. the SubstitutionFormat
                // can also be used to control the format of the API version in route templates
                options.SubstituteApiVersionInUrl = true;
            });

            services.AddScoped<ICatalogContext, CatalogContext>();

            services.AddTransient<IProductRepository, ProductRepository>();

            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
            services.AddSwaggerGen(options => options.OperationFilter<SwaggerDefaultValues>());

            // format global json message response
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = actionContext =>
                {
                    var errors = actionContext.ModelState.Where(e =>
                       e.Value.Errors.Count > 0
                       ).Select(e => new
                       {
                           Error = e.Value.Errors.First().ErrorMessage,
                       }).ToArray();

                    return new BadRequestObjectResult(new
                    {
                        Status = false,
                        Message = errors,
                        Data = new { }
                    });
                };
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider provider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(
                    options =>
                    {
                    // build a swagger endpoint for each discovered API version
                    foreach (var description in provider.ApiVersionDescriptions)
                        {
                        //options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
                        options.SwaggerEndpoint($"{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
                        }
                    });
            }

            app.UseMiddleware<GlobalErrorHandler>();

            app.UseRouting();

            // use cors
            app.UseCors(AllowedOriginSpecifications);
            app.UseResponseCaching();

            app.UseAuthorization();

            app.UseMiddleware<RateLimitMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
