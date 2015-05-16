using Microsoft.FSharp.Collections;
using System;
using System.Linq.Expressions;
using static Hyperboliq.Domain.Stream;
using static Hyperboliq.Domain.Types;
using static Hyperboliq.Domain.ExpressionParts;
using static Hyperboliq.Domain.SelectExpressionParts;
using Hyperboliq.Domain;

namespace Hyperboliq
{
    public class SelectWhere : ISqlQuery, ISqlExpressionTransformable, ISelectExpressionTransformable
    {
        private SelectExpression expr;
        internal SelectWhere(SelectExpression expr) { this.expr = expr; }

        public SelectWhere And<TTableType>(Expression<Func<TTableType, bool>> predicate)
        {

            expr =
                WithWhereClause(
                    expr,
                    AddOrCreateWhereAndClause(expr.Where, predicate, TableReferenceFromType<TTableType>()));
            return this;
        }

        public SelectWhere And<TTable1, TTable2>(Expression<Func<TTable1, TTable2, bool>> predicate)
        {
            expr =
                WithWhereClause(
                    expr,
                    AddOrCreateWhereAndClause(expr.Where, predicate, TableReferenceFromType<TTable1>(), TableReferenceFromType<TTable2>()));
            return this;
        }

        public SelectWhere Or<TTableType>(Expression<Func<TTableType, bool>> predicate)
        {
            expr =
                WithWhereClause(
                    expr,
                    AddOrCreateWhereOrClause(expr.Where, predicate, TableReferenceFromType<TTableType>()));
            return this;
        }

        public SelectWhere Or<TTable1, TTable2>(Expression<Func<TTable1, TTable2, bool>> predicate)
        {
            expr =
                WithWhereClause(
                    expr,
                    AddOrCreateWhereOrClause(expr.Where, predicate, TableReferenceFromType<TTable1>(), TableReferenceFromType<TTable2>()));
            return this;
        }

        public OrderBy OrderBy<TTableType>(Expression<Func<TTableType, object>> orderExpr, Direction direction = null, NullsOrdering nullsOrdering = null)
        {
            return new OrderBy(expr).ThenBy(orderExpr, direction, nullsOrdering);
        }

        public SelectExpression ToSelectExpression() => expr;

        public SqlExpression ToSqlExpression() => SqlExpression.NewSelect(expr);

        public string ToSql(ISqlDialect dialect) => SqlGen.SqlifyExpression(dialect, ToSqlExpression());
    }
}
