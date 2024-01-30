using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

using System.Threading;

using PetaPoco.Core;
using PetaPoco.Utilities;

namespace PetaPoco.Providers
{
    // TODO: Rename class: SqlServerMSDataDatabaseProvider

    /// <summary>
    /// Provides an implementation of <see cref="DatabaseProvider"/> for Microsoft SQL Server databases.
    /// </summary>
    /// <remarks>
    /// This provider uses the "Microsoft.Data.SqlClient" ADO.NET driver for data access.
    /// </remarks>
    public class SqlServerMsDataDatabaseProvider : SqlServerDatabaseProvider
    {
        /// <inheritdoc/>
        public override DbProviderFactory GetFactory()
            => GetFactory("Microsoft.Data.SqlClient.SqlClientFactory, Microsoft.Data.SqlClient");


        /// <inheritdoc/>
        public override string BuildPageQuery(long skip, long take, SQLParts parts, ref object[] args)
        {
            var helper = (PagingHelper)PagingUtility;
            // when the query does not contain an "order by", it is very slow
            if (helper.SimpleRegexOrderBy.IsMatch(parts.SqlSelectRemoved))
            {
                var m = helper.SimpleRegexOrderBy.Match(parts.SqlSelectRemoved);
                if (m.Success)
                {
                    var g = m.Groups[0];
                    parts.SqlSelectRemoved = parts.SqlSelectRemoved.Substring(0, g.Index);
                }
            }

            if (helper.RegexDistinct.IsMatch(parts.SqlSelectRemoved))
                parts.SqlSelectRemoved = "peta_inner.* FROM (SELECT " + parts.SqlSelectRemoved + ") peta_inner";

            var sqlPage =
                $"SELECT * FROM (SELECT ROW_NUMBER() OVER ({parts.SqlOrderBy ?? "ORDER BY (SELECT NULL)"}) peta_rn, {parts.SqlSelectRemoved}) peta_paged WHERE peta_rn > @{args.Length} AND peta_rn <= @{args.Length + 1} order by peta_rn";
            args = args.Concat(new object[] { skip, skip + take }).ToArray();
            return sqlPage;
        }

        /// <inheritdoc/>
        public override object ExecuteInsert(Database db, IDbCommand cmd, string primaryKeyName)
            => ExecuteScalarHelper(db, cmd);

#if ASYNC
        /// <inheritdoc/>
        public override Task<object> ExecuteInsertAsync(CancellationToken cancellationToken, Database db, IDbCommand cmd, string primaryKeyName)
            => ExecuteScalarHelperAsync(cancellationToken, db, cmd);
#endif

        /// <inheritdoc/>
        public override string GetExistsSql()
            => "IF EXISTS (SELECT 1 FROM {0} WHERE {1}) SELECT 1 ELSE SELECT 0";

        /// <inheritdoc/>
        public override string GetInsertOutputClause(string primaryKeyName)
            => $" OUTPUT INSERTED.[{primaryKeyName}]";
    }
}
