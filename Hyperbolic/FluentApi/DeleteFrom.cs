using Microsoft.FSharp.Core;
using System;
using System.Linq.Expressions;
using static Hyperboliq.Domain.Stream;
using static Hyperboliq.Domain.ExpressionParts;
using static Hyperboliq.Domain.Types;
using Hyperboliq.Domain;

namespace Hyperboliq.FluentApi
{
    public class DeleteFrom<TTable> : ISqlExpressionTransformable, ISqlTransformable
    {
        private DeleteExpression expr;
        public DeleteFrom()
        {
            var from = AddFromTable(NewFromExpression(), TableReferenceFromType<TTable>());
            this.expr = new DeleteExpression(from, FSharpOption<WhereExpressionNode>.None);
        }

        public DeleteWhere Where<TTableType>(Expression<Func<TTableType, bool>> predicate) =>
            new DeleteWhere(expr).And(predicate);

        public DeleteWhere Where<TTable1, TTable2>(Expression<Func<TTable1, TTable2, bool>> predicate) =>
            new DeleteWhere(expr).And(predicate);


        public SqlExpression ToSqlExpression() => SqlExpression.NewDelete(expr);

        public string ToSql(ISqlDialect dialect) => SqlGenerator.SqlifyExpression(dialect, ToSqlExpression());
    }

}
