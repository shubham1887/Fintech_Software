using DinkToPdf;
using DinkToPdf.Contracts;
using FluentValidation;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Project.BAL;
using Project.Model;
using Project.Processing;
using Project.StaticModel;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace Project
{
    public class Startup
    {
        private int StatusCheckInterval = 5;
        private int BackupMinuteInterval = 5;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private void SetExpiryTime()
        {
            int expiryTime = 120;
            try
            {
                string time = Configuration["Jwt:ExpiryInMinute"];

                if (time == null || time.Length <= 0)
                {
                    MemoryCacheSetting.ExpiryTimeInMinute = expiryTime;
                    return;
                }

                expiryTime = Convert.ToInt16(time);
            }
            catch (Exception ex)
            {
            }
            MemoryCacheSetting.ExpiryTimeInMinute = expiryTime;
        }

        private int getBackupInterval()
        {
            int minuteInInt = 30;

            try
            {
                minuteInInt = new SettingMasterBal().SelectByKeyWithInteger(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.BackupEmailTimeIntervalInMinute));

                if (minuteInInt <= 0)
                {
                    return 30;
                }

                if (minuteInInt >= 60)
                {
                    return 59;
                }
            }
            catch (Exception)
            {
                return minuteInInt;
            }

            return minuteInInt;
        }

        private int getStatusCheckInterval(string input)
        {
            int value = 5;
            try
            {
                if (input == null || input.Length <= 0)
                {
                    return value;
                }

                value = Convert.ToInt32(input);

                if (value >= 20)
                {
                    value = 5;
                }

                if (value <= 0)
                {
                    value = 2;
                }
            }
            catch (Exception)
            {
            }
            return value;
        }

        public IConfiguration Configuration { get; }

        private string connectionStringWithTrustAttribute(string conString)
        {
            if (!conString.ToLower().Contains("TrustServerCertificate=True".ToLower()))
            {
                conString = conString + ";TrustServerCertificate=True;";
            }
            return conString;
        }

        private string jwtKey(string key)
        {
            key = key.TrimIfNotNull();
            try
            {
                string appendString = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";

                if (key.Length <= 31)
                {
                    key = key + appendString.Substring(0, 32 - key.Length);
                    return key;
                }

                if (key.Length >= 33)
                {
                    key = key.Substring(0, 32);
                    return key;
                }
            }
            catch (Exception ex)
            {
            }

            return key;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            ValidatorOptions.Global.CascadeMode = CascadeMode.Stop;

            var ui_url = this.Configuration.GetSection("Web_UI_URL").GetChildren()
                              .Select(x => x.Value).ToArray();

            services.AddCors(options =>
            {
                options.AddPolicy("CORSProject",
                builder =>
                {
                    foreach (string url in ui_url)
                    {
                        builder.WithOrigins(url.Trim())
                                              .AllowAnyHeader()
                                              .AllowAnyMethod();
                    }
                });
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Finnovative Token Authentication API",
                    Description = "Finnovative Web API"
                });
                // To Enable authorization using Swagger (JWT)
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Finnovative Authorization",
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement {
                {
                     new OpenApiSecurityScheme
                     {
                       Reference = new OpenApiReference
                       {
                         Type = ReferenceType.SecurityScheme,
                         Id = "Bearer"
                       }
                      },
                      new string[] { }
                    }
                  });

                c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
                c.CustomSchemaIds(x => x.FullName);
            });

            //services.AddMemoryCache();

            string conString = ConfigurationExtensions.GetConnectionString(this.Configuration, "DBConnection");
            conString = connectionStringWithTrustAttribute(conString);
            ConnectionClass.connectionString = conString;

            string stsInterval = Configuration["StatusCheckInterval"];
            StatusCheckInterval = getStatusCheckInterval(stsInterval);

            BackupMinuteInterval = getBackupInterval();

            services.AddDbContextPool<DatabaseContext>(
                        item => item.UseSqlServer(conString,
                        o => o.CommandTimeout(10 * 60)));

            services.AddHangfire(x => x.UseMemoryStorage());
            services.AddHangfireServer();

            services.AddAuthentication(option =>
            {
                option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = Configuration["Jwt:Issuer"],
                    ValidAudience = Configuration["Jwt:Issuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey(Configuration["Jwt:Key"])))
                };
            });
            SetExpiryTime();

            services.AddMiniProfiler(options => options.RouteBasePath = "/profiler");

            services.Configure<BrotliCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Optimal;
            });

            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<BrotliCompressionProvider>();
            });

            services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
            services.AddControllersWithViews();
            services.AddMemoryCache();

            services.AddMvcCore(options =>
            {
                options.ModelBinderProviders.Insert(0, new HeaderComplexModelBinderProvider());
            });
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            InitOperation();
        }

        private void InitOperation()
        {
            new CircleAPIBal().SetCircle();
            new CircleAPIBal().SetOperatorList();

            new CompanyProcessing().SelectCompanyDetails();

            Processing.XMPP.XMPPChat objXMPP = new Processing.XMPP.XMPPChat();
            objXMPP.Connect();
        }

        private static readonly Lazy<string> profilerIndexValue = new Lazy<string>(ProfilerIndex);

        private static string ProfilerIndex()
        {
            var sb = new StringBuilder();

            sb.AppendLine("<html>");
            sb.AppendLine("<title>MiniProfiler Index</title>");
            sb.AppendLine($"<link href=\"/profiler/includes.min.css?v={StackExchange.Profiling.Internal.MiniProfilerBaseOptions.Version}\" rel=\"stylesheet\" />");
            sb.AppendLine("<body>");
            sb.AppendLine("<h2><a href='/profiler/results-index'>List of all tracings (auto-refresh)</a></h2>");
            sb.AppendLine("<h2><a href='/profiler/results'>Current/last request</a></h2>");
            sb.AppendLine("<h4><a href='/profiler/results-list'>List of all requests as json</a></h4>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.

        private void hangfireJobs()
        {
            // added scheduled job : refer for expression : https://crontab.guru/

            try
            {
                var timeZoneOptions = new RecurringJobOptions();
                timeZoneOptions.TimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

                string cronBackUP = $"*/{BackupMinuteInterval} * * * *"; // "*/5 * * * *" At every 5th minute past every hour.

                 
                RecurringJob.AddOrUpdate("User Balance History", () => new UserBalanceHistoryBAL().BulkInsertOpeningBalance(), "01 00 * * *", timeZoneOptions);// every day at 12.01AM

                RecurringJob.AddOrUpdate("Archive Data", () => new ArchiveProcessing().ArchiveDataProcessing(), "03 00 * * *", timeZoneOptions);// every day at 12.03AM

                //   recurring job below here, above are timed jobs i.e. at particular time

                RecurringJob.AddOrUpdate("PG Status Check", () => new PaymentGatewayBAL().PGStatusCheckAuto(), "*/8 * * * *", timeZoneOptions); // every 8 min

                string cronStatusCheck = $"*/{StatusCheckInterval} * * * *"; // "*/5 * * * *"

                RecurringJob.AddOrUpdate("Txn Status Check", () => new StatusCheckProcessing().StatusCheck(), cronStatusCheck, timeZoneOptions); // every n min, check StatusCheckInterval in app.setting
                
                RecurringJob.AddOrUpdate("NEFT Status Check", () => new DMTStatusCheckProcessing().StatusCheckForNEFT(), "*/15 * * * *", timeZoneOptions); // every 15 min

                RecurringJob.AddOrUpdate("IMPS Status Check", () => new DMTStatusCheckProcessing().StatusCheckForIMPS(), "*/5 * * * *", timeZoneOptions); // every 5 min

                RecurringJob.AddOrUpdate("AEPS Status Check", () => new AEPSStatusCheckProcessing().StatusCheck(), "*/10 * * * *", timeZoneOptions); // every 10 min

                RecurringJob.AddOrUpdate("MicroATM Status Check", () => new MicroATMStatusCheckProcessing().StatusCheck(), "*/10 * * * *", timeZoneOptions); // every 10 min
                
                RecurringJob.AddOrUpdate("Payout Status Check", () => new PayoutStatusCheckProcessing().StatusCheck(), "*/10 * * * *", timeZoneOptions); // every 10 min

                RecurringJob.AddOrUpdate("Delete Bulk TransactionAPIUserRequestMaster", () => new TransactionAPIUserRequestBAL().DeleteOldData(), "10 05 * * *", timeZoneOptions); // every day at 05.10AM
            }
            catch (Exception ex)
            {
            }
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //! Dont change sequence
            app.Use(async (context, next) =>
            {
                if (env.IsDevelopment())
                {
                    if (context.Request.Path == "/profiler")
                    {
                        await context.Response.WriteAsync(profilerIndexValue.Value, Encoding.UTF8);

                        return;
                    }
                }

                await next();
            });

            if (env.IsDevelopment())
            {
                app.UseMiniProfiler();
                app.UseDeveloperExceptionPage();
                app.UseHangfireDashboard("/HangfireDashboard", new DashboardOptions
                {
                    IsReadOnlyFunc = (Hangfire.Dashboard.DashboardContext context) => true
                });
                app.UseSwagger();

                app.UseSwaggerUI(C =>
                {
                    C.SwaggerEndpoint(url: "./v1/swagger.json", name: "New API");
                    C.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
                });
            }

            app.UseAuthentication();

            hangfireJobs();

            app.UseStaticFiles();

            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(
                              Path.Combine(Directory.GetCurrentDirectory(), @"Media")),
                RequestPath = new PathString("/app-docs")
            });
            app.UseRouting();

            app.UseCors("CORSProject");

            app.UseAuthorization();
            app.UseResponseCompression();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}