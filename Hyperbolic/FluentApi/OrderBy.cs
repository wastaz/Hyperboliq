using System;
using System.Linq.Expressions;
using static Hyperboliq.Domain.Stream;
using static Hyperboliq.Domain.Types;
using static Hyperboliq.Domain.ExpressionParts;
using static Hyperboliq.Domain.SelectExpressionParts;
using Hyperboliq.Domain;

namespace Hyperboliq
{
    public class OrderBy : ISqlExpressionTransformable, ISqlTransformable
    {
        private SelectExpression expr;

        internal OrderBy(SelectExpression expr) { this.expr = expr; }

        public OrderBy ThenBy<TTableType>(Expression<Func<TTableType, object>> orderExpr, Direction direction = null, NullsOrdering nullsOrdering = null)
        {
            expr =
                WithOrderClause(
                    expr,
                    AddOrCreateOrderingClause(
                        expr.OrderBy,
                        TableReferenceFromType<TTableType>(), 
                        direction ?? Direction.Ascending, 
                        nullsOrdering ?? NullsOrdering.NullsUndefined,
                        orderExpr));
            return this;
        }

        public SqlExpression ToSqlExpression() => SqlExpression.NewSelect(expr);

        public string ToSql(ISqlDialect dialect) => SqlGen.SqlifyExpression(dialect, ToSqlExpression());
    }
}
