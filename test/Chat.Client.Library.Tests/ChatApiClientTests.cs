using Chat.Client.Library.Services;
using Chat.Common.Entities;
using FluentAssertions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Chat.Client.Library.Tests
{
    public class ChatApiClientTests
    {
        private readonly IChatApiClient _chatApiClient;
        public ChatApiClientTests()
        {
            var httpClient = new HttpClient(new FakeMessageHandler())
            {
                BaseAddress = new Uri("http://localhost")
            };
            _chatApiClient = new ChatApiClient(httpClient);
        }

        [Fact]
        public async Task SendMessageAsync_ShouldBeTrue_IfAuthorAndMessageAreNotNullOrEmpty()
        {
            //Arrange
            var message = new ChatMessage { Author = "ValidAuthor", Message = "ValidMessage" };

            //Act
            var result = await _chatApiClient.SendMessageAsync(message);

            //Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task SendMessageAsync_ShouldBeFalse_IfAuthorIsNullOrEmpty()
        {
            //Arrange
            var message = new ChatMessage { Message = "ValidMessage" };

            //Act
            var result = await _chatApiClient.SendMessageAsync(message);

            //Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task SendMessageAsync_ShouldBeFalse_IfMessageIsNullOrEmpty()
        {
            //Arrange
            var message = new ChatMessage { Author = "ValidAuthor" };

            //Act
            var result = await _chatApiClient.SendMessageAsync(message);

            //Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetChatMessagesAsync_ShouldHaveCount1_()
        {
            //Arrange
            var message = new ChatMessage { Author = "ValidAuthor" };

            //Act
            var result = await _chatApiClient.GetChatMessagesAsync();

            //Assert
            result.Should().HaveCount(1);
        }

        private class FakeMessageHandler : HttpMessageHandler
        {
            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                if (request.Method == HttpMethod.Post)
                {
                    return await ProcessPostAsync(request, cancellationToken);
                }
                else
                {
                    return await ProcessGetAsync(request, cancellationToken);
                }
            }


            private async Task<HttpResponseMessage> ProcessPostAsync(HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                var content = await request.Content.ReadAsStringAsync();
                var message = JsonConvert.DeserializeObject<ChatMessage>(content);

                var response = new HttpResponseMessage();
                if (!string.IsNullOrEmpty(message.Author) && !string.IsNullOrEmpty(message.Message))
                {
                    response.StatusCode = HttpStatusCode.OK;
                }
                else
                {
                    response.StatusCode = HttpStatusCode.InternalServerError;
                }

                return response;
            }

            private Task<HttpResponseMessage> ProcessGetAsync(HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                var messages = new List<ChatMessage>
                {
                    new ChatMessage(),
                };

                var response = new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonConvert.SerializeObject(messages), Encoding.UTF8, "application/json")
                };

                return Task.FromResult(response);
            }
        }
    }
}
