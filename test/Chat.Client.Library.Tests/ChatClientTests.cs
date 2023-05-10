using Chat.Client.Library.Services;
using Chat.Common.Entities;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Chat.Client.Library.Tests
{
    public class ChatClientTests : IDisposable
    {
        private readonly IChatClient _chatClient;
        private readonly MockRepository _repository;
        private readonly Mock<IUserApiClient> _userApi;
        private readonly Mock<IChatApiClient> _chatApi;

        public ChatClientTests()
        {
            _repository = new MockRepository(MockBehavior.Strict);
            _chatApi = _repository.Create<IChatApiClient>();
            _userApi = _repository.Create<IUserApiClient>();

            _userApi.Setup(user => user.LoginAsync("Usuario1", "P2ssw0rd!")).Returns(Task.FromResult(new ChatUser { Name = "Usuario1" }));
            _userApi.Setup(user => user.LoginAsync("Usuario2", "")).Returns(Task.FromResult<ChatUser>(null));
            _userApi.Setup(user => user.LoginAsync("Usuario3", "P2ssw0rd!")).Returns(Task.FromResult(new ChatUser { Name = "Usuario3" }));
            _userApi.Setup(user => user.CreateUserAsync("Usuario1", "P2ssw0rd!")).Returns(Task.FromResult(new ChatUser()));
            _userApi.Setup(user => user.CreateUserAsync("Usuario2", "")).Returns(Task.FromResult<ChatUser>(null));

            _chatApi.Setup(chat => chat.SendMessageAsync(It.IsAny<ChatMessage>())).Returns(Task.FromResult(true));

            var messages = new List<ChatMessage>
            {
                new ChatMessage
                {
                    Author = "Usuario3",
                    Message = "Test",
                    Date = DateTime.Now
                }
            };
            _chatApi.Setup(chat => chat.GetChatMessagesAsync()).Returns(Task.FromResult((IEnumerable<ChatMessage>)messages));

            _chatClient = new ChatClient(_chatApi.Object, _userApi.Object);
        }

        [Theory]
        [InlineData(true, "Usuario1", "P2ssw0rd!")]
        [InlineData(false, "Usuario2", "")]
        public async Task LoginAsync_ShouldBeExpectedResult(bool expected, string username, string password)
        {
            //Arrange

            //Act
            var result = await _chatClient.LoginAsync(username, password);

            //Assert
            result.Should().Be(expected);
        }

        [Fact]
        public async Task LoginAsync_ShouldBeArgumentNullException_IfArgumentNull()
        {
            //Arrange

            //Act
            Func<Task> act = async () => { _ = await _chatClient.LoginAsync(null, null); };

            //Assert
            await act.Should().ThrowExactlyAsync<ArgumentNullException>().WithMessage("Value cannot be null. (Parameter 'username')");

        }

        [Theory]
        [InlineData(true, "Usuario1", "P2ssw0rd!")]
        [InlineData(false, "Usuario2", "")]
        public async Task CreateUserAsync_ShouldBeExpectedResult(bool expected, string username, string password)
        {
            //Arrange

            //Act
            var result = await _chatClient.CreateUserAsync(username, password);

            //Assert
            result.Should().Be(expected);
        }

        [Fact]
        public async Task CreateUserAsync_ShouldBeArgumentNullException_IfArgumentNull()
        {
            //Arrange

            //Act
            Func<Task> act = async () => { _ = await _chatClient.CreateUserAsync(null, null); };

            //Assert
            await act.Should().ThrowExactlyAsync<ArgumentNullException>().WithMessage("Value cannot be null. (Parameter 'username')");

        }

        [Fact]
        public async Task SendMessageAsync_ShouldBeTrue_IfLoggedAndConnected()
        {
            //Arrange
            var message = "Message1";
            await _chatClient.LoginAsync("Usuario1", "P2ssw0rd!");
            _chatClient.Connect();

            //Act
            var result = await _chatClient.SendMessageAsync(message);

            //Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task SendMessageAsync_ShouldBeFalse_IfNotLogged()
        {
            //Arrange
            var message = "Message1";

            //Act
            var result = await _chatClient.SendMessageAsync(message);

            //Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task NewMessageRecived_ShouldBeRaised_IfConnect()
        {
            await _chatClient.LoginAsync("Usuario1", "P2ssw0rd!");
            using (var monitoredITem = _chatClient.Monitor())
            {
                _chatClient.Connect();

                //Act
                await Task.Delay(100); //Le damos tiempo para que se ejecute el evento

                //Assert
                monitoredITem.Should().Raise("NewMessageRecived");
            }
        }

        [Fact]
        public async Task OverwriteLastLine_ShouldBeRaised_IfOneMessageAndAuthorIsWhoseIsLogged()
        {
            await _chatClient.LoginAsync("Usuario3", "P2ssw0rd!");
            using (var monitoredITem = _chatClient.Monitor())
            {
                _chatClient.Connect();

                //Act
                await Task.Delay(100); //Le damos tiempo para que se ejecute el evento

                //Assert
                monitoredITem.Should().Raise("OverwriteLastLine");
            }
        }

        [Fact]
        public async Task OverwriteLastLine_ShouldNotBeRaised_IfOneMessageAndAuthorIsOther()
        {
            await _chatClient.LoginAsync("Usuario1", "P2ssw0rd!");
            using (var monitoredITem = _chatClient.Monitor())
            {
                _chatClient.Connect();

                //Act
                await Task.Delay(100); //Le damos tiempo para que se ejecute el evento

                //Assert
                monitoredITem.Should().NotRaise("OverwriteLastLine");
            }
        }

        [Fact]
        public async Task OverwriteLastLine_ShouldNotBeRaised_IfMoreThanOneMessage()
        {
            var messages = new List<ChatMessage>
            {
                new ChatMessage
                {
                    Author = "Usuario3",
                    Message = "Test",
                    Date = DateTime.Now
                },
                new ChatMessage
                {
                    Author = "Usuario3",
                    Message = "Test",
                    Date = DateTime.Now
                }
            };
            _chatApi.Setup(chat => chat.GetChatMessagesAsync()).Returns(Task.FromResult((IEnumerable<ChatMessage>)messages));
            await _chatClient.LoginAsync("Usuario1", "P2ssw0rd!");
            using (var monitoredITem = _chatClient.Monitor())
            {
                _chatClient.Connect();

                //Act
                await Task.Delay(100); //Le damos tiempo para que se ejecute el evento

                //Assert
                monitoredITem.Should().NotRaise("OverwriteLastLine");
            }
        }

        public void Dispose()
        {
            _chatClient?.Dispose();
        }
    }
}
