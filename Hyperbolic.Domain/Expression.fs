namespace Hyperboliq.Domain

module ExpressionParts =
    open Types
    open Stream

    type FromExpression = 
        {
            Tables : ITableReference list
        }
    
    let NewFromExpression () = { FromExpression.Tables = [] }

    let AddFromTable fromExpr tbl =
        { fromExpr with FromExpression.Tables = tbl :: fromExpr.Tables }
    
    let FlattenFromExpression fromExpr =
        let rec internalFlatten expr acc =
            match expr with
            | [] -> List.rev acc
            | head::tail -> Table(TableToken(head)) :: acc
        internalFlatten fromExpr [ Keyword(KeywordNode.From) ]

    type JoinClause = 
        { 
            SourceTables: ITableReference list
            TargetTable: ITableReference
            Type: JoinType
            Condition: System.Linq.Expressions.Expression
        }
        member x.Flatten() =
            Keyword(Join(x.Type)) :: 
            Table(TableToken(x.TargetTable)) :: 
            Keyword(KeywordNode.On) :: 
            (ExpressionVisitor.Visit x.Condition (x.SourceTables @ [ x.TargetTable ]))


    type JoinExpression = 
        {
            Clauses: JoinClause list
        }
    
    let NewJoinExpression () = { Clauses = [] }

    let CreateJoinClause2<'a, 'b> joinType condition =
        {
            SourceTables = [ TableReferenceFromType<'a> ]
            TargetTable = TableReferenceFromType<'b>
            Type = joinType
            Condition = condition
        }

    let CreateJoinClause3<'a, 'b, 'c> joinType condition =
        {
            SourceTables = [ TableReferenceFromType<'a>; TableReferenceFromType<'b> ]
            TargetTable = TableReferenceFromType<'c>
            Type = joinType
            Condition = condition
        }

    let AddJoinClause expr clause =
        { Clauses = clause :: expr.Clauses }

    let internal keywordForJoinType joinType =
        match joinType with
        | InnerJoin -> "INNER JOIN"
        | LeftJoin -> "LEFT JOIN"
        | RightJoin -> "RIGHT JOIN"
        | FullJoin -> "FULL JOIN"

    let FlattenJoinExpression (joinExpr : JoinExpression) =
        joinExpr.Clauses 
        |> List.map (fun (clause : JoinClause) -> clause.Flatten())
        |> List.concat

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
