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
    public class ChatMessagesControllerTests : TestServerBaseTests
    {
        public ChatMessagesControllerTests():base()
        {
            
        }

        [Fact]
        public async Task PostAsyncChatMessage_ShouldBeMessage()
        {
            //Arrange
            var client = _server.CreateClient();
            var message = new ChatMessage
            {
                Author = "Test",
                Date = DateTime.Now,
                Message = "Hello"
            };

            //Act
            var response = await client.PostAsync("api/chatmessages", new StringContent(JsonConvert.SerializeObject(message), Encoding.UTF8, "application/json"));
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<ChatMessage>(content);

            //Assert
            result.Author.Should().Be(message.Author);
            result.Date.Should().Be(message.Date);
            result.Message.Should().Be(message.Message);
        }

        [Fact]
        public async Task GetAsyncChatMessage_ShouldBe2MessagesMessage()
        {
            //Arrange
            var client = _server.CreateClient();
            var message = new ChatMessage
            {
                Author = "Test",
                Date = DateTime.Now,
                Message = "Hello"
            };
            await client.PostAsync("api/chatmessages", new StringContent(JsonConvert.SerializeObject(message), Encoding.UTF8, "application/json"));
            await client.PostAsync("api/chatmessages", new StringContent(JsonConvert.SerializeObject(message), Encoding.UTF8, "application/json"));

            //Act
            var response = await client.GetAsync("api/chatmessages");
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<IEnumerable<ChatMessage>>(content);

            //Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
        }
    }
}
