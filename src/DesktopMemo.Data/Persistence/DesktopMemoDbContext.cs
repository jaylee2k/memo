using System;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.SQLite;
using System.ComponentModel.DataAnnotations.Schema;
using DesktopMemo.Data.Infrastructure;
using DesktopMemo.Domain.Entities;

namespace DesktopMemo.Data.Persistence;

public class DesktopMemoDbContext : DbContext
{
    public DbSet<MemoGroup> MemoGroups { get; set; }
    public DbSet<Note> Notes { get; set; }
    public DbSet<StickyWindowState> StickyWindowStates { get; set; }
    public DbSet<AppSetting> AppSettings { get; set; }

    public DesktopMemoDbContext() : base(CreateConnection(), true)
    {
        Configuration.LazyLoadingEnabled = true;
    }

    private static DbConnection CreateConnection()
    {
        var builder = new SQLiteConnectionStringBuilder
        {
            DataSource = DatabasePathProvider.GetDatabasePath(),
            BinaryGUID = false,
            ForeignKeys = true,
            JournalMode = SQLiteJournalModeEnum.Wal,
            SyncMode = SynchronizationModes.Normal
        };

        return new SQLiteConnection(builder.ConnectionString);
    }

    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MemoGroup>().HasKey(x => x.Id);
        modelBuilder.Entity<MemoGroup>()
            .Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(120);

        modelBuilder.Entity<MemoGroup>()
            .HasOptional(x => x.ParentGroup)
            .WithMany(x => x.Children)
            .HasForeignKey(x => x.ParentGroupId)
            .WillCascadeOnDelete(false);

        modelBuilder.Entity<MemoGroup>()
            .Property(x => x.ParentGroupId)
            .HasColumnAnnotation(
                IndexAnnotation.AnnotationName,
                new IndexAnnotation(new IndexAttribute("IX_MemoGroups_ParentGroup_IsDeleted", 1)));

        modelBuilder.Entity<MemoGroup>()
            .Property(x => x.IsDeleted)
            .HasColumnAnnotation(
                IndexAnnotation.AnnotationName,
                new IndexAnnotation(new IndexAttribute("IX_MemoGroups_ParentGroup_IsDeleted", 2)));

        modelBuilder.Entity<MemoGroup>()
            .Property(x => x.DeletedAtUtc)
            .HasColumnAnnotation(
                IndexAnnotation.AnnotationName,
                new IndexAnnotation(new IndexAttribute("IX_MemoGroups_DeletedAt")));

        modelBuilder.Entity<Note>().HasKey(x => x.Id);
        modelBuilder.Entity<Note>()
            .Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        modelBuilder.Entity<Note>()
            .Property(x => x.ContentMarkdown)
            .IsRequired();

        modelBuilder.Entity<Note>()
            .HasRequired(x => x.Group)
            .WithMany(x => x.Notes)
            .HasForeignKey(x => x.GroupId)
            .WillCascadeOnDelete(false);

        modelBuilder.Entity<Note>()
            .Property(x => x.GroupId)
            .HasColumnAnnotation(
                IndexAnnotation.AnnotationName,
                new IndexAnnotation(new IndexAttribute("IX_Notes_Group_IsDeleted", 1)));

        modelBuilder.Entity<Note>()
            .Property(x => x.IsDeleted)
            .HasColumnAnnotation(
                IndexAnnotation.AnnotationName,
                new IndexAnnotation(new IndexAttribute("IX_Notes_Group_IsDeleted", 2)));

        modelBuilder.Entity<Note>()
            .Property(x => x.AlarmEnabled)
            .HasColumnAnnotation(
                IndexAnnotation.AnnotationName,
                new IndexAnnotation(new IndexAttribute("IX_Notes_Alarm", 1)));

        modelBuilder.Entity<Note>()
            .Property(x => x.AlarmAtUtc)
            .HasColumnAnnotation(
                IndexAnnotation.AnnotationName,
                new IndexAnnotation(new IndexAttribute("IX_Notes_Alarm", 2)));

        modelBuilder.Entity<Note>()
            .Property(x => x.SnoozeUntilUtc)
            .HasColumnAnnotation(
                IndexAnnotation.AnnotationName,
                new IndexAnnotation(new IndexAttribute("IX_Notes_Alarm", 3)));

        modelBuilder.Entity<Note>()
            .Property(x => x.IsDeleted)
            .HasColumnAnnotation(
                IndexAnnotation.AnnotationName,
                new IndexAnnotation(new IndexAttribute("IX_Notes_Alarm", 4)));

        modelBuilder.Entity<Note>()
            .Property(x => x.DeletedAtUtc)
            .HasColumnAnnotation(
                IndexAnnotation.AnnotationName,
                new IndexAnnotation(new IndexAttribute("IX_Notes_DeletedAt")));

        modelBuilder.Entity<StickyWindowState>().HasKey(x => x.NoteId);
        modelBuilder.Entity<StickyWindowState>()
            .HasRequired(x => x.Note)
            .WithOptional(x => x.StickyWindowState);

        modelBuilder.Entity<AppSetting>().HasKey(x => x.Key);
        modelBuilder.Entity<AppSetting>().Property(x => x.Key).HasMaxLength(120);
        modelBuilder.Entity<AppSetting>().Property(x => x.Value).IsRequired();

        base.OnModelCreating(modelBuilder);
    }
}
