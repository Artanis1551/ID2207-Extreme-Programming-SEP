using Microsoft.EntityFrameworkCore;
using WebAPI.DataModels;

namespace WebAPI.DatabaseContexts
{
    public class SEPDbContext : DbContext
    {
        public DbSet<EventApplication> EventApplications { get; set; }
        public DbSet<SEPTask> Tasks { get; set; }
        public DbSet<RecruitmentRequest> RecruitmentRequests { get; set; }
        public DbSet<FinancialRequest> FinancialRequests { get; set; }

        public SEPDbContext(DbContextOptions options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}
