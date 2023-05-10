using Chat.Common.Entities;
using Chat.Server.Library.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Chat.Server.Tests
{
    public class ChatUsersControllerTests : TestServerBaseTests
    {
        public ChatUsersControllerTests():base()
        {
        }

        [Fact]
        public async Task PostAsyncChatMessage_ShouldBeSameNameAndPassword_IfUserNotExists()
        {
            //Arrange
            var client = _server.CreateClient();
            var expectedUser = new ChatUser()
            {
                Name = "TestUser",
                Password = "TestPassword"
            };

            //Act
            var response = await client.PostAsync($"api/chatusers?username={expectedUser.Name}&password={expectedUser.Password}", null);
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<ChatUser>(content);

            //Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(expectedUser.Name);
            result.Password.Should().Be(expectedUser.Password);
        }

        [Fact]
        public async Task GetAsyncChatMessage_ShouldBeSameNameAndPassword_IfUserExists()
        {
            //Arrange
            var client = _server.CreateClient();
            var expectedUser = new ChatUser()
            {
                Name = "NotExisting",
                Password = "TestPassword"
            };
            await client.PostAsync($"api/chatusers?username={expectedUser.Name}&password={expectedUser.Password}", null);

            //Act
            var response = await client.GetAsync($"api/chatusers?username={expectedUser.Name}&password={expectedUser.Password}");
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<ChatUser>(content);

            //Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(expectedUser.Name);
            result.Password.Should().Be(expectedUser.Password);
        }

        [Fact]
        public async Task GetAsyncChatMessage_ShouldBeNull_IfUserNotExists()
        {
            //Arrange
            var client = _server.CreateClient();
            var expectedUser = new ChatUser()
            {
                Name = "TestUser",
                Password = "TestPassword"
            };

            //Act
            var response = await client.GetAsync($"api/chatusers?username={expectedUser.Name}&password={expectedUser.Password}");
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<ChatUser>(content);

            //Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAsyncChatMessage_ShouldBeNull_IfUserExistsButWorngPassword()
        {
            //Arrange
            var client = _server.CreateClient();
            var expectedUser = new ChatUser()
            {
                Name = "TestUser",
                Password = "TestPassword"
            };
            var errorPassword = "Error";
            await client.PostAsync($"api/chatusers?username={expectedUser.Name}&password={expectedUser.Password}", null);

            //Act
            var response = await client.GetAsync($"api/chatusers?username={expectedUser.Name}&password={errorPassword}");
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<ChatUser>(content);

            //Assert
            result.Should().BeNull();
        }
    }
}
