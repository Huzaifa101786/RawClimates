using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using RawClimates.Controllers;
using RawClimates.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RawClimates
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
            services.AddDistributedMemoryCache();
            services.AddSession();
            services.AddCors();
            /*services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.Cookie.Name = "huzaifa";
                options.ExpireTimeSpan = TimeSpan.FromMinutes(20); // Set cookie expiration time
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.LoginPath = new PathString("/api/auth/login");
            });*/
           // services.AddSingleton<Trigger>();
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.Cookie.Name = "SessionDef";
                options.IdleTimeout = TimeSpan.FromMinutes(20); // Set session expiration time
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // Other service configurations...

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "RawClimates", Version = "v1" });

            });

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                               .AllowAnyMethod()
                               .AllowAnyHeader();
                    });
            });

            services.AddSingleton<IMongoClient>(s =>
            {
                var connectionString = "mongodb://localhost:27017"; // Replace with your MongoDB connection string
                return new MongoClient(connectionString);
            });

            services.AddScoped<IMongoDatabase>(s =>
            {
                var client = s.GetService<IMongoClient>();
                var databaseName = "RawClimate"; // Replace with your database name
                return client.GetDatabase(databaseName);
            });

            services.AddScoped<IMongoCollection<User>>(s =>
            {
                var database = s.GetService<IMongoDatabase>();
                var collectionName = "User"; // Replace with your collection name
                return database.GetCollection<User>(collectionName);
            });

            services.AddScoped<IMongoCollection<WeatherData>>(s =>
            {
                var database = s.GetService<IMongoDatabase>();
                var collectionName = "WeatherData"; // Replace with your new collection name
                return database.GetCollection<WeatherData>(collectionName);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "RawClimates v1"));
            }

            app.UseCors(builder =>
            builder.WithOrigins()
               .AllowAnyMethod()
               .AllowAnyHeader());

            app.UseSession();

            app.UseCors("AllowAll");


            app.UseHttpsRedirection();

            app.UseRouting();

            //app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
