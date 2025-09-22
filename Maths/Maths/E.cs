using Maths.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Drawing;
using Color = SixLabors.ImageSharp.Color;
using PointF = SixLabors.ImageSharp.PointF;
namespace Maths
{
    public class E : IMathematicalConstant
    {
        // Static instance variable
        private static readonly Lazy<E> _instance = new Lazy<E>(() => new E());

        // Private constructor to prevent instantiation
        private E() { }

        // Public property to access the singleton instance
        public static E Instance => _instance.Value;

        public string EquationFor => "e = lim (1 + 1/n)^n as n approaches infinity";

        public string BriefDescription => "The mathematical constant 𝑒 (approximately equal to 2.71828) " +
            "is one of the most important numbers in mathematics. It serves as the base for natural logarithms " +
            "and is fundamental in calculus, particularly in problems involving exponential growth and decay.";

        public string[] Characteristics => new string[]
        {
            "Irrational and Transcendental: 𝑒 is both an irrational number (it cannot be expressed as a fraction) " +
                "and a transcendental number (it is not a root of any non-zero polynomial equation with rational coefficients).",
            "Value: The approximate value of 𝑒 is 2.718281828459045... and continues infinitely without repeating.",
            "Applications: 𝑒 is widely used in various mathematical fields, including calculus, complex analysis, " +
                "probability theory, and finance."
        };

        public double Value => Math.E;

        public char AsciiCharacter => 'e';
        public string HexCharacterCode => ((int)AsciiCharacter).ToString("X");

        public string Origin => "Discovered in the context of compound interest by Jacob Bernoulli.";
        public string Approximation => "2.71828";
        public string[] Applications => new string[] { "Calculus", "Compound Interest", "Exponential Growth" };

        // RelatedConstants now returns instances of IMathematicalConstant
        public IMathematicalConstant[] RelatedConstants => new IMathematicalConstant[]
        {
            null, // π (pi)
            null, // i (imaginary unit)
            // Add more constants here as needed
        };
        private Image<Rgba32> _GraphicalRepresentation;
        public Image<Rgba32> GraphicalRepresentation
        {
            get
            {
                if (_GraphicalRepresentation == null)
                {
                    _GraphicalRepresentation = CreateGraphicalRepresentation();
                }
                return _GraphicalRepresentation;
            }
        }

        public void Explain()
        {
            Console.WriteLine(EquationFor);
            Console.WriteLine(BriefDescription);

            Console.WriteLine("Characteristics of e:");
            foreach (var characteristic in Characteristics)
            {
                Console.WriteLine($"- {characteristic}");
            }

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
                    Console.WriteLine($"- Not Implemented (example: π, i)");
                }
                else
                {
                    // Here you would call Explain() on the constant if it were implemented
                    // constant.Explain();
                }
            }

        }

        private static double CalculateE(int n)
        {
            return Math.Pow(1 + 1.0 / n, n);
        }
        private Image<Rgba32> CreateGraphicalRepresentation()
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

            // Draw the graph of y = e^x
            pen = Pens.Solid(Color.Blue, 2);
            var points = new List<PointF>();

            // Calculate points for the graph
            for (float x = -3; x < 3; x += 0.01f)
            {
                float y = (float)Math.Pow(Math.E, x); // Calculate e^x
                int screenX = (int)(width / 2 + x * 100); // Scale and translate
                int screenY = (int)(height / 2 - y * 10); // Scale and translate
                points.Add(new PointF(screenX, screenY));
            }

            // Draw the lines connecting the points
            image.Mutate(ctx => ctx.DrawLine(pen, points.ToArray()));

            return image;
        }

    }
}
