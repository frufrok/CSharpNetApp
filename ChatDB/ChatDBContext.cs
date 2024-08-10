using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace CodeFirstDB
{
    internal class ChatDBContext :  DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Message> Messages { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=ChatDB;" +
                "Username=postgres;Password=password").UseLazyLoadingProxies();
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(user =>
            {
                user.ToTable("Users");
                user.HasKey(x => x.ID).HasName("PK_Users");
                user.HasIndex(x => x.Nickname).IsUnique().HasDatabaseName("IX_Users_Nickname");
                user.Property(x => x.ID).HasColumnName("ID").IsRequired(true);
                user.Property(x => x.Nickname).HasColumnName("Nickname").HasMaxLength(255).IsRequired(true);
            });

            modelBuilder.Entity<Message>(message =>
            {
                message.ToTable("Messages");
                message.HasKey(x => x.ID).HasName("PK_Messages");
                message.Property(x => x.ID).HasColumnName("ID").IsRequired(true);
                message.Property(x => x.Text).HasColumnName("Text").IsRequired(true);
                message.Property(x => x.DateTimeSend).HasColumnName("DateTimeSend").IsRequired(true);
                message.Property(x => x.UserFromID).HasColumnName("UserFromID").IsRequired(true);
                message.Property(x => x.UserToID).HasColumnName("UserToID").IsRequired(true);
                message.HasOne(m => m.UserFrom).WithMany(u => u.SentMessages)
                    .HasForeignKey(x => x.UserFromID).HasConstraintName("FK_Messages_Users_From");
                message.HasOne(m => m.UserTo).WithMany(u => u.RecievedMessages)
                    .HasForeignKey(x => x.UserToID).HasConstraintName("FK_Messages_Users_To");
            });
        }
    }
}
