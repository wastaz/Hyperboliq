using Hyperboliq.Domain;
using static Hyperboliq.Domain.Stream;
using static Hyperboliq.Domain.Types;
using System;
using Microsoft.FSharp.Collections;
using static Hyperboliq.Domain.StreamGenerator;
using static Hyperboliq.Domain.InsertExpressionPart;
using System.Linq.Expressions;

namespace Hyperboliq
{
    public class InsertExpression<TTable> : ISqlStatement, ISqlStreamTransformable
    {
        private InsertIntoExpression InsertInto;
        private InsertValuesExpression InsertValues = NewInsertValuesExpression();

        private Func<TTable, object> selector = obj => obj;

        public InsertExpression<TTable> AllColumns()
        {
            InsertInto = NewInsertIntoExpressionWithAllColumns<TTable>(TableReferenceFromType<TTable>());
            return this;
        }

        public InsertExpression<TTable> Columns(Expression<Func<TTable, object>> selector)
        {
            InsertInto = NewInsertIntoExpression(TableReferenceFromType<TTable>(), selector);
            this.selector = selector.Compile();
            return this;
        }

        public InsertExpression<TTable> Value(TTable instance)
        {
            InsertValues = AddInsertValue(InsertInto, InsertValues, selector(instance));
            return this;
        }

        public FSharpList<SqlNode> ToSqlStream()
        {
            var stream = GenerateStream(
                new[] {
                    StreamInput.NewInsertInto(InsertInto),
                    StreamInput.NewInsertValues(InsertValues),
                });

            return stream;
        }

        public string ToSql(ISqlDialect dialect) => SqlGenerator.Sqlify(dialect, ToSqlStream());
    }
}
