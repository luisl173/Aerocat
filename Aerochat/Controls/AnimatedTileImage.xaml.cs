﻿using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Timer = System.Timers.Timer;

namespace Aerochat.Controls
{
    public enum AnimatedTileImageFrameDirection
    {
        Horizontal,
        Vertical,
    }

    /// <summary>
    /// Play an image from an animated sprite sheet.
    /// </summary>
    public partial class AnimatedTileImage : UserControl
    {
        private bool _paused = false;
        public AnimatedTileImage()
        {
            InitializeComponent();

            _timer = new Timer();

            Loaded += AnimatedTileImage_Loaded;
            Unloaded += AnimatedTileImage_Unloaded;
        }

        private void SetUpTimer()
        {
            _timer.Elapsed += Timer_Elapsed;
            _timer.AutoReset = true;
            _timer.Start();
        }

        public static readonly DependencyProperty FrameWidthProperty = DependencyProperty.Register("FrameWidth", typeof(int), typeof(AnimatedTileImage), new PropertyMetadata(0, OnChange));
        public static readonly DependencyProperty FrameHeightProperty = DependencyProperty.Register("FrameHeight", typeof(int), typeof(AnimatedTileImage), new PropertyMetadata(0, OnChange));
        public static readonly DependencyProperty FrameDurationProperty = DependencyProperty.Register("FrameDuration", typeof(int), typeof(AnimatedTileImage), new PropertyMetadata(0, OnChange));
        public static readonly DependencyProperty FrameDirectionProperty = DependencyProperty.Register("FrameDirection", typeof(AnimatedTileImageFrameDirection), typeof(AnimatedTileImage), new PropertyMetadata(AnimatedTileImageFrameDirection.Horizontal, OnChange));
        public static readonly DependencyProperty ImageProperty = DependencyProperty.Register("Image", typeof(ImageSource), typeof(AnimatedTileImage), new PropertyMetadata(default(ImageSource), OnChange));
        public static readonly DependencyProperty LoopProperty = DependencyProperty.Register("Loop", typeof(bool), typeof(AnimatedTileImage), new PropertyMetadata(true, OnChange));
        public static readonly DependencyProperty NumberOfFramesProperty = DependencyProperty.Register("NumberOfFrames", typeof(int?), typeof(AnimatedTileImage), new PropertyMetadata(null, OnChange));

        private static void OnChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AnimatedTileImage control && control._timer != null)
            {
                // if its 0 set it to 50 by default
                if (control.FrameDuration == 0)
                {
                    control.FrameDuration = 50;
                }

                if (e.Property == ImageProperty)
                {
                    control.Reset();
                }
                control._timer.Interval = control.FrameDuration;

                control.SetupImageProperties();
                control.InvalidateVisual();
            }
        }

        public int FrameWidth
        {
            get => (int)GetValue(FrameWidthProperty);
            set => SetValue(FrameWidthProperty, value);
        }

        public int FrameHeight
        {
            get => (int)GetValue(FrameHeightProperty);
            set => SetValue(FrameHeightProperty, value);
        }

        public int FrameDuration
        {
            get => (int)GetValue(FrameDurationProperty);
            set => SetValue(FrameDurationProperty, value);
        }

        public AnimatedTileImageFrameDirection FrameDirection
        {
            get => (AnimatedTileImageFrameDirection)GetValue(FrameDirectionProperty);
            set => SetValue(FrameDirectionProperty, value);
        }

        public ImageSource? Image
        {
            get { return (ImageSource)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); InvalidateVisual(); }
        }

        public int? NumberOfFrames
        {
            get => (int?)GetValue(NumberOfFramesProperty);
            set
            {
                SetValue(NumberOfFramesProperty, value);
            }
        }

        public bool Loop
        {
            get => (bool)GetValue(LoopProperty);
            set => SetValue(LoopProperty, value);
        }

        private int _currentFrame = 0;

        public int CurrentFrame
        {
            get => _currentFrame;
            set
            {
                _currentFrame = value;
                UpdateFrameRenderProperties();
            }
        }

        private int _frameCount = 0;
        private Timer _timer;

        private bool SetupImageProperties()
        {
            if (Image != null &&
                Image is BitmapSource bitmapSource &&
                bitmapSource.PixelWidth != 0 &&
                bitmapSource.PixelHeight != 0 &&
                (FrameWidth != 0 &&
                FrameHeight != 0 || NumberOfFrames != null)
            )
            {
                if (NumberOfFrames != null)
                {
                    if (FrameDirection == AnimatedTileImageFrameDirection.Horizontal)
                    {
                        FrameWidth = bitmapSource.PixelWidth / Math.Max(NumberOfFrames ?? 1, 1);
                        if (FrameHeight == 0)
                            FrameHeight = bitmapSource.PixelHeight;
                    }
                    else // Vertical
                    {
                        FrameHeight = bitmapSource.PixelHeight / Math.Max(NumberOfFrames ?? 1, 1);
                        if (FrameWidth == 0)
                            FrameWidth = bitmapSource.PixelWidth;
                    }
                }

                if (FrameDirection == AnimatedTileImageFrameDirection.Horizontal)
                {
                    _frameCount = (int)(bitmapSource.PixelWidth / FrameWidth);
                }
                else // Vertical
                {
                    _frameCount = (int)(bitmapSource.PixelHeight / FrameHeight);
                }

                if (_frameCount == 0)
                {
                    return false;
                }

                return true;
            }

            return false;
        }

        // The control is added to the visual tree.
        private void AnimatedTileImage_Loaded(object sender, RoutedEventArgs e)
        {
            SetUpTimer();
            SetupImageProperties();
            UpdateFrameRenderProperties();
        }

        // The control is removed from the visual tree.
        private void AnimatedTileImage_Unloaded(object sender, RoutedEventArgs e)
        {
            // Clear the timer because we don't need it anymore.
            _timer.Stop();
            _timer.Elapsed -= Timer_Elapsed;

            // force a GC collection
            GC.Collect(2, GCCollectionMode.Forced, true, true);
        }

        private void UpdateFrameRenderProperties()
        {
            if (FrameDirection == AnimatedTileImageFrameDirection.Horizontal)
            {
                _imageElement.RenderTransform = new TranslateTransform(
                    -1 * FrameWidth * CurrentFrame,
                    0
                );
            }
            else // Vertical
            {
                _imageElement.RenderTransform = new TranslateTransform(
                    0,
                    -1 * FrameHeight * CurrentFrame
                );
            }
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (Application.Current == null || _paused) return;
            if (_frameCount > 0)
            {
                Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    if (Loop)
                    {
                        CurrentFrame = (CurrentFrame + 1) % _frameCount;
                    }
                    else
                    {
                        if (CurrentFrame < _frameCount - 1)
                        {
                            CurrentFrame++;
                        }
                    }
                });
            }
        }
        public void Reset()
        {
            CurrentFrame = 0;
            _timer.Start();
            _paused = false;
        }

        public void Pause()
        {
            _timer.Stop();
            _paused = true;
        }

        public void Play()
        {
            _timer.Start();
            _paused = false;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            return new Size(FrameWidth, FrameHeight);
        }
    }
}
