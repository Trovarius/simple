using System;
using System.Data;
using Simple.Migrator.Framework;

namespace Simple.Migrator.Providers.SqlServer
{
    /// <summary>
    /// Migration transformations provider for Microsoft SQL Server.
    /// </summary>
    public class SqlServerCeTransformationProvider : SqlServerTransformationProvider
    {
        public SqlServerCeTransformationProvider(Dialect dialect, string invariantProvider, string connectionString)
            : base(dialect, invariantProvider, connectionString)
        { }

       
        public override bool ConstraintExists(string table, string name)
        {
            using (IDataReader reader =
                ExecuteQuery(string.Format("SELECT cont.constraint_name FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS cont WHERE cont.Constraint_Name='{0}'", name)))
            {
                return reader.Read();
            }
        }

        public override void RenameColumn(string tableName, string oldColumnName, string newColumnName)
        {
            if (ColumnExists(tableName, newColumnName))
                throw new MigrationException(String.Format("Table '{0}' has column named '{1}' already", tableName, newColumnName));

            if (ColumnExists(tableName, oldColumnName))
            {
                Column column = GetColumnByName(tableName, oldColumnName);

                AddColumn(tableName, new Column(newColumnName, column.Type, column.ColumnProperty, column.DefaultValue));
                ExecuteNonQuery(string.Format("UPDATE {0} SET {1}={2}", tableName, newColumnName, oldColumnName));
                RemoveColumn(tableName, oldColumnName);
            }
        }

        // Not supported by SQLCe when we have a better schemadumper which gives the exact sql construction including constraints we may use it to insert into a new table and then drop the old table...but this solution is dangerous for big tables.
        public override void RenameTable(string oldName, string newName)
        {

            if (TableExists(newName))
                throw new MigrationException(String.Format("Table with name '{0}' already exists", newName));

            //if (TableExists(oldName))
            //    ExecuteNonQuery(String.Format("EXEC sp_rename {0}, {1}", oldName, newName));
        }

        protected override string FindConstraints(string table, string column)
        {
            return
                string.Format("SELECT cont.constraint_name FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE cont "
                    + "WHERE cont.Table_Name='{0}' AND cont.column_name = '{1}'",
                    table, column);
        }
    }
}
