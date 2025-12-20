using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Oqtane.Modules;
using Oqtane.Repository;
using Oqtane.Infrastructure;
using Oqtane.Repository.Databases.Interfaces;

namespace MountainStates.MSSA.Module.HelloWorld.Repository
{
    public class HelloWorldContext : DBContextBase, ITransientService, IMultiDatabase
    {
        public virtual DbSet<Models.HelloWorld> HelloWorld { get; set; }

        public HelloWorldContext(IDBContextDependencies DBContextDependencies) : base(DBContextDependencies)
        {
            // ContextBase handles multi-tenant database connections
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Models.HelloWorld>().ToTable(ActiveDatabase.RewriteName("MountainStates.MSSAHelloWorld"));
        }
    }
}
