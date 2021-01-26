using System.Text;
using System.Threading.Tasks;
using API.Middleware;
using Application.Interfaces;
using Application.User.Login;
using Domain;
using EFData;
using Infrastructure.Security;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using NJsonSchema;
using NJsonSchema.Generation;
using NSwag;

namespace API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
			services.AddSwaggerDocument(settings =>
			{
				settings.SchemaType = SchemaType.Swagger2;

				settings.AllowReferencesWithProperties = true;
				settings.DefaultReferenceTypeNullHandling = ReferenceTypeNullHandling.NotNull;
				settings.IgnoreObsoleteProperties = true;

				settings.GenerateXmlObjects = true;
				settings.GenerateAbstractProperties = true;

				settings.PostProcess = document =>
				{
					document.Info.Version = "v1";
					document.Info.Title = typeof(Startup).Namespace;
					document.Info.Description = "¿œ»";
				};
			});

            services.AddDbContext<DataContext>(opt => opt.UseNpgsql(Configuration.GetConnectionString("DefaultConnection")));
            
            services.AddMediatR(typeof(LoginHandler).Assembly);

			services.AddMvc(option =>
				{
					option.EnableEndpointRouting = false;
					var policy = new AuthorizationPolicyBuilder()
						.RequireAuthenticatedUser()
						.Build();
					option.Filters.Add(new AuthorizeFilter(policy));
				}).SetCompatibilityVersion(CompatibilityVersion.Latest);

            services.TryAddSingleton<ISystemClock, SystemClock>();

            //var builder = services.AddIdentity<AppUser, AppRole>();
            //         var identityBuilder = new IdentityBuilder(builder.UserType, builder.Services);
            //         identityBuilder.AddEntityFrameworkStores<DataContext>();
            //identityBuilder.AddRoleStore<IdentityRole<string>>();
            //         identityBuilder.AddSignInManager<SignInManager<AppUser>>();

			var builder = services.AddIdentity<AppUser, AppRole>()
				.AddEntityFrameworkStores<DataContext>()
				
				.AddSignInManager<SignInManager<AppUser>>();

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["TokenKey"]));
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(
                opt =>
                    {
                        opt.TokenValidationParameters = new TokenValidationParameters
                                                            {
                                                                ValidateIssuerSigningKey = true,
                                                                IssuerSigningKey = key,
                                                                ValidateAudience = false,
                                                                ValidateIssuer = false,
                                                            };
                    });

            services.AddScoped<IJwtGenerator, JwtGenerator>();

			
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
            app.UseMiddleware<ErrorHandlingMiddleware>();
            app.UseAuthentication();
            app.UseMvcWithDefaultRoute();

			//Swagger
			app.UseOpenApi(settings => settings.PostProcess = (doc, _) => doc.Schemes = new[] { OpenApiSchema.Https, OpenApiSchema.Http });
			app.UseSwaggerUi3();
		}

		
    }
}
