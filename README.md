### necessary package

> - dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
> - dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
> - dotnet add package Microsoft.EntityFrameworkCore.Sqlite
> - dotnet add package Microsoft.EntityFrameworkCore.Tools

### migration

> - dotnet ef migrations add InitialCreate
> - dotnet ef database update

---

## Implementation Explanation

### 1. Package Dependencies

- Microsoft.AspNetCore.Authentication.JwtBearer: Handles JWT token authentication
- Microsoft.AspNetCore.Identity.EntityFrameworkCore: Provides Identity framework integration with Entity Framework
- Microsoft.EntityFrameworkCore: ORM for database operations
- Microsoft.EntityFrameworkCore.Tools: For migrations and database updates

## Key Components

### Database Context

- ApplicationDbContext extends IdentityDbContext<ApplicationUser> to integrate with Identity
- This provides tables for users, roles, claims, and other Identity-related entities

### User Model

- ApplicationUser extends IdentityUser to add custom properties
- You can add additional user fields as needed (FirstName, LastName, etc.)

### JWT Service

- JwtService generates JWT tokens with appropriate claims
- Claims include user ID, username, email, and roles
- Token includes expiration time, issuer, and audience information

### Authentication Configuration

In Program.cs:

- Configures Identity services with EntityFramework storage
- Sets up JWT Bearer authentication with validation parameters

### Security Configuration

- Secret key is stored in appsettings.json
- Token settings (issuer, audience, expiration) are configurable

---

## The relationship between JWT Tokens and UserManager

The JWT token and UserManager are connected through a chain of operations, but they serve different purposes in the authentication flow:

### 1. UserManager's Role

UserManager is responsible for:

- Creating and managing user accounts
- Storing user credentials and profile data
- Verifying passwords
- Managing user roles and claims
- Handling user-related operations like password reset

#### UserManager doesn't directly handle JWT tokens. Instead, it provides the user data that gets encoded into JWT tokens.

### 2. The Authentication Flow Connection

Here's how they work together in the authentication process:

#### 2-1. User Lookup and Verification:

```
var user = await _userManager.FindByEmailAsync(model.Email);
var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);
```

- UserManager finds the user by email
- SignInManager (which works with UserManager) verifies the password

#### 2-2. Retrieving User Claims Data:

```
var roles = await _userManager.GetRolesAsync(user);
```

- UserManager provides the roles and other user information that will be encoded in the token

#### 2-3. Token Generation

```
var token = _jwtService.GenerateJwtToken(user, roles);
```

- The JwtService uses the user object and roles to create claims
- These claims are packed into the JWT token

### 3. The User Identity Connection

When a request comes in with a JWT token:

1. The JWT middleware automatically:
   > - Validates the token
   > - Extracts the claims
   > - Creates a ClaimsPrincipal (User object)
2. The controller can access this identity

```
[Authorize]
public IActionResult Get()
{
    // User.Identity.Name is from the Name claim in the token
    // User.IsInRole("Admin") checks the role claims in the token
    return Ok(new { Message = "Secured data", User = User.Identity.Name });
}
```

3. If needed, you can look up the full user again:

```
var user = await _userManager.FindByNameAsync(User.Identity.Name);
// Now you have access to all user properties and methods
```

### 4. The Claims Connection

#### The most important link between UserManager and JWT tokens is the claims data:

```
public string GenerateJwtToken(ApplicationUser user, IList<string> roles)
{
    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id),
        new Claim(ClaimTypes.Name, user.UserName),
        new Claim(ClaimTypes.Email, user.Email),
        // ...
    };

    // Add role claims
    foreach (var role in roles)
    {
        claims.Add(new Claim(ClaimTypes.Role, role));
    }

    // Create and return token...
}
```

These claims come from:

- The user object properties (from UserManager)
- The roles (retrieved via UserManager.GetRolesAsync())

### 4. Key Points About This Architecture

#### 4-1 Separation of Concerns:

- UserManager manages users
- JWT service handles token generation
- Authentication middleware validates tokens

#### 4-2 Stateless Authentication:

- The JWT token contains all needed user information
- After login, the server doesn't need to look up the user in the database for authentication
- UserManager is only needed again for user profile updates or additional data lookup

#### 4-3 Security Implementation:

- UserManager securely stores passwords (hashed and salted)
- JWT tokens are cryptographically signed but don't contain sensitive data like passwords

---

## Explanation of Identity Configuration Code:

```
// Configure Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
```

1. `AddIdentity<ApplicationUser, IdentityRole>()`
   This line registers the ASP.NET Core Identity services in the dependency injection container with two type parameters:

- ApplicationUser: This is your custom user class that inherits from IdentityUser.
  > - IdentityUser provides standard properties like Id, UserName, Email, PasswordHash, etc.
  > - ApplicationUser extends this with your custom properties like FirstName and LastName.
- IdentityRole: This is the class used for representing roles in the system.
  > - You could create a custom role class, but here you're using the built-in IdentityRole which provides properties like Id and Name.
  >   This method also registers core Identity services like:
- UserManager<ApplicationUser> - For managing users
- SignInManager<ApplicationUser> - For handling sign-in/sign-out
- RoleManager<IdentityRole> - For managing roles

2. `.AddEntityFrameworkStores<ApplicationDbContext>()`
   This configures Identity to use Entity Framework Core as the persistence mechanism:

- It tells Identity to use your ApplicationDbContext for storing and retrieving identity-related data.
- It automatically creates the necessary tables in your database:
  > - AspNetUsers - Stores user data
  > - AspNetRoles - Stores role definitions
  > - AspNetUserRoles - Stores user to role mappings
  > - AspNetUserClaims - Stores user claims
  > - AspNetUserLogins - Stores external login information
  > - AspNetUserTokens - Stores tokens like password reset or email confirmation tokens

3. `.AddDefaultTokenProviders()`
   This registers the default token providers used by Identity for operations that require security tokens:
   > - Email confirmation tokens: Used to verify email addresses when users register
   > - Password reset tokens: Used in password recovery flows
   > - Two-factor authentication tokens: Used for 2FA authentication
   > - Change email tokens: Used when users want to change their email
   > - Phone number verification tokens: Used to verify phone numbers

#### This token is nothing to do with JWT Token.
