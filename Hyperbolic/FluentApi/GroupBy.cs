using static Hyperboliq.Domain.Stream;
using System;
using System.Linq.Expressions;
using static Hyperboliq.Domain.Types;
using static Hyperboliq.Domain.ExpressionParts;
using static Hyperboliq.Domain.SelectExpressionParts;
using Hyperboliq.Domain;

namespace Hyperboliq
{
    public class GroupBy : ISqlExpressionTransformable, ISqlTransformable
    {
        private SelectExpression expr;
        internal GroupBy(SelectExpression expr) { this.expr = expr; }

        public GroupBy ThenBy<TTableType>(Expression<Func<TTableType, object>> groupByExpr)
        {
            expr =
                WithGroupClause(
                    expr,
                    AddOrCreateGroupByClause(expr.GroupBy, groupByExpr, TableReferenceFromType<TTableType>()));
            return this;
        }

        public Having Having<TTableType>(Expression<Func<TTableType, bool>> predicate)
        {
            return new Having(expr).And(predicate);
        }

        public Having Having<TTable1, TTable2>(Expression<Func<TTable1, TTable2, bool>> predicate)
        {
            return new Having(expr).And(predicate);
        }

        public SqlExpression ToSqlExpression() => SqlExpression.NewSelect(expr);

        public string ToSql(ISqlDialect dialect) => SqlGen.SqlifyExpression(dialect, ToSqlExpression());
    }
}
