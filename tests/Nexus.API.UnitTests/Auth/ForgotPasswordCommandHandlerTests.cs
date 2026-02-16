using Ardalis.Result;
using Nexus.API.Core.Interfaces;
using Nexus.API.UseCases.Auth.Commands;
using NSubstitute;
using Shouldly;

using UserDto = Nexus.API.Core.Interfaces.UserDto;

namespace Nexus.API.UnitTests.Auth;

public class ForgotPasswordCommandHandlerTests
{
  private readonly IUserService _userService;
  private readonly IEmailService _emailService;
  private readonly ForgotPasswordCommandHandler _handler;

  public ForgotPasswordCommandHandlerTests()
  {
    _userService = Substitute.For<IUserService>();
    _emailService = Substitute.For<IEmailService>();
    _handler = new ForgotPasswordCommandHandler(_userService, _emailService);
  }

  [Fact]
  public async Task Handle_WithValidEmail_ReturnsSuccessMessage()
  {
    // Arrange
    var userDto = new UserDto
    {
      Id = Guid.NewGuid().ToString(),
      Email = "user@nexus.dev",
      UserName = "testuser"
    };
    _userService.FindByEmailAsync("user@nexus.dev", Arg.Any<CancellationToken>())
      .Returns(userDto);
    _userService.GeneratePasswordResetTokenAsync(userDto.Id, Arg.Any<CancellationToken>())
      .Returns("reset-token-123");

    var command = new ForgotPasswordCommand("user@nexus.dev");

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.IsSuccess.ShouldBeTrue();
    result.Value.Message.ShouldContain("password reset link has been sent");
  }

  [Fact]
  public async Task Handle_WithValidEmail_SendsResetEmail()
  {
    // Arrange
    var userDto = new UserDto
    {
      Id = Guid.NewGuid().ToString(),
      Email = "user@nexus.dev",
      UserName = "testuser"
    };
    _userService.FindByEmailAsync("user@nexus.dev", Arg.Any<CancellationToken>())
      .Returns(userDto);
    _userService.GeneratePasswordResetTokenAsync(userDto.Id, Arg.Any<CancellationToken>())
      .Returns("reset-token-123");

    var command = new ForgotPasswordCommand("user@nexus.dev");

    // Act
    await _handler.Handle(command, CancellationToken.None);

    // Assert
    await _emailService.Received(1).SendPasswordResetEmailAsync(
      "user@nexus.dev",
      "testuser",
      "reset-token-123",
      Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task Handle_WithNonExistentEmail_ReturnsSuccessToPreventEnumeration()
  {
    // Arrange
    _userService.FindByEmailAsync("unknown@nexus.dev", Arg.Any<CancellationToken>())
      .Returns((UserDto?)null);

    var command = new ForgotPasswordCommand("unknown@nexus.dev");

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert - should still return success to prevent email enumeration
    result.IsSuccess.ShouldBeTrue();
    result.Value.Message.ShouldContain("password reset link has been sent");
  }

  [Fact]
  public async Task Handle_WithNonExistentEmail_DoesNotSendEmail()
  {
    // Arrange
    _userService.FindByEmailAsync("unknown@nexus.dev", Arg.Any<CancellationToken>())
      .Returns((UserDto?)null);

    var command = new ForgotPasswordCommand("unknown@nexus.dev");

    // Act
    await _handler.Handle(command, CancellationToken.None);

    // Assert
    await _emailService.DidNotReceive().SendPasswordResetEmailAsync(
      Arg.Any<string>(),
      Arg.Any<string>(),
      Arg.Any<string>(),
      Arg.Any<CancellationToken>());
  }

  [Theory]
  [InlineData("")]
  [InlineData("  ")]
  [InlineData(null)]
  public async Task Handle_WithEmptyEmail_ReturnsValidationError(string? email)
  {
    // Arrange
    var command = new ForgotPasswordCommand(email!);

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Status.ShouldBe(ResultStatus.Invalid);
    result.ValidationErrors.ShouldContain(e => e.Identifier == "Email");
  }
}
