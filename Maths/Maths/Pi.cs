using Maths.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing;
using System;

namespace Maths
{
    public class Pi : IMathematicalConstant
    {
        // Static instance variable
        private static readonly Lazy<Pi> _instance = new Lazy<Pi>(() => new Pi());

        // Private constructor to prevent instantiation
        private Pi() { }

        // Public property to access the singleton instance
        public static Pi Instance => _instance.Value;

        public string EquationFor => "π = C / d, where C is the circumference and d is the diameter of a circle.";

        public string BriefDescription => "The mathematical constant π (pi) is the ratio of the circumference of a circle to its diameter. " +
            "It is an irrational number, meaning it cannot be expressed as a simple fraction, and its decimal representation is non-repeating and infinite.";

        public string[] Characteristics => new string[]
        {
            "Irrational: π cannot be expressed as a fraction of two integers.",
            "Transcendental: π is not a root of any non-zero polynomial equation with rational coefficients.",
            "Approximate Value: The approximate value of π is 3.14159265358979323846...",
            "Applications: π is widely used in geometry, trigonometry, and calculus, as well as in fields such as physics and engineering."
        };

        public double Value => Math.PI;

        public char AsciiCharacter => 'π'; // ASCII representation for pi
        public string HexCharacterCode => ((int)AsciiCharacter).ToString("X"); // Hex code for 'π'

        public string Origin => "First approximated by ancient civilizations, with formal mathematical definitions evolving over centuries.";
        public string Approximation => "3.14159";
        public string[] Applications => new string[] { "Geometry", "Trigonometry", "Calculus" };

        // RelatedConstants now returns instances of IMathematicalConstant
        public IMathematicalConstant[] RelatedConstants => new IMathematicalConstant[]
        {
            E.Instance, // e (Euler's number)
            null, // i (imaginary unit)
            // Add more constants here as needed
        };

        public string NumericalRepresentation => throw new NotImplementedException();// Convert.ToString(Value, 2); // Binary representation
        private Image<Rgba32> _GraphicalRepresentation;
        public Image<Rgba32> GraphicalRepresentation { get {
                if (_GraphicalRepresentation == null) {
                    _GraphicalRepresentation = CreateGraphicalRepresentation();
                }
                return _GraphicalRepresentation;
            } }

        public void Explain()
        {
            Console.WriteLine(EquationFor);
            Console.WriteLine(BriefDescription);

            Console.WriteLine("Characteristics of π:");
            foreach (var characteristic in Characteristics)
            {
                Console.WriteLine($"- {characteristic}");
            }

            Console.WriteLine($"Approximation of π: {Value}");
            Console.WriteLine($"Circumference of a circle with radius 1: {CalculateCircumference(1)}");
            Console.WriteLine($"Area of a circle with radius 1: {CalculateArea(1)}");

            Console.WriteLine($"ASCII Character: {AsciiCharacter}");
            Console.WriteLine($"Hexadecimal Character Code: {HexCharacterCode}");
            Console.WriteLine($"Origin: {Origin}");
            Console.WriteLine($"Approximation: {Approximation}");

            Console.WriteLine("Applications:");
            foreach (var app in Applications)
            {
                Console.WriteLine($"- {app}");
            }

            // Display related constants
            Console.WriteLine("Related Constants:");
            foreach (var constant in RelatedConstants)
            {
                if (constant == null)
                {
                    Console.WriteLine($"- Not Implemented (example: e, i)");
                }
                else
                {
                    // Here you would call Explain() on the constant if it were implemented
                    // constant.Explain();
                }
            }

            Console.WriteLine($"Numerical Representation (Binary): {NumericalRepresentation}");
            Console.WriteLine($"Graphical Representation: {GraphicalRepresentation}");

            // Generate the graph
            //CreateGraphicalRepresentation();
        }

        private double CalculateCircumference(double radius)
        {
            return 2 * Value * radius; // C = 2πr
        }

        private double CalculateArea(double radius)
        {
            return Value * radius * radius; // A = πr²
        }
        public Image<Rgba32> CreateGraphicalRepresentation()
        {
            int width = 800;
            int height = 600;
            var image = new Image<Rgba32>(width, height);

            // Set background color
            image.Mutate(ctx => ctx.Fill(Color.White));

            // Draw axes
            var pen = Pens.Solid(Color.Black, 2);
            image.Mutate(ctx => {
                ctx.DrawLine(pen, new PointF[] {
            new PointF(0, height / 2), // X-axis
            new PointF(width, height / 2),
            new PointF(width / 2, 0), // Y-axis
            new PointF(width / 2, height)
        });
            });

            // Draw a circle with radius 1 to represent π using EllipsePolygon
            pen = Pens.Solid(Color.Blue, 2);
            var circle = new EllipsePolygon(width / 2, height / 2, 100, 100); // Circle with radius 100 pixels
            image.Mutate(ctx => ctx.Draw(pen, circle));
            return image;
        }
    }
}
