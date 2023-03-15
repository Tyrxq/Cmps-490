using Microsoft.EntityFrameworkCore;
using SeniorProj.Shared;
namespace SeniorProj.Server;

public class UserDbContext: DbContext
{
    protected override void OnConfiguring(
        DbContextOptionsBuilder options) => options.UseNpgsql("Data Source=Users.db"); 
    //Need to update to correct connection string and test once DB is connected
    public DbSet<User>? Users { get; set; }
}