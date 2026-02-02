using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Nexus.API.Infrastructure.Data;

public static class SeedData
{
  public static async Task InitializeAsync(IServiceProvider serviceProvider)
  {
    using var scope = serviceProvider.CreateScope();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

    // Create default roles as per architecture document
    string[] roleNames = { "Admin", "Editor", "Viewer", "Guest" };
    
    foreach (var roleName in roleNames)
    {
      var roleExist = await roleManager.RoleExistsAsync(roleName);
      if (!roleExist)
      {
        await roleManager.CreateAsync(new IdentityRole<Guid> 
        { 
          Id = Guid.NewGuid(),
          Name = roleName 
        });
      }
    }
  }
}
