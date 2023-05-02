using Microsoft.EntityFrameworkCore;
using SeniorProj.Shared;
namespace SeniorProj.Server;

public class UserDbContext: DbContext
{
    protected override void OnConfiguring(
        DbContextOptionsBuilder options) => options.UseSqlite("Data Source=SeniorProj/Sever/users.db"); 
    //Need to update to correct connection string and test once DB is connected
    public DbSet<User>? Users { get; set; }
    public DbSet<Outage>? Outages { get; set; }
    public DbSet<ForumPost>? ForumPosts { get; set; }
}
