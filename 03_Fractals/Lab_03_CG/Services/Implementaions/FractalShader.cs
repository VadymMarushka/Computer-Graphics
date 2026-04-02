using ComputeSharp;
using ComputeSharp.D2D1;

namespace Lab_03_CG.Shaders
{
    [D2DRequiresScenePosition]
    [D2DShaderProfile(D2D1ShaderProfile.PixelShader50)]
    [D2DInputCount(0)]
    [D2DGeneratedPixelShaderDescriptor]
    public readonly partial struct FractalShader : ID2D1PixelShader
    {
        public readonly float Width;
        public readonly float Height;
        public readonly float Time;
        public readonly float CReal;
        public readonly float CImaginary;
        public readonly float MaxIterations;
        public readonly float EscapeRadius;
        public readonly float Formula;
        public readonly float Palette;
        public readonly float PanX;
        public readonly float PanY;
        public readonly float Zoom;
        public readonly float Mode;
        public readonly float Z0Real;
        public readonly float Z0Imaginary;

        public readonly bool ChromaticMode;

        public FractalShader(
            float width, float height, float time,
            float cReal, float cImaginary,
            float maxIterations, float escapeRadius,
            float formula, float palette,
            float mode, float z0Real, float z0Imaginary,
            float panX, float panY, float zoom,
            bool chromaticMode)
        {
            Width = width; Height = height; Time = time;
            CReal = cReal; CImaginary = cImaginary;
            MaxIterations = maxIterations; EscapeRadius = escapeRadius;
            Formula = formula; Palette = palette;
            Mode = mode; Z0Real = z0Real; Z0Imaginary = z0Imaginary;
            PanX = panX; PanY = panY; Zoom = zoom;
            ChromaticMode = chromaticMode;
        }

        // Inigo Quilez
        // Color(t) = a + b * cos (2π * (c * t + D))
        private static float3 CosPalette(float t, float3 a, float3 b, float3 c, float3 d)
        {
            float3 phase = c * t + d;
            return a + b * new float3(
                Hlsl.Cos(6.28318f * phase.X),
                Hlsl.Cos(6.28318f * phase.Y),
                Hlsl.Cos(6.28318f * phase.Z));
        }

        private static float3 HsvToRgb(float h, float s, float v)
        {
            h = Hlsl.Frac(h);
            float r = Hlsl.Abs(h * 6f - 3f) - 1f;
            float g = 2f - Hlsl.Abs(h * 6f - 2f);
            float b = 2f - Hlsl.Abs(h * 6f - 4f);
            float3 rgb = Hlsl.Clamp(new float3(r, g, b), 0f, 1f);
            return ((rgb - 1f) * s + 1f) * v;
        }

        public float4 Execute()
        {
            float2 pixelPos = (float2)D2D.GetScenePosition().XY;
            float2 uv = (pixelPos / new float2(Width, Height)) * 2.0f - 1.0f;
            uv.X *= Width / Height;

            float2 mappedPos = uv * Zoom + new float2(PanX, PanY);

            float2 z, c;
            if (Mode == 0) { z = new float2(0f, 0f); c = mappedPos; }
            else if (Mode == 1) { z = new float2(Z0Real, Z0Imaginary); c = mappedPos; }
            else if (Mode == 2) { z = mappedPos; c = new float2(CReal, CImaginary); }
            else { z = new float2(Z0Real, Z0Imaginary); c = new float2(-mappedPos.X, -mappedPos.Y); }

            int iterations = 0;
            float radiusSq = EscapeRadius * EscapeRadius;

            int maxIter = (int)MaxIterations;

            for (int i = 0; i < maxIter; i++)
            {
                float nx = 0f, ny = 0f;
                if (Formula == 0)
                {
                    nx = z.X * z.X - z.Y * z.Y + c.X;
                    ny = 2f * z.X * z.Y + c.Y;
                }
                else if (Formula == 1)
                {
                    nx = z.X * z.X * z.X - 3f * z.X * z.Y * z.Y + c.X;
                    ny = 3f * z.X * z.X * z.Y - z.Y * z.Y * z.Y + c.Y;
                }
                else if (Formula == 2)
                {
                    nx = Hlsl.Sin(z.X) * Hlsl.Cosh(z.Y) + c.X;
                    ny = Hlsl.Cos(z.X) * Hlsl.Sinh(z.Y) + c.Y;
                }
                else
                {
                    float u = z.X * z.X - z.Y * z.Y;
                    float v = 2f * z.X * z.Y;
                    float denom = Hlsl.Cos(2f * u) + Hlsl.Cosh(2f * v);
                    nx = Hlsl.Sin(2f * u) / denom + c.X;
                    ny = Hlsl.Sinh(2f * v) / denom + c.Y;
                }
                z = new float2(nx, ny);
                if (z.X * z.X + z.Y * z.Y > radiusSq) break;
                iterations++;
            }

            if (iterations == maxIter)
            {
                if (Palette == 2)
                {
                    return new float4(0f, 0f, 0.05f, 1f);
                }
                if (Palette == 4)
                {
                    return new float4(0f, 0.05f, 0f, 1f);
                }
                return new float4(0f, 0f, 0f, 1f);
            }

            float z2 = z.X * z.X + z.Y * z.Y;
            float logZn = Hlsl.Log2(z2) * 0.5f;
            float nu = Hlsl.Log2(logZn);
            float smooth = iterations + 1f - nu;

            if (ChromaticMode)
            {
                float hue = Hlsl.Frac(smooth * 0.05f + Time * 0.12f);
                float sat = 0.85f + 0.15f * Hlsl.Sin(smooth * 0.3f);
                float val = 0.95f;
                float3 col = HsvToRgb(hue, sat, val);
                return new float4(col.X, col.Y, col.Z, 1f);
            }

            float t = 0.5f - 0.5f * Hlsl.Cos(smooth * 0.1f);
            float3 rgb;

            if (Palette == 0) // Gray Scale
            {
                float lum = Hlsl.Pow(t, 0.5f);
                rgb = new float3(lum, lum, lum);
            }
            else if (Palette == 1) // Solar Eclipse
            {
                rgb = CosPalette(t,
                    new float3(0.50f, 0.12f, 0.00f), // базове значення RGB
                    new float3(0.40f, 1.00f, 0.20f), // амплітуда
                    new float3(0.95f, 0.80f, 0.90f), // швидкість зміни
                    new float3(0.00f, 0.10f, 0.90f)); // Важко... він визначає фору кожного кольору і впливає найсильніше, коли t мале
            }
            else if (Palette == 2) // Aquamarine
            {
                rgb = CosPalette(t,
                    new float3(0.00f, 0.35f, 0.55f), 
                    new float3(0.00f, 0.40f, 0.35f),
                    new float3(0.80f, 0.70f, 0.60f),
                    new float3(0.00f, 0.60f, 0.50f)); 
            }
            else if (Palette == 3) // Arctic
            {
                rgb = CosPalette(t,
                    new float3(0.75f, 0.80f, 0.90f),
                    new float3(0.25f, 0.20f, 0.10f), 
                    new float3(1.00f, 1.00f, 0.80f),
                    new float3(0.15f, 0.05f, 0.15f));
            }
            else // Radioactive
            {
                rgb = CosPalette(t,
                    new float3(0.20f, 0.60f, 0.0f),
                    new float3(0.50f, 0.40f, 0.30f), 
                    new float3(0.90f, 0.90f, 0.90f), 
                    new float3(0.95f, 0.95f, 0.00f)); 
            }

            rgb = Hlsl.Clamp(rgb, 0f, 1f);
            return new float4(rgb.X, rgb.Y, rgb.Z, 1f);
        }
    }
}