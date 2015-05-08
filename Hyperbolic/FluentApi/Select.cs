using System;
using System.Linq.Expressions;
using static Hyperboliq.Domain.Types;
using static Hyperboliq.Domain.Stream;
using static Hyperboliq.Domain.ExpressionParts;

namespace Hyperboliq
{
    // Oh the things you need to do to get the API as you want it to be!

    public class SelectImpl
    {
        private SelectExpressionNode expr = NewSelectExpression();
        internal SelectImpl() { }

        public SelectImpl Star<TTableType>()
        {
            expr = SelectAllColumns(expr, TableReferenceFromType<TTableType>());
            return this;
        }

        public SelectImpl Column<TTableType>(Expression<Func<TTableType, object>> selector)
        {
            expr = SelectColumns(expr, selector, TableReferenceFromType<TTableType>());
            return this;
        }

        public SelectImpl Column<TTableType>(ITableReference<TTableType> table, Expression<Func<TTableType, object>> selector)
        {
            expr = SelectColumns(expr, selector, table);
            return this;
        }

        internal SelectImpl Distinct
        {
            get
            {
                expr = MakeDistinct(expr);
                return this;
            }
        }

        public SelectFrom<TTableType> From<TTableType>(ITableReference<TTableType> table)
        {
            return new SelectFrom<TTableType>(table, expr);
        }

        public SelectFrom<TTableType> From<TTableType>()
        {
            return From(TableReferenceFromType<TTableType>());
        }
    }

    public class Select
    {
        public static SelectImpl Distinct
        {
            get { return new SelectImpl().Distinct; }
        }

        public static SelectImpl Star<TTableType>()
        {
            return new SelectImpl().Star<TTableType>();
        }

        public static SelectImpl Column<TTableType>(Expression<Func<TTableType, object>> selector)
        {
            return new SelectImpl().Column<TTableType>(selector);
        }

        public static SelectImpl Column<TTableType>(ITableReference<TTableType> table, Expression<Func<TTableType, object>> selector)
        {
            return new SelectImpl().Column(table, selector);
        }
            
    }
}
