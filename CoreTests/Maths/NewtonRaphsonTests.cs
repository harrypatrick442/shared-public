namespace CoreTests
{
    using NUnit.Framework;
    using System;
    using System.IO;
    using Core.Maths;
    using Core.FileSystem;
    using System.Reflection;
    using Core.Maths.IterativeSolvers.NewtonRaphson;

    namespace CoreTests.Maths
    {
        [TestFixture]
        public class NewtonRaphsonTests
        {

            [SetUp]
            public void Setup()
            {

            }

            [TearDown]
            public void TearDown()
            {

            }

            [Test]
            public void ConvergesToCorrectSolution()
            {
                //x = 2+x^2 + x^3
                Func<double, double> calculateX = x => 2d + Math.Pow(x, 2d) + Math.Pow(x, 3d);
                double result = NewtonRaphsonSingleSolver.Solve(1, 
                    (double x, out double f, out double fPrime) =>
                {
                    f = calculateX(x);
                    double delta = 0.00001;
                    double x2 = x * (1d + delta);
                    double df = (calculateX(x2) - f);
                    double dx = x2 - x;
                    fPrime = df / dx;
                });
                Assert.IsTrue(Math.Abs(result+1.6956207696424908)<1e-10);
            }


        }
    }
}