using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations;
using Oqtane.Migrations.EntityBuilders;

namespace MountainStates.MSSA.Module.TopDogs.Migrations.EntityBuilders
{
    public class TopDogsEntityBuilder : AuditableBaseEntityBuilder<TopDogsEntityBuilder>
    {
        private const string _entityTableName = "MountainStates.MSSATopDogs";
        private readonly PrimaryKey<TopDogsEntityBuilder> _primaryKey = new("PK_MountainStates.MSSATopDogs", x => x.TopDogsId);
        private readonly ForeignKey<TopDogsEntityBuilder> _moduleForeignKey = new("FK_MountainStates.MSSATopDogs_Module", x => x.ModuleId, "Module", "ModuleId", ReferentialAction.Cascade);

        public TopDogsEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
            ForeignKeys.Add(_moduleForeignKey);
        }

        protected override TopDogsEntityBuilder BuildTable(ColumnsBuilder table)
        {
            TopDogsId = AddAutoIncrementColumn(table,"TopDogsId");
            ModuleId = AddIntegerColumn(table,"ModuleId");
            Name = AddMaxStringColumn(table,"Name");
            AddAuditableColumns(table);
            return this;
        }

        public OperationBuilder<AddColumnOperation> TopDogsId { get; set; }
        public OperationBuilder<AddColumnOperation> ModuleId { get; set; }
        public OperationBuilder<AddColumnOperation> Name { get; set; }
    }
}
