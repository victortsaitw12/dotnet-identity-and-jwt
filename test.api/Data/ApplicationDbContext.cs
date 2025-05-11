using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using test.api.Models;

namespace test.api.Data;

public class ApplicationDbContext: IdentityDbContext<ApplicationUser>
{
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
            : base(options)
        {
        }
}
