using Nexus.API.Core.Aggregates.WorkspaceAggregate;
using Nexus.API.Core.Aggregates.TeamAggregate;
using Nexus.API.Core.Aggregates.UserAggregate;
using Nexus.API.Core.Enums;
using Nexus.API.Core.Exceptions;
using Nexus.API.Core.ValueObjects;
using Shouldly;

namespace Nexus.API.UnitTests.Core.WorkspaceAggregate;

public class WorkspaceTests
{
  private readonly UserId _creatorId = UserId.Create(Guid.NewGuid());
  private readonly TeamId _teamId = TeamId.CreateNew();

  private Workspace CreateWorkspace(string name = "Test Workspace", string? description = null)
  {
    return Workspace.Create(name, description, _teamId, _creatorId);
  }

  [Fact]
  public void Create_WithValidData_ReturnsWorkspace()
  {
    var workspace = CreateWorkspace("My Workspace", "A description");

    workspace.Name.ShouldBe("My Workspace");
    workspace.Description.ShouldBe("A description");
    workspace.TeamId.ShouldBe(_teamId);
    workspace.CreatedBy.ShouldBe(_creatorId);
    workspace.IsDeleted.ShouldBeFalse();
  }

  [Fact]
  public void Create_AddsCreatorAsOwnerMember()
  {
    var workspace = CreateWorkspace();

    workspace.Members.Count.ShouldBe(1);
    workspace.Members.First().UserId.ShouldBe(_creatorId);
    workspace.Members.First().Role.ShouldBe(WorkspaceMemberRole.Owner);
    workspace.Members.First().IsActive.ShouldBeTrue();
  }

  [Theory]
  [InlineData("")]
  [InlineData("   ")]
  [InlineData(null)]
  public void Create_WithEmptyName_ThrowsException(string? name)
  {
    Should.Throw<Exception>(() =>
      Workspace.Create(name!, null, _teamId, _creatorId));
  }

  [Fact]
  public void Create_WithNameExceeding200Chars_ThrowsDomainException()
  {
    var longName = new string('a', 201);
    Should.Throw<DomainException>(() =>
      Workspace.Create(longName, null, _teamId, _creatorId));
  }

  [Fact]
  public void Create_WithDescriptionExceeding1000Chars_ThrowsDomainException()
  {
    var longDesc = new string('a', 1001);
    Should.Throw<DomainException>(() =>
      Workspace.Create("Workspace", longDesc, _teamId, _creatorId));
  }

  [Fact]
  public void Update_ChangesNameAndDescription()
  {
    var workspace = CreateWorkspace();

    workspace.Update("New Name", "New Description");

    workspace.Name.ShouldBe("New Name");
    workspace.Description.ShouldBe("New Description");
  }

  [Fact]
  public void Update_UpdatesTimestamp()
  {
    var workspace = CreateWorkspace();
    var originalUpdatedAt = workspace.UpdatedAt;

    workspace.Update("Updated", null);

    workspace.UpdatedAt.ShouldBeGreaterThanOrEqualTo(originalUpdatedAt);
  }

  [Fact]
  public void Update_WithNameExceeding200Chars_ThrowsDomainException()
  {
    var workspace = CreateWorkspace();
    var longName = new string('a', 201);

    Should.Throw<DomainException>(() => workspace.Update(longName, null));
  }

  [Fact]
  public void AddMember_WithValidData_AddsMember()
  {
    var workspace = CreateWorkspace();
    var newUserId = UserId.Create(Guid.NewGuid());

    workspace.AddMember(newUserId, WorkspaceMemberRole.Editor, _creatorId);

    workspace.Members.Count.ShouldBe(2);
    workspace.IsMember(newUserId).ShouldBeTrue();
  }

  [Fact]
  public void AddMember_DuplicateUser_ThrowsDomainException()
  {
    var workspace = CreateWorkspace();
    var userId = UserId.Create(Guid.NewGuid());
    workspace.AddMember(userId, WorkspaceMemberRole.Editor, _creatorId);

    Should.Throw<DomainException>(() =>
      workspace.AddMember(userId, WorkspaceMemberRole.Editor, _creatorId));
  }

  [Fact]
  public void RemoveMember_RemovesMember()
  {
    var workspace = CreateWorkspace();
    var userId = UserId.Create(Guid.NewGuid());
    workspace.AddMember(userId, WorkspaceMemberRole.Editor, _creatorId);

    workspace.RemoveMember(userId, _creatorId);

    workspace.IsMember(userId).ShouldBeFalse();
  }

  [Fact]
  public void RemoveMember_LastOwner_ThrowsDomainException()
  {
    var workspace = CreateWorkspace();

    Should.Throw<DomainException>(() =>
      workspace.RemoveMember(_creatorId, _creatorId));
  }

  [Fact]
  public void RemoveMember_WithoutPermission_ThrowsDomainException()
  {
    var workspace = CreateWorkspace();
    var editorId = UserId.Create(Guid.NewGuid());
    var viewerId = UserId.Create(Guid.NewGuid());
    workspace.AddMember(editorId, WorkspaceMemberRole.Editor, _creatorId);
    workspace.AddMember(viewerId, WorkspaceMemberRole.Viewer, _creatorId);

    // Editor cannot remove members
    Should.Throw<DomainException>(() =>
      workspace.RemoveMember(viewerId, editorId));
  }

  [Fact]
  public void RemoveMember_NonExistentMember_ThrowsDomainException()
  {
    var workspace = CreateWorkspace();
    var nonMember = UserId.Create(Guid.NewGuid());

    Should.Throw<DomainException>(() =>
      workspace.RemoveMember(nonMember, _creatorId));
  }

  [Fact]
  public void ChangeMemberRole_ChangesRole()
  {
    var workspace = CreateWorkspace();
    var userId = UserId.Create(Guid.NewGuid());
    workspace.AddMember(userId, WorkspaceMemberRole.Viewer, _creatorId);

    workspace.ChangeMemberRole(userId, WorkspaceMemberRole.Editor, _creatorId);

    workspace.GetMemberRole(userId).ShouldBe(WorkspaceMemberRole.Editor);
  }

  [Fact]
  public void ChangeMemberRole_DemoteLastOwner_ThrowsDomainException()
  {
    var workspace = CreateWorkspace();

    Should.Throw<DomainException>(() =>
      workspace.ChangeMemberRole(_creatorId, WorkspaceMemberRole.Editor, _creatorId));
  }

  [Fact]
  public void ChangeMemberRole_WithoutPermission_ThrowsDomainException()
  {
    var workspace = CreateWorkspace();
    var editorId = UserId.Create(Guid.NewGuid());
    var viewerId = UserId.Create(Guid.NewGuid());
    workspace.AddMember(editorId, WorkspaceMemberRole.Editor, _creatorId);
    workspace.AddMember(viewerId, WorkspaceMemberRole.Viewer, _creatorId);

    // Editor cannot change roles
    Should.Throw<DomainException>(() =>
      workspace.ChangeMemberRole(viewerId, WorkspaceMemberRole.Editor, editorId));
  }

  [Fact]
  public void Delete_SoftDeletesWorkspace()
  {
    var workspace = CreateWorkspace();

    workspace.Delete();

    workspace.IsDeleted.ShouldBeTrue();
    workspace.DeletedAt.ShouldNotBeNull();
  }

  [Fact]
  public void IsMember_ReturnsTrueForActiveMember()
  {
    var workspace = CreateWorkspace();
    workspace.IsMember(_creatorId).ShouldBeTrue();
  }

  [Fact]
  public void IsMember_ReturnsFalseForNonMember()
  {
    var workspace = CreateWorkspace();
    workspace.IsMember(UserId.Create(Guid.NewGuid())).ShouldBeFalse();
  }

  [Fact]
  public void CanManageMembers_ReturnsTrueForOwner()
  {
    var workspace = CreateWorkspace();
    workspace.CanManageMembers(_creatorId).ShouldBeTrue();
  }

  [Fact]
  public void CanManageMembers_ReturnsTrueForAdmin()
  {
    var workspace = CreateWorkspace();
    var adminId = UserId.Create(Guid.NewGuid());
    workspace.AddMember(adminId, WorkspaceMemberRole.Admin, _creatorId);

    workspace.CanManageMembers(adminId).ShouldBeTrue();
  }

  [Fact]
  public void CanManageMembers_ReturnsFalseForEditor()
  {
    var workspace = CreateWorkspace();
    var editorId = UserId.Create(Guid.NewGuid());
    workspace.AddMember(editorId, WorkspaceMemberRole.Editor, _creatorId);

    workspace.CanManageMembers(editorId).ShouldBeFalse();
  }

  [Fact]
  public void CanManageMembers_ReturnsFalseForViewer()
  {
    var workspace = CreateWorkspace();
    var viewerId = UserId.Create(Guid.NewGuid());
    workspace.AddMember(viewerId, WorkspaceMemberRole.Viewer, _creatorId);

    workspace.CanManageMembers(viewerId).ShouldBeFalse();
  }

  [Fact]
  public void CanEditContent_ReturnsTrueForOwnerAdminEditor()
  {
    var workspace = CreateWorkspace();
    var adminId = UserId.Create(Guid.NewGuid());
    var editorId = UserId.Create(Guid.NewGuid());
    workspace.AddMember(adminId, WorkspaceMemberRole.Admin, _creatorId);
    workspace.AddMember(editorId, WorkspaceMemberRole.Editor, _creatorId);

    workspace.CanEditContent(_creatorId).ShouldBeTrue();
    workspace.CanEditContent(adminId).ShouldBeTrue();
    workspace.CanEditContent(editorId).ShouldBeTrue();
  }

  [Fact]
  public void CanEditContent_ReturnsFalseForViewer()
  {
    var workspace = CreateWorkspace();
    var viewerId = UserId.Create(Guid.NewGuid());
    workspace.AddMember(viewerId, WorkspaceMemberRole.Viewer, _creatorId);

    workspace.CanEditContent(viewerId).ShouldBeFalse();
  }
}
