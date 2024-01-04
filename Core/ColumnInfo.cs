﻿using System;
using System.Linq;
using System.Reflection;

namespace PetaPoco
{
    /// <summary>
    /// The ColumnInfo class stores information about a column in the database.
    /// </summary>
    /// <remarks>
    /// Typically ColumnInfo is automatically populated from the attributes on a POCO object and its properties. It can, however, also be
    /// returned from the <see cref="IMapper"/> interface allowing you to provide your own custom bindings between the DB and your POCOs.
    /// </remarks>
    /// <seealso cref="ColumnAttribute"/>
    /// <seealso cref="Core.PocoColumn"/>
    public class ColumnInfo
    {
        /// <summary>
        /// Gets or sets the database column name this property maps to.
        /// </summary>
        /// <value>When not <see langword="null"/>, overrides this property's inflected column name from the mapper.</value>
        /// <seealso cref="ColumnAttribute.Name"/>
        public string ColumnName { get; set; }

        /// <summary>
        /// Gets or sets whether this column represents a property that should be updated in queries that include a user-supplied
        /// <c>SELECT</c> statement, but ignored in queries generated by auto-select. Result columns are always ignored in <c>UPDATE</c> and
        /// <c>INSERT</c> operations.
        /// </summary>
        /// <value>If <see langword="true"/>, this property will be updated in SQL query operations containing a user-supplied <c>SELECT</c>
        /// statement, and ignored for all other database operations.</value>
        /// <seealso cref="ResultColumnAttribute"/>
        public bool ResultColumn { get; set; }

        /// <summary>
        /// Gets or sets whether this serves as a ResultColumn that is included with auto-select queries as well as queries containing
        /// user-supplied <c>SELECT</c> statements.
        /// </summary>
        /// <value>If <see langword="true"/>, this property will be updated in all SQL queries, but ignored for all other database
        /// operations such as INSERT and UPDATE.</value>
        /// <seealso cref="ResultColumnAttribute.IncludeInAutoSelect"/>
        public bool AutoSelectedResultColumn { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the column's data type should be treated as <see cref="System.Data.DbType.AnsiString"/>.
        /// </summary>
        /// <remarks>
        /// For use with <see cref="string"/> properties. This property is implicitly <see langword="true"/> for properties of type <see
        /// cref="AnsiString"/>.
        /// </remarks>
        /// <value>If <see langword="true"/>, the column's data type is assumed to be <see
        /// cref="System.Data.DbType.AnsiString">DbType.AnsiString</see> (equivalent to the DB data type <c>VARCHAR</c>).</value>
        /// <seealso cref="AnsiString"/>
        /// <seealso cref="ColumnAttribute.ForceToAnsiString"/>
        public bool ForceToAnsiString { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the column containing a DateTime data type should be treated as <see
        /// cref="System.Data.DbType.DateTime">DbType.DateTime</see>.
        /// </summary>
        /// <remarks>
        /// For use with <see cref="DateTime"/> properties. This property is implicitly <see langword="true"/> for properties of type <see
        /// cref="DateTime2"/>.
        /// </remarks>
        /// <value>If <see langword="true"/>, the column's data type is assumed to be <see
        /// cref="System.Data.DbType.DateTime">DbType.DateTime</see>.</value>
        /// <seealso cref="DateTime2"/>
        /// <seealso cref="ColumnAttribute.ForceToDateTime2"/>
        public bool ForceToDateTime2 { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the DateTime value of this column should be treated as UTC.
        /// </summary>
        /// <remarks>
        /// For use with <see cref="DateTime"/> or <see cref="DateTime2"/> properties. No conversion is applied - the <see
        /// cref="DateTimeKind"/> of the POCO property's underlying <see cref="DateTime"/> value is simply set to correctly reflect the UTC
        /// timezone as an invariant.
        /// </remarks>
        /// <value>If <see langword="true"/>, the DateTime value is assumed to be in UTC. If <see langword="false"/>, the <see
        /// cref="DateTime.Kind"/> property will be assigned <see cref="DateTimeKind.Unspecified"/>.</value>
        /// <seealso cref="DateTime2"/>
        /// <seealso cref="ColumnAttribute.ForceToUtc"/>
        public bool ForceToUtc { get; set; }

        /// <summary>
        /// Gets or sets the template used for <c>INSERT</c> operations.
        /// </summary>
        /// <remarks>
        /// When set, this template is used for generating the <c>INSERT</c> portion of the SQL statement instead of the default
        /// <br/><c>String.Format("{0}{1}", paramPrefix, index)</c>.
        /// <para>Setting this allows database-related interactions, such as:
        /// <br/><c>String.Format("CAST({0}{1} AS JSON)", paramPrefix, index)</c>.</para>
        /// </remarks>
        /// <seealso cref="ColumnAttribute.InsertTemplate"/>
        public string InsertTemplate { get; set; }

        /// <summary>
        /// Gets or sets the template used for <c>UPDATE</c> operations.
        /// </summary>
        /// <remarks>
        /// When set, this template is used for generating the <c>UPDATE</c> portion of the SQL statement instead of the default
        /// <br/><c>String.Format("{0} = {1}{2}", colName, paramPrefix, index)</c>.
        /// <para>Setting this allows database-related interactions, such as:
        /// <br/><c>String.Format("{0} = CAST({1}{2} AS JSON)", colName, paramPrefix, index)</c></para>
        /// </remarks>
        /// <seealso cref="ColumnAttribute.UpdateTemplate"/>
        public string UpdateTemplate { get; set; }

        /// <summary>
        /// Creates and populates a ColumnInfo from the attributes of a POCO property.
        /// </summary>
        /// <param name="propertyInfo">The POCO property to use for initializing the ColumnInfo.</param>
        /// <returns>A ColumnInfo instance.</returns>
        public static ColumnInfo FromProperty(PropertyInfo propertyInfo)
        {
            var ci = new ColumnInfo();
            PopulateFromProperty(propertyInfo, ref ci, out _);
            return ci;
        }

        internal static void PopulateFromProperty(PropertyInfo pi, ref ColumnInfo ci, out ColumnAttribute columnAttr)
        {
            // Check if declaring poco has [Explicit] attribute
            var isExplicit = pi.DeclaringType.GetCustomAttributes(typeof(ExplicitColumnsAttribute), true).Any();

            // Check for [Column]/[Ignore] Attributes
            columnAttr = Attribute.GetCustomAttributes(pi, typeof(ColumnAttribute)).FirstOrDefault() as ColumnAttribute;
            var isIgnore = Attribute.GetCustomAttributes(pi, typeof(IgnoreAttribute)).Any();

            if (isIgnore || (isExplicit && columnAttr == null))
            {
                ci = null;
            }
            else
            {
                ci = ci ?? new ColumnInfo();

                ci.ColumnName = columnAttr?.Name ?? pi.Name;
                ci.ForceToAnsiString = columnAttr?.ForceToAnsiString == true;
                ci.ForceToDateTime2 = columnAttr?.ForceToDateTime2 == true;
                ci.ForceToUtc = columnAttr?.ForceToUtc == true;
                ci.InsertTemplate = columnAttr?.InsertTemplate;
                ci.UpdateTemplate = columnAttr?.UpdateTemplate;

                if (columnAttr is ResultColumnAttribute resAttr)
                {
                    ci.ResultColumn = true;
                    ci.AutoSelectedResultColumn = resAttr.IncludeInAutoSelect == IncludeInAutoSelect.Yes;
                }
            }
        }
    }
}
