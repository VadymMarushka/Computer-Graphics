using Lab_03_CG.Data.Entities;
using System;
using System.Collections.Generic;

namespace Lab_03_CG.MVVM.ViewModels
{
    /// <summary>
    /// Wraps a Fractal entity and exposes all display-ready properties
    /// so the FlipView DataTemplate can bind directly without any converters.
    /// </summary>
    public class FractalCardViewModel
    {
        private readonly Fractal _fractal;

        public FractalCardViewModel(Fractal fractal)
        {
            _fractal   = fractal;
            Parameters = BuildParameters(fractal);
        }

        // ── Passthrough ───────────────────────────────────────────────────────

        public Fractal Entity       => _fractal;
        public string  Title        => _fractal.Title       ?? "Untitled";
        public string  Description  => string.IsNullOrWhiteSpace(_fractal.Description)
                                           ? "No description provided."
                                           : _fractal.Description;
        public string  ImagePath    => _fractal.Path        ?? string.Empty;
        public string  FractalType  => _fractal is AlgebraicFractal
                                           ? "Algebraic" : "Geometric";

        // ── Parameter rows ────────────────────────────────────────────────────
        public List<ParameterRow> Parameters { get; }

        private static List<ParameterRow> BuildParameters(Fractal f)
        {
            var rows = new List<ParameterRow>();

            switch (f)
            {
                case AlgebraicFractal a:
                    rows.Add(new("Formula",         a.Formula   ?? "—"));
                    rows.Add(new("Family",           a.Family    ?? "—"));
                    rows.Add(new("Palette",          a.Palette   ?? "—"));
                    rows.Add(new("Max Iterations",   a.MaxIterations.ToString()));
                    rows.Add(new("Escape Radius",    $"{a.EscapeRadius:F3}"));
                    rows.Add(new("C",                FormatComplex(a.C_Real, a.C_Imaginary)));
                    rows.Add(new("Z₀",               FormatComplex(a.Z0_Real, a.Z0_Imaginary)));
                    break;

                case GeometricalFractal g:
                    rows.Add(new("Axiom",            g.Initializer ?? "—"));
                    rows.Add(new("Generator",        g.Generator   ?? "—"));
                    rows.Add(new("Center",           FormatPoint(g.CenterX, g.CenterY)));
                    rows.Add(new("Side Length",      $"{g.SideLength:F2}"));
                    rows.Add(new("Rotation",         $"{g.RotationAngle}°"));
                    rows.Add(new("Line Color",       g.LineColor   ?? "—"));
                    break;
            }

            return rows;
        }

        // ── Formatting helpers ────────────────────────────────────────────────

        /// <summary>Formats a + bi, handles negatives and zero cleanly.</summary>
        private static string FormatComplex(double re, double im)
        {
            string reStr = $"{re:F4}";
            if (im >= 0)  return $"{reStr} + {im:F4}i";
            else          return $"{reStr} − {Math.Abs(im):F4}i";
        }

        private static string FormatPoint(double x, double y)
            => $"({x:F3},  {y:F3})";
    }

    /// <summary>A single label/value pair shown in the parameter grid.</summary>
    public record ParameterRow(string Label, string Value);
}
