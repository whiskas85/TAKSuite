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

    public DbSet<TaskEntity> Tasks { get; set; }
    public DbSet<TaskLog> TaskLogs { get; set; }
    public DbSet<TaskHierarchy> TaskHierarchy { get; set; }
    public DbSet<TaskPriority> TaskPriorities { get; set; }
    public DbSet<EventEntity> EventEntities { get; set; }

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
            .WithOne(u => u.LedTeam) // Un utente può essere leader di un solo team
            .HasForeignKey<Team>(t => t.TeamLeaderId)
            .IsRequired(false); // Il leader può essere opzionale

        // Relazione uno-a-molti tra Team e i suoi membri
        modelBuilder.Entity<Team>()
            .HasMany(t => t.Members)
            .WithOne(u => u.Team)
            .HasForeignKey(u => u.TeamId)
            .IsRequired(false); // Gli utenti possono essere senza team

        // Definizione della relazione gerarchica: un team può avere un team padre e più sotto-team
        modelBuilder.Entity<Team>()
            .HasOne(t => t.ParentTeam)   // Un team ha un team padre (opzionale)
            .WithMany(t => t.SubTeams)   // Un team può avere più sotto-team
            .HasForeignKey(t => t.ParentTeamId)  // Chiave esterna per la relazione
            .OnDelete(DeleteBehavior.Restrict);  // Evita la cancellazione a cascata


        // Configurazione della tabella di giunzione DocumentationOwner
        modelBuilder.Entity<DocumentationOwner>()
            .HasKey(_ => new { _.DocumentationId, _.OwnerId, _.OwnerType });  // Combinazione delle chiavi

        modelBuilder.Entity<DocumentationOwner>()
            .HasOne(_ => _.Documentation)
            .WithMany(d => d.DocumentationOwners)
            .HasForeignKey(_ => _.DocumentationId);



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
    }
}