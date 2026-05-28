using TAKSuite.Data.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace TAKSuite.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Team> Teams { get; set; }
    public DbSet<RegistrationCode> RegistrationCodes { get; set; }
    public DbSet<TeamRadioChannel> TeamRadioChannels { get; set; }
    public DbSet<RadioChannel> RadioChannels { get; set; }
    public DbSet<UserAtak> UsersAtak { get; set; }

    public DbSet<Documentation> Documents { get; set; }
    public DbSet<DocumentationOwner> DocumentationOwners { get; set; }
    public DbSet<DocumentType> DocumentTypes { get; set; }

    public DbSet<TaskEntity> Tasks { get; set; }
    public DbSet<TaskStringItem> TaskStringItems { get; set; }

    public DbSet<TaskLog> TaskLogs { get; set; }
    public DbSet<TaskHierarchy> TaskHierarchy { get; set; }
    public DbSet<TaskPriority> TaskPriorities { get; set; }
    public DbSet<EventEntity> EventEntities { get; set; }

    public DbSet<MissionSuite> MissionsTakSuite { get; set; }
    public DbSet<MissionPhotoJoinConfig> MissionPhotoJoinConfigs { get; set; }
    public DbSet<CotPriorityRule> CotPriorityRules { get; set; }

    public DbSet<AiSettings>      AiSettings      { get; set; }
    public DbSet<TakSettings>     TakSettings     { get; set; }
    public DbSet<TakSubscription> TakSubscriptions { get; set; }
    public DbSet<CotTemplate>     CotTemplates    { get; set; }
    public DbSet<MissionRadioContact> MissionRadioContacts { get; set; }
    public DbSet<PhoneContact> PhoneContacts { get; set; }
    public DbSet<MissionPhoneContact> MissionPhoneContacts { get; set; }

    public DbSet<TaskScoreEntry> TaskScoreEntries { get; set; }
    public DbSet<ScoreConfig> ScoreConfigs { get; set; }
    public DbSet<AtakMapSource>   AtakMapSources   { get; set; }
    public DbSet<CachedCoTEntry>  CachedCoTEntries { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);


        // Configurazione per RadioChannel principale
        modelBuilder.Entity<TeamRadioChannel>()
            .HasOne(trc => trc.RadioChannel)
            .WithMany()  // Puoi aggiungere una collezione a RadioChannel se desideri una relazione inversa
            .HasForeignKey(trc => trc.RadioChannelId)
            .OnDelete(DeleteBehavior.Restrict); // Impedisce la cancellazione a cascata

        // Configurazione per BackupRadioChannel
        modelBuilder.Entity<TeamRadioChannel>()
            .HasOne(trc => trc.BackupRadioChannel)
            .WithMany()  // Puoi aggiungere una collezione a RadioChannel se desideri una relazione inversa
            .HasForeignKey(trc => trc.BackupRadioChannelId)
            .OnDelete(DeleteBehavior.Restrict); // Impedisce la cancellazione a cascata
                                                // Configurazione per Frequency con tipo double
                                                // Relazione uno-a-uno tra Team e il suo Leader
        modelBuilder.Entity<Team>()
            .HasOne(t => t.TeamLeader)
            .WithOne(u => u.LedTeam) // Un utente pu� essere leader di un solo team
            .HasForeignKey<Team>(t => t.TeamLeaderId)
            .IsRequired(false); // Il leader pu� essere opzionale

        // Relazione uno-a-molti tra Team e i suoi membri
        modelBuilder.Entity<Team>()
            .HasMany(t => t.Members)
            .WithOne(u => u.Team)
            .HasForeignKey(u => u.TeamId)
            .IsRequired(false); // Gli utenti possono essere senza team

        // Definizione della relazione gerarchica: un team pu� avere un team padre e pi� sotto-team
        modelBuilder.Entity<Team>()
            .HasOne(t => t.ParentTeam)   // Un team ha un team padre (opzionale)
            .WithMany(t => t.SubTeams)   // Un team pu� avere pi� sotto-team
            .HasForeignKey(t => t.ParentTeamId)  // Chiave esterna per la relazione
            .OnDelete(DeleteBehavior.Restrict);  // Evita la cancellazione a cascata


        // Configurazione della tabella di giunzione DocumentationOwner
        modelBuilder.Entity<DocumentationOwner>()
            .HasKey(_ => new { _.DocumentationId, _.OwnerId, _.OwnerType });  // Combinazione delle chiavi

        modelBuilder.Entity<DocumentationOwner>()
            .HasOne(_ => _.Documentation)
            .WithMany(d => d.DocumentationOwners)
            .HasForeignKey(_ => _.DocumentationId);

        modelBuilder.Entity<Documentation>()
            .HasOne(d => d.DocumentType)
            .WithMany()
            .HasForeignKey(d => d.DocumentTypeId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);



        // AiSettings: singleton row
        modelBuilder.Entity<AiSettings>()
            .Property(a => a.Id)
            .ValueGeneratedNever();

        // TakSettings: singleton row, Id is always 1 — never auto-generated
        modelBuilder.Entity<TakSettings>()
            .Property(t => t.Id)
            .ValueGeneratedNever();

        // MissionSuite → Team (responsabile)
        modelBuilder.Entity<MissionSuite>()
            .HasOne(m => m.Team)
            .WithMany()
            .HasForeignKey(m => m.TeamId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        // MissionSuite ↔ MissionPhotoJoinConfig (1-to-1, cascade delete)
        modelBuilder.Entity<MissionPhotoJoinConfig>()
            .HasOne(c => c.Mission)
            .WithOne(m => m.PhotoJoinConfig)
            .HasForeignKey<MissionPhotoJoinConfig>(c => c.MissionId)
            .OnDelete(DeleteBehavior.Cascade);

        // MissionPhotoJoinConfig → CotPriorityRules (1-to-many, cascade delete)
        modelBuilder.Entity<CotPriorityRule>()
            .HasOne(r => r.PhotoJoinConfig)
            .WithMany(c => c.PriorityRules)
            .HasForeignKey(r => r.PhotoJoinConfigId)
            .OnDelete(DeleteBehavior.Cascade);

        // MissionRadioContact → MissionSuite (cascade delete)
        modelBuilder.Entity<MissionRadioContact>()
            .HasOne(r => r.Mission)
            .WithMany()
            .HasForeignKey(r => r.MissionId)
            .OnDelete(DeleteBehavior.Cascade);

        // MissionRadioContact → RadioChannel (restrict)
        modelBuilder.Entity<MissionRadioContact>()
            .HasOne(r => r.RadioChannel)
            .WithMany()
            .HasForeignKey(r => r.RadioChannelId)
            .OnDelete(DeleteBehavior.Restrict);

        // MissionRadioContact → BackupRadioChannel (restrict, optional)
        modelBuilder.Entity<MissionRadioContact>()
            .HasOne(r => r.BackupRadioChannel)
            .WithMany()
            .HasForeignKey(r => r.BackupRadioChannelId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        // MissionPhoneContact → MissionSuite (cascade delete)
        modelBuilder.Entity<MissionPhoneContact>()
            .HasOne(r => r.Mission)
            .WithMany()
            .HasForeignKey(r => r.MissionId)
            .OnDelete(DeleteBehavior.Cascade);

        // MissionPhoneContact → PhoneContact (restrict)
        modelBuilder.Entity<MissionPhoneContact>()
            .HasOne(r => r.PhoneContact)
            .WithMany()
            .HasForeignKey(r => r.PhoneContactId)
            .OnDelete(DeleteBehavior.Restrict);

        // model builder for TaskEntity
        modelBuilder.Entity<TaskEntity>()
            .HasOne(t => t.AssignedTeam)
            .WithMany()
            .HasForeignKey(t => t.AssignedTeamId);

        modelBuilder.Entity<TaskEntity>()
            .HasOne(t => t.ExecutingTeam)
            .WithMany()
            .HasForeignKey(t => t.ExecutingTeamId);

        modelBuilder.Entity<TaskLog>()
            .HasOne(tl => tl.Task)
            .WithMany(t => t.Logs)
            .HasForeignKey(tl => tl.TaskId);

        modelBuilder.Entity<TaskHierarchy>()
                .HasOne(tl => tl.Task)
                .WithMany(t => t.Hierarchy)
                .HasForeignKey(tl => tl.TaskId);

        modelBuilder.Entity<TaskScoreEntry>()
            .HasOne(s => s.Task)
            .WithMany()
            .HasForeignKey(s => s.TaskEntityId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ScoreConfig>()
            .HasOne(c => c.Task)
            .WithMany()
            .HasForeignKey(c => c.TaskEntityId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AtakMapSource>()
            .HasIndex(m => m.FileName)
            .IsUnique();

        modelBuilder.Entity<CachedCoTEntry>()
            .HasKey(e => e.Uid);
    }
}