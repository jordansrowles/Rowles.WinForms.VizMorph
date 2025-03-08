using System.ComponentModel;

namespace Rowles.WinForms.VizMorph
{
    public partial class MorphingChart : UserControl
    {
		/// <summary>
		/// Gets or sets the data points to be displayed.
		/// </summary>
		[Category("Data")]
		[Description("The data points to be displayed.")]
		public List<float> DataPoints
		{
			get => new List<float>(_dataPoints);
			set
			{
				_dataPoints = value;
				int requiredLeft = CalculateRequiredLeftPadding();
				if (requiredLeft != Padding.Left)
					Padding = new Padding(requiredLeft, Padding.Top, Padding.Right, Padding.Bottom);
				Invalidate();
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether the chart is displayed as a bar chart.
		/// If false, a line chart is displayed.
		/// </summary>
		[Category("Behavior")]
		[Description("Indicates whether the chart is displayed as a bar chart. If false, a line chart is displayed.")]
		public bool IsBarChart
		{
			get => _isBarChart;
			set
			{
				if (_isBarChart == value)
					return;
				_isBarChart = value;
				_animationTimer.Start();
			}
		}

		/// <summary>
		/// Gets or sets the color of the line in line chart mode.
		/// </summary>
		[Category("Appearance")]
		[Description("The color of the line used in line chart mode.")]
		public Color LineColor
		{
			get => _lineColor;
			set { _lineColor = value; Invalidate(); }
		}

		/// <summary>
		/// Gets or sets the color of the bars in bar chart mode.
		/// </summary>
		[Category("Appearance")]
		[Description("The color of the bars used in bar chart mode.")]
		public Color BarColor
		{
			get => _barColor;
			set { _barColor = value; Invalidate(); }
		}

		/// <summary>
		/// Gets or sets the color of the axes.
		/// </summary>
		[Category("Appearance")]
		[Description("The color of the axes (both X and Y).")]
		public Color AxisColor
		{
			get => _axisColor;
			set { _axisColor = value; Invalidate(); }
		}

		/// <summary>
		/// Gets or sets the color of the markers used to denote data points in line chart mode.
		/// </summary>
		[Category("Appearance")]
		[Description("The color of the markers used in line chart mode.")]
		public Color MarkerColor
		{
			get => _markerColor;
			set { _markerColor = value; Invalidate(); }
		}

		/// <summary>
		/// Gets or sets the size (diameter) of the markers in line chart mode.
		/// </summary>
		[Category("Appearance")]
		[Description("The size (diameter) of the markers in line chart mode.")]
		public float MarkerSize
		{
			get => _markerSize;
			set { _markerSize = value; Invalidate(); }
		}

		/// <summary>
		/// Gets or sets the font used for rendering axis labels and value labels.
		/// </summary>
		[Category("Appearance")]
		[Description("The font used for rendering axis labels and value labels.")]
		public Font AxisFont
		{
			get => _axisFont;
			set
			{
				_axisFont = value;
				int requiredLeft = CalculateRequiredLeftPadding();
				if (requiredLeft != Padding.Left)
					Padding = new Padding(requiredLeft, Padding.Top, Padding.Right, Padding.Bottom);
				Invalidate();
			}
		}

		/// <summary>
		/// Gets or sets the animation interval in milliseconds.
		/// </summary>
		[Category("Behavior")]
		[Description("The interval in milliseconds between animation frames.")]
		public int AnimationInterval
		{
			get => _animationInterval;
			set
			{
				_animationInterval = value;
				_animationTimer.Interval = _animationInterval;
			}
		}

		/// <summary>
		/// Gets or sets the animation step used to increment or decrement the morph progress.
		/// </summary>
		[Category("Behavior")]
		[Description("The step value for animation transitions.")]
		public float AnimationStep
		{
			get => _animationStep;
			set { _animationStep = value; }
		}

		/// <summary>
		/// Gets or sets the pixel threshold used for detecting proximity to a data point.
		/// </summary>
		[Category("Behavior")]
		[Description("The pixel threshold within which the mouse is considered to be over a data point.")]
		public int TooltipThreshold
		{
			get => _tooltipThreshold;
			set { _tooltipThreshold = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether X-axis labels are displayed.
		/// </summary>
		[Category("Appearance")]
		[Description("Indicates whether X-axis labels (data point indices) are displayed.")]
		public bool ShowXAxisLabels
		{
			get => _showXAxisLabels;
			set { _showXAxisLabels = value; Invalidate(); }
		}

		/// <summary>
		/// Gets or sets a value indicating whether Y-axis labels are displayed.
		/// </summary>
		[Category("Appearance")]
		[Description("Indicates whether Y-axis labels (min, max, and zero) are displayed.")]
		public bool ShowYAxisLabels
		{
			get => _showYAxisLabels;
			set { _showYAxisLabels = value; Invalidate(); }
		}

		/// <summary>
		/// Gets or sets the title of the chart.
		/// </summary>
		[Category("Appearance")]
		[Description("The title of the chart displayed at the top.")]
		public string ChartTitle
		{
			get => _chartTitle;
			set { _chartTitle = value; Invalidate(); }
		}

		/// <summary>
		/// Gets or sets the timeout for tooltips in milliseconds.
		/// </summary>
		[Category("Behavior")]
		[Description("The duration (in milliseconds) that a tooltip is displayed when hovering over a data point.")]
		public int TooltipTimeout
		{
			get => _tooltipTimeout;
			set { _tooltipTimeout = value; }
		}

		/// <summary>
		/// Gets or sets the vertical spacing (in pixels) between the chart title and the chart area.
		/// </summary>
		[Category("Appearance")]
		[Description("The vertical spacing (in pixels) between the chart title and the chart area.")]
		public int TitleMargin
		{
			get => _titleMargin;
			set { _titleMargin = value; Invalidate(); }
		}

		/// <summary>
		/// Gets or sets a value indicating whether to display value labels on the chart.
		/// When enabled, value labels are drawn on or near the data points instead of using hover tooltips.
		/// </summary>
		[Category("Appearance")]
		[Description("When enabled, value labels are drawn on or near the data points instead of using hover tooltips.")]
		public bool ShowValues
		{
			get => _showValues;
			set
			{
				_showValues = value;
				// Hide any active tooltip if we are now showing permanent labels.
				if (_showValues)
					_toolTip.Hide(this);
				Invalidate();
			}
		}

		
	}
}
