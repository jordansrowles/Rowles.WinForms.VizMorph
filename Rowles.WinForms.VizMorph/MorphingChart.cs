using System.ComponentModel;
using Timer = System.Windows.Forms.Timer;

namespace Rowles.WinForms.VizMorph
{

    /// <summary>
    /// A versatile chart control that displays data as either a line or bar chart.
    /// It supports smooth animations, negative values, axes rendering, and many customizable appearance options.
    /// When <see cref="ShowValues"/> is enabled, value labels are permanently drawn on the chart.
    /// </summary>
    public partial class MorphingChart : UserControl
    {
        // Core functionality fields
        private List<float> _dataPoints = new List<float>();
        private bool _isBarChart;
        private float _morphProgress;
        private Timer _animationTimer;
        private ToolTip _toolTip;
        // Stores the bar rectangles for hit-testing and label placement in bar mode
        private List<RectangleF> _barRectangles = new List<RectangleF>();

        // Default constants
        private const int DEFAULT_ANIMATION_INTERVAL = 16; // ~60 FPS
        private const float DEFAULT_ANIMATION_STEP = 0.05f;
        private const int DEFAULT_TOOLTIP_THRESHOLD = 6;

        // Customization fields with default values
        private Color _lineColor = Color.OrangeRed;
        private Color _barColor = Color.SteelBlue;
        private Color _axisColor = Color.Black;
        private Color _markerColor = Color.OrangeRed;
        private float _markerSize = 6f;
        private Font _axisFont = new Font("Arial", 8);
        private int _animationInterval = DEFAULT_ANIMATION_INTERVAL;
        private float _animationStep = DEFAULT_ANIMATION_STEP;
        private int _tooltipThreshold = DEFAULT_TOOLTIP_THRESHOLD;
        private bool _showXAxisLabels = true;
        private bool _showYAxisLabels = true;
        private string _chartTitle = string.Empty;
        private int _tooltipTimeout = 1000;
        private int _titleMargin = 20;
        private bool _showValues = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="MorphingChart"/> control.
        /// </summary>
        public MorphingChart()
        {
            DoubleBuffered = true;
            // Increase padding to allow space for axis labels.
            Padding = new Padding(40, 20, 20, 40);

            _animationTimer = new Timer { Interval = _animationInterval };
            _animationTimer.Tick += AnimationTimer_Tick;

            // Initialize tooltip and subscribe to mouse events for hit testing.
            _toolTip = new ToolTip();
            this.MouseMove += MorphingChart_MouseMove;
            this.MouseLeave += MorphingChart_MouseLeave;
        }

        #region Private Methods

        private int CalculateRequiredLeftPadding()
        {
            if (!_showYAxisLabels)
                return Padding.Left;

            float originalMinValue = _dataPoints.Count > 0 ? _dataPoints.Min() : -1;
            float originalMaxValue = _dataPoints.Count > 0 ? _dataPoints.Max() : 1;

            string maxLabel = originalMaxValue.ToString("F2");
            Size maxLabelSize = TextRenderer.MeasureText(maxLabel, _axisFont);

            string minLabel = originalMinValue.ToString("F2");
            Size minLabelSize = TextRenderer.MeasureText(minLabel, _axisFont);

            int maxWidth = Math.Max(maxLabelSize.Width, minLabelSize.Width);

            if (originalMinValue < 0 && originalMaxValue > 0)
            {
                string zeroLabel = "0";
                Size zeroLabelSize = TextRenderer.MeasureText(zeroLabel, _axisFont);
                maxWidth = Math.Max(maxWidth, zeroLabelSize.Width);
            }

            return maxWidth + 8; // Buffer of 8 pixels
        }

        /// <summary>
        /// Handles the animation timer tick event to update the morph progress and trigger repainting.
        /// </summary>
        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            // Determine target progress: 1 for bar chart, 0 for line chart.
            var target = _isBarChart ? 1f : 0f;
            _morphProgress = Math.Min(Math.Max(_morphProgress + (_isBarChart ? _animationStep : -_animationStep), 0), 1);

            // Stop the timer when the morph animation completes.
            if (Math.Abs(_morphProgress - target) < float.Epsilon)
                _animationTimer.Stop();

            Invalidate();
        }

        /// <summary>
        /// Returns the rectangle that represents the drawing area within the control, accounting for padding and title spacing.
        /// </summary>
        private RectangleF GetDrawingArea()
        {
            // If a title is provided, shift the drawing area down by the TitleMargin.
            int titleOffset = !string.IsNullOrEmpty(_chartTitle) ? _titleMargin : 0;
            return new RectangleF(
                Padding.Left,
                Padding.Top + titleOffset,
                Width - Padding.Horizontal,
                Height - Padding.Vertical - titleOffset);
        }

        /// <summary>
        /// Calculates the minimum and maximum values from the data points.
        /// Adjusts the range if all values are identical.
        /// </summary>
        /// <param name="minValue">The minimum value in the data set.</param>
        /// <param name="maxValue">The maximum value in the data set.</param>
        public void GetMinMax(out float drawingMinValue, out float drawingMaxValue)
        {
            if (_dataPoints.Count > 0)
            {
                drawingMinValue = _dataPoints.Min();
                drawingMaxValue = _dataPoints.Max();

                // Only adjust if all values are identical
                if (Math.Abs(drawingMaxValue - drawingMinValue) < float.Epsilon)
                {
                    // Handle negative values correctly
                    if (drawingMinValue >= 0)
                    {
                        drawingMaxValue += 1;
                    }
                    else
                    {
                        drawingMinValue -= 1;
                        drawingMaxValue += 1; //keep 0 in the display range for cases like -5, -5, -5
                    }
                }
            }
            else
            {
                drawingMinValue = -1;
                drawingMaxValue = 1;
            }
        }

        /// <summary>
        /// Computes the pixel positions for each data point based on the drawing area and data range.
        /// Interpolates between line (evenly spaced along the full width) and bar positions (centered in cells).
        /// </summary>
        /// <param name="drawingArea">The drawing area rectangle.</param>
        /// <returns>A list of points representing the data points on the chart.</returns>
        private List<PointF> GetDataPointPositions(RectangleF drawingArea)
        {
            List<PointF> points = new List<PointF>();
            int n = _dataPoints.Count;
            if (n == 0) return points;
            float lineXStep = (n > 1) ? drawingArea.Width / (n - 1) : 0;
            float barXStep = drawingArea.Width / (float)n;
            float drawingMinValue, drawingMaxValue;
            GetMinMax(out drawingMinValue, out drawingMaxValue);  // Get the *adjusted* min/max for drawing
            for (int i = 0; i < n; i++)
            {
                // In line mode, data points are at: drawingArea.Left + i * lineXStep.
                // In bar mode, they are centered in cells: drawingArea.Left + (i + 0.5) * barXStep.
                float lineX = (n > 1) ? drawingArea.Left + i * lineXStep : drawingArea.Left;
                float barX = drawingArea.Left + (i + 0.5f) * barXStep;
                float x = lineX * (1 - _morphProgress) + barX * _morphProgress;
                // Use drawingMinValue/drawingMaxValue for normalization:
                float normalized = (drawingMaxValue - drawingMinValue == 0) ? 0 : (_dataPoints[i] - drawingMinValue) / (drawingMaxValue - drawingMinValue);
                float y = drawingArea.Bottom - normalized * drawingArea.Height;
                points.Add(new PointF(x, y));
            }
            return points;
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the paint event to render the chart.
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            var drawingArea = GetDrawingArea();
            // Get *both* original and drawing min/max:
            GetMinMax(out float drawingMinValue, out float drawingMaxValue);  //For drawing
            float originalMinValue = _dataPoints.Count > 0 ? _dataPoints.Min() : -1; //For axis labels.
            float originalMaxValue = _dataPoints.Count > 0 ? _dataPoints.Max() : 1;  //For axis labels.


            var points = GetDataPointPositions(drawingArea);
            int n = _dataPoints.Count;

            // Determine the baseline for value 0, using the *original* min/max.
            float baselineY;
            if (originalMinValue > 0)
                baselineY = drawingArea.Bottom;
            else if (originalMaxValue < 0)
                baselineY = drawingArea.Top;
            else
            {
                float zeroPosition = (0 - originalMinValue) / (originalMaxValue - originalMinValue);
                baselineY = drawingArea.Bottom - zeroPosition * drawingArea.Height;
            }

            // Compute the X-axis Y coordinate: interpolate between baseline (line mode) and the bottom (bar mode).
            float xAxisY = baselineY * (1 - _morphProgress) + drawingArea.Bottom * _morphProgress;

            // Draw axes (no changes here)
            using (var axisPen = new Pen(_axisColor, 1))
            {
                g.DrawLine(axisPen, drawingArea.Left, drawingArea.Top, drawingArea.Left, drawingArea.Bottom);
                g.DrawLine(axisPen, drawingArea.Left, xAxisY, drawingArea.Right, xAxisY);
            }

            // Draw Y-axis tick marks and labels if enabled.
            if (_showYAxisLabels)
            {
                using (var brush = new SolidBrush(_axisColor))
                {
                    // Maximum value label, using originalMaxValue.
                    string maxLabel = originalMaxValue.ToString("F2");
                    SizeF maxLabelSize = g.MeasureString(maxLabel, _axisFont);
                    g.DrawString(maxLabel, _axisFont, brush, drawingArea.Left - maxLabelSize.Width - 4, drawingArea.Top - maxLabelSize.Height / 2);

                    // Minimum value label, using originalMinValue.
                    string minLabel = originalMinValue.ToString("F2");
                    SizeF minLabelSize = g.MeasureString(minLabel, _axisFont);
                    g.DrawString(minLabel, _axisFont, brush, drawingArea.Left - minLabelSize.Width - 4, drawingArea.Bottom - minLabelSize.Height / 2);

                    // Zero label if applicable, using baselineY calculated with original min/max.
                    if (originalMinValue < 0 && originalMaxValue > 0)
                    {
                        string zeroLabel = "0";
                        SizeF zeroLabelSize = g.MeasureString(zeroLabel, _axisFont);
                        g.DrawString(zeroLabel, _axisFont, brush, drawingArea.Left - zeroLabelSize.Width - 4, baselineY - zeroLabelSize.Height / 2);
                    }
                }
            }

            // Draw X-axis tick marks and labels if enabled.
            if (n > 0 && n <= 20 && _showXAxisLabels)
            {
                float lineXStep = (n > 1) ? drawingArea.Width / (n - 1) : 0;
                float barXStep = drawingArea.Width / (float)n;
                using (var brush = new SolidBrush(_axisColor))
                {
                    for (int i = 0; i < n; i++)
                    {
                        float lineX = (n > 1) ? drawingArea.Left + i * lineXStep : drawingArea.Left;
                        float barX = drawingArea.Left + (i + 0.5f) * barXStep;
                        float x = lineX * (1 - _morphProgress) + barX * _morphProgress;
                        g.DrawLine(Pens.Black, x, xAxisY - 3, x, xAxisY + 3);
                        string label = i.ToString();
                        SizeF labelSize = g.MeasureString(label, _axisFont);
                        g.DrawString(label, _axisFont, brush, x - labelSize.Width / 2, xAxisY + 4);
                    }
                }
            }

            // Draw chart title. Centered at the top with extra space below defined by TitleMargin.
            if (!string.IsNullOrEmpty(_chartTitle))
            {
                using (var titleFont = new Font("Arial", 10, FontStyle.Bold))
                using (var brush = new SolidBrush(_axisColor))
                {
                    SizeF titleSize = g.MeasureString(_chartTitle, titleFont);
                    float titleX = (Width - titleSize.Width) / 2;
                    // Position the title above the drawing area.
                    g.DrawString(_chartTitle, titleFont, brush, titleX, Padding.Top - titleSize.Height);
                }
            }

            // Clear previous bar rectangles.
            _barRectangles.Clear();

            // Draw bars if in bar chart mode.
            if (_morphProgress > 0 && points.Count > 0)
            {
                float barXStep = drawingArea.Width / (float)n;
                float barWidth = barXStep * 0.8f * _morphProgress;
                using (var barBrush = new SolidBrush(Color.FromArgb((int)(200 * _morphProgress), _barColor)))
                {
                    for (int i = 0; i < points.Count; i++)
                    {
                        var pt = points[i];
                        // Draw a bar centered at pt.X.
                        float barTop = Math.Min(pt.Y, xAxisY);
                        float barHeight = Math.Abs(xAxisY - pt.Y);
                        var barRect = new RectangleF(pt.X - barWidth / 2, barTop, barWidth, barHeight);
                        _barRectangles.Add(barRect);
                        g.FillRectangle(barBrush, barRect);
                    }
                }
            }

            // Draw line and markers if in line chart mode.
            if (_morphProgress < 1 && points.Count > 0)
            {
                using (var linePen = new Pen(Color.FromArgb((int)(200 * (1 - _morphProgress)), _lineColor), 2))
                {
                    if (points.Count > 1)
                        g.DrawLines(linePen, points.ToArray());
                }
                foreach (var pt in points)
                {
                    RectangleF markerRect = new RectangleF(pt.X - _markerSize / 2, pt.Y - _markerSize / 2, _markerSize, _markerSize);
                    g.FillEllipse(Brushes.White, markerRect);
                    g.DrawEllipse(new Pen(_markerColor), markerRect);
                }
            }

            // Draw value labels if ShowValues is enabled.
            if (_showValues)
            {
                // Use a temporary brush (could also add a separate property for label color).
                using (var valueBrush = new SolidBrush(_axisColor))
                {
                    // If in line mode (or if no bars are drawn), label near the marker.
                    if (_morphProgress < 0.5f || _barRectangles.Count == 0)
                    {
                        for (int i = 0; i < points.Count; i++)
                        {
                            string valueStr = _dataPoints[i].ToString("F2");
                            SizeF labelSize = TextRenderer.MeasureText(valueStr, _axisFont); // More accurate measurement

                            // Calculate safe X position
                            float labelX;
                            if (i == 0) // First label
                            {
                                labelX = points[i].X; // Align left edge with marker
                            }
                            else if (i == points.Count - 1) // Last label
                            {
                                labelX = points[i].X - labelSize.Width; // Align right edge with marker
                            }
                            else // Middle labels
                            {
                                labelX = points[i].X - labelSize.Width / 2; // Centered
                            }

                            // Ensure label stays within control bounds
                            labelX = Math.Clamp(labelX,
                                Padding.Left,
                                Width - Padding.Right - labelSize.Width);

                            PointF labelPos = new PointF(labelX, points[i].Y - labelSize.Height - 4);
                            g.DrawString(valueStr, _axisFont, valueBrush, labelPos);
                        }
                    }
                    // Inside the OnPaint method's bar chart label section:
                    else // Bar mode: position based on bar rectangle.
                    {
                        using (var brush = new SolidBrush(_axisColor))
                        {
                            // We need to iterate based on the *points*, not the rectangles directly.
                            for (int i = 0; i < points.Count; i++)
                            {
                                // Find the corresponding bar rectangle.
                                RectangleF barRect = _barRectangles[i];

                                float value = _dataPoints[i]; // Use the correct data point!

                                // Format value with trailing minus for negatives
                                string valueStr = value.ToString("F2");
                                if (value < 0)
                                {
                                    valueStr = $"-{Math.Abs(value):F2}";
                                }

                                float startX = barRect.X + barRect.Width / 2; // Center horizontally

                                // --- Calculate total text height ---
                                float totalTextHeight = 0;
                                foreach (char c in valueStr)
                                {
                                    SizeF charSize = g.MeasureString(c.ToString(), _axisFont);
                                    totalTextHeight += charSize.Width * 0.8f; // Accumulate widths (rotated)
                                }

                                // --- Determine startY based on bar and text height ---
                                float startY;
                                if (totalTextHeight <= barRect.Height)
                                {
                                    // Text fits INSIDE the bar
                                    startY = barRect.Top + Math.Min(20, barRect.Height / 2); // at most halfway down
                                }
                                else
                                {
                                    // Text is TALLER than the bar, position above
                                    startY = barRect.Top - totalTextHeight - 4; // Space above the bar
                                    startY = Math.Max(startY, Padding.Top); // Ensure it's not off-screen
                                }

                                // Calculate the starting position of the *last* character
                                float currentY = startY + totalTextHeight - (g.MeasureString(valueStr.Last().ToString(), _axisFont).Width * 0.8f);


                                // Draw each character vertically stacked
                                foreach (char c in valueStr)
                                {
                                    string charStr = c.ToString();
                                    SizeF charSize = g.MeasureString(charStr, _axisFont);

                                    // Save graphics state
                                    var state = g.Save();

                                    // Move origin to character position
                                    g.TranslateTransform(startX, currentY); // Use currentY

                                    // Rotate 90° counter-clockwise
                                    g.RotateTransform(-90);

                                    // Draw rotated character centered
                                    g.DrawString(charStr, _axisFont, brush,
                                        new RectangleF(
                                            -charSize.Width / 2,  // Center horizontally
                                            -charSize.Height / 2, // Center vertically
                                            charSize.Width,
                                            charSize.Height
                                        ),
                                        new StringFormat
                                        {
                                            Alignment = StringAlignment.Center,
                                            LineAlignment = StringAlignment.Center
                                        }
                                    );

                                    // Restore graphics state
                                    g.Restore(state);

                                    // --- NEW: DECREMENT currentY for the *next* character ---
                                    currentY -= charSize.Width * 0.8f; // Move UP
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// MouseMove event handler. When ShowValues is enabled, no tooltip is shown.
        /// Otherwise, tooltips are updated based on the mouse location.
        /// </summary>
        private void MorphingChart_MouseMove(object sender, MouseEventArgs e)
        {
            if (_showValues)
            {
                // If value labels are permanently shown, hide the tooltip.
                _toolTip.Hide(this);
                return;
            }
            // Existing tooltip logic can remain here if needed.
            var drawingArea = GetDrawingArea();
            var points = GetDataPointPositions(drawingArea);
            bool found = false;
            int tooltipIndex = -1;

            if (_isBarChart || _morphProgress >= 0.5)
            {
                for (int i = 0; i < _barRectangles.Count; i++)
                {
                    if (_barRectangles[i].Contains(e.Location))
                    {
                        tooltipIndex = i;
                        found = true;
                        break;
                    }
                }
            }
            if (!found)
            {
                for (int i = 0; i < points.Count; i++)
                {
                    var pt = points[i];
                    if (Math.Abs(e.X - pt.X) <= _tooltipThreshold && Math.Abs(e.Y - pt.Y) <= _tooltipThreshold)
                    {
                        tooltipIndex = i;
                        found = true;
                        break;
                    }
                }
            }
            if (found)
            {
                string tip = $"Value: {_dataPoints[tooltipIndex]:F2}";
                _toolTip.Show(tip, this, e.Location, _tooltipTimeout);
            }
            else
            {
                _toolTip.Hide(this);
            }
        }

        /// <summary>
        /// MouseLeave event handler to hide the tooltip when the mouse leaves the control.
        /// </summary>
        private void MorphingChart_MouseLeave(object sender, EventArgs e)
        {
            _toolTip.Hide(this);
        }

        #endregion
    }
}
