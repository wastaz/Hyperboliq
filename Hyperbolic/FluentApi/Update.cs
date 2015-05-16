using System;
using System.Linq.Expressions;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;
using static Hyperboliq.Domain.Stream;
using static Hyperboliq.Domain.Types;
using static Hyperboliq.Domain.ExpressionParts;
using static Hyperboliq.Domain.SqlGen;
using static Hyperboliq.Domain.UpdateExpressionPart;
using Hyperboliq.Domain;

namespace Hyperboliq.FluentApi
{

    public class UpdateSet<TTable> : ISqlExpressionTransformable, ISqlTransformable
    {
        private UpdateExpression expr;

        internal UpdateSet() 
        {
            expr = 
                new UpdateExpression(
                    NewUpdateHead(TableReferenceFromType<TTable>()), 
                    FSharpOption<WhereExpressionNode>.None);
        }

        public UpdateSet<TTable> Set<TObj>(Expression<Func<TTable, TObj>> selector,
                                           TObj value)
        {
            expr =
                WithHead(
                    expr,
                    AddObjectSetExpression(expr.UpdateSet, selector, value));
            return this;
        }

        public UpdateSet<TTable> Set<TObj>(Expression<Func<TTable, TObj>> selector,
                                           Expression<Func<TTable, TObj>> valueUpdate)
        {
            expr =
                WithHead(
                    expr,
                    AddValueExpression(expr.UpdateSet, selector, valueUpdate));
            return this;
        }

        public UpdateSet<TTable> Set(Expression<Func<TTable, object>> selector, SelectExpression selectExpr)
        {
            expr =
                WithHead(
                    expr,
                    AddSingleValueSetExpression(expr.UpdateSet, selector, selectExpr));
            return this;
        }

        public UpdateWhere<TTable> Where(Expression<Func<TTable, bool>> predicate) =>
            new UpdateWhere<TTable>(expr).And(predicate);

        public SqlExpression ToSqlExpression() => SqlExpression.NewUpdate(expr);

        public string ToSql(ISqlDialect dialect) => SqlGen.SqlifyExpression(dialect, ToSqlExpression());
    }

    public class UpdateWhere<TTable> : ISqlExpressionTransformable, ISqlTransformable
    {
        private UpdateExpression expr;

        internal UpdateWhere(UpdateExpression expr)
        {
            this.expr = expr;
        }

        public UpdateWhere<TTable> And(Expression<Func<TTable, bool>> predicate)
        {

            expr =
                WithWhereClause(
                    expr,
                    AddOrCreateWhereAndClause(expr.Where, predicate, TableReferenceFromType<TTable>()));
            return this;
        }
            

        public UpdateWhere<TTable> Or(Expression<Func<TTable, bool>> predicate)
        {
            expr =
                WithWhereClause(
                    expr,
                    AddOrCreateWhereOrClause(expr.Where, predicate, TableReferenceFromType<TTable>()));
            return this;
        }

        public SqlExpression ToSqlExpression() => SqlExpression.NewUpdate(expr);

        public string ToSql(ISqlDialect dialect) => SqlGen.SqlifyExpression(dialect, ToSqlExpression());
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
