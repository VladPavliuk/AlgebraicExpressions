#define DEBUG

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AlgebraicExpressions
{
    class Program
    {
        static void Main(string[] args)
        {
            var textExpression = " 23/5*45+123-67 -  123+    4 *  22    ";
            var tokens = GetTokens(textExpression);
            var expressionsOrder = GetExpressionsOrder(tokens);
            var res = EvaluateExpressions(tokens, expressionsOrder);
        }

        static float EvaluateExpressions(List<Token> tokens, Queue<KeyValuePair<Token, List<Guid>>> expressionsOrder)
        {
            var tokensValue = tokens.Where(t => t.Type == TokenType.Operand)
                .ToDictionary(t => t.Id, t => float.Parse(t.Content));

            var tokensOperatos = tokens.Where(t => t.Type == TokenType.Operator)
                .ToDictionary(t => t.Id, t => getAlgebraicOperation(t.Content));

            while (expressionsOrder.Any())
            {
                var expression = expressionsOrder.Dequeue();

                // Fix it later. Looks like a shit.
                var leftId = expression.Value[0];
                var operationId = expression.Value[1];
                var rightId = expression.Value[2];

                var leftOperand = tokensValue[leftId];
                var rightOperand = tokensValue[rightId];
                var operation = tokensOperatos[operationId];

                var value = EvaluateAlgebraicExpression(leftOperand, rightOperand, operation);

#if DEBUG
                Console.WriteLine($"{expression.Key.Content} -> {value}");
#endif

                tokensValue.Add(expression.Key.Id, value);
            }

            return tokensValue.Last().Value;
        }
        static Queue<KeyValuePair<Token, List<Guid>>> GetExpressionsOrder(List<Token> tokens)
        {
            var shadowTokens = new List<Token>();
            var operators = new List<string[]>() { new string[] { "*", "/" }, new string[] { "+", "-", } };
            var tokensMap = tokens.Select(t => t.Id).ToList();
            var expressionsOrder = new Queue<KeyValuePair<Token, List<Guid>>>();
            var hasOperator = false;
            var currentOperatorsGroupIndex = 0;

            Func<Guid, Token> getTokenById = (Guid id) =>
            {
                var res = tokens.FirstOrDefault(t => t.Id == id);

                if (res == null)
                {
                    res = shadowTokens.FirstOrDefault(t => t.Id == id);

                    if (res == null)
                    {
                        throw new Exception("FUCKING SHIT!");
                    }
                }

                return res;
            };

            do
            {
                hasOperator = false;

                for (int i = 0; i < tokensMap.Count; i++)
                {
                    var currentToken = getTokenById(tokensMap[i]);

                    if (currentToken.Type == TokenType.Operator
                        && operators[currentOperatorsGroupIndex].Any(o => o.Equals(currentToken.Content)))
                    {
                        hasOperator = true;

                        // Add out of boundaries exeption handling.
                        var leftOperand = getTokenById(tokensMap[i - 1]);
                        var rightOperand = getTokenById(tokensMap[i + 1]);

                        if (leftOperand.Type != TokenType.Operand || rightOperand.Type != TokenType.Operand)
                        {
                            throw new Exception("BLAT");
                        }

                        var shadowToken = new Token(TokenType.Operand,
                            new Range(leftOperand.Location.Start, rightOperand.Location.End),
                            leftOperand.Content + currentToken.Content + rightOperand.Content);

                        expressionsOrder.Enqueue(new KeyValuePair<Token, List<Guid>>(
                            shadowToken,
                            new List<Guid>() { leftOperand.Id, currentToken.Id, rightOperand.Id }
                        ));

                        // Try to remove this variable.
                        shadowTokens.Add(shadowToken);

                        tokensMap[i] = shadowToken.Id;

                        tokensMap.RemoveAt(i + 1);
                        tokensMap.RemoveAt(i - 1);

                        break;
                    }
                }

                if (!hasOperator)
                {
                    currentOperatorsGroupIndex++;
                }

            } while (currentOperatorsGroupIndex < operators.Count);


            return expressionsOrder;
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

        static float EvaluateAlgebraicExpression(float left, float right, AlgebraicOperation operation)
        {
            switch (operation)
            {
                case AlgebraicOperation.Add: return left + right;
                case AlgebraicOperation.Subtract: return left - right;
                case AlgebraicOperation.Multiply: return left * right;
                case AlgebraicOperation.Divide: return left / right;
            }

            throw new Exception("Unknown algebraic operation.");
        }

        static AlgebraicOperation getAlgebraicOperation(string textAlgebraicOperation)
        {
            switch (textAlgebraicOperation)
            {
                case "+": return AlgebraicOperation.Add;
                case "-": return AlgebraicOperation.Subtract;
                case "*": return AlgebraicOperation.Multiply;
                case "/": return AlgebraicOperation.Divide;
            }

            throw new Exception("Unknown algebraic operation.");
        }
    }

    public class Token
    {
        public Guid Id { get; } = Guid.NewGuid();

        public string Content { get; set; }

        public TokenType Type { get; set; }

        public Range Location { get; set; }

        public Token(TokenType type, Range location, string content)
        {
            Type = type;
            Location = location;
            Content = content;
        }
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
