using System.Linq;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace KnowledgeBank.Multitenant
{
	public class TenantSqlServerMigrationSqlGenerator : SqlServerMigrationsSqlGenerator
	{

		public TenantSqlServerMigrationSqlGenerator(IRelationalCommandBuilderFactory commandBuilderFactory,
			ISqlGenerationHelper sqlGenerationHelper,
			IRelationalTypeMapper typeMapper,
			IRelationalAnnotationProvider annotations,
			IMigrationsAnnotationProvider migrationsAnnotations) : base(commandBuilderFactory, sqlGenerationHelper, typeMapper, annotations, migrationsAnnotations)
		{
		}

		protected virtual void AddPolicy(string schema, string table, MigrationCommandListBuilder builder)
		{
			if (string.IsNullOrWhiteSpace(schema))
				schema = "dbo";

			builder.AppendLines(
$@"ALTER SECURITY POLICY rls.tenantAccessPolicy
	ADD FILTER PREDICATE rls.fn_tenantAccessPredicate(TenantId) ON {schema}.{table},
	ADD BLOCK PREDICATE rls.fn_tenantAccessPredicate(TenantId) ON {schema}.{table}");
			builder.EndCommand(false);
		}

		protected virtual void DropPolicy(string schema, string table, MigrationCommandListBuilder builder)
		{
			if (string.IsNullOrWhiteSpace(schema))
				schema = "dbo";

			builder.AppendLines(
$@"ALTER SECURITY POLICY rls.tenantAccessPolicy
	DROP FILTER PREDICATE ON {schema}.{table},
	DROP BLOCK PREDICATE ON {schema}.{table}");
			builder.EndCommand(false);
		}

		protected override void Generate(CreateTableOperation operation, IModel model, MigrationCommandListBuilder builder)
		{
			System.Console.WriteLine("CREATE TABLE");

			base.Generate(operation, model, builder);
			if (!operation.Columns.Any(x => x.Name == "TenantId"))
				return;
			AddPolicy(operation.Schema, operation.Name, builder);
		}

		protected override void Generate(RenameTableOperation operation, IModel model, MigrationCommandListBuilder builder)
		{
			System.Console.WriteLine("RENAME TABLE DROP");
			DropPolicy(operation.Schema, operation.Name, builder);

			base.Generate(operation, model, builder);

			System.Console.WriteLine("RENAME TABLE ADD");
			AddPolicy(operation.NewSchema, operation.NewName, builder);
		}

		protected override void Generate(DropTableOperation operation, IModel model, MigrationCommandListBuilder builder)
		{
			System.Console.WriteLine("DROP TABLE");
			DropPolicy(operation.Schema, operation.Name, builder); ;

			base.Generate(operation, model, builder);
		}

		protected override void Generate(AddColumnOperation operation, IModel model, MigrationCommandListBuilder builder)
		{
			base.Generate(operation, model, builder);
			if (operation.Name != "TenantId")
				return;
			System.Console.WriteLine("ADD COLUMN");
			AddPolicy(operation.Schema, operation.Table, builder);
		}


		protected override void Generate(RenameColumnOperation operation, IModel model, MigrationCommandListBuilder builder)
		{
			System.Console.WriteLine("ADD COLUMN DROP");
			if (operation.Name == "TenantId")
				DropPolicy(operation.Schema, operation.Table, builder);

			base.Generate(operation, model, builder);

			System.Console.WriteLine("ADD COLUMN ADD");
			if (operation.NewName == "TenantId")
				AddPolicy(operation.Schema, operation.Table, builder);
		}

		protected override void Generate(DropColumnOperation operation, IModel model, MigrationCommandListBuilder builder)
		{
			System.Console.WriteLine("DROP COLUMN");
			if (operation.Name == "TenantId")
				DropPolicy(operation.Schema, operation.Table, builder);
			base.Generate(operation, model, builder);
		}

	}
}