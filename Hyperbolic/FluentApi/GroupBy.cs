using Microsoft.FSharp.Collections;
using static Hyperboliq.Domain.Stream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Hyperboliq
{
    public class GroupBy : ISqlStreamTransformable
    {
        private SelectExpression expr;
        internal GroupBy(SelectExpression expr) { this.expr = expr; }

        public GroupBy ThenBy<TTableType>(Expression<Func<TTableType, object>> groupByExpr)
        {
            expr.GroupBy(groupByExpr);
            return this;
        }

        public Having Having<TTableType>(Expression<Func<TTableType, bool>> predicate)
        {
            return new Having(expr).And(predicate);
        }

        public Having Having<TTable1, TTable2>(Expression<Func<TTable1, TTable2, bool>> predicate)
        {
            return new Having(expr).And(predicate);
        }

        public FSharpList<SqlNode> ToSqlStream() { return expr.ToSqlStream(); }
    }
}
