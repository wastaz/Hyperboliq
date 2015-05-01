using System;
using System.Linq.Expressions;
using Microsoft.FSharp.Collections;
using static Hyperboliq.Domain.Stream;

namespace Hyperboliq.FluentApi
{

    public class UpdateSet<TTable> : ISqlStreamTransformable {
        private readonly UpdateExpression<TTable> expr;

        internal UpdateSet() 
        {
            expr = new UpdateExpression<TTable>();
        }

        public UpdateSet<TTable> Set<TObj>(Expression<Func<TTable, TObj>> selector,
                                           TObj value)
        {
            expr.Set(selector, value);
            return this;
        }

        public UpdateSet<TTable> Set<TObj>(Expression<Func<TTable, TObj>> selector,
                                           Expression<Func<TTable, TObj>> valueUpdate)
        {
            expr.Set(selector, valueUpdate);
            return this;
        }

        public UpdateSet<TTable> Set(Expression<Func<TTable, object>> selector, SelectExpression selectExpr)
        {
            expr.Set(selector, selectExpr);
            return this;
        }

        public UpdateWhere<TTable> Where(Expression<Func<TTable, bool>> predicate)
        {
            expr.Where(predicate);
            return new UpdateWhere<TTable>(expr);
        }

        public FSharpList<SqlNode> ToSqlStream() => expr.ToSqlStream();
    }

    public class UpdateWhere<TTable> : ISqlStreamTransformable
    {
        private readonly UpdateExpression<TTable> expr;

        internal UpdateWhere(UpdateExpression<TTable> expr)
        {
            this.expr = expr;
        }

        public UpdateWhere<TTable> And(Expression<Func<TTable, bool>> predicate)
        {
            expr.And(predicate);
            return this;
        }
            

        public UpdateWhere<TTable> Or(Expression<Func<TTable, bool>> predicate)
        {
            expr.Or(predicate);
            return this;
        }

        public FSharpList<SqlNode> ToSqlStream() => expr.ToSqlStream();
    }

    public static class Update<TTable>
    {
        public static UpdateSet<TTable> Set<TObj>(Expression<Func<TTable, TObj>> selector, 
                                                  TObj value) 
            => new UpdateSet<TTable>().Set(selector, value);

        public static UpdateSet<TTable> Set<TObj>(Expression<Func<TTable, TObj>> selector, 
                                                  Expression<Func<TTable, TObj>> valueUpdate) 
            => new UpdateSet<TTable>().Set(selector, valueUpdate);

        public static UpdateSet<TTable> Set(Expression<Func<TTable, object>> selector, SelectExpression selectExpr) 
            => new UpdateSet<TTable>().Set(selector, selectExpr);
    }
}
