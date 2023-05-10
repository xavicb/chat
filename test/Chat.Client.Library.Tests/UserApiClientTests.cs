using Chat.Client.Library.Services;
using Chat.Common.Entities;
using FluentAssertions;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Chat.Client.Library.Tests
{
    public class UserApiClientTests
    {
        private readonly IUserApiClient _userApiClient;
        public UserApiClientTests()
        {
            var httpClient = new HttpClient(new FakeMessageHandler())
            {
                BaseAddress = new Uri("http://localhost")
            };
            _userApiClient = new UserApiClient(httpClient);
        }

        [Fact]
        public async Task CreateUserAsync_ShouldBeNull_IfUserExists()
        {
            //Arrange
            var username = "ExistingUser";
            var password = "1234";

            //Act
            var result = await _userApiClient.CreateUserAsync(username, password);

            //Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task CreateUserAsync_ShouldBeUser_IfUserNotExists()
        {
            //Arrange
            var username = "NotExistingUser";
            var password = "1234";

            //Act
            var result = await _userApiClient.CreateUserAsync(username, password);

            //Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(username);
            result.Password.Should().Be(password);
        }

        [Fact]
        public async Task CreateUserAsync_ShouldBeNull_IfHttpRequestException()
        {
            //Arrange
            var username = "ExceptionUser";
            var password = "IncorrectPassword";

            //Act
            var result = await _userApiClient.CreateUserAsync(username, password);

            //Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task LoginAsync_ShouldBeUser_IfUserExistsAndCorrectPassword()
        {
            //Arrange
            var username = "ExistingUser";
            var password = "CorrectPassword";

            //Act
            var result = await _userApiClient.LoginAsync(username, password);

            //Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(username);
            result.Password.Should().Be(password);
        }

        [Fact]
        public async Task LoginAsync_ShouldBeNull_IfUserExistsAndIncorrectPassword()
        {
            //Arrange
            var username = "ExistingUser";
            var password = "IncorrectPassword";

            //Act
            var result = await _userApiClient.LoginAsync(username, password);

            //Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task LoginAsync_ShouldBeNull_IfUserNotExists()
        {
            //Arrange
            var username = "NotExistingUser";
            var password = "IncorrectPassword";

            //Act
            var result = await _userApiClient.LoginAsync(username, password);

            //Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task LoginAsync_ShouldBeNull_IfHttpRequestException()
        {
            //Arrange
            var username = "ExceptionUser";
            var password = "IncorrectPassword";

            //Act
            var result = await _userApiClient.LoginAsync(username, password);

            //Assert
            result.Should().BeNull();
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
            
            private Task<HttpResponseMessage> ProcessGetAsync(HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                var response = new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                };

                var parameters = System.Web.HttpUtility.ParseQueryString(request.RequestUri.Query);
                if (parameters["username"] == "ExistingUser" && parameters["password"] == "CorrectPassword")
                {
                    var user = new ChatUser
                    {
                        Name = parameters["username"],
                        Password = parameters["password"]
                    };
                    response.Content = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8,
                        "application/json");
                }
                else if (parameters["username"] == "ExceptionUser")
                {
                    throw new HttpRequestException();
                }
                else
                {
                    response.Content = new StringContent(JsonConvert.SerializeObject(null), Encoding.UTF8,
                        "application/json");
                }

                return Task.FromResult(response);
            }

            private Task<HttpResponseMessage> ProcessPostAsync(HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                var response = new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                };

                var parameters = System.Web.HttpUtility.ParseQueryString(request.RequestUri.Query);
                if (parameters["username"] == "NotExistingUser")
                {
                    var user = new ChatUser
                    {
                        Name = parameters["username"],
                        Password = parameters["password"]
                    };
                    response.Content = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8,
                        "application/json");
                }
                else if (parameters["username"] == "ExceptionUser")
                {
                    throw new HttpRequestException();
                }
                else
                {
                    response.Content = new StringContent(JsonConvert.SerializeObject(null), Encoding.UTF8,
                        "application/json");
                }

                return Task.FromResult(response);
            }
        }
    }
}
