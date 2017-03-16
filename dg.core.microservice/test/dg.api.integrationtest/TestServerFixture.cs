﻿
using dg.common.validation;
using dg.dataservice;
using dg.contract;
using dg.validator;
using FluentValidation.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using dg.repository.Models;

namespace dg.unittest.api
{
    public class TestServerFixture: IDisposable
    {
        public HttpClient Client { get; }
        public TestServer Server { get; }
        public IConfigurationRoot Configuration { get; }

        public TestServerFixture()
        {
            var builder = new ConfigurationBuilder()
                  .SetBasePath(Directory.GetCurrentDirectory())
                  .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                  .AddEnvironmentVariables();
            Configuration = builder.Build();

            var webHostBuilder = new WebHostBuilder()
                    .UseEnvironment("Testing")
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .UseConfiguration(Configuration)
                    .UseKestrel()
               //     .UseStartup<dg.api.Startup>()
                    // Configure
                    .Configure(app => app.UseMvc())
                    .ConfigureServices(ConfigureServices);
            ;
            Server = new TestServer(webHostBuilder);
            Client = Server.CreateClient();
            Client.BaseAddress = new Uri(@"http://localhost:5000/");

        }
      
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddValidateInputAttribute<PersonValidator>();
            services.AddDbContext<PeopleContext>(options => options.UseSqlServer("ConnectionStrings:DefaultConnection"));     
            services.AddScoped<IPeopleService, PeopleSqlService>();
        }

        public void Dispose()
        {
            if (Client != null)
            {
                Client.Dispose();
            }
            if (Server != null)
            {
                Server.Dispose();
            }
        }


        public StringContent BuildRequestContent(contract.Person person)
        {
            var json = JsonConvert.SerializeObject(person);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            return content;
        }

        public ValidationResult GetValidationResult(HttpResponseMessage response)
        {
            var json = response.Content.ReadAsStringAsync().Result;
            var errorResponse = JsonConvert.DeserializeObject<ValidationResult>(json);
            return errorResponse;
        }
    }
}
