using System;
using test.api.Models;

namespace test.api.Services;

public interface IJwtService
{
    string GenerateJwtToken(ApplicationUser user, IList<string> roles);
}
