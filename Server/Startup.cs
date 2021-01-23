using System;
using System.IO;
using NSwag;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NJsonSchema;
using NJsonSchema.Generation;
using Server.Data;

namespace Server
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddControllers();

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
					document.Info.Description = "Файловое хранилище";
				};
			});

			services.AddDbContext<ApplicationDbContext>(options =>
				options.UseNpgsql(
					Configuration.GetConnectionString("DefaultConnection")));

			services.Configure<IdentityOptions>(options =>
			{
				// Password settings.
				options.Password.RequireDigit = true;
				options.Password.RequireLowercase = true;
				options.Password.RequireNonAlphanumeric = true;
				options.Password.RequireUppercase = true;
				options.Password.RequiredLength = 6;
				options.Password.RequiredUniqueChars = 1;

				// Lockout settings.
				options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
				options.Lockout.MaxFailedAccessAttempts = 5;
				options.Lockout.AllowedForNewUsers = true;

				// User settings.
				options.User.AllowedUserNameCharacters =
					"abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
				options.User.RequireUniqueEmail = false;
			});

			services.ConfigureApplicationCookie(options =>
			{
				// Cookie settings
				options.Cookie.HttpOnly = true;
				options.ExpireTimeSpan = TimeSpan.FromMinutes(5);

				options.LoginPath = "/Identity/Account/Login";
				options.AccessDeniedPath = "/Identity/Account/AccessDenied";
				options.SlidingExpiration = true;
			});
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}


			app.UseHttpsRedirection();

			app.UseRouting();


			//Swagger
			app.UseOpenApi(settings => settings.PostProcess = (doc, _) => doc.Schemes = new[] { OpenApiSchema.Https, OpenApiSchema.Http });
			app.UseSwaggerUi3();

			app.UseAuthentication();
			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}
	}
}
