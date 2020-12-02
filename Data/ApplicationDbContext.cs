using System;
using System.Collections.Generic;
using System.Text;
using FinancialPortalProject.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FinancialPortalProject.Data
{
    public class ApplicationDbContext : IdentityDbContext<FpUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}
