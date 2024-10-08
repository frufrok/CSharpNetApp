﻿// <auto-generated />
using System;
using ChatDB.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ChatDB.Migrations
{
    [DbContext(typeof(ChatDBContext))]
    partial class ChatDBContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.7")
                .HasAnnotation("Proxies:ChangeTracking", false)
                .HasAnnotation("Proxies:CheckEquality", false)
                .HasAnnotation("Proxies:LazyLoading", true)
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("CodeFirstDB.Message", b =>
                {
                    b.Property<int?>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("ID");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int?>("ID"));

                    b.Property<DateTime?>("DateTimeSend")
                        .IsRequired()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("DateTimeSend");

                    b.Property<bool?>("IsSent")
                        .HasColumnType("boolean");

                    b.Property<string>("Text")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("Text");

                    b.Property<int?>("UserFromID")
                        .IsRequired()
                        .HasColumnType("integer")
                        .HasColumnName("UserFromID");

                    b.Property<int?>("UserToID")
                        .IsRequired()
                        .HasColumnType("integer")
                        .HasColumnName("UserToID");

                    b.HasKey("ID")
                        .HasName("PK_Messages");

                    b.HasIndex("UserFromID");

                    b.HasIndex("UserToID");

                    b.ToTable("Messages", (string)null);
                });

            modelBuilder.Entity("CodeFirstDB.User", b =>
                {
                    b.Property<int?>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("ID");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int?>("ID"));

                    b.Property<string>("Nickname")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("Nickname");

                    b.HasKey("ID")
                        .HasName("PK_Users");

                    b.HasIndex("Nickname")
                        .IsUnique()
                        .HasDatabaseName("IX_Users_Nickname");

                    b.ToTable("Users", (string)null);
                });

            modelBuilder.Entity("CodeFirstDB.Message", b =>
                {
                    b.HasOne("CodeFirstDB.User", "UserFrom")
                        .WithMany("SentMessages")
                        .HasForeignKey("UserFromID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("FK_Messages_Users_From");

                    b.HasOne("CodeFirstDB.User", "UserTo")
                        .WithMany("RecievedMessages")
                        .HasForeignKey("UserToID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("FK_Messages_Users_To");

                    b.Navigation("UserFrom");

                    b.Navigation("UserTo");
                });

            modelBuilder.Entity("CodeFirstDB.User", b =>
                {
                    b.Navigation("RecievedMessages");

                    b.Navigation("SentMessages");
                });
#pragma warning restore 612, 618
        }
    }
}
