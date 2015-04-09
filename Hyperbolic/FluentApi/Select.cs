using System;
using System.Linq.Expressions;

namespace Hyperboliq
{
    // Oh the things you need to do to get the API as you want it to be!

    public class SelectImpl
    {
        private SelectExpression expr;
        internal SelectImpl() { expr = new SelectExpression(); }
        internal SelectImpl(SelectExpression expr) { this.expr = expr; }

        public SelectImpl Star<TTableType>()
        {
            expr.SelectAll<TTableType>();
            return this;
        }

        public SelectImpl Column<TTableType>(Expression<Func<TTableType, object>> selector)
        {
            expr.Select(selector);
            return this;
        }

        internal SelectImpl Distinct
        {
            get
            {
                expr.Distinct();
                return this;
            }
        }

        public SelectFrom<TTableType> From<TTableType>()
        {
            return new SelectFrom<TTableType>(expr);
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
            return new SelectImpl(new SelectExpression().SelectAll<TTableType>());
        }

        public static SelectImpl Column<TTableType>(Expression<Func<TTableType, object>> selector)
        {
            return new SelectImpl(new SelectExpression().Select<TTableType>(selector));
        }
    }
}
