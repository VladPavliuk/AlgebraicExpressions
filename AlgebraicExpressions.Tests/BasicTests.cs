using System;
using System.Data;
using Xunit;

namespace AlgebraicExpressions.Tests
{
    public class BasicTests
    {
        [Fact]
        public void RandomInteger_Success()
        {
            var amountOfTests = 100;

            for (int i = 0; i < amountOfTests; i++)
            {
                try
                {
                    var lengthOfExpression = Convert.ToUInt32(20 * i / amountOfTests) + 1;
                    var expressionToTest = AlgebraicExpressionsProvider.GetRandomAlgebraicExpression(true, lengthOfExpression);

                    var actual = AlgebraicEvaluator.EvaluateAlgebraicExpression(expressionToTest);
                    var expected = GetAccurateEvaluation(expressionToTest, res => Convert.ToDouble(res));

                    Assert.Equal(expected, actual);
                }
                catch (OverflowException)
                {
                    continue;
                }
            }
        }

        private T GetAccurateEvaluation<T>(string expressionToTest, Func<object, T> convertor)
        {
            // Roslyn compiler has a slightly different way of algebraic expression evaluation than, let's say JS V8.
            // It seems that in Roslyn multiplication has higher priority than division instead of treating them equally.
            //
            // Example of Ryslyn compiler evaluator call:
            // return (int)await CSharpScript.EvaluateAsync(expressionToTest);
            //
            // On the other hand Compute() method of DataTable class does not have this property.

            return convertor(new DataTable().Compute(expressionToTest, string.Empty));
        }
    }
}
