using Microsoft.EntityFrameworkCore;

namespace WebhookReceiver.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<WebhookEvent> WebhookEvents { get; set; } = null!;
}
