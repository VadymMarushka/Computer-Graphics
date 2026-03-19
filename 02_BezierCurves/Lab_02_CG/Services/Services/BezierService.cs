using Lab_02_CG.Domain;
using Lab_02_CG.Services.Abstractions;
using Lab_02_CG.Services.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lab_02_CG.Services.Services
{
    public class BezierService : IBezierService
    {
        public ParametricBezierResponse CalculateParametric(List<ControlPoint> controlPoints, double dT, double MinT = 0, double MaxT = 1)
        {
            ParametricBezierResponse result = new ParametricBezierResponse();
            int n = controlPoints.Count - 1;

            double[] binomials = new double[n + 1];
            for (int i = 0; i <= n; i++)
            {
                binomials[i] = BinomialCoefficient(n, i);
            }

            int expectedCapacity = (int)Math.Ceiling((MaxT - MinT) / dT) + 2;
            List<ParametricBezierPoint> tempPoints = new List<ParametricBezierPoint>(expectedCapacity);

            int id = 1;
            double[] tPowers = new double[n + 1];
            double[] oneMinusTPowers = new double[n + 1];

            for (double t = MinT; t <= MaxT + 0.001; t += dT)
            {
                if (t > 1.0) t = 1.0;

                ParametricBezierPoint row = new ParametricBezierPoint();
                row.T = t;
                row.Id = id;

                double x = 0, y = 0;
                double oneMinusT = 1.0 - t;

                tPowers[0] = 1.0;
                oneMinusTPowers[0] = 1.0;

                for (int p = 1; p <= n; p++)
                {
                    tPowers[p] = tPowers[p - 1] * t;
                    oneMinusTPowers[p] = oneMinusTPowers[p - 1] * oneMinusT;
                }

                for (int i = 0; i <= n; i++)
                {
                    double bernsteinPolynomial = binomials[i] * tPowers[i] * oneMinusTPowers[n - i];

                    x += bernsteinPolynomial * controlPoints[i].X;
                    y += bernsteinPolynomial * controlPoints[i].Y;

                    row.BernsteinValues.Add(bernsteinPolynomial);
                }

                row.X = x;
                row.Y = y;

                tempPoints.Add(row);
                id++;
            }

            result.Points = tempPoints.ToArray();
            return result;
        }

        public MatrixBezierResponse CalculateMatrix(List<ControlPoint> controlPoints, double dT, double MinT = 0, double MaxT = 1)
        {
            MatrixBezierResponse result = new MatrixBezierResponse();
            int n = controlPoints.Count - 1;

            double[][] N = new double[n + 1][];
            for (int i = 0; i < n + 1; i++)
            {
                N[i] = new double[(n + 1) - i];
                for (int j = 0; j < (n + 1) - i; j++)
                {
                    N[i][j] = BinomialCoefficient(n, j) * BinomialCoefficient(n - j, n - i - j) * (((n - i - j) % 2 == 0) ? 1.0 : -1.0);
                }
            }

            result.N = N;

            // Cx i Cy (CoefficientsX and Y) - добуток N*P 

            double[] Cx = new double[n + 1];
            double[] Cy = new double[n + 1];

            for (int i = 0; i <= n; i++)
            {
                Cx[i] = 0;
                Cy[i] = 0;
                for (int j = 0; j < (n + 1) - i; j++)
                {
                    Cx[i] += N[i][j] * controlPoints[j].X;
                    Cy[i] += N[i][j] * controlPoints[j].Y;
                }
            }

            int id = 1;

            List<BezierPoint> tempPoints = new List<BezierPoint>();

            for (double t = MinT; t <= MaxT + 0.0001; t += dT)
            {
                if (t > 1.0) t = 1.0;

                double x = 0;
                double y = 0;
                double currentT = 1.0;

                for (int i = n; i >= 0; i--)
                {
                    x += Cx[i] * currentT;
                    y += Cy[i] * currentT;
                    currentT *= t;
                }

                tempPoints.Add(new BezierPoint(id, x, y, t));
                id++;
            }

            result.Points = tempPoints.ToArray();
            return result;
        }
        public double BinomialCoefficient(int n, int i)
        {
            if (i < 0 || i > n) return 0; // некоректний ввід
            if (i == 0 || i == n) return 1; // єдиний спосіб

            if (i > n / 2) i = n - i; // Оскільки С(i, n) == C(n-i, n)

            double result = 1;
            for (int k = 1; k <= i; k++)
            {
                result *= n - k + 1;
                result /= k;
            }
            return result;
        }
    }
}
