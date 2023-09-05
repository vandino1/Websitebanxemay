using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using websitebanxemay.Models;

namespace websitebanxemay.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<websitebanxemay.Models.Category> Category { get; set; }      
        public DbSet<websitebanxemay.Models.Product> Product { get; set; }
        public DbSet<websitebanxemay.Models.Comment> Comment { get; set; }
        public DbSet<websitebanxemay.Models.Rely> Rely { get; set; }
        public DbSet<websitebanxemay.Models.ActivityLog> ActivityLog { get; set; }
        public DbSet<websitebanxemay.Models.Bill> Bill { get; set; }
        public DbSet<websitebanxemay.Models.BillDetail> BillDetail { get; set; }      
        public DbSet<websitebanxemay.Models.Statistics> Statistics { get; set; }
    }
}
