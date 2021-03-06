using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Finbuckle.MultiTenant;
// ***************************
using System.Net.Http;
using BlazorMultytenantDemo.Data;
using BlazorMultytenantDemo.Services;
using Finbuckle.MultiTenant.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authentication.Cookies;
// ***************************

using DbContext = BlazorMultytenantDemo.Data.DbContext;
using Microsoft.EntityFrameworkCore;

namespace BlazorMultytenantDemo
{

    public class ToDoItem
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public bool Completed { get; set; }
    }

    public class ToDoDbContext : MultiTenantDbContext
    {
        public ToDoDbContext(ITenantInfo tenantInfo) : base(tenantInfo)
        {
        }

        public ToDoDbContext(ITenantInfo tenantInfo, DbContextOptions<ToDoDbContext> options) : base(tenantInfo, options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(TenantInfo.ConnectionString);
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ToDoItem>().IsMultiTenant();

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<ToDoItem> ToDoItems { get; set; }
    }
    public class Startup
    {
        // To hold the values from the appsettings.json file
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
                .AddNewtonsoftJson();
            services.AddDbContext<Data.DbContext>(options =>
            {
                options.UseSqlite("Data Source = Orgs.db");
                ;
            });
            services.AddScoped<DbController>();

            services.AddSingleton<WeatherForecastService>();

            // ***********************************************
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie();
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.Cookie.IsEssential = true;
            });
            services.AddAuthentication().AddGoogle(options =>
            {
                options.ClientId = Configuration["Google:ClientId"];
                options.ClientSecret = Configuration["Google:ClientSecret"];
            });

            // From: https://github.com/aspnet/Blazor/issues/1554
            // Adds HttpContextAccessor
            // Used to determine if a user is logged in
            // and what their username is
            services.AddHttpContextAccessor();
            services.AddScoped<HttpContextAccessor>();

            // Required for HttpClient support in the Blazor Client project
            services.AddHttpClient();
            services.AddScoped<HttpClient>();

            // Pass settings to other components
            services.AddSingleton<IConfiguration>(Configuration);
            // ***********************************************

            services.AddRazorPages();
            services.AddServerSideBlazor();

            services.AddMultiTenant<TenantInfo>().
                WithConfigurationStore().
                WithSessionStrategy();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            
            app.UseRouting();

            app.UseAuthentication();

            // ******
            app.UseCookiePolicy(new CookiePolicyOptions()
            {
                MinimumSameSitePolicy = SameSiteMode.Lax    
            });
            app.UseAuthentication();
            // ******
            app.UseMultiTenant();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });

            SetupDb();
        }

        private void SetupDb()
        {
            var ti = new TenantInfo { Id = "finbuckle", ConnectionString = "Data Source=Data/ToDoList.db" };
            using (var db = new ToDoDbContext(ti))
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
                db.ToDoItems.Add(new ToDoItem { Title = "Call Lawyer ", Completed = false });
                db.ToDoItems.Add(new ToDoItem { Title = "File Papers", Completed = false });
                db.ToDoItems.Add(new ToDoItem { Title = "Send Invoices", Completed = true });
                db.SaveChanges();
            }


        }
    }
}
