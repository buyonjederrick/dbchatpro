using Microsoft.EntityFrameworkCore;
using DbChatPro.Core.Models;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;

namespace DbChatPro.Core.Data
{
    public class DbChatProContext : DbContext
    {
        public DbChatProContext(DbContextOptions<DbChatProContext> options) : base(options)
        {
        }

        public DbSet<DatabaseConnection> DatabaseConnections { get; set; }
        public DbSet<QueryHistory> QueryHistories { get; set; }
        public DbSet<DatabaseSchema> DatabaseSchemas { get; set; }
        public DbSet<DatabaseTable> DatabaseTables { get; set; }
        public DbSet<DatabaseColumn> DatabaseColumns { get; set; }
        public DbSet<UserSession> UserSessions { get; set; }
        public DbSet<SystemConfiguration> SystemConfigurations { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure base entity
            modelBuilder.Entity<BaseEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // Configure DatabaseConnection
            modelBuilder.Entity<DatabaseConnection>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.DatabaseType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ConnectionString).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Environment).HasMaxLength(100);
                
                entity.HasIndex(e => e.Name).IsUnique();
                entity.HasIndex(e => e.IsActive);
            });

            // Configure QueryHistory
            modelBuilder.Entity<QueryHistory>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserPrompt).IsRequired();
                entity.Property(e => e.GeneratedSql).IsRequired();
                entity.Property(e => e.AiModel).HasMaxLength(50);
                entity.Property(e => e.AiService).HasMaxLength(50);
                entity.Property(e => e.ErrorMessage).HasMaxLength(1000);
                entity.Property(e => e.UserId).HasMaxLength(100);
                entity.Property(e => e.SessionId).HasMaxLength(100);
                
                entity.HasOne(e => e.DatabaseConnection)
                    .WithMany(e => e.QueryHistories)
                    .HasForeignKey(e => e.DatabaseConnectionId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.IsSuccessful);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.SessionId);
            });

            // Configure DatabaseSchema
            modelBuilder.Entity<DatabaseSchema>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.SchemaRaw).IsRequired();
                
                entity.HasOne(e => e.DatabaseConnection)
                    .WithMany(e => e.Schemas)
                    .HasForeignKey(e => e.DatabaseConnectionId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasIndex(e => e.LastRefreshed);
            });

            // Configure DatabaseTable
            modelBuilder.Entity<DatabaseTable>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Schema).HasMaxLength(100);
                
                entity.HasOne(e => e.DatabaseSchema)
                    .WithMany(e => e.Tables)
                    .HasForeignKey(e => e.DatabaseSchemaId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasIndex(e => new { e.DatabaseSchemaId, e.Name });
            });

            // Configure DatabaseColumn
            modelBuilder.Entity<DatabaseColumn>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.DataType).HasMaxLength(100);
                entity.Property(e => e.ReferencedTable).HasMaxLength(100);
                entity.Property(e => e.ReferencedColumn).HasMaxLength(100);
                
                entity.HasOne(e => e.DatabaseTable)
                    .WithMany(e => e.Columns)
                    .HasForeignKey(e => e.DatabaseTableId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasIndex(e => new { e.DatabaseTableId, e.Name });
                entity.HasIndex(e => e.IsPrimaryKey);
                entity.HasIndex(e => e.IsForeignKey);
            });

            // Configure UserSession
            modelBuilder.Entity<UserSession>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.SessionId).IsRequired().HasMaxLength(100);
                entity.Property(e => e.UserId).HasMaxLength(100);
                entity.Property(e => e.UserName).HasMaxLength(100);
                entity.Property(e => e.IpAddress).HasMaxLength(50);
                entity.Property(e => e.UserAgent).HasMaxLength(500);
                
                entity.HasIndex(e => e.SessionId).IsUnique();
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.LastActivity);
            });

            // Configure SystemConfiguration
            modelBuilder.Entity<SystemConfiguration>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Key).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Value).IsRequired();
                entity.Property(e => e.Category).HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                
                entity.HasIndex(e => e.Key).IsUnique();
                entity.HasIndex(e => e.Category);
            });

            // Configure AuditLog
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Action).IsRequired().HasMaxLength(50);
                entity.Property(e => e.EntityName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.UserId).HasMaxLength(100);
                entity.Property(e => e.UserName).HasMaxLength(100);
                entity.Property(e => e.IpAddress).HasMaxLength(50);
                entity.Property(e => e.UserAgent).HasMaxLength(500);
                entity.Property(e => e.AdditionalData).HasMaxLength(1000);
                
                entity.HasIndex(e => e.Action);
                entity.HasIndex(e => e.EntityName);
                entity.HasIndex(e => e.EntityId);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.CreatedAt);
            });
        }

        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            UpdateTimestamps();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries<BaseEntity>()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                }
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}