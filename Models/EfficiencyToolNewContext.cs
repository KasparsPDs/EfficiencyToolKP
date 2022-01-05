using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace EfficiencyToolKPlocal.Models
{
    public partial class EfficiencyToolNewContext : DbContext
    {
        public EfficiencyToolNewContext()
        {
        }

        public EfficiencyToolNewContext(DbContextOptions<EfficiencyToolNewContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Employee> Employees { get; set; }
        public virtual DbSet<Project> Projects { get; set; }
        public virtual DbSet<Task> Tasks { get; set; }
        public virtual DbSet<Worklog> Worklogs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                //optionsBuilder.UseSqlServer("Server=(LocalDB)\\MSSQLLocalDB;Database=EfficiencyToolNew;Trusted_Connection=True;");

            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<Employee>(entity =>
            {
                entity.ToTable("Employee");

                entity.Property(e => e.EmployeeId).HasMaxLength(128);

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.FirstLastName)
                    .IsRequired()
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<Project>(entity =>
            {
                entity.HasKey(e => e.ProjectKey)
                    .HasName("PK__Project__C048AC9430FE7972");

                entity.ToTable("Project");

                entity.Property(e => e.ProjectKey).HasMaxLength(10);

                entity.Property(e => e.ProjectName).HasMaxLength(60);
            });

            modelBuilder.Entity<Task>(entity =>
            {
                entity.ToTable("Task");

                entity.Property(e => e.TaskId).HasMaxLength(15);

                entity.Property(e => e.AsigneeId)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.DoneDate).HasColumnType("date");

                entity.Property(e => e.ProjectKey).HasMaxLength(10);

                entity.Property(e => e.ReporterId)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.TaskName)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.HasOne(d => d.Asignee)
                    .WithMany(p => p.TaskAsignees)
                    .HasForeignKey(d => d.AsigneeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Task__AsigneeId__286302EC");

                entity.HasOne(d => d.ProjectKeyNavigation)
                    .WithMany(p => p.Tasks)
                    .HasForeignKey(d => d.ProjectKey)
                    .HasConstraintName("FK__Task__ProjectKey__29572725");

                entity.HasOne(d => d.Reporter)
                    .WithMany(p => p.TaskReporters)
                    .HasForeignKey(d => d.ReporterId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Task__ReporterId__276EDEB3");
            });

            modelBuilder.Entity<Worklog>(entity =>
            {
                entity.ToTable("Worklog");

                entity.Property(e => e.EmployeeId)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.TaskId)
                    .IsRequired()
                    .HasMaxLength(15);

                entity.Property(e => e.WorklogDate).HasColumnType("date");

                entity.HasOne(d => d.Employee)
                    .WithMany(p => p.Worklogs)
                    .HasForeignKey(d => d.EmployeeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Worklog__Employe__2C3393D0");

                entity.HasOne(d => d.Task)
                    .WithMany(p => p.Worklogs)
                    .HasForeignKey(d => d.TaskId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Worklog__TaskId__2D27B809");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    }
}
