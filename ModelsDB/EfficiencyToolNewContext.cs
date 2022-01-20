using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace NEWefficiencyTool.ModelsDB
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
        public virtual DbSet<EmployeeAvgEfficiency> EmployeeAvgEfficiencies { get; set; }
        public virtual DbSet<Project> Projects { get; set; }
        public virtual DbSet<Task> Tasks { get; set; }
        public virtual DbSet<TaskEmployeeEfficiency> TaskEmployeeEfficiencies { get; set; }
        public virtual DbSet<Worklog> Worklogs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {

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

            modelBuilder.Entity<EmployeeAvgEfficiency>(entity =>
            {
                entity.HasKey(e => e.EmplEfficiencyId)
                    .HasName("PK__Employee__EE1B18430041E375");

                entity.ToTable("EmployeeAvgEfficiency");

                entity.Property(e => e.DateFrom).HasColumnType("date");

                entity.Property(e => e.DateTo).HasColumnType("date");

                entity.Property(e => e.EmployeeId)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.HasOne(d => d.Employee)
                    .WithMany(p => p.EmployeeAvgEfficiencies)
                    .HasForeignKey(d => d.EmployeeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__EmployeeA__Emplo__33D4B598");
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

            modelBuilder.Entity<TaskEmployeeEfficiency>(entity =>
            {
                entity.HasKey(e => e.EfficiencyId)
                    .HasName("PK__TaskEmpl__1E611D8F6879DC18");

                entity.ToTable("TaskEmployeeEfficiency");

                entity.Property(e => e.EmployeeId)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.TaskId)
                    .IsRequired()
                    .HasMaxLength(15);

                entity.HasOne(d => d.Employee)
                    .WithMany(p => p.TaskEmployeeEfficiencies)
                    .HasForeignKey(d => d.EmployeeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__TaskEmplo__Emplo__300424B4");

                entity.HasOne(d => d.Task)
                    .WithMany(p => p.TaskEmployeeEfficiencies)
                    .HasForeignKey(d => d.TaskId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__TaskEmplo__TaskI__30F848ED");
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
