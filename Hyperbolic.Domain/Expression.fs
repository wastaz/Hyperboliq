namespace Hyperboliq.Domain

module ExpressionParts =
    open Types
    open Stream

    let NewFromExpression () = { Tables = []; Joins = [] }

    let AddFromTable fromExpr tbl =
        { fromExpr with Tables = tbl :: fromExpr.Tables }

    let AddJoinClause fromExpr joinClause =
        { fromExpr with Joins = joinClause :: fromExpr.Joins }

    let CreateJoinClause joinType condition targetTable ([<System.ParamArray>] sourceTables : ITableReference array) =
        let srcList = List.ofArray sourceTables
        let envList = srcList @ [ targetTable ]
        {
            SourceTables = srcList
            TargetTable = targetTable
            Type = joinType
            Condition = ExpressionVisitor.Visit condition envList
        }

    let NewSelectExpression () =
        { 
            IsDistinct = false
            Values = []
        }

    let MakeDistinct select =
        { select with IsDistinct = true }

    let SelectAllColumns select tableReference =
        { select with Values = SqlNode.Column("*", tableReference) :: select.Values }

    let SelectColumns select expr tableReference =
        let stream = ExpressionVisitor.Visit expr [ tableReference ]
        { select with Values = stream @ select.Values }

    let NewOrderByExpression () = { OrderByExpressionNode.Clauses = [] }

    let AddOrderingClause orderExpr tbl direction nullsorder expr =
        let clause = { OrderByClauseNode.Direction = direction; NullsOrdering = nullsorder; Selector = ExpressionVisitor.Visit expr [ tbl ] }
        { orderExpr with OrderByExpressionNode.Clauses = clause :: orderExpr.Clauses }

    let NewWhereExpression expr ([<System.ParamArray>] tables : ITableReference array) : WhereExpressionNode = { 
        Start = ExpressionVisitor.Visit expr tables
        AdditionalClauses = []
    }

    let private CreateWhereClause cmbType whereExpr expr ([<System.ParamArray>] tables : ITableReference array) =
        let clause = { Combinator = cmbType; Expression = ExpressionVisitor.Visit expr tables }
        { whereExpr with AdditionalClauses = clause :: whereExpr.AdditionalClauses }

    let AddWhereAndClause whereExpr expr ([<System.ParamArray>] tables : ITableReference array) = 
        CreateWhereClause And whereExpr expr tables

    let AddWhereOrClause whereExpr expr ([<System.ParamArray>] tables : ITableReference array) = 
        CreateWhereClause Or whereExpr expr tables

    let NewGroupByExpression () = { 
        Clauses = []
        Having = [] 
    }

    let internal AddHavingClause groupByExpr joinType expr ([<System.ParamArray>] tables : ITableReference array) =
        let clause = { WhereClauseNode.Combinator = joinType; Expression = ExpressionVisitor.Visit expr tables }
        { groupByExpr with GroupByExpressionNode.Having = clause :: groupByExpr.Having }

    let AddHavingAndClause groupByExpr expr ([<System.ParamArray>] tables : ITableReference array) = 
        AddHavingClause groupByExpr And expr tables

    let AddHavingOrClause groupByExpr expr ([<System.ParamArray>] tables : ITableReference array) = 
        AddHavingClause groupByExpr Or expr tables

    let AddGroupByClause groupByExpr expr ([<System.ParamArray>] tables : ITableReference array) = 
        let cols = ExpressionVisitor.Visit expr tables
        { groupByExpr with GroupByExpressionNode.Clauses = groupByExpr.Clauses @ cols }
