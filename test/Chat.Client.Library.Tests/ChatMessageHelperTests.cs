using System;
using Chat.Client.Library.Helpers;
using Chat.Client.Library.Tests.TestData;
using Chat.Common.Entities;
using FluentAssertions;
using Xunit;

namespace Chat.Client.Library.Tests
{
    public class ChatMessageHelperTests
    {
        [Theory]
        [ClassData(typeof(ChatMessageHelperData))]
        public void GenerateString_ShouldBeExpectedMessage(ChatMessage message,string expected)
        {
            //Arrange

            //Act
            var result = ChatMessageHelper.GenerateString(message);

            //Assert
            result.Should().Be(expected);
        }
    }
}
