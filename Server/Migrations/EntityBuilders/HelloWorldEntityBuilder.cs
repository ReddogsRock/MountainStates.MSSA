using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations;
using Oqtane.Migrations.EntityBuilders;

namespace MountainStates.MSSA.Module.HelloWorld.Migrations.EntityBuilders
{
    public class HelloWorldEntityBuilder : AuditableBaseEntityBuilder<HelloWorldEntityBuilder>
    {
        private const string _entityTableName = "MountainStates.MSSAHelloWorld";
        private readonly PrimaryKey<HelloWorldEntityBuilder> _primaryKey = new("PK_MountainStates.MSSAHelloWorld", x => x.HelloWorldId);
        private readonly ForeignKey<HelloWorldEntityBuilder> _moduleForeignKey = new("FK_MountainStates.MSSAHelloWorld_Module", x => x.ModuleId, "Module", "ModuleId", ReferentialAction.Cascade);

        public HelloWorldEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
            ForeignKeys.Add(_moduleForeignKey);
        }

        protected override HelloWorldEntityBuilder BuildTable(ColumnsBuilder table)
        {
            HelloWorldId = AddAutoIncrementColumn(table,"HelloWorldId");
            ModuleId = AddIntegerColumn(table,"ModuleId");
            Name = AddMaxStringColumn(table,"Name");
            AddAuditableColumns(table);
            return this;
        }

        public OperationBuilder<AddColumnOperation> HelloWorldId { get; set; }
        public OperationBuilder<AddColumnOperation> ModuleId { get; set; }
        public OperationBuilder<AddColumnOperation> Name { get; set; }
    }
}
