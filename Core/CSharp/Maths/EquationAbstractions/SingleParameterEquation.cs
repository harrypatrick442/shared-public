using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using EvaluateClass = Core.Maths.Evaluate;
namespace Core.Maths.EquationAbstractions
{
    public class SingleParameterEquation
    {
        private Func<double, double> _Evaluate;
        private Func<double, double> _EvaluateDerivative;
        public Func<double, double> EvaluateMethod => _Evaluate;
        public Func<double, double> EvaluateDerivativeMethod => _EvaluateDerivative;
        public SingleParameterEquation(Func<double, double> evaluate, Func<double, double> evaluateDerivative) { 
            _Evaluate = evaluate;
            _EvaluateDerivative = evaluateDerivative;
        }
        public double Evaluate(double x) { 
            return _Evaluate(x);
        }
        public double EvaluateDerivative(double x) {
            return _EvaluateDerivative(x);
        }
        public static SingleParameterEquation Polynomial(params double[] coefficients) {
            return new SingleParameterEquation(
                x=>EvaluateClass.Polynomial(x, coefficients),
                x=>EvaluateClass.PolynomialDerivative(x, coefficients)
            );
        }
    }

}