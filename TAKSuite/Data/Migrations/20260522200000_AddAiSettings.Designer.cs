using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TAKSuite.Data.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260522200000_AddAiSettings")]
    partial class AddAiSettings
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder) { }
    }
}
