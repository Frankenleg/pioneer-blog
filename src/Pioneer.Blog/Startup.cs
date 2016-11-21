﻿using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Pioneer.Blog.DAL;
using Pioneer.Blog.DAL.Entites;
using Pioneer.Blog.Model;
using Pioneer.Blog.Repository;
using Pioneer.Blog.Service;
using Pioneer.Pagination;

namespace Pioneer.Blog
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets();
            }

            Configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.

            services.AddDbContext<BlogContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<UserEntity, IdentityRole>()
                .AddEntityFrameworkStores<BlogContext>()
                .AddDefaultTokenProviders();

            services.AddMvc();

            // Add application services.
            services.Configure<AppConfiguration>(Configuration.GetSection("AppConfiguration"));
            services.AddTransient<IPaginatedMetaService, PaginatedMetaService>();

            // Repositories
            services.AddTransient<ICategoryRepository, CategoryRepository>();
            services.AddTransient<ITagRepository, TagRepository>();
            services.AddTransient<IPostRepository, PostRepository>();

            // Services
            services.AddTransient<ICategoryService, CategoryService>();
            services.AddTransient<IPostService, PostService>();
            services.AddTransient<IContactService, ContactService>();
            services.AddTransient<ITagService, TagService>();
            services.AddTransient<ISiteMapService, SiteMapService>();
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            ServiceMapperConfig.Config();

            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = context =>
                {
                    var headers = context.Context.Response.GetTypedHeaders();
                    headers.CacheControl = new CacheControlHeaderValue
                    {
                        MaxAge = TimeSpan.FromSeconds(31536000)
                    };
                }
            });

            app.UseIdentity();

            // Add external authentication middleware below. To configure them please see http://go.microsoft.com/fwlink/?LinkID=532715

            app.UseMvc(routes =>
            {
                // Areas support
                routes.MapRoute(
                  name: "areaRoute",
                  template: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

                routes.MapRoute(
                  name: "Post",
                  template: "post/{id}",
                  defaults: new { controller = "Post", action = "Index" });

                routes.MapRoute(
                  name: "BlogPost",
                  template: "post",
                  defaults: new { controller = "blog", action = "Index" });

                routes.MapRoute(
                  name: "BlogTag",
                  template: "tag",
                  defaults: new { controller = "blog", action = "Index" });

                routes.MapRoute(
                  name: "BlogCategory",
                  template: "category",
                  defaults: new { controller = "blog", action = "Index" });

                routes.MapRoute(
                   name: "Category",
                   template: "category/{id}/{page?}",
                   defaults: new { controller = "blog", action = "Category" });

                routes.MapRoute(
                   name: "Tag",
                   template: "tag/{id}/{page?}",
                   defaults: new { controller = "blog", action = "Tag" });

                routes.MapRoute(
                   name: "Blog",
                   template: "blog/{page?}",
                   defaults: new { controller = "blog", action = "Index" });

                routes.MapRoute(
                   name: "SiteMap",
                   template: "sitemap.xml",
                   defaults: new { controller = "home", action = "SiteMap" });

                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
