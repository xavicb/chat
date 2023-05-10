using Chat.Common.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Chat.Server.Library.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Chat.Server.Tests
{
    public class TestServerBaseTests
    {
        protected readonly TestServer _server;

        public TestServerBaseTests()
        {
            var instanceName = Guid.NewGuid().ToString();
            var builder = new WebHostBuilder()
                .UseStartup<TestStartup>()
                .ConfigureServices(services =>
                {
                    services.AddDbContext<ChatDbContext>(options =>
                    {
                        options.UseInMemoryDatabase(databaseName: instanceName);
                    });
                });

            _server = new TestServer(builder);
        }

        public void Dispose()
        {
            _server?.Dispose();
        }
    }
}
