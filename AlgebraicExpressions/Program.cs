using System;

namespace AlgebraicExpressions
{
    class Program
    {
        static void Main(string[] args)
        {
            //var textExpression = " (23/(5*((45/123)-67) - ((( 123+    4) *  22))    ))";

            // TODO: Add exceptions handlers later.
            while (true)
            {
                Console.WriteLine("Type an albegraic expression to evaluate (you can use +,-,*,/ operators and parentheses brackets \"()\" ):");
                var textExpression = Console.ReadLine();

                Console.WriteLine($"\nExpression To Evaluate:\n{textExpression}\n");

                var result = AlgebraicEvaluator.EvaluateAlgebraicExpression(textExpression);

                Console.WriteLine($"\nResult:\n{result}\n");
            }
        }

    }
}
