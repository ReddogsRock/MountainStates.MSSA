using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations;
using MountainStates.MSSA.Module.HelloWorld.Migrations.EntityBuilders;
using MountainStates.MSSA.Module.HelloWorld.Repository;

namespace MountainStates.MSSA.Module.HelloWorld.Migrations
{
    [DbContext(typeof(HelloWorldContext))]
    [Migration("MountainStates.MSSA.Module.HelloWorld.01.00.00.00")]
    public class HelloWorldInitialize : MultiDatabaseMigration
    {
        public HelloWorldInitialize(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var entityBuilder = new HelloWorldEntityBuilder(migrationBuilder, ActiveDatabase);
            entityBuilder.Create();
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var entityBuilder = new HelloWorldEntityBuilder(migrationBuilder, ActiveDatabase);
            entityBuilder.Drop();
        }
    }
}
