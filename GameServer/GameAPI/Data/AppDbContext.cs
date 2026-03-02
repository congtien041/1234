using Microsoft.EntityFrameworkCore;
using GameAPI.Models;

namespace GameAPI.Data;

public class AppDbContext : DbContext {
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    
    public DbSet<Player> Players { get; set; }
    public DbSet<PlayerCharacter> PlayerCharacters { get; set; }
    public DbSet<Inbox> Inboxes { get; set; }
}