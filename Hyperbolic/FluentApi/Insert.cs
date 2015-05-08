using System;
using System.Linq.Expressions;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;
using static Hyperboliq.Domain.Stream;
using static Hyperboliq.Domain.Types;
using static Hyperboliq.Domain.ExpressionParts;
using static Hyperboliq.Domain.SqlGenerator;
using static Hyperboliq.Domain.InsertExpressionPart;
using Hyperboliq.Domain;

namespace Hyperboliq.FluentApi
{
    public class InsertInto<TTable>
    {
        private InsertExpression expr;

        public InsertInto()
        {
            expr = new InsertExpression(
                NewInsertHead(TableReferenceFromType<TTable>()), 
                FSharpList<InsertValueToken>.Empty);
        }

        public InsertValues<TTable> AllColumns
        {
            get
            {
                expr =
                    WithHead(
                        expr,
                        AddAllColumns(expr.InsertInto));
                return new InsertValues<TTable>(expr);
            }
        }

        public InsertValues<TTable> Columns(Expression<Func<TTable, object>> selector)
        {
            expr =
                WithHead(
                    expr,
                    AddColumns(expr.InsertInto, selector));
            return new InsertValues<TTable>(expr);
        }
    }


    public static class Insert
    {
        public static InsertInto<TTable> Into<TTable>()
        {
            return new InsertInto<TTable>();
        }
    }
}
