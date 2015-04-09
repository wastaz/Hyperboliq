using Microsoft.FSharp.Collections;
using System;
using System.Linq.Expressions;
using static Hyperboliq.Domain.Stream;

namespace Hyperboliq.FluentApi
{
    public class DeleteWhere : ISqlStreamTransformable
    {
        private readonly DeleteExpression expr;
        public DeleteWhere(DeleteExpression expr)
        {
            this.expr = expr;
        }

        public DeleteWhere And<TTableType>(Expression<Func<TTableType, bool>> predicate)
        {
            expr.And(predicate);
            return this;
        }

        public DeleteWhere And<TTable1, TTable2>(Expression<Func<TTable1, TTable2, bool>> predicate)
        {
            expr.And(predicate);
            return this;
        }

        public DeleteWhere Or<TTableType>(Expression<Func<TTableType, bool>> predicate)
        {
            expr.Or(predicate);
            return this;
        }

        public DeleteWhere Or<TTable1, TTable2>(Expression<Func<TTable1, TTable2, bool>> predicate)
        {
            expr.Or(predicate);
            return this;
        }

        public FSharpList<SqlNode> ToSqlStream() { return expr.ToSqlStream(); }
    }
}
