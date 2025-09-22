using Maths.Interfaces;
namespace Maths; 
public class Limit : IMathematicalEntity
{
    // Property for the general limit equation
    public string LimitEquation => "Limit = lim f(x) as x approaches c";

    // Brief description of what a limit is
    public string Description => "A limit describes the value that a function approaches as the input approaches a certain point. " +
        "It is a fundamental concept in calculus that helps define continuity, derivatives, and integrals.";

    // Characteristics of limits
    public string[] Properties => new string[]
    {
        "Existence: A limit may or may not exist depending on the behavior of the function.",
        "One-Sided Limits: Limits can be approached from the left (left-hand limit) or the right (right-hand limit).",
        "Limits at Infinity: Limits can describe the behavior of a function as the input approaches infinity or negative infinity.",
        "Indeterminate Forms: Certain limits yield indeterminate forms, which require further analysis to resolve."
    };

    // Examples of specific limits
    public string[] ExampleLimits => new string[]
    {
        "Example 1: lim (1/x) as x approaches 0 results in infinity.",
        "Example 2: lim (sin(x)/x) as x approaches 0 results in 1.",
        "Example 3: lim (x^2 - 1)/(x - 1) as x approaches 1 requires simplification."
    };

    // Method to explain the limit concept
    public void Explain()
    {
        Console.WriteLine(LimitEquation);
        Console.WriteLine(Description);

        Console.WriteLine("Properties of Limits:");
        foreach (var property in Properties)
        {
            Console.WriteLine($"- {property}");
        }

        Console.WriteLine("Example Limits:");
        foreach (var example in ExampleLimits)
        {
            Console.WriteLine($"- {example}");
        }

        // Calculating various limits
        Console.WriteLine("Calculating various limits:");
        Console.WriteLine($"Limit of x^2 as x approaches 2: {CalculateLimitXSquared(2)}");
        Console.WriteLine($"Limit of 1/x as x approaches 0: {CalculateLimitOneOverX(0)} (approaches infinity)");
        Console.WriteLine($"Limit of sin(x)/x as x approaches 0: {CalculateLimitSinXOverX(0)}");
        Console.WriteLine($"Limit for e as n approaches infinity: {CalculateLimitForE(1000000)}");
    }

    // Example limit calculation methods
    private static double CalculateLimitXSquared(double x)
    {
        return x * x; // f(x) = x^2
    }

    private static double CalculateLimitOneOverX(double x)
    {
        if (x == 0) return double.PositiveInfinity; // Limiting case as x approaches 0
        return 1 / x; // f(x) = 1/x
    }

    private static double CalculateLimitSinXOverX(double x)
    {
        if (x == 0) return 1; // Limit result for sin(x)/x as x approaches 0
        return Math.Sin(x) / x; // f(x) = sin(x)/x
    }

    private static double CalculateLimitForE(int n)
    {
        return Math.Pow(1 + 1.0 / n, n); // e = lim (1 + 1/n)^n as n approaches infinity
    }
}
