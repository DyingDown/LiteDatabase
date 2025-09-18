using System.Globalization;
using LiteDatabase.Utils;
using Microsoft.Extensions.Logging;

namespace LiteDatabase.Sql.Token;

public class Tokenizer {
    private int currentPosition;
    private List<Token> tokens = new List<Token>();

    private string sql;

    private int currentTokenPosition;

    private readonly ILogger<Tokenizer> _logger;

    public Tokenizer(string statement) {
        _logger = Program.LoggerFactory.CreateLogger<Tokenizer>();
        sql = statement;
        currentPosition = 0;
        currentTokenPosition = 0;
        getAllTokens();
    }

    public Token? GetNextToken() {
        if (currentTokenPosition >= tokens.Count) {
            return new Token(TokenType.END, "");
        }
        return tokens[currentTokenPosition ++];
    }

    private void getAllTokens() {
        while (true) {
            Token t = scanNextToken();
            if (t.Type == TokenType.ILLEGAL) {
                _logger.LogError("Illegal token: {Lexeme} at position {Position}", t.Lexeme, currentPosition);
                throw new InvalidOperationException($"Invalid SQL syntax: illegal token '{t.Lexeme}' at position {currentPosition}");
            }
            tokens.Add(t);
            if (t.Type == TokenType.END) {
                break;
            }
        }
    }

    private Token scanNextToken() {
        while (currentPosition < sql.Length && char.IsWhiteSpace(sql[currentPosition])) {
            currentPosition++;
        }
        if (currentPosition >= sql.Length) {
            return new Token(TokenType.END, "");
        }
        else if (sql[currentPosition] == '"' || sql[currentPosition] == '\'') {
            return getString();
        }
        else if (char.IsDigit(sql[currentPosition])) {
            return getNumber();
        }
        else if (char.IsLetter(sql[currentPosition]) || sql[currentPosition] == '_') {
            return getWord();
        }
        else {
            return getPunct();
        }
    }

    private Token getString() {
        char quote = sql[currentPosition];
        int startPos = currentPosition;
        currentPosition++;
        while (currentPosition < sql.Length && sql[currentPosition] != quote) {
            currentPosition++;
        }
        if (currentPosition < sql.Length && sql[currentPosition] == quote) {
            currentPosition++;
            return new Token(TokenType.STRING_LITERAL, sql.Substring(startPos + 1, currentPosition - startPos - 2));
        }
        return new Token(TokenType.ILLEGAL, sql.Substring(startPos, currentPosition - startPos));
    }

    private Token getNumber() {
        bool hasDot = false;
        bool hasExp = false;
        int startPos = currentPosition;
        while (currentPosition < sql.Length) {
            char currentChar = sql[currentPosition];
            if (char.IsDigit(currentChar)) {
                currentPosition++;
            }
            else if (currentChar == '.' && !hasDot) {
                currentPosition++;
                hasDot = true;
            }
            else if ((currentChar == 'e' || currentChar == 'E') && !hasExp) { // 科学计数法，后面后面会立即跟符号 eg. 231e-10
                hasExp = true;
                currentChar = sql[++ currentPosition];
                if (currentPosition < sql.Length && (currentChar == '+' || currentChar == '-')) {
                    currentPosition++;
                }

            }
            else {
                break;
            }
        }

        string number = sql.Substring(startPos, currentPosition - startPos);

        if (int.TryParse(number, out _)) {
            return new Token(TokenType.INT, number);
        }
        else if (decimal.TryParse(number, System.Globalization.NumberStyles.Float, CultureInfo.InvariantCulture, out _)) {
            return new Token(TokenType.FLOAT, number);
        }
        else if (double.TryParse(number, System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out _)) {
            return new Token(TokenType.FLOAT, number);
        }

        return new Token(TokenType.ILLEGAL, number.ToString());
    }

    private Token getWord() {
        int startPos = currentPosition;
        while (currentPosition < sql.Length) {
            if (char.IsLetter(sql[currentPosition]) || char.IsDigit(sql[currentPosition]) || sql[currentPosition] == '_') {
                currentPosition++;
            }
            else {
                break;
            }
        }
        string str = sql.Substring(startPos, currentPosition - startPos).ToLower();
        return str switch {
            "add" => new Token(TokenType.ADD, str),
            "abort" => new Token(TokenType.ABORT, str),
            "alter" => new Token(TokenType.ALTER, str),
            "all" => new Token(TokenType.ALL, str),
            "and" => new Token(TokenType.AND, str),
            "any" => new Token(TokenType.ANY, str),
            "as" => new Token(TokenType.AS, str),
            "asc" => new Token(TokenType.ASC, str),
            "avg" => new Token(TokenType.AVG, str),
            "bool" => new Token(TokenType.BOOL, str),
            "begin" => new Token(TokenType.BEGIN, str),
            "between" => new Token(TokenType.BETWEEN, str),
            "by" => new Token(TokenType.BY, str),
            "check" => new Token(TokenType.CHECK, str),
            "column" => new Token(TokenType.COLUMN, str),
            "commit" => new Token(TokenType.COMMIT, str),
            "count" => new Token(TokenType.COUNT, str),
            "create" => new Token(TokenType.CREATE, str),
            "default" => new Token(TokenType.DEFAULT, str),
            "delete" => new Token(TokenType.DELETE, str),
            "desc" => new Token(TokenType.DESC, str),
            "drop" => new Token(TokenType.DROP, str),
            "distinct" => new Token(TokenType.DISTINCT, str),
            "except" => new Token(TokenType.EXCEPT, str),
            "foreign" => new Token(TokenType.FOREIGN, str),
            "from" => new Token(TokenType.FROM, str),
            "group" => new Token(TokenType.GROUP, str),
            "having" => new Token(TokenType.HAVING, str),
            "in" => new Token(TokenType.IN, str),
            "index" => new Token(TokenType.INDEX, str),
            "int" => new Token(TokenType.INT, str),
            "is" => new Token(TokenType.IS, str),
            "insert" => new Token(TokenType.INSERT, str),
            "into" => new Token(TokenType.INTO, str),
            "join" => new Token(TokenType.JOIN, str),
            "key" => new Token(TokenType.KEY, str),
            "like" => new Token(TokenType.LIKE, str),
            "min" => new Token(TokenType.MIN, str),
            "max" => new Token(TokenType.MAX, str),
            "not" => new Token(TokenType.NOT, str),
            "null" => new Token(TokenType.NULL, str),
            "or" => new Token(TokenType.OR, str),
            "order" => new Token(TokenType.ORDER, str),
            "primary" => new Token(TokenType.PRIMARY, str),
            "string" => new Token(TokenType.VARCHAR, str),
            "varchar" => new Token(TokenType.VARCHAR, str),
            "float" => new Token(TokenType.FLOAT, str),
            "table" => new Token(TokenType.TABLE, str),
            "select" => new Token(TokenType.SELECT, str),
            "set" => new Token(TokenType.SET, str),
            "sum" => new Token(TokenType.SUM, str),
            "update" => new Token(TokenType.UPDATE, str),
            "union" => new Token(TokenType.UNION, str),
            "unique" => new Token(TokenType.UNIQUE, str),
            "values" => new Token(TokenType.VALUES, str),
            "where" => new Token(TokenType.WHERE, str),
            "true" => new Token(TokenType.TRUE, str),
            "false" => new Token(TokenType.FALSE, str),
            _ => new Token(TokenType.ID, str),
        };
    }

    private Token getPunct() {
        int startPos = currentPosition;
        while (currentPosition < sql.Length && CharUtils.IsPunct(sql[currentPosition])) {
            currentPosition++;
            if (currentPosition - startPos == 1 && (sql[startPos] == '+' || sql[startPos] == '-' ||
                                                    sql[startPos] == '*' || sql[startPos] == '/' ||
                                                    sql[startPos] == ',' || sql[startPos] == ';' ||
                                                    sql[startPos] == '(' || sql[startPos] == ')')) {
                break;
            }
        }
        string str = sql.Substring(startPos, currentPosition - startPos);
        return str switch {
            "+" => new Token(TokenType.PLUS, str),
            "-" => new Token(TokenType.MINUS, str),
            "*" => new Token(TokenType.ASTERISK, str),
            "/" => new Token(TokenType.DIVISION, str),
            ";" => new Token(TokenType.SEMICOLON, str),
            "," => new Token(TokenType.COMMA, str),
            ">" => new Token(TokenType.GREATER_THAN, str),
            "<" => new Token(TokenType.LESS_THAN, str),
            "(" => new Token(TokenType.L_BRACKET, str),
            ")" => new Token(TokenType.R_BRACKET, str),
            "=" => new Token(TokenType.EQUAL, str),
            ">=" => new Token(TokenType.GREATER_EQUAL_TO, str),
            "<=" => new Token(TokenType.LESS_EQUAL_TO, str),
            "!=" => new Token(TokenType.NOT_EQUAL, str),
            "<>" => new Token(TokenType.NOT_EQUAL, str),
            "." => new Token(TokenType.DOT, str),
            _ => new Token(TokenType.ILLEGAL, str),
        };
    }
}