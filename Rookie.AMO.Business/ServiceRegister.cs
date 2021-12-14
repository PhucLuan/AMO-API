using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Refit;
using Rookie.AMO.Business.Interfaces;
using Rookie.AMO.Business.Services;
using Rookie.AMO.DataAccessor;
using System;
using System.Reflection;

namespace Rookie.AMO.Business
{
    public static class ServiceRegister
    {
        public static void AddBusinessLayer(this IServiceCollection services, IConfiguration configuration)
        {

            services.AddDataAccessorLayer(configuration);
            services.AddAutoMapper(Assembly.GetExecutingAssembly());
            services.AddTransient(typeof(IBaseRepository<>), typeof(BaseRepository<>));
            services.AddTransient<ICategoryService, CategoryService>();
            services.AddTransient<IAssetService, AssetService>();
            services.AddTransient<IAssignmentService, AssignmentService>();
            services.AddTransient<IRequestService, RequestService>();
            services.AddTransient<IReportService, ReportService>();
            services
                .AddRefitClient<IIdentityProvider>(new RefitSettings
                {
                    ContentSerializer = new NewtonsoftJsonContentSerializer(
                        new JsonSerializerSettings
                        {
                            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                            DateFormatString = "dd'/'MM'/'yyyy"
                        }
                    )
                })
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(configuration.GetSection("IdentityRefitUrl").Value));
        }
    }
}