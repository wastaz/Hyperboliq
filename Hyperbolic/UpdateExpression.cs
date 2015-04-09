using System;
using System.Linq.Expressions;
using static Hyperboliq.Domain.Types;
using static Hyperboliq.Domain.Stream;
using static Hyperboliq.Domain.UpdateExpressionPart;
using static Hyperboliq.Domain.ExpressionParts;
using static Hyperboliq.Domain.StreamGenerator;
using Microsoft.FSharp.Collections;

namespace Hyperboliq
{
    public class UpdateExpression<TTable> : ISqlStatement, ISqlStreamTransformable
    {
        private UpdateExpression Head { get; set; } = NewUpdateExpression(TableReferenceFromType<TTable>());
        private WhereExpression WhereExpression { get; set; } = NewWhereExpression();

        public UpdateExpression<TTable> Set<TObj>(Expression<Func<TTable, TObj>> selector, TObj value)
        {
            if(typeof(TObj).IsValueType || typeof(TObj) == typeof(string))
            {
                Head = AddSingleValueSetExpression(Head, selector, value);
            }
            else
            {
                Head = AddMultipleValueSetExpression(Head, selector, value);
            }
            
            return this;
        }

        public UpdateExpression<TTable> Set<TObj>(Expression<Func<TTable, TObj>> selector,
                                                  Expression<Func<TTable, TObj>> valueUpdate)
        {
            Head = AddValueExpression(Head, selector, valueUpdate);
            return this;
        }

        public UpdateExpression<TTable> Set(Expression<Func<TTable, object>> selector, SelectExpression selectExpr)
        {
            Head = AddSingleValueSetExpression(Head, selector, selectExpr);
            return this;
        }

        public UpdateExpression<TTable> Where(Expression<Func<TTable, bool>> predicate) => And(predicate);


        public UpdateExpression<TTable> And(Expression<Func<TTable, bool>> predicate)
        {
            WhereExpression = AddWhereAndClause(WhereExpression, predicate, TableReferenceFromType<TTable>());
            return this;
        }

        public UpdateExpression<TTable> Or(Expression<Func<TTable, bool>> predicate)
        {
            WhereExpression = AddWhereOrClause(WhereExpression, predicate, TableReferenceFromType<TTable>());
            return this;
        }

        public string ToSql(ISqlDialect value)
        {
            throw new NotImplementedException();
        }

        public FSharpList<SqlNode> ToSqlStream()
        {
            var stream = GenerateStream(
                new[] {
                    StreamInput.NewUpdateSet(Head),
                    StreamInput.NewWhere(WhereExpression),
                });

            return stream;
        }
    }
}
