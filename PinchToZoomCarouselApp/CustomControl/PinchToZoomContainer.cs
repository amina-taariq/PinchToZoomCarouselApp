using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace PinchToZoomCarouselApp.CustomControls;

public class PinchToZoomContainer : ContentView
{
    private double _currentScale = 1;
    private double _startScale = 1;
    private double _xOffset = 0;
    private double _yOffset = 0;
    private bool _secondDoubleTapp = false;
    private double _xOffsetCorrection = 0;
    private double _yOffsetCorrection = 0;

    private double _originalScale = 1;
    private double _originalXOffset = 0;
    private double _originalYOffset = 0;

    private PanGestureRecognizer _panGesture;

    public PinchToZoomContainer()
    {
        var pinchGesture = new PinchGestureRecognizer();
        pinchGesture.PinchUpdated += PinchUpdated;
        GestureRecognizers.Add(pinchGesture);

        _panGesture = new PanGestureRecognizer();
        _panGesture.PanUpdated += OnPanUpdated;

        var tapGesture = new TapGestureRecognizer { NumberOfTapsRequired = 2 };
        tapGesture.Tapped += DoubleTapped;
        GestureRecognizers.Add(tapGesture);
    }

    private async void PinchUpdated(object? sender, PinchGestureUpdatedEventArgs e)
    {
        switch (e.Status)
        {
            case GestureStatus.Started:
                _startScale = Content.Scale;
                Content.AnchorX = 0;
                Content.AnchorY = 0;

                _originalScale = Content.Scale;
                _originalXOffset = Content.TranslationX;
                _originalYOffset = Content.TranslationY;
                break;

            case GestureStatus.Running:
                {
                    _currentScale += (e.Scale - 1) * _startScale;
                    _currentScale = Math.Max(1, _currentScale);
                    if (_currentScale > 1 && !GestureRecognizers.Contains(_panGesture))
                    {
                        GestureRecognizers.Add(_panGesture);
                    }

                    var renderedX = Content.X + _xOffset;
                    var deltaX = renderedX / Width;
                    var deltaWidth = Width / (Content.Width * _startScale);
                    var originX = (e.ScaleOrigin.X - deltaX) * deltaWidth;

                    var renderedY = Content.Y + _yOffset;
                    var deltaY = renderedY / Height;
                    var deltaHeight = Height / (Content.Height * _startScale);
                    var originY = (e.ScaleOrigin.Y - deltaY) * deltaHeight;

                    var targetX = _xOffset - (originX * Content.Width) * (_currentScale - _startScale);
                    var targetY = _yOffset - (originY * Content.Height) * (_currentScale - _startScale);

                    Content.TranslationX = Math.Min(0, Math.Max(targetX, -Content.Width * (_currentScale - 1)));
                    Content.TranslationY = Math.Min(0, Math.Max(targetY, -Content.Height * (_currentScale - 1)));

                    Content.Scale = _currentScale;
                    break;
                }

            case GestureStatus.Completed:
                await AnimateToOriginalPosition();
                break;
        }
    }

    private async Task AnimateToOriginalPosition()
    {
        var animationTasks = new List<Task>
        {
            Content.ScaleTo(_originalScale, 300, Easing.CubicOut),
            Content.TranslateTo(_originalXOffset, _originalYOffset, 300, Easing.CubicOut)
        };

        await Task.WhenAll(animationTasks);

        _xOffset = _originalXOffset;
        _yOffset = _originalYOffset;
        _currentScale = _originalScale;

        if (_currentScale <= 1 && GestureRecognizers.Contains(_panGesture))
        {
            GestureRecognizers.Remove(_panGesture);
        }
    }

    View? ParentView => this.Parent as View;

    public void OnPanUpdated(object? sender, PanUpdatedEventArgs e)
    {
        if (Content.Scale <= 1)
        {
            return;
        }

        switch (e.StatusType)
        {
            case GestureStatus.Started:
                break;
            case GestureStatus.Running:

                var newX = (e.TotalX * Scale) + _xOffset - _xOffsetCorrection;
                var newY = (e.TotalY * Scale) + _yOffset - _yOffsetCorrection;

                var width = (Content.Width * Content.Scale);
                var height = (Content.Height * Content.Scale);

                var parentWidth = ParentView!.Width;
                var parentHeight = ParentView!.Height;

                var canMoveX = width > parentWidth;
                var canMoveY = height > parentHeight;

                if (canMoveX)
                {
                    var minX = (width - (parentWidth / 2)) * -1;
                    var maxX = Math.Min(parentWidth / 2, width / 2);

                    if (newX < minX)
                    {
                        newX = minX;
                    }

                    if (newX > maxX)
                    {
                        newX = maxX;
                    }
                }
                else
                {
                    newX = 0;
                }

                if (canMoveY)
                {
                    var minY = (height - (parentHeight / 2)) * -1;
                    var maxY = Math.Min(parentHeight / 2, height / 2);

                    if (newY < minY)
                    {
                        newY = minY;
                    }

                    if (newY > maxY)
                    {
                        newY = maxY;
                    }
                }
                else
                {
                    newY = 0;
                }
                if (_xOffsetCorrection == 0 & _yOffsetCorrection == 0)
                {
                    _xOffsetCorrection = newX - _xOffset;
                    _yOffsetCorrection = newY - _yOffset;
                    Content.TranslationX = newX - _xOffsetCorrection;
                    Content.TranslationY = newY - _yOffsetCorrection;
                }
                else
                {
                    Content.TranslationX = newX;
                    Content.TranslationY = newY;
                }

                Content.TranslationX = newX;
                Content.TranslationY = newY;
                _currentScale = Content.Scale;
                _xOffsetCorrection = 0;
                _yOffsetCorrection = 0;
                break;
            case GestureStatus.Completed:
                _xOffset = Content.TranslationX;
                _yOffset = Content.TranslationY;
                break;
            case GestureStatus.Canceled:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public async void DoubleTapped(object? sender, TappedEventArgs e)
    {
        var multiplicator = Math.Pow(2, 1.0 / 10.0);
        _startScale = Content.Scale;
        Content.AnchorX = 0;
        Content.AnchorY = 0;

        for (var i = 0; i < 10; i++)
        {
            if (!_secondDoubleTapp)
            {
                _currentScale *= multiplicator;
            }
            else
            {
                _currentScale /= multiplicator;
            }

            var renderedX = Content.X + _xOffset;
            var deltaX = renderedX / Width;
            var deltaWidth = Width / (Content.Width * _startScale);
            var originX = (0.5 - deltaX) * deltaWidth;

            var renderedY = Content.Y + _yOffset;
            var deltaY = renderedY / Height;
            var deltaHeight = Height / (Content.Height * _startScale);
            var originY = (0.5 - deltaY) * deltaHeight;

            var targetX = _xOffset - (originX * Content.Width) * (_currentScale - _startScale);
            var targetY = _yOffset - (originY * Content.Height) * (_currentScale - _startScale);

            Content.TranslationX = Math.Min(0, Math.Max(targetX, -Content.Width * (_currentScale - 1)));
            Content.TranslationY = Math.Min(0, Math.Max(targetY, -Content.Height * (_currentScale - 1)));

            Content.Scale = _currentScale;
            await Task.Delay(10);
        }
        _secondDoubleTapp = !_secondDoubleTapp;
        _xOffset = Content.TranslationX;
        _yOffset = Content.TranslationY;

        if (_currentScale > 1 && !GestureRecognizers.Contains(_panGesture))
        {
            GestureRecognizers.Add(_panGesture);
        }
        else if (_currentScale <= 1 && GestureRecognizers.Contains(_panGesture))
        {
            GestureRecognizers.Remove(_panGesture);
        }
    }
}