using Microsoft.FSharp.Collections;
using static Hyperboliq.Domain.Stream;
using static Hyperboliq.Domain.Types;

namespace Hyperboliq.FluentApi
{
    public class InsertValues<TTable> : ISqlStatement, ISqlTransformable
    {
        private readonly InsertExpression<TTable> expr;

        internal InsertValues(InsertExpression<TTable> expr)
        {
            this.expr = expr;
        }

        public InsertValues<TTable> Value(TTable instance)
        {
            expr.Value(instance);
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

        public string ToSql(ISqlDialect dialect)
        {
            return expr.ToSql(dialect);
        }

        public FSharpList<SqlNode> ToSqlStream()
        {
            return expr.ToSqlStream();
        }
    }
}
