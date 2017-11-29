using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using KnowledgeBank.Persistence;

namespace KnowledgeBank.Persistence.Migrations.ApplicationDb
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20170615153825_EmailTemplate")]
    partial class EmailTemplate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.2")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("KnowledgeBank.Domain.Attachment", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("CaseId");

                    b.Property<string>("Location")
                        .IsRequired();

                    b.Property<string>("Name")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("CaseId");

                    b.ToTable("Attachments");
                });

            modelBuilder.Entity("KnowledgeBank.Domain.Case", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Category");

                    b.Property<DateTimeOffset>("CreatedAt");

                    b.Property<DateTimeOffset>("LastModifiedAt");

                    b.Property<long>("TenantId");

                    b.Property<string>("Title");

                    b.HasKey("Id");

                    b.ToTable("Cases");
                });

            modelBuilder.Entity("KnowledgeBank.Domain.Emailtemplate", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("CaseId");

                    b.Property<string>("Contents");

                    b.HasKey("Id");

                    b.HasIndex("CaseId");

                    b.ToTable("Emailtemplate");
                });

            modelBuilder.Entity("KnowledgeBank.Domain.Link", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("CaseId");

                    b.Property<string>("Description");

                    b.Property<string>("Location")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("CaseId");

                    b.ToTable("Links");
                });

            modelBuilder.Entity("KnowledgeBank.Domain.Step", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("CaseId");

                    b.Property<string>("Description");

                    b.Property<int>("OrderNumber");

                    b.HasKey("Id");

                    b.HasIndex("CaseId");

                    b.ToTable("Steps");
                });

            modelBuilder.Entity("KnowledgeBank.Domain.Tag", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("CaseId");

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.HasIndex("CaseId");

                    b.ToTable("Tags");
                });

            modelBuilder.Entity("KnowledgeBank.Domain.Attachment", b =>
                {
                    b.HasOne("KnowledgeBank.Domain.Case", "Case")
                        .WithMany("Attachments")
                        .HasForeignKey("CaseId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("KnowledgeBank.Domain.Emailtemplate", b =>
                {
                    b.HasOne("KnowledgeBank.Domain.Case", "Case")
                        .WithMany("EmailTemplates")
                        .HasForeignKey("CaseId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("KnowledgeBank.Domain.Link", b =>
                {
                    b.HasOne("KnowledgeBank.Domain.Case", "Case")
                        .WithMany("Links")
                        .HasForeignKey("CaseId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("KnowledgeBank.Domain.Step", b =>
                {
                    b.HasOne("KnowledgeBank.Domain.Case", "Case")
                        .WithMany("Steps")
                        .HasForeignKey("CaseId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("KnowledgeBank.Domain.Tag", b =>
                {
                    b.HasOne("KnowledgeBank.Domain.Case", "Case")
                        .WithMany("Tags")
                        .HasForeignKey("CaseId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}
