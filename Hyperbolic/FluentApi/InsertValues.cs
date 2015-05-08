using static Hyperboliq.Domain.Stream;
using static Hyperboliq.Domain.Types;
using static Hyperboliq.Domain.SqlGenerator;
using static Hyperboliq.Domain.InsertExpressionPart;

namespace Hyperboliq.FluentApi
{
    public class InsertValues<TTable> : ISqlStatement, ISqlTransformable
    {
        private InsertExpression expr;

        internal InsertValues(InsertExpression expr)
        {
            this.expr = expr;
        }

        public InsertValues<TTable> Value(TTable instance)
        {
            expr =
                WithValues(
                    expr,
                    AddInsertValue(expr.InsertInto, expr.InsertValues, instance));
            return this;
        }

        public InsertValues<TTable> Values(params TTable[] instances)
        {
            foreach (var instance in instances)
            {
                Value(instance);
            }
            return this;
        }

        public SqlExpression ToSqlExpression() => SqlExpression.NewInsert(expr);

        public string ToSql(ISqlDialect dialect) => SqlifyExpression(dialect, ToSqlExpression());

    }
}
