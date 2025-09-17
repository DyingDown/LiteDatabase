using System.Data;
using LiteDatabase.Sql.Ast;
using LiteDatabase.Sql.Ast.Expressions;
using LiteDatabase.Sql.Token;

namespace LiteDatabase.Sql;

class Parser {
    private readonly Tokenizer tokenizer;
    private Token.Token? currentToken;

    public Parser(string sql) {
        tokenizer = new Tokenizer(sql);
        currentToken = tokenizer.GetNextToken();
    }

    public SqlNode ParseStatement() {
        var node = currentToken?.Type switch {
            TokenType.CREATE => (SqlNode)ParseCreateTableStatement(),
            TokenType.DROP => (SqlNode)ParseDropTableStatement(),
            TokenType.SELECT => (SqlNode)ParseSelectStatement(),
            TokenType.INSERT => (SqlNode)ParseInsertStatement(),
            TokenType.UPDATE => (SqlNode)ParseUpdateStatement(),
            TokenType.DELETE => (SqlNode)ParseDeleteStatement(),
            _ => throw new Exception($"Unsupported SQL statement: {currentToken?.Lexeme}")
        };
        if (currentToken?.Type != TokenType.SEMICOLON) {
            throw new Exception("SQL statement must end with a semicolon");
        }
        return node;
    }

    private CreateTableNode ParseCreateTableStatement() {
        var node = new CreateTableNode();

        if (currentToken?.Type != TokenType.CREATE) {
            throw new Exception("CREATE TABLE statement must start with CREATE");
        }
        NextToken();

        if (currentToken?.Type != TokenType.TABLE) {
            throw new Exception("CREATE TABLE statement must start with CREATE TABLE");
        }
        NextToken();

        if (currentToken?.Type != TokenType.ID) {
            throw new Exception("CREATE TABLE statement missing table name identifier");
        }
        node.TableName = currentToken.Lexeme;
        NextToken();

        if (currentToken?.Type != TokenType.L_BRACKET) {
            throw new Exception("Missing '(' in CREATE TABLE statement");
        }
        NextToken();

        node.Columns = ParseColumnDefinition();

        if (currentToken?.Type != TokenType.R_BRACKET) {
            throw new Exception("Missing ')' in CREATE TABLE statement");
        }
        NextToken();

        return node;
    }

    private SelectNode ParseSelectStatement() {

        var node = new SelectNode();

        if (currentToken?.Type != TokenType.SELECT) {
            throw new Exception("SELECT statement must start with SELECT");
        }

        NextToken();

        node.SelectList = ParseSelectList();

        if (currentToken?.Type != TokenType.FROM) {
            throw new Exception("Missing FROM clause in SELECT statement");
        }
        NextToken();
        node.TableNamesWithAlias = ParseTableList();

        if (currentToken?.Type == TokenType.WHERE) {
            NextToken();
            node.WhereClause = ParseExpression();
        }

        if (currentToken?.Type == TokenType.GROUP) {
            NextToken();
            if (currentToken?.Type != TokenType.BY) {
                throw new Exception("GROUP must be followed by BY");
            }
            NextToken();
            node.GroupByColumns = ParseGroupByColumns();
        }

        if (currentToken?.Type == TokenType.ORDER) {
            NextToken();
            if (currentToken?.Type != TokenType.BY) {
                throw new Exception("ORDER must be followed by BY");
            }
            NextToken();
            node.OrderItems = ParseOrderBy();
        }
        return node;
    }

    private DropTableNode ParseDropTableStatement() {

        DropTableNode node = new DropTableNode();

        if (currentToken?.Type != TokenType.DROP) {
            throw new Exception("DROP TABLE statement must start with DROP");
        } 
        NextToken();

        if (currentToken?.Type != TokenType.TABLE) {
            throw new Exception("DROP TABLE statement must start with DROP TABLE");
        }
        NextToken();

        while (true) {
            if (currentToken?.Type != TokenType.ID) {
                throw new Exception("DROP TABLE statement must specify table name");
            }
            node.TableNameList.Add(currentToken.Lexeme);
            NextToken();
            if (currentToken?.Type == TokenType.COMMA) {
                NextToken();
                continue;
            }
            break;
        }

        // 去重
        node.TableNameList = node.TableNameList.Distinct().ToList();
        
        return node;
    }

    private InsertNode ParseInsertStatement() {
        InsertNode node = new InsertNode {
            ColumnNames = []
        };

        if (currentToken?.Type != TokenType.INSERT) {
            throw new Exception("INSERT statement must start with INSERT");
        } 
        NextToken();

        if (currentToken?.Type != TokenType.INTO) {
            throw new Exception("INSERT statement must start with INSERT INTO");
        }
        NextToken();

        if (currentToken?.Type != TokenType.ID) {
            throw new Exception("INSERT statement must specify table name identifier");
        }

        node.TableName = currentToken.Lexeme;
        NextToken();

        if (currentToken?.Type == TokenType.L_BRACKET) {
            NextToken();
            while (true) {
                if (currentToken?.Type != TokenType.ID) {
                    throw new Exception("Missing column name in INSERT statement");
                }
                node.ColumnNames.Add(currentToken.Lexeme);
                NextToken();
                if (currentToken?.Type == TokenType.COMMA) {
                    NextToken();
                    continue;
                }
                break;
            }
            if (currentToken?.Type != TokenType.R_BRACKET) {
                throw new Exception("Missing ')' in INSERT statement columns");
            }
            NextToken();
        }

        if (currentToken?.Type != TokenType.VALUES) {
            throw new Exception("Missing 'VALUES' in INSERT statement");
        }
        NextToken();

        if (currentToken?.Type != TokenType.L_BRACKET) {
            throw new Exception("Missing '(' in INSERT statement values");
        }
        NextToken();

        while (true) {
            if (currentToken?.Type is not (TokenType.INT or TokenType.FLOAT or TokenType.NULL or TokenType.TRUE or TokenType.FALSE or TokenType.STRING or TokenType.NULL)) {
                throw new Exception("Invalid value type");
            }
            node.Values.Add(currentToken?.Type switch {
                TokenType.INT => new SqlValue(Ast.ValueType.Int, int.TryParse(currentToken.Lexeme, out int v) ? v : 0),
                TokenType.FLOAT => new SqlValue(Ast.ValueType.Float, double.TryParse(currentToken.Lexeme, out double d) ? d : 0.0),
                TokenType.TRUE => new SqlValue(Ast.ValueType.True, true),
                TokenType.FALSE => new SqlValue(Ast.ValueType.False, false),
                TokenType.STRING => new SqlValue(Ast.ValueType.String, currentToken.Lexeme),
                TokenType.NULL => new SqlValue(Ast.ValueType.Null, null),
            });
            NextToken();

            if (currentToken?.Type == TokenType.COMMA) {
                NextToken();
                continue;
            }
            break;

        }
        return node;        
    }

    private UpdateNode ParseUpdateStatement() {
        UpdateNode node = new UpdateNode();
        if (currentToken?.Type != TokenType.UPDATE) {
            throw new Exception("UPDATE statement must start with UPDATE");
        } 
        NextToken();

        if (currentToken?.Type != TokenType.ID) {
            throw new Exception("UPDATE statement must have a table name");
        }
        node.TableName = currentToken.Lexeme;
        NextToken();

        if (currentToken?.Type != TokenType.SET) {
            throw new Exception("UPDATE statement must be followed by 'SET'");
        } 
        NextToken();

        while (true) {
            if (currentToken?.Type != TokenType.ID) {
                throw new Exception("UPDATE statement assignment must start with column name");
            }
            string columnName = currentToken.Lexeme;
            NextToken();

            if (currentToken?.Type != TokenType.EQUAL) {
                throw new Exception("UPDATE statement assignment must use '=' operator");
            }
            NextToken();

            Expression expr = ParseExpression();
            node.Assigns.Add(new Assign(columnName, expr));

            if (currentToken?.Type == TokenType.COMMA) {
                NextToken();
                continue;
            }
            break;
        }

        if (currentToken?.Type != TokenType.WHERE) {
            NextToken();
            node.WhereClause = ParseExpression();
        }
        return node;
    }

    private DeleteNode ParseDeleteStatement() {
        DeleteNode node = new DeleteNode();

        if (currentToken?.Type != TokenType.DELETE) {
            throw new Exception("DELETE statement must start with DELETE");
        }
        NextToken();

        if (currentToken?.Type != TokenType.FROM) {
            throw new Exception("DELETE statement must start with DELETE FROM");
        }
        NextToken();

        if (currentToken?.Type != TokenType.ID) {
            throw new Exception("DELETE statement must specify table name");
        }
        node.TableName = currentToken.Lexeme;
        NextToken();

        if (currentToken?.Type != TokenType.WHERE) {
            NextToken();
            node.WhereClause = ParseExpression();
        }
        return node;

    }

    private List<ColumnDefinition> ParseColumnDefinition() {
        var list = new List<ColumnDefinition>();
        while (true) {
            // column name
            if (currentToken?.Type != TokenType.ID) {
                throw new Exception("Column definition must start with column name");
            }
            string columnName = currentToken.Lexeme;
            NextToken();

            // column data type
            if (currentToken?.Type is not (TokenType.INT or TokenType.FLOAT or TokenType.STRING)) {
                throw new Exception("Invalid data type for column");
            }
            ColumnType type = currentToken?.Type switch {
                TokenType.INT => ColumnType.Int,
                TokenType.FLOAT => ColumnType.Float,
                TokenType.STRING => ColumnType.String,
            };
            NextToken();
            int? len = null;
            if (type == ColumnType.String && currentToken?.Type == TokenType.L_BRACKET) {
                NextToken();
                if (currentToken?.Type != TokenType.INT)
                    throw new Exception("STRING type with length must be an integer");
                len = int.TryParse(currentToken.Lexeme, out int v) ? v : 0;
                NextToken();
                if (currentToken?.Type != TokenType.R_BRACKET)
                    throw new Exception("Missing ')' after STRING length");
                NextToken();
            }

            var listCC = new List<ColumnConstraint>();
            // constraint list
            while (currentToken?.Type is TokenType.PRIMARY or TokenType.NOT or TokenType.UNIQUE or TokenType.DEFAULT or TokenType.INDEX) {
                var constraint = new ColumnConstraint();
                switch (currentToken.Type) {
                    case TokenType.PRIMARY:
                        NextToken();
                        if (currentToken?.Type != TokenType.KEY)
                            throw new Exception("PRIMARY must be followed by KEY");
                        constraint.Type = ColumnConstraintType.PrimaryKey;
                        NextToken();
                        break;
                    case TokenType.NOT:
                        NextToken();
                        if (currentToken?.Type != TokenType.NULL)
                            throw new Exception("NOT must be followed by NULL");
                        constraint.Type = ColumnConstraintType.NotNull;
                        NextToken();
                        break;
                    case TokenType.UNIQUE:
                        constraint.Type = ColumnConstraintType.Unique;
                        NextToken();
                        break;
                    case TokenType.DEFAULT:
                        constraint.Type = ColumnConstraintType.Default;
                        NextToken();
                        constraint.Value = currentToken?.Lexeme;
                        NextToken();
                        break;
                }
                listCC.Add(constraint);
            }
            list.Add(new ColumnDefinition(columnName, type, len, listCC));
            if (currentToken?.Type == TokenType.COMMA) {
                NextToken();
                continue;
            }
            break;
        }
        return list;
    }

    private List<SelectItem> ParseSelectList() {
        var list = new List<SelectItem>();
        while (true) {
            if (currentToken?.Type == TokenType.ASTERISK) {
                list.Add(new SelectItem(true, new StarExpression(null), null));
                NextToken();
            }
            else if (currentToken?.Type == TokenType.ID) {
                string tableName = "", columnName = currentToken.Lexeme;
                NextToken();
                if (currentToken?.Type == TokenType.DOT) {
                    NextToken();
                    if (currentToken?.Type == TokenType.ASTERISK) {
                        list.Add(new SelectItem(true, new StarExpression(tableName), null));
                        NextToken();
                    }
                    else if (currentToken?.Type == TokenType.ID) {
                        tableName = columnName;
                        columnName = currentToken.Lexeme;
                        var aliasName = "";
                        if (currentToken?.Type == TokenType.AS) {
                            NextToken();
                            if (currentToken?.Type != TokenType.ID) {
                                throw new Exception("AS must be followed by an identifier");
                            }
                            aliasName = currentToken.Lexeme;
                            NextToken();
                        }
                        list.Add(new SelectItem(false, new ColumnRefExpression(columnName, tableName), aliasName));

                    }
                    else {
                        throw new Exception("Identifier or * expected after '.'");
                    }
                }

            }
            else {
                var expr = ParsePrimary();
                string? aliasName = "";
                if (currentToken?.Type == TokenType.AS) {
                    NextToken();
                    if (currentToken?.Type != TokenType.ID) {
                        throw new Exception("AS must be followed by an identifier");
                    }
                    aliasName = currentToken.Lexeme;
                    NextToken();
                }
                list.Add(new SelectItem(false, expr, aliasName));
            }
            if (currentToken?.Type == TokenType.COMMA) {
                NextToken();
                continue;
            }
            break;
        }
        return list;
    }

    private List<(string, string)> ParseTableList() {
        var list = new List<(string, string)>();
        while (true) {
            if (currentToken?.Type != TokenType.ID) {
                throw new Exception("FROM must be followed by a table name identifier");
            }
            string tableName = currentToken.Lexeme, aliasName = "";
            NextToken();
            if (currentToken?.Type == TokenType.AS) {
                NextToken();
                if (currentToken?.Type != TokenType.ID) {
                    throw new Exception("AS must be followed by alias identifier");
                }
                aliasName = currentToken.Lexeme;
                NextToken();
            }
            list.Add((tableName, aliasName));
            if (currentToken?.Type == TokenType.COMMA) {
                NextToken();
                continue;
            }
            break;
        }
        return list;
    }

    private List<ColumnRefExpression> ParseGroupByColumns() {
        var list = new List<ColumnRefExpression>();
        while (true) {
            (string tableName, string columnName) = ParseColumnRef();
            list.Add(new ColumnRefExpression(columnName, tableName));
            NextToken();
            if (currentToken?.Type == TokenType.COMMA) {
                NextToken();
                continue;
            }
            break;
        }
        return list;
    }

    private (string tableName, string columnName) ParseColumnRef() {
        if (currentToken?.Type != TokenType.ID) {
            throw new Exception("Column reference must start with identifier");
        }
        string first = currentToken.Lexeme;
        NextToken();
        if (currentToken?.Type == TokenType.DOT) {
            NextToken();
            if (currentToken?.Type != TokenType.ID)
                throw new Exception("Dot must be followed by column name");
            string second = currentToken.Lexeme;
            NextToken();
            return (first, second); // table.column
        }
        return ("", first); // just column
    }
    private List<OrderItem> ParseOrderBy() {
        var list = new List<OrderItem>();
        while (true) {
            (string tableName, string columnName) = ParseColumnRef();
            OrderType orderType = OrderType.ASC;
            if (currentToken?.Type == TokenType.ASC || currentToken?.Type == TokenType.DESC) {
                orderType = currentToken?.Type == TokenType.ASC ? OrderType.ASC : OrderType.DESC;
                NextToken();
            }
            list.Add(new OrderItem(new ColumnRefExpression(columnName, tableName), orderType));
            if (currentToken?.Type == TokenType.COMMA) {
                NextToken();
                continue;
            }
            break;
        }
        return list;
    }

    private Expression ParseExpression() {
        return ParseOr();
    }

    private Expression ParseOr() {
        var left = ParseAnd();
        while (currentToken?.Type == TokenType.OR) {
            NextToken();
            var right = ParseAnd();
            left = new BinaryExpression(left, BinaryOperatorType.Or, right);
        }
        return left;
    }

    private Expression ParseAnd() {
        var left = ParseComparison();
        while (currentToken?.Type == TokenType.AND) {
            NextToken();
            var right = ParseComparison();
            left = new BinaryExpression(left, BinaryOperatorType.And, right);
        }
        return left;
    }

    private Expression ParseComparison() {
        var left = ParseAddSub();
        while (currentToken?.Type == TokenType.EQUAL || currentToken?.Type == TokenType.GREATER_THAN ||
              currentToken?.Type == TokenType.LESS_THAN || currentToken?.Type == TokenType.GREATER_EQUAL_TO || currentToken?.Type == TokenType.LESS_EQUAL_TO) {
            var op = currentToken?.Type switch {
                TokenType.EQUAL => BinaryOperatorType.Equal,
                TokenType.LESS_EQUAL_TO => BinaryOperatorType.LessOrEqual,
                TokenType.LESS_THAN => BinaryOperatorType.LessThan,
                TokenType.GREATER_EQUAL_TO => BinaryOperatorType.GreaterOrEqual,
                TokenType.GREATER_THAN => BinaryOperatorType.GreaterThan,
            };
            NextToken();
            var right = ParseAddSub();
            left = new BinaryExpression(left, op, right);
        }
        return left;
    }

    private Expression ParseAddSub() {
        var left = ParseMulDiv();
        while (currentToken?.Type == TokenType.PLUS || currentToken?.Type == TokenType.MINUS) {
            var op = currentToken?.Type == TokenType.PLUS ? BinaryOperatorType.Add : BinaryOperatorType.Subtract;
            NextToken();
            var right = ParseMulDiv();
            left = new BinaryExpression(left, op, right);
        }
        return left;
    }

    private Expression ParseMulDiv() {
        var left = ParsePrimary();
        while (currentToken?.Type == TokenType.ASTERISK || currentToken?.Type == TokenType.DIVISION) {
            var op = currentToken?.Type == TokenType.ASTERISK ? BinaryOperatorType.Multiply : BinaryOperatorType.Divide;
            NextToken();
            var right = ParsePrimary();
            left = new BinaryExpression(left, op, right);
        }
        return left;
    }

    private Expression ParsePrimary() {
        if (currentToken?.Type == TokenType.L_BRACKET) {
            NextToken();
            if (currentToken?.Type == TokenType.SELECT) {
                var subquery = ParseSelectStatement();
                if (currentToken?.Type != TokenType.R_BRACKET) {
                    throw new Exception("Missing ')' for subquery");
                }
                NextToken();
                return new SubqueryExpression(subquery);
            }
            var expr = ParseExpression();
            if (currentToken?.Type != TokenType.R_BRACKET) {
                throw new Exception("Missing ')'");
            }
            NextToken();
            return expr;
        }

        // unary expression 
        if (currentToken?.Type == TokenType.NOT) {
            NextToken();
            var expr = ParsePrimary();
            return new UnaryExpression(UnaryOperatorType.Not, expr);
        }
        if (currentToken?.Type == TokenType.MINUS) {
            NextToken();
            var expr = ParsePrimary();
            return new UnaryExpression(UnaryOperatorType.Minus, expr);
        }
        if (currentToken?.Type == TokenType.PLUS) {
            NextToken();
            var expr = ParsePrimary();
            return new UnaryExpression(UnaryOperatorType.Plus, expr);
        }

        // function call
        if (currentToken?.Type == TokenType.ID) {
            string name = currentToken.Lexeme;

            NextToken();
            var arguments = new List<Expression>();
            if (currentToken?.Type == TokenType.L_BRACKET) {
                while (true) {
                    arguments.Add(ParseExpression());
                    if (currentToken?.Type == TokenType.COMMA) {
                        NextToken();
                        continue;
                    }
                    break;
                }
                if (currentToken?.Type != TokenType.R_BRACKET) {
                    throw new Exception("Missing ')' for function call");
                }
                NextToken();
                FunctionName funcName = name switch {
                    "sum" => FunctionName.Sum,
                    "avg" => FunctionName.Avg,
                    "count" => FunctionName.Count,
                    "min" => FunctionName.Min,
                    "max" => FunctionName.Max,
                    _ => throw new Exception("Unsupported function")
                };
                return new FunctionCallExpression(funcName, arguments);
            }

            if (currentToken?.Type == TokenType.DOT) {
                NextToken();
                if (currentToken?.Type != TokenType.ID) {
                    throw new Exception("Dot must be followed by column name");
                }
                string columnName = currentToken.Lexeme;
                return new ColumnRefExpression(columnName, name);
            }
            return new ColumnRefExpression(name);
        }
        if (currentToken?.Type == TokenType.INT) {
            object value = int.TryParse(currentToken.Lexeme, out var v) ? v : currentToken.Lexeme;
            NextToken();
            return new LiteralExpression(value);
        }
        if (currentToken?.Type == TokenType.FLOAT) {
            object value = double.TryParse(currentToken.Lexeme, out var v) ? v : currentToken.Lexeme;
            NextToken();
            return new LiteralExpression(value);
        }
        if (currentToken?.Type == TokenType.STRING) {
            object value = currentToken.Lexeme;
            NextToken();
            return new LiteralExpression(value);
        }
        throw new Exception($"Unable to parse expression, unknown token: {currentToken?.Lexeme}");
    }

    private void NextToken() {
        currentToken = tokenizer.GetNextToken();
    }
}