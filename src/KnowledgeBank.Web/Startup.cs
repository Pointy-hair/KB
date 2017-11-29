using IdentityServer4.EntityFramework.DbContexts;
using KnowledgeBank.Domain;
using KnowledgeBank.Multitenant;
using KnowledgeBank.Persistence;
using KnowledgeBank.Persistence.Repositories;
using KnowledgeBank.Web.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using KnowledgeBank.Web.Extensions;
using KnowledgeBank.Web.Settings;
using KnowledgeBank.Web.Helpers;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Rewrite;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration.UserSecrets;
using Serilog;

[assembly: UserSecretsId("aspnet-knowledgebank-d97f0c40-90bf-4c2a-b69c-0f2d9c8377c2")]
namespace KnowledgeBank.Web
{
	public class Startup
	{
		public Startup(IHostingEnvironment env)
		{
			var builder = new ConfigurationBuilder()
				.SetBasePath(env.ContentRootPath)
				.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
				.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
				.AddEnvironmentVariables();

			if (env.IsDevelopment())
				builder.AddUserSecrets<Startup>();

			Configuration = builder.Build();

			Log.Logger = new LoggerConfiguration()
				.WriteTo.LiterateConsole()
				.WriteTo.RollingFile(Configuration["Logging:LogPath"], buffered: true)
				.CreateLogger();

			HostingEnvironment = env;
		}

		private IHostingEnvironment HostingEnvironment { get; set; }
		public IConfigurationRoot Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
            var assetSettings = Configuration.GetSection("AssetSettings");
            services.Configure<AssetSettings>(assetSettings);

			var mailSettings = Configuration.GetSection("MailSettings");
			services.Configure<MailSettings>(mailSettings);
            
			var s2 = Configuration.GetSection("ShardingSettings");
			services.Configure<ShardingConfiguration>(s2);

			string defaultConnectionString = Configuration.GetConnectionString("DefaultConnection");
			services.AddDbContext<ApplicationIdentityDbContext>(options =>
				options.UseSqlServer(defaultConnectionString));

			var shardMapConnectionString = Configuration["ConnectionStrings:ShardMapConnection"];
			services.AddTenantContextFactory<ApplicationDbContext, long, TenantProvider>(
				shardMapConnectionString,
				(id, factory, options) => new ApplicationDbContext(id, factory, options));
			services.AddShardManager<long>(shardMapConnectionString);

			// Add framework services.
			services.AddScoped<IHttpContextAccessor, HttpContextAccessor>();
			services.AddScoped<IEmailSender, AuthMessageSender>();
			services.AddScoped<ISmsSender, AuthMessageSender>();

			ConfigureRepositories(services);

			services.AddIdentity<ApplicationUser, IdentityRole>()
				.AddEntityFrameworkStores<ApplicationIdentityDbContext>()
				.AddDefaultTokenProviders();

			services.AddMvc(options =>
			{
				if (HostingEnvironment.IsProduction())
					options.Filters.Add(new RequireHttpsAttribute());
			});

			var migrationsAssembly = typeof(ApplicationDbContext).GetTypeInfo().Assembly.GetName().Name;
			string certName = Configuration["IdentityServer:Certificate"];
			string certPassword = Configuration["IdentityServer:CertificatePassword"];
			var cert = new X509Certificate2(certName, certPassword);
			services.AddIdentityServer()
				.AddSigningCredential(cert)
				.AddConfigurationStore(builder =>
					builder.UseSqlServer(defaultConnectionString, options =>
						options.MigrationsAssembly(migrationsAssembly)))
                .AddConfigurationStoreCache()
				.AddOperationalStore(builder =>
					builder.UseSqlServer(defaultConnectionString, options =>
						options.MigrationsAssembly(migrationsAssembly)))
				.AddAspNetIdentity<ApplicationUser>();
		}

		private void ConfigureRepositories(IServiceCollection services)
		{
			services.AddScoped<CaseRepository>();
			services.AddScoped<IRepository<Step>, Repository<Step>>();
			services.AddScoped<IRepository<Tag>, Repository<Tag>>();
			services.AddScoped<IRepository<Link>, Repository<Link>>();
			services.AddScoped<IRepository<Attachment>, Repository<Attachment>>();
		}

		private void UseAuthentication(IApplicationBuilder api, string identityAuthority)
		{
			api.UseIdentityServerAuthentication(new IdentityServerAuthenticationOptions
			{
				Authority = identityAuthority,
				RequireHttpsMetadata = true,
				AutomaticAuthenticate = true,
				AutomaticChallenge = true,
				ApiName = "webapi",
			});
		}

		private void ApiPipeline(IApplicationBuilder api)
		{
			string identityAuthority = Configuration.GetValue<string>("IdentityServer:Authority");
			UseAuthentication(api, identityAuthority);


			api.Use(async (context, next) =>
			{
				string selectedTenant = context.Request.Query["tenant"];

				if (string.IsNullOrWhiteSpace(selectedTenant))
				{
					context.Response.StatusCode = 400;
					var response = Encoding.Default.GetBytes("Tenant not selected");
					await context.Response.Body.WriteAsync(response, 0, response.Length);
					return;
				}

				if (!context.User.Identity.IsAuthenticated)
				{
					context.Response.StatusCode = 401;
					return;
				}

				//request is authorized if role is admin or has claims on selected tenants
				var isAuthorized =
					context.User.FindFirst(x => x.Type == "role").Value.Equals(Role.Admin, StringComparison.InvariantCultureIgnoreCase) ||
					context.User.FindAll("tenant").Any(x => x.Value.Equals(selectedTenant, StringComparison.InvariantCultureIgnoreCase));

				if (isAuthorized)
				{
					var tenant = long.Parse(selectedTenant);
					context.Items["tenant"] = tenant;
					await next();
				}
				else
				{
					context.Response.StatusCode = 403;
					var response = Encoding.Default.GetBytes("Access to selected tenant is forbidden");
					await context.Response.Body.WriteAsync(response, 0, response.Length);
				}
			});

			api.UseMvc();
		}
		private void InfoPipeline(IApplicationBuilder api)
		{
			string identityAuthority = Configuration.GetValue<string>("IdentityServer:Authority");
			UseAuthentication(api, identityAuthority);

			api.UseMvc();
		}
		private void IdentityPipeline(IApplicationBuilder identityApp)
		{
			identityApp.UseIdentity();
			identityApp.UseMicrosoftAccountAuthentication(new MicrosoftAccountOptions()
			{
				ClientId = Configuration["Authentication:Microsoft:ClientId"],
				ClientSecret = Configuration["Authentication:Microsoft:ClientSecret"],
			});

			identityApp.UseIdentityServer();

			identityApp.UseMvcWithDefaultRoute();
		}


		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
			loggerFactory.AddSerilog();

			var appLifeTime = app.ApplicationServices.GetService<IApplicationLifetime>();
			appLifeTime.ApplicationStopped.Register(Log.CloseAndFlush);

			var dbContext = app.ApplicationServices.GetService<ApplicationIdentityDbContext>();
			var persistedGrantDbContext = app.ApplicationServices.GetService<PersistedGrantDbContext>();
			var configurationDbContext = app.ApplicationServices.GetService<ConfigurationDbContext>();

			dbContext.Database.Migrate();
			persistedGrantDbContext.Database.Migrate();
			configurationDbContext.Database.Migrate();
			DbInitializer.Initialize(dbContext, app.ApplicationServices);
			//in production environment js client will be hosted on same server as the api
			var jsHost = env.IsDevelopment() ? Configuration["Hosting:JS"] : Configuration["Hosting:API"];
			DbInitializer.InitializeConfigStore(configurationDbContext, jsHost);

			if (env.IsProduction())
				app.UseRewriter(new RewriteOptions().AddRedirectToHttpsPermanent());

			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseDatabaseErrorPage();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
			}

			app.UseDefaultFiles();
			app.UseStaticFiles();
			app.UseStaticFiles("/identity");

			app.Map("/api", ApiPipeline);
			app.Map("/info", InfoPipeline);
			app.Map("/identity", IdentityPipeline);

			app.UseMvc(router =>
			{
				router.MapRoute("Spa", "{*url}", defaults: new { controller = "Spa", action = "Index" });
			});


		}
	}
}
