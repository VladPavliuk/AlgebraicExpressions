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
            //var textExpression = "2*(1+2)";
            var textExpression = " (23/(5*((45/123)-67) - ((( 123+    4) *  22))    ))";
            var tokens = GetTokens(textExpression);
            var expressionsOrder = GetExpressionsQueue(tokens);
            var res = new Dictionary<Token, List<Token>>();

            foreach (var expression in expressionsOrder)
            {
                var test = GetExecutionQueueByAlgebraicOperations(expression.Value);

                if (test.Count() > 0)
                {
                    foreach (var test1 in test.Reverse().Skip(1).Reverse())
                    {
                        res.Add(test1.Key, test1.Value);
                    }
                }

                var outerExpression = test.LastOrDefault();

                if (outerExpression.Value == null)
                {
                    res.Add(expression.Key, expression.Value.Where(token => token.Type == TokenType.Operand).ToList());
                }
                else
                {
                    res.Add(expression.Key, outerExpression.Value);
                }
            }

            var test2 = EvaluateExpressions(tokens, res);

            Console.ReadKey();
        }

        static float EvaluateExpressions(List<Token> tokens, Dictionary<Token, List<Token>> expressionsOrder)
        {
            var tokensValue = tokens.Where(t => t.Type == TokenType.Operand)
                .ToDictionary(t => t.Id, t => float.Parse(t.Content));

            var tokensOperatos = tokens.Where(t => t.Type == TokenType.Operator)
                .ToDictionary(t => t.Id, t => getAlgebraicOperation(t.Content));

            foreach (var expression in expressionsOrder)
            {
                float value;

                // Fix it later. Looks like a shit.
                if (expression.Value.Any(token => token.Type == TokenType.Operator)
                    && expression.Value.Where(token => token.Type == TokenType.Operand).Count() == 2)
                {
                    var leftId = expression.Value.First(token => token.Type == TokenType.Operand).Id;
                    var operationId = expression.Value.First(token => token.Type == TokenType.Operator).Id;
                    var rightId = expression.Value.Last(token => token.Type == TokenType.Operand).Id;

                    var leftOperand = tokensValue[leftId];
                    var rightOperand = tokensValue[rightId];
                    var operation = tokensOperatos[operationId];

                    value = EvaluateAlgebraicExpression(leftOperand, rightOperand, operation);
                }
                else if (expression.Value.Where(token => token.Type == TokenType.Operand).Count() == 1)
                {
                    var operandId = expression.Value.First(token => token.Type == TokenType.Operand).Id;

                    var operand = tokensValue[operandId];

                    value = operand;
                }
                else
                {
                    throw new Exception("BLAT");
                }

#if DEBUG
                Console.WriteLine($"{expression.Key.Content} -> {value}");
#endif

                tokensValue.Add(expression.Key.Id, value);
            }

            return tokensValue.Last().Value;
        }

        static Dictionary<Token, List<Token>> GetExpressionsQueue(List<Token> tokens)
        {
            var tokensMap = tokens.Select(t => t.Id).ToList();
            var shadowTokens = new List<Token>();

            //Refactor later
            Func<Guid, int> getTokenPositionById = (Guid id) =>
            {
                var res = tokensMap.FindIndex(tokenPosition => tokenPosition == id);

                if (res == -1)
                {
                    throw new Exception("FUCKING SHIT!");
                }

                return res;
            };

            var priorityTokensLevels = new Dictionary<int, List<Guid>>();

            var unclosedBrackets = 0;

            Action<int, Guid> updatePriorityTokensLevels = (int nestingLevel, Guid priorityTokenId) =>
            {
                if (priorityTokensLevels.ContainsKey(unclosedBrackets))
                {
                    priorityTokensLevels[unclosedBrackets].Add(priorityTokenId);
                }
                else
                {
                    priorityTokensLevels.Add(unclosedBrackets, new List<Guid> { priorityTokenId });
                }
            };

            foreach (var priorityToken in tokens.Where(t => t.Type == TokenType.Priority))
            {
                switch (priorityToken.Content)
                {
                    case "(":
                        {
                            unclosedBrackets++;
                            updatePriorityTokensLevels(unclosedBrackets, priorityToken.Id);
                            continue;
                        }
                    case ")":
                        {
                            updatePriorityTokensLevels(unclosedBrackets, priorityToken.Id);
                            unclosedBrackets--;
                            continue;
                        }
                }
            }

            var expressionsQueue = new Dictionary<Token, List<Token>>();

            foreach (var priorityTokensLevel in priorityTokensLevels.OrderByDescending(level => level.Key).ToDictionary(l => l.Key, l => l.Value))
            {
                for (var i = 0; i < priorityTokensLevel.Value.Count; i += 2)
                {
                    var leftPriorityTokenIndex = getTokenPositionById(priorityTokensLevel.Value[i]);
                    var rightPriorityTokenIndex = getTokenPositionById(priorityTokensLevel.Value[i + 1]);
                    var expressionLength = rightPriorityTokenIndex - leftPriorityTokenIndex;

                    if (expressionLength <= 0)
                    {
                        throw new Exception("SHIT");
                    }

                    var tokensIds = tokensMap.GetRange(leftPriorityTokenIndex, expressionLength + 1);
                    var expressionsInBrackets = tokensIds
                        .Join(tokens.Concat(shadowTokens), id => id, token => token.Id, (id, token) => token).ToList();

                    var shadowToken = new Token(TokenType.Operand,
                        new Range(expressionsInBrackets.First().Location.Start, expressionsInBrackets.Last().Location.End),
                        expressionsInBrackets.Aggregate(string.Empty, (acc, token) => acc + token.Content));

                    tokensMap.RemoveRange(leftPriorityTokenIndex + 1, expressionLength);
                    tokensMap[leftPriorityTokenIndex] = shadowToken.Id;

                    shadowTokens.Add(shadowToken);
                    expressionsQueue.Add(shadowToken, expressionsInBrackets);
                }
            }

            var expressionsOutBrackets = tokensMap
                        .Join(tokens.Concat(shadowTokens), id => id, token => token.Id, (id, token) => token).ToList();

            expressionsQueue.Add(new Token(TokenType.Operand,
                        new Range(expressionsOutBrackets.First().Location.Start, expressionsOutBrackets.Last().Location.End),
                        expressionsOutBrackets.Aggregate(string.Empty, (acc, token) => acc + token.Content)), expressionsOutBrackets);

            return expressionsQueue;
        }

        static Dictionary<Token, List<Token>> GetExecutionQueueByAlgebraicOperations(List<Token> tokens)
        {
            var shadowTokens = new List<Token>();
            var operators = new List<string[]>() { new string[] { "*", "/" }, new string[] { "+", "-", } };
            var tokensMap = tokens.Select(t => t.Id).ToList();
            var executionOrder = new Dictionary<Token, List<Token>>();
            var hasOperator = false;
            var currentOperatorsGroupIndex = 0;

            //Refactor later
            Func<Guid, Token> getTokenById = (Guid id) =>
            {
                var res = tokens.Concat(shadowTokens).FirstOrDefault(t => t.Id == id);

                if (res == null)
                {
                    throw new Exception("FUCKING SHIT!");
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

                        if (leftOperand.Type == TokenType.Unknown)
                        {
                            throw new Exception("BLAT");
                        }

                        var shadowToken = new Token(TokenType.Operand,
                            new Range(leftOperand.Location.Start, rightOperand.Location.End),
                            leftOperand.Content + currentToken.Content + rightOperand.Content);

                        executionOrder.Add(shadowToken, new List<Token>() { leftOperand, currentToken, rightOperand });

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


            return executionOrder;
        }

        static List<Token> GetTokens(string textExpression)
        {
            var operandsSymbols = "0123456789".ToCharArray();
            var operatorsSymbols = "+-*/".ToCharArray();
            var prioritySymbols = "()".ToCharArray();

            Func<char, bool> isOperand = (char symbol) => operandsSymbols.Any(n => n.Equals(symbol));
            Func<char, bool> isOperator = (char symbol) => operatorsSymbols.Any(n => n.Equals(symbol));
            Func<char, bool> isPriority = (char symbol) => prioritySymbols.Any(n => n.Equals(symbol));

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

            var tokens = new List<Token>();

            for (int i = 0; i < textExpression.Length; i++)
            {
                if (isOperand(textExpression[i]))
                {
                    var (beginningOfToken, endOfToken, tokenContent) = getToken(TokenType.Operand, isOperand, i);
                    i = endOfToken;

                    // ranges start with 1 (not 0 like arrays)
                    tokens.Add(new Token(TokenType.Operand, new Range(beginningOfToken + 1, endOfToken + 1), tokenContent.ToString()));
                }
                else if (isOperator(textExpression[i]))
                {
                    // ranges start with 1 (not 0 like arrays)
                    tokens.Add(new Token(TokenType.Operator, new Range(i + 1, i + 1), textExpression[i].ToString()));
                }
                else if (isPriority(textExpression[i]))
                {
                    // ranges start with 1 (not 0 like arrays)
                    tokens.Add(new Token(TokenType.Priority, new Range(i + 1, i + 1), textExpression[i].ToString()));
                }
            }

            // Add more input validations.
            if (!ValidatePriorityTokens(tokens))
            {
                throw new Exception("SUKA");
            }

            return tokens;
        }

        static bool ValidatePriorityTokens(List<Token> tokens)
        {
            var unclosedBrackets = 0;

            foreach (var priorityToken in tokens.Where(t => t.Type == TokenType.Priority))
            {
                switch (priorityToken.Content)
                {
                    case "(":
                        {
                            unclosedBrackets++;
                            continue;
                        }
                    case ")":
                        {
                            unclosedBrackets--;
                            continue;
                        }
                }
            }

            return unclosedBrackets == 0;
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
        Unknown,
        Operand,
        Operator,
        Priority
    }

    public enum AlgebraicOperation
    {
        Add,
        Subtract,
        Multiply,
        Divide
    }
}
