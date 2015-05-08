using Microsoft.FSharp.Collections;
using System;
using System.Linq.Expressions;
using static Hyperboliq.Domain.Stream;
using static Hyperboliq.Domain.Types;
using static Hyperboliq.Domain.ExpressionParts;
using static Hyperboliq.Domain.SqlGenerator;
using static Hyperboliq.Domain.DeleteExpressionParts;
using Hyperboliq.Domain;

namespace Hyperboliq.FluentApi
{
    public class DeleteWhere : ISqlExpressionTransformable, ISqlTransformable
    {
        private DeleteExpression expr;
        public DeleteWhere(DeleteExpression expr)
        {
            this.expr = expr;
        }

        public DeleteWhere And<TTableType>(Expression<Func<TTableType, bool>> predicate)
        {
            expr =
                WithWhereClause(
                    expr,
                    AddOrCreateWhereAndClause(expr.Where, predicate, TableReferenceFromType<TTableType>()));
            return this;
        }

        public DeleteWhere And<TTable1, TTable2>(Expression<Func<TTable1, TTable2, bool>> predicate)
        {
            expr =
                WithWhereClause(
                    expr,
                    AddOrCreateWhereAndClause(expr.Where, predicate, TableReferenceFromType<TTable1>(), TableReferenceFromType<TTable2>()));
            return this;
        }

        public DeleteWhere Or<TTableType>(Expression<Func<TTableType, bool>> predicate)
        {
            expr =
                WithWhereClause(
                    expr,
                    AddOrCreateWhereOrClause(expr.Where, predicate, TableReferenceFromType<TTableType>()));
            return this;
        }

        public DeleteWhere Or<TTable1, TTable2>(Expression<Func<TTable1, TTable2, bool>> predicate)
        {
            expr =
                WithWhereClause(
                    expr,
                    AddOrCreateWhereOrClause(expr.Where, predicate, TableReferenceFromType<TTable1>(), TableReferenceFromType<TTable2>()));
            return this;
        }
        public SqlExpression ToSqlExpression() => SqlExpression.NewDelete(expr);

        public string ToSql(ISqlDialect dialect) => SqlGenerator.SqlifyExpression(dialect, ToSqlExpression());
    }
}
