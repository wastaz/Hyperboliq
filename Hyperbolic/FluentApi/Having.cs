using System;
using System.Linq.Expressions;
using Microsoft.FSharp.Collections;
using static Hyperboliq.Domain.Stream;
using static Hyperboliq.Domain.Types;
using static Hyperboliq.Domain.ExpressionParts;
using static Hyperboliq.Domain.SelectExpressionParts;
using Hyperboliq.Domain;

namespace Hyperboliq
{
    public class Having : ISqlExpressionTransformable, ISqlTransformable
    {
        private SelectExpression expr;

        internal Having(SelectExpression expr)
        {
            this.expr = expr;
        }

        public Having And<TTableType>(Expression<Func<TTableType, bool>> predicate)
        {
            expr =
                WithGroupClause(
                    expr,
                    AddHavingAndClause(expr.GroupBy, predicate, TableReferenceFromType<TTableType>()));
            return this;
        }

        public Having And<TTable1, TTable2>(Expression<Func<TTable1, TTable2, bool>> predicate)
        {
            expr =
                WithGroupClause(
                    expr,
                    AddHavingAndClause(expr.GroupBy, predicate, TableReferenceFromType<TTable1>(), TableReferenceFromType<TTable2>()));
            return this;
        }

        public Having Or<TTableType>(Expression<Func<TTableType, bool>> predicate)
        {
            expr =
                WithGroupClause(
                    expr,
                    AddHavingOrClause(expr.GroupBy, predicate, TableReferenceFromType<TTableType>()));
            return this;
        }

        public Having Or<TTable1, TTable2>(Expression<Func<TTable1, TTable2, bool>> predicate)
        {
            expr =
                WithGroupClause(
                    expr,
                    AddHavingOrClause(expr.GroupBy, predicate, TableReferenceFromType<TTable1>(), TableReferenceFromType<TTable2>()));
            return this;
        }

        public SqlExpression ToSqlExpression() => SqlExpression.NewSelect(expr);

        public string ToSql(ISqlDialect dialect) => SqlGen.SqlifyExpression(dialect, ToSqlExpression());
    }

}
