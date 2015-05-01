using Microsoft.FSharp.Collections;
using System;
using System.Linq.Expressions;
using static Hyperboliq.Domain.Stream;

namespace Hyperboliq.FluentApi
{
    public class DeleteFrom<TTable> : ISqlStreamTransformable
    {
        private DeleteExpression expr;
        public DeleteFrom(DeleteExpression expr)
        {
            this.expr = expr;
            this.expr.From<TTable>();
        }

        public DeleteWhere Where<TTableType>(Expression<Func<TTableType, bool>> predicate)
        {
            expr.Where(predicate);
            return new DeleteWhere(expr);
        }

        public DeleteWhere Where<TTable1, TTable2>(Expression<Func<TTable1, TTable2, bool>> predicate)
        {
            expr.Where(predicate);
            return new DeleteWhere(expr);
        }

        public FSharpList<SqlNode> ToSqlStream() { return expr.ToSqlStream(); }
    }

}
