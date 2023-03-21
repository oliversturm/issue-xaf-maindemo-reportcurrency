using DevExpress.ExpressApp.Design;
using DevExpress.ExpressApp.EFCore.DesignTime;
using DevExpress.ExpressApp.EFCore.Updating;
using DevExpress.ExpressApp.Security;
using DevExpress.Persistent.BaseImpl.EF;
using DevExpress.Persistent.BaseImpl.EF.PermissionPolicy;
using DevExpress.Persistent.BaseImpl.EFCore.AuditTrail;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MainDemo.Module.BusinessObjects;

public class EFDemoDbContextInitializer : DbContextTypesInfoInitializerBase {
    // This code allows our Model Editor to get relevant EF Core metadata at design time.
    // For details, please refer to https://supportcenter.devexpress.com/ticket/details/t933891.
    protected override DbContext CreateDbContext() {
        var optionsBuilder = new DbContextOptionsBuilder<MainDemoDbContext>()
            .UseSqlServer(@";")
            .UseChangeTrackingProxies()
            .UseObjectSpaceLinkProxies();
        return new MainDemoDbContext(optionsBuilder.Options);
    }
}
//This factory creates DbContext for design-time services. For example, it is required for database migration.
public class MainDemoDesignTimeDbContextFactory : IDesignTimeDbContextFactory<MainDemoDbContext> {
    public MainDemoDbContext CreateDbContext(string[] args) {
        throw new InvalidOperationException("Make sure that the database connection string and connection provider are correct. After that, uncomment the code below and remove this exception.");
        //var optionsBuilder = new DbContextOptionsBuilder<MainDemoDbContext>();
        //optionsBuilder.UseSqlServer(@"Integrated Security=SSPI;Pooling=false;MultipleActiveResultSets=true;Data Source=(localdb)\\mssqllocaldb;Initial Catalog=MainDemo.NET.EFCore_v22.2;ConnectRetryCount=0;");
        //optionsBuilder.UseChangeTrackingProxies();
        //return new MainDemoDbContext(optionsBuilder.Options);
    }
}
[TypesInfoInitializer(typeof(EFDemoDbContextInitializer))]
public class MainDemoDbContext : DbContext {
    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues);

        modelBuilder.Entity<MediaDataObject>().HasOne(md => md.MediaResource).WithOne().HasForeignKey<MediaResourceObject>(p => p.ID);

        modelBuilder.Entity<AuditEFCoreWeakReference>()
            .HasMany(p => p.AuditItems)
            .WithOne(p => p.AuditedObject);
        modelBuilder.Entity<AuditEFCoreWeakReference>()
            .HasMany(p => p.OldItems)
            .WithOne(p => p.OldObject);
        modelBuilder.Entity<AuditEFCoreWeakReference>()
            .HasMany(p => p.NewItems)
            .WithOne(p => p.NewObject);
        modelBuilder.Entity<AuditEFCoreWeakReference>()
            .HasMany(p => p.UserItems)
            .WithOne(p => p.UserObject);

        modelBuilder.Entity<Employee>()
            .HasOne(r => r.Location)
            .WithOne(p => p.Employee)
            .HasForeignKey<Location>(fk => fk.EmployeeRef);
        modelBuilder.Entity<Department>()
            .HasMany(p => p.Employees)
            .WithOne(r => r.Department);
        modelBuilder.Entity<Department>()
            .HasOne(r => r.DepartmentHead);
        modelBuilder.Entity<Task>()
            .HasOne(task => task.AssignedTo)
            .WithMany()
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<ApplicationUserLoginInfo>(b => {
            b.HasIndex(nameof(ISecurityUserLoginInfo.LoginProviderName), nameof(ISecurityUserLoginInfo.ProviderUserKey)).IsUnique();
        });
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        base.OnConfiguring(optionsBuilder);
    }
    public MainDemoDbContext(DbContextOptions<MainDemoDbContext> options)
        : base(options) {
    }

    public DbSet<Address> Addresses { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<Paycheck> Paycheck { get; set; }
    public DbSet<PortfolioFileData> FileAttachments { get; set; }
    public DbSet<FileData> FileData { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Country> Countries { get; set; }
    public DbSet<ReportDataV2> ReportData { get; set; }
    public DbSet<Analysis> Analysis { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<PhoneNumber> PhoneNumbers { get; set; }
    public DbSet<Position> Positions { get; set; }
    public DbSet<Resume> Resumes { get; set; }
    public DbSet<Note> Notes { get; set; }
    public DbSet<Task> Tasks { get; set; }


    #region Default XAF Configurations
    public DbSet<ApplicationUser> Users { get; set; }
    public DbSet<ApplicationUserLoginInfo> UserLoginInfos { get; set; }
    public DbSet<ModelDifference> ModelDifferences { get; set; }
    public DbSet<ModelDifferenceAspect> ModelDifferenceAspects { get; set; }
    public DbSet<PermissionPolicyRole> Roles { get; set; }
    public DbSet<ModuleInfo> ModulesInfo { get; set; }
    public DbSet<AuditDataItemPersistent> AuditData { get; set; }
    public DbSet<AuditEFCoreWeakReference> AuditEFCoreWeakReference { get; set; }
    #endregion
}

public class AuditingDbContext : DbContext {
    public AuditingDbContext(DbContextOptions<AuditingDbContext> options)
        : base(options) {
    }

    public DbSet<AuditDataItemPersistent> AuditData { get; set; }
    public DbSet<AuditEFCoreWeakReference> AuditEFCoreWeakReference { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues);
        modelBuilder.Entity<AuditEFCoreWeakReference>()
            .HasMany(p => p.AuditItems)
            .WithOne(p => p.AuditedObject);
        modelBuilder.Entity<AuditEFCoreWeakReference>()
            .HasMany(p => p.OldItems)
            .WithOne(p => p.OldObject);
        modelBuilder.Entity<AuditEFCoreWeakReference>()
            .HasMany(p => p.NewItems)
            .WithOne(p => p.NewObject);
        modelBuilder.Entity<AuditEFCoreWeakReference>()
            .HasMany(p => p.UserItems)
            .WithOne(p => p.UserObject);
    }
}
