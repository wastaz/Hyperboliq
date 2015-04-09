using System;
using System.Linq.Expressions;
using Microsoft.FSharp.Collections;
using static Hyperboliq.Domain.Types;
using static Hyperboliq.Domain.ExpressionParts;
using static Hyperboliq.Domain.Stream;
using static Hyperboliq.Domain.StreamGenerator;
using Hyperboliq.Domain;

namespace Hyperboliq
{
    public class DeleteExpression : ISqlStatement, ISqlStreamTransformable
    {
        internal FromExpression FromExpression { get; private set; } = NewFromExpression();
        internal WhereExpression WhereExpression { get; private set; } = NewWhereExpression();

        public DeleteExpression From<TTableType>()
        {
            FromExpression = AddFromTable(FromExpression, TableReferenceFromType<TTableType>());
            return this;
        }


        public DeleteExpression Where<TTableType>(Expression<Func<TTableType, bool>> predicate) => And(predicate);

        public DeleteExpression Where<TFirstTable, TSecondTable>(Expression<Func<TFirstTable, TSecondTable, bool>> predicate) => And(predicate);


        public DeleteExpression And<TTableType>(Expression<Func<TTableType, bool>> predicate)
        {
            WhereExpression = AddWhereAndClause(WhereExpression, predicate, TableReferenceFromType<TTableType>());
            return this;
        }

        public DeleteExpression And<TFirstTable, TSecondTable>(Expression<Func<TFirstTable, TSecondTable, bool>> predicate)
        {
            WhereExpression = AddWhereAndClause(WhereExpression, predicate, TableReferenceFromType<TFirstTable>(), TableReferenceFromType<TSecondTable>());
            return this;
        }

        public DeleteExpression Or<TTableType>(Expression<Func<TTableType, bool>> predicate)
        {
            WhereExpression = AddWhereOrClause(WhereExpression, predicate, TableReferenceFromType<TTableType>());
            return this;
        }

        public DeleteExpression Or<TFirstTable, TSecondTable>(Expression<Func<TFirstTable, TSecondTable, bool>> predicate)
        {
            WhereExpression = AddWhereOrClause(WhereExpression, predicate, TableReferenceFromType<TFirstTable>(), TableReferenceFromType<TSecondTable>());
            return this;
        }

        public FSharpList<SqlNode> ToSqlStream()
        {
            var stream = GenerateStream(
                new[] {
                    StreamInput.Delete,
                    StreamInput.NewFrom(FromExpression),
                    StreamInput.NewWhere(WhereExpression),
                });

            return stream;
        }

        public string ToSql(ISqlDialect dialect) => SqlGenerator.Sqlify(dialect, ToSqlStream());
    }
}
