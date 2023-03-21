using DevExpress.ExpressApp.ApplicationBuilder;
using DevExpress.ExpressApp.Blazor.ApplicationBuilder;
using DevExpress.ExpressApp.Blazor.Services;
using DevExpress.ExpressApp.Core;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.WebApi.Services;
using MainDemo.Blazor.Server.API.NonPersistent;
using MainDemo.Module;
using MainDemo.Module.BusinessObjects;
using MainDemo.Module.BusinessObjects.NonPersistent;
using MainDemo.WebApi.Services;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.OpenApi.Models;

namespace MainDemo.Blazor.Server;

public class Startup
{
  public Startup(IConfiguration configuration)
  {
    Configuration = configuration;
  }

  public IConfiguration Configuration { get; }

  public void ConfigureServices(IServiceCollection services)
  {
    //Non Persistent
    services.TryAddEnumerable(ServiceDescriptor.Scoped<IObjectSpaceCustomizer, NonPersistentObjectSpaceCustomizerService>());
    services.AddSingleton<NonPersistentObjectStorageService>();

    services.AddScoped<IDataService, CustomDataService>();

    services.AddRazorPages();
    services.AddServerSideBlazor();
    services.AddHttpContextAccessor();
    services.AddScoped<CircuitHandler, Services.Circuits.CircuitHandlerProxy>();
    services.AddXaf(Configuration, builder =>
    {
      builder.UseApplication<MainDemoBlazorApplication>();
      builder.Modules
              .AddAuditTrailEFCore()
              .AddConditionalAppearance()
              .AddFileAttachments()
              .AddOffice()
              .AddReports(options =>
              {
              options.EnableInplaceReports = true;
              options.ReportDataType = typeof(DevExpress.Persistent.BaseImpl.EF.ReportDataV2);
              options.ReportStoreMode = DevExpress.ExpressApp.ReportsV2.ReportStoreModes.XML;
            })
              .AddValidation(options =>
              {
              options.AllowValidationDetailsAccess = false;
            })
              .Add<MainDemoModule>()
              .Add<MainDemoBlazorModule>();
      builder.ObjectSpaceProviders
              .AddEFCore().WithDbContext<MainDemoDbContext>(
                      (serviceProvider, businessObjectDbContextOptions) =>
                      {
                        // Uncomment this code to use an in-memory database. This database is recreated each time the server starts. With the in-memory database, you don't need to make a migration when the data model is changed.
                        // Do not use this code in production environment to avoid data loss.
                        // We recommend that you refer to the following help topic before you use an in-memory database: https://docs.microsoft.com/en-us/ef/core/testing/in-memory
                        //businessObjectDbContextOptions.UseInMemoryDatabase("InMemory");
                      string? connectionString = GetConnectionString(Configuration);

                      ArgumentNullException.ThrowIfNull(connectionString);
                      businessObjectDbContextOptions.UseSqlServer(connectionString);
                      businessObjectDbContextOptions.UseLazyLoadingProxies();
                      businessObjectDbContextOptions.UseChangeTrackingProxies();
                      businessObjectDbContextOptions.UseObjectSpaceLinkProxies();

                    })
              .AddNonPersistent();
      //builder.Security
      //    .UseIntegratedMode(options =>
      //    {
      //        options.RoleType = typeof(PermissionPolicyRole);
      //        options.UserType = typeof(ApplicationUser);
      //        options.UserLoginInfoType = typeof(ApplicationUserLoginInfo);
      //        options.SupportNavigationPermissionsForTypes = false;
      //    })
      //    .AddPasswordAuthentication(options => options.IsSupportChangePassword = true);
      builder.AddBuildStep(application =>
          {
          application.ApplicationName = "MainDemo";
          application.CheckCompatibilityType = DevExpress.ExpressApp.CheckCompatibilityType.DatabaseSchema;
          application.DatabaseVersionMismatch += (s, e) =>
              {
              e.Updater.Update();
              e.Handled = true;
            };
          application.LastLogonParametersRead += (s, e) =>
              {
              if (e.LogonObject is AuthenticationStandardLogonParameters logonParameters && string.IsNullOrEmpty(logonParameters.UserName))
              {
                logonParameters.UserName = "Sam";
              }
            };
        });
    });

    //services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    //    .AddCookie(options =>
    //    {
    //        options.LoginPath = "/LoginPage";
    //    })
    //    .AddJwtBearer(options =>
    //    {
    //        options.TokenValidationParameters = new TokenValidationParameters()
    //        {
    //            ValidIssuer = Configuration["Authentication:Jwt:ValidIssuer"],
    //            ValidAudience = Configuration["Authentication:Jwt:ValidAudience"],
    //            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Authentication:Jwt:IssuerSigningKey"]))
    //        };
    //    });

    //services.AddAuthorization(options =>
    //{
    //    options.DefaultPolicy = new AuthorizationPolicyBuilder(
    //        JwtBearerDefaults.AuthenticationScheme)
    //            .RequireAuthenticatedUser()
    //            .RequireXafAuthentication()
    //            .Build();
    //});

    services.AddXafWebApi(Configuration, options =>
    {
      options.BusinessObject<ApplicationUser>();
      options.BusinessObject<Department>();
      options.BusinessObject<Employee>();
      options.BusinessObject<Location>();
      options.BusinessObject<Paycheck>();
      options.BusinessObject<PortfolioFileData>();
      options.BusinessObject<Position>();
      options.BusinessObject<Resume>();
      options.BusinessObject<DemoTask>();

      options.BusinessObject<CustomNonPersistentObject>();
    });

    services
        .AddControllers()
        .AddOData((options, serviceProvider) =>
        {
          options
                  .EnableQueryFeatures(100)
                  .AddRouteComponents("api/odata", new EdmModelBuilder(serviceProvider).GetEdmModel());
        });
    services.AddSwaggerGen(c =>
    {
      c.EnableAnnotations();
      c.SwaggerDoc("v1", new OpenApiInfo
      {
        Title = "MainDemo",
        Version = "v1"
      });

      //c.AddSecurityDefinition("JWT", new OpenApiSecurityScheme()
      //{
      //    Type = SecuritySchemeType.Http,
      //    Name = "Bearer",
      //    Scheme = "bearer",
      //    BearerFormat = "JWT",
      //    In = ParameterLocation.Header
      //});
      //c.AddSecurityRequirement(new OpenApiSecurityRequirement()
      //    {
      //            {
      //                new OpenApiSecurityScheme() {
      //                    Reference = new OpenApiReference() {
      //                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
      //                        Id = "JWT"
      //                    }
      //                },
      //                new string[0]
      //            },
      //    });
    });
  }

  public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
  {
    if (env.IsDevelopment())
    {
      app.UseDeveloperExceptionPage();

      app.UseSwagger();
      app.UseSwaggerUI(c =>
      {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MainDemo WebApi v1");
      });
    }
    else
    {
      app.UseExceptionHandler("/Error");
      app.UseHsts();
    }
    //app.UseHttpsRedirection();
    app.UseRequestLocalization();
    app.UseStaticFiles();
    app.UseRouting();
    //app.UseAuthentication();
    //app.UseAuthorization();
    app.UseXaf();
    app.UseEndpoints(endpoints =>
    {
      endpoints.MapBlazorHub();
      endpoints.MapFallbackToPage("/_Host");
      endpoints.MapControllers();
    });
  }

  string? GetConnectionString(IConfiguration configuration)
  {
    string? connectionString = null;
    if (configuration.GetConnectionString("ConnectionString") != null)
    {
      connectionString = configuration.GetConnectionString("ConnectionString");
    }
#if EASYTEST
            if(configuration.GetConnectionString("EasyTestConnectionString") != null) {
                connectionString = configuration.GetConnectionString("EasyTestConnectionString");
            }
#endif
    return connectionString;
  }
}
