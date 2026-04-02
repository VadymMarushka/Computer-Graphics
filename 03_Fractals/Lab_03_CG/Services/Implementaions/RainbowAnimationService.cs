using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace Lab_03_CG.Services.Implementaions
{
    public interface IRainbowAnimationService
    {
        SolidColorBrush AnimatedBrush { get; }
        LinearGradientBrush AnimatedGradientBrush { get; } 
        void Start();
        void Stop();
    }

    public class RainbowAnimationService : IRainbowAnimationService
    {
        public SolidColorBrush AnimatedBrush { get; private set; }
        public LinearGradientBrush AnimatedGradientBrush { get; private set; }

        private double _time = 0;
        private bool _isRunning = false;

        public RainbowAnimationService()
        {
            AnimatedBrush = new SolidColorBrush();

            AnimatedGradientBrush = new LinearGradientBrush
            {
                StartPoint = new Windows.Foundation.Point(0,0),
                EndPoint = new Windows.Foundation.Point(1, 1),
            };

            AnimatedGradientBrush.GradientStops.Add(new GradientStop { Offset = 0.0 });
            AnimatedGradientBrush.GradientStops.Add(new GradientStop { Offset = 0.5 });
            AnimatedGradientBrush.GradientStops.Add(new GradientStop { Offset = 1.0 });

            Application.Current.Resources["SineRainbowGradientBrush"] = AnimatedGradientBrush;
        }

        public void Start()
        {
            if (_isRunning) return;
            CompositionTarget.Rendering += OnRenderFrame;
            _isRunning = true;
        }

        public void Stop()
        {
            if (!_isRunning) return;
            CompositionTarget.Rendering -= OnRenderFrame;
            _isRunning = false;
        }

        private void OnRenderFrame(object sender, object e)
        {
            _time += 0.03;
            AnimatedBrush.Color = GetRainbowColor(_time);
            AnimatedGradientBrush.GradientStops[0].Color = GetRainbowColor(_time);
            AnimatedGradientBrush.GradientStops[1].Color = GetRainbowColor(_time + 1.5);
            AnimatedGradientBrush.GradientStops[2].Color = GetRainbowColor(_time + 3.0);
        }

        private Color GetRainbowColor(double timePosition)
        {
            byte r = (byte)(Math.Sin(timePosition) * 127 + 128);
            byte g = (byte)(Math.Sin(timePosition + 2) * 127 + 128);
            byte b = (byte)(Math.Sin(timePosition + 4) * 127 + 128);

            return Color.FromArgb(255, r, g, b);
        }
    }
}