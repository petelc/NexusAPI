using Ardalis.Result;
using Nexus.API.Core.Interfaces;
using Nexus.API.UseCases.Auth.Commands;
using NSubstitute;
using Shouldly;

using UserDto = Nexus.API.Core.Interfaces.UserDto;

namespace Nexus.API.UnitTests.Auth;

public class ResetPasswordCommandHandlerTests
{
  private readonly IUserService _userService;
  private readonly ResetPasswordCommandHandler _handler;

  public ResetPasswordCommandHandlerTests()
  {
    _userService = Substitute.For<IUserService>();
    _handler = new ResetPasswordCommandHandler(_userService);
  }

  [Fact]
  public async Task Handle_WithValidData_ReturnsSuccess()
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
    _userService.ResetPasswordAsync(userDto.Id, "valid-token", "NewPass123!", Arg.Any<CancellationToken>())
      .Returns(true);

    var command = new ResetPasswordCommand("user@nexus.dev", "valid-token", "NewPass123!", "NewPass123!");

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.IsSuccess.ShouldBeTrue();
    result.Value.Message.ShouldContain("reset successfully");
  }

  [Fact]
  public async Task Handle_WithMismatchedPasswords_ReturnsValidationError()
  {
    // Arrange
    var command = new ResetPasswordCommand("user@nexus.dev", "token", "Password1!", "DifferentPass1!");

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Status.ShouldBe(ResultStatus.Invalid);
    result.ValidationErrors.ShouldContain(e =>
      e.Identifier == "ConfirmPassword" && e.ErrorMessage.Contains("do not match"));
  }

  [Fact]
  public async Task Handle_WithEmptyEmail_ReturnsValidationError()
  {
    // Arrange
    var command = new ResetPasswordCommand("", "token", "NewPass123!", "NewPass123!");

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Status.ShouldBe(ResultStatus.Invalid);
    result.ValidationErrors.ShouldContain(e => e.Identifier == "Email");
  }

  [Fact]
  public async Task Handle_WithEmptyToken_ReturnsValidationError()
  {
    // Arrange
    var command = new ResetPasswordCommand("user@nexus.dev", "", "NewPass123!", "NewPass123!");

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Status.ShouldBe(ResultStatus.Invalid);
    result.ValidationErrors.ShouldContain(e => e.Identifier == "Token");
  }

  [Fact]
  public async Task Handle_WithEmptyPassword_ReturnsValidationError()
  {
    // Arrange
    var command = new ResetPasswordCommand("user@nexus.dev", "token", "", "");

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Status.ShouldBe(ResultStatus.Invalid);
    result.ValidationErrors.ShouldContain(e => e.Identifier == "NewPassword");
  }

  [Fact]
  public async Task Handle_WithNonExistentUser_ReturnsError()
  {
    // Arrange
    _userService.FindByEmailAsync("unknown@nexus.dev", Arg.Any<CancellationToken>())
      .Returns((UserDto?)null);

    var command = new ResetPasswordCommand("unknown@nexus.dev", "token", "NewPass123!", "NewPass123!");

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Status.ShouldBe(ResultStatus.Error);
  }

  [Fact]
  public async Task Handle_WithInvalidToken_ReturnsError()
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
    _userService.ResetPasswordAsync(userDto.Id, "invalid-token", "NewPass123!", Arg.Any<CancellationToken>())
      .Returns(false);

    var command = new ResetPasswordCommand("user@nexus.dev", "invalid-token", "NewPass123!", "NewPass123!");

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Status.ShouldBe(ResultStatus.Error);
  }
}
