using Microsoft.AspNetCore.Authorization;
using TravelGuide.API.Controllers;

namespace TravelGuide.Tests;

public class AuthorizationTests
{
    [Fact]
    public void AdminController_RequiresAdminRole()
    {
        var attribute = Assert.Single(
            typeof(AdminController).GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
                .Cast<AuthorizeAttribute>());

        Assert.Equal("Admin", attribute.Roles);
    }
}
