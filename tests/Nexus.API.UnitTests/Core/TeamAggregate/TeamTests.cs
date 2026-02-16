using Nexus.API.Core.Aggregates.TeamAggregate;
using Nexus.API.Core.Enums;
using Nexus.API.Core.Exceptions;
using Shouldly;

namespace Nexus.API.UnitTests.Core.TeamAggregate;

public class TeamTests
{
  private readonly Guid _creatorId = Guid.NewGuid();

  private Team CreateTeam(string name = "Test Team", string? description = null)
  {
    return Team.Create(name, description, _creatorId);
  }

  [Fact]
  public void Create_WithValidData_ReturnsTeam()
  {
    var team = CreateTeam("My Team", "A description");

    team.Name.ShouldBe("My Team");
    team.Description.ShouldBe("A description");
    team.CreatedBy.ShouldBe(_creatorId);
    team.IsDeleted.ShouldBeFalse();
  }

  [Fact]
  public void Create_AddsCreatorAsOwnerMember()
  {
    var team = CreateTeam();

    team.Members.Count.ShouldBe(1);
    team.Members.First().UserId.ShouldBe(_creatorId);
    team.Members.First().Role.ShouldBe(TeamRole.Owner);
    team.Members.First().IsActive.ShouldBeTrue();
  }

  [Fact]
  public void Create_TrimsNameAndDescription()
  {
    var team = Team.Create("  Trimmed Name  ", "  Trimmed Desc  ", _creatorId);

    team.Name.ShouldBe("Trimmed Name");
    team.Description.ShouldBe("Trimmed Desc");
  }

  [Theory]
  [InlineData("")]
  [InlineData("   ")]
  [InlineData(null)]
  public void Create_WithEmptyName_ThrowsException(string? name)
  {
    Should.Throw<Exception>(() => Team.Create(name!, null, _creatorId));
  }

  [Fact]
  public void Create_WithEmptyCreatorId_ThrowsDomainException()
  {
    Should.Throw<DomainException>(() => Team.Create("Team", null, Guid.Empty));
  }

  [Fact]
  public void Create_WithNameExceeding200Chars_ThrowsDomainException()
  {
    var longName = new string('a', 201);
    Should.Throw<DomainException>(() => Team.Create(longName, null, _creatorId));
  }

  [Fact]
  public void Create_WithDescriptionExceeding1000Chars_ThrowsDomainException()
  {
    var longDesc = new string('a', 1001);
    Should.Throw<DomainException>(() => Team.Create("Team", longDesc, _creatorId));
  }

  [Fact]
  public void Update_ChangesNameAndDescription()
  {
    var team = CreateTeam();

    team.Update("New Name", "New Description");

    team.Name.ShouldBe("New Name");
    team.Description.ShouldBe("New Description");
  }

  [Fact]
  public void Update_UpdatesTimestamp()
  {
    var team = CreateTeam();
    var originalUpdatedAt = team.UpdatedAt;

    team.Update("Updated", null);

    team.UpdatedAt.ShouldBeGreaterThanOrEqualTo(originalUpdatedAt);
  }

  [Fact]
  public void AddMember_WithValidData_AddsMember()
  {
    var team = CreateTeam();
    var newUserId = Guid.NewGuid();

    team.AddMember(newUserId, TeamRole.Member, _creatorId);

    team.Members.Count.ShouldBe(2);
    team.IsMember(newUserId).ShouldBeTrue();
  }

  [Fact]
  public void AddMember_DuplicateUser_ThrowsDomainException()
  {
    var team = CreateTeam();
    var userId = Guid.NewGuid();
    team.AddMember(userId, TeamRole.Member, _creatorId);

    Should.Throw<DomainException>(() =>
      team.AddMember(userId, TeamRole.Member, _creatorId));
  }

  [Fact]
  public void AddMember_WithEmptyUserId_ThrowsDomainException()
  {
    var team = CreateTeam();

    Should.Throw<DomainException>(() =>
      team.AddMember(Guid.Empty, TeamRole.Member, _creatorId));
  }

  [Fact]
  public void RemoveMember_RemovesMember()
  {
    var team = CreateTeam();
    var userId = Guid.NewGuid();
    team.AddMember(userId, TeamRole.Member, _creatorId);

    team.RemoveMember(userId);

    team.IsMember(userId).ShouldBeFalse();
  }

  [Fact]
  public void RemoveMember_LastOwner_ThrowsDomainException()
  {
    var team = CreateTeam();

    Should.Throw<DomainException>(() => team.RemoveMember(_creatorId));
  }

  [Fact]
  public void RemoveMember_NonExistentMember_ThrowsDomainException()
  {
    var team = CreateTeam();

    Should.Throw<DomainException>(() => team.RemoveMember(Guid.NewGuid()));
  }

  [Fact]
  public void ChangeMemberRole_ChangesRole()
  {
    var team = CreateTeam();
    var userId = Guid.NewGuid();
    team.AddMember(userId, TeamRole.Member, _creatorId);

    team.ChangeMemberRole(userId, TeamRole.Admin);

    team.GetMemberRole(userId).ShouldBe(TeamRole.Admin);
  }

  [Fact]
  public void ChangeMemberRole_DemoteLastOwner_ThrowsDomainException()
  {
    var team = CreateTeam();

    Should.Throw<DomainException>(() =>
      team.ChangeMemberRole(_creatorId, TeamRole.Member));
  }

  [Fact]
  public void ChangeMemberRole_CanDemoteOwnerIfOtherOwnersExist()
  {
    var team = CreateTeam();
    var secondOwnerId = Guid.NewGuid();
    team.AddMember(secondOwnerId, TeamRole.Owner, _creatorId);

    // Now demoting the first owner should work
    team.ChangeMemberRole(_creatorId, TeamRole.Admin);

    team.GetMemberRole(_creatorId).ShouldBe(TeamRole.Admin);
  }

  [Fact]
  public void Delete_SoftDeletesTeam()
  {
    var team = CreateTeam();

    team.Delete(_creatorId);

    team.IsDeleted.ShouldBeTrue();
    team.DeletedAt.ShouldNotBeNull();
  }

  [Fact]
  public void Delete_AlreadyDeleted_ThrowsDomainException()
  {
    var team = CreateTeam();
    team.Delete(_creatorId);

    Should.Throw<DomainException>(() => team.Delete(_creatorId));
  }

  [Fact]
  public void IsMember_ReturnsTrueForActiveMember()
  {
    var team = CreateTeam();
    team.IsMember(_creatorId).ShouldBeTrue();
  }

  [Fact]
  public void IsMember_ReturnsFalseForNonMember()
  {
    var team = CreateTeam();
    team.IsMember(Guid.NewGuid()).ShouldBeFalse();
  }

  [Fact]
  public void CanManageMembers_ReturnsTrueForOwner()
  {
    var team = CreateTeam();
    team.CanManageMembers(_creatorId).ShouldBeTrue();
  }

  [Fact]
  public void CanManageMembers_ReturnsTrueForAdmin()
  {
    var team = CreateTeam();
    var adminId = Guid.NewGuid();
    team.AddMember(adminId, TeamRole.Admin, _creatorId);

    team.CanManageMembers(adminId).ShouldBeTrue();
  }

  [Fact]
  public void CanManageMembers_ReturnsFalseForRegularMember()
  {
    var team = CreateTeam();
    var memberId = Guid.NewGuid();
    team.AddMember(memberId, TeamRole.Member, _creatorId);

    team.CanManageMembers(memberId).ShouldBeFalse();
  }
}
