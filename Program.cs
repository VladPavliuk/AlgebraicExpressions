using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AlgebraicExpressions
{
    class Program
    {
        Dictionary<AlgebraicOperation, string> Operations = new Dictionary<AlgebraicOperation, string>()
        {
            {AlgebraicOperation.Add, "+"},
            {AlgebraicOperation.Subtract, "-"},
            {AlgebraicOperation.Multiply, "*"},
            {AlgebraicOperation.Divide, "/"},
        };

        static void Main(string[] args)
        {
            var textExpression = "   123+    4 *  22    ";
            var tokens = GetTokens(textExpression);

            var test = textExpression[2..];

            Console.WriteLine("Hello World!");
        }

        static List<Token> GetTokens(string textExpression)
        {
            var operandsSymbols = new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            var operatorsSymbols = new[] { '+', '-', '*', '/' };

            Func<char, bool> isOperand = (char symbol) => operandsSymbols.Any(n => n.Equals(symbol));
            Func<char, bool> isOperator = (char symbol) => operatorsSymbols.Any(n => n.Equals(symbol));

            Func<TokenType, Func<char, bool>, int, (int, int, string)> getToken =
                (TokenType tokenType, Func<char, bool> validator, int index) =>
              {
                  int start = index;
                  int end = index;
                  var content = new StringBuilder();

                  for (; index < textExpression.Length; index++)
                  {
                      if (!validator(textExpression[index]))
                      {
                          break;
                      }

                      content.Append(textExpression[index]);
                      end = index;
                  }

                  return (start, end, content.ToString());
              };

            var tokes = new List<Token>();

            for (int i = 0; i < textExpression.Length; i++)
            {
                if (isOperand(textExpression[i]))
                {
                    var (beginningOfToken, endOfToken, tokenContent) = getToken(TokenType.Operand, isOperand, i);
                    i = endOfToken;

                    // ranges start with 1 (not 0 like arrays)
                    tokes.Add(new Token(TokenType.Operand, new Range(beginningOfToken + 1, endOfToken + 1), tokenContent.ToString()));
                }
                else if (isOperator(textExpression[i]))
                {
                    var (beginningOfToken, endOfToken, tokenContent) = getToken(TokenType.Operator, isOperator, i);
                    i = endOfToken;

                    // ranges start with 1 (not 0 like arrays)
                    tokes.Add(new Token(TokenType.Operator, new Range(beginningOfToken + 1, endOfToken + 1), tokenContent.ToString()));
                }
            }

            return tokes;
        }
    }

    public class AlgebraicExpression
    {
        public AlgebraicOperation Operation { get; set; }

        public AlgebraicExpression Value { get; set; }
    }

    public class Token
    {
        public Token(TokenType type, Range location, string content)
        {
            Type = type;
            Location = location;
            Content = content;
        }

        public string Content { get; set; }

        public TokenType Type { get; set; }

        public Range Location { get; set; }
    }

    public enum TokenType
    {
        Operand,
        Operator
    }

    public enum AlgebraicOperation
    {
        Add,
        Subtract,
        Multiply,
        Divide
    }
}
