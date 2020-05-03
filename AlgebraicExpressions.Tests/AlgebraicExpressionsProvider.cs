using System;
using System.Collections.Generic;

namespace AlgebraicExpressions.Tests
{
    public static class AlgebraicExpressionsProvider
    {
        public static double GetInteger(int minValue = 0, int maxValue = 100)
        {
            return new Random().Next(minValue, maxValue);
        }

        public static double GetNumber(float minValue = 0, float maxValue = 100)
        {
            return (maxValue + minValue) * new Random().NextDouble() + minValue;
        }

        public static char GetRandomOperation()
        {
            return "*/+-".ToCharArray()[new Random().Next(0, 3)];
        }

        public static string GetRandomAlgebraicExpression(
            bool onlyInteger = false,
            uint amountOfOperatorsInExpression = 10)
        {
            Func<string> getNumberLiteral = () => onlyInteger ? GetInteger().ToString() : GetNumber().ToString();
            var res = getNumberLiteral();

            for (int i = 0; i < amountOfOperatorsInExpression; i++)
            {
                res += GetRandomOperation() + getNumberLiteral();
            }

            res += getNumberLiteral();

            return res;
        }
    }
}
