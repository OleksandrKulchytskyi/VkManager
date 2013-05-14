using Carousel.Helpers;
using Expression.Samples.PathListBoxUtils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using System.Windows.Media.Animation;

namespace Carousel
{
	public enum PathType { Ellipse, Wave, Diagonal, ZigZag, Custom };
	public enum ButtonPosition
	{
		TopLeft, TopCenter, TopRight, LeftCenter,
		RightCenter, BottomLeft, BottomCenter, BottomRight
	};

	/// <summary>
	/// The Carousel itself is based on PathListBox and PathListBoxUtils 
	/// using the codeplex available PathListBoxutils
	/// http://expressionblend.codeplex.com/wikipage?title=CarouselWPFTestAppUtils&referringTitle=Documentation
	/// 
	/// This article is also useful to see how the basic Carousel is made
	/// http://www.microsoft.com/design/toolbox/tutorials/CarouselWPFTestApp/carousel.aspx
	/// </summary>
	public partial class CarouselControl : UserControl
	{
		#region Data
		private List<PathListBoxItemTransformer> transformers = new List<PathListBoxItemTransformer>();
		private PathListBoxScrollBehavior pathListBoxScrollBehavior;
		#endregion

		#region Ctor
		public CarouselControl()
		{
			InitializeComponent();
			this.Loaded += CarouselControl_Loaded;
			pathListBox.SelectionChanged += PathListBox_SelectionChanged;
		}

		#endregion

		#region Private Methods

		private static bool IsValidNumberOfItemsOnPath(object value)
		{
			int v = (int)value;
			return (!v.Equals(int.MinValue) && !v.Equals(int.MaxValue));
		}

		private void SetButtonPosition(ButtonPosition buttonPosition)
		{
			switch (buttonPosition)
			{
				case ButtonPosition.TopLeft:
					spButtons.VerticalAlignment = VerticalAlignment.Top;
					spButtons.HorizontalAlignment = HorizontalAlignment.Left;
					spButtons.Orientation = System.Windows.Controls.Orientation.Horizontal;
					break;
				case ButtonPosition.TopCenter:
					spButtons.VerticalAlignment = VerticalAlignment.Top;
					spButtons.HorizontalAlignment = HorizontalAlignment.Center;
					spButtons.Orientation = System.Windows.Controls.Orientation.Horizontal;
					break;
				case ButtonPosition.TopRight:
					spButtons.VerticalAlignment = VerticalAlignment.Top;
					spButtons.HorizontalAlignment = HorizontalAlignment.Right;
					spButtons.Orientation = System.Windows.Controls.Orientation.Horizontal;
					break;
				case ButtonPosition.LeftCenter:
					spButtons.VerticalAlignment = VerticalAlignment.Center;
					spButtons.HorizontalAlignment = HorizontalAlignment.Left;
					spButtons.Orientation = System.Windows.Controls.Orientation.Vertical;
					break;
				case ButtonPosition.RightCenter:
					spButtons.VerticalAlignment = VerticalAlignment.Center;
					spButtons.HorizontalAlignment = HorizontalAlignment.Right;
					spButtons.Orientation = System.Windows.Controls.Orientation.Vertical;
					break;
				case ButtonPosition.BottomLeft:
					spButtons.VerticalAlignment = VerticalAlignment.Bottom;
					spButtons.HorizontalAlignment = HorizontalAlignment.Left;
					spButtons.Orientation = System.Windows.Controls.Orientation.Horizontal;
					break;
				case ButtonPosition.BottomCenter:
					spButtons.VerticalAlignment = VerticalAlignment.Bottom;
					spButtons.HorizontalAlignment = HorizontalAlignment.Center;
					spButtons.Orientation = System.Windows.Controls.Orientation.Horizontal;
					break;
				case ButtonPosition.BottomRight:
					spButtons.VerticalAlignment = VerticalAlignment.Bottom;
					spButtons.HorizontalAlignment = HorizontalAlignment.Right;
					spButtons.Orientation = System.Windows.Controls.Orientation.Horizontal;
					break;

			}
		}

		private void SetVisibilityForPath(PathType pathType)
		{
			foreach (UIElement uiElement in gridForKnownPaths.Children)
			{
				uiElement.Visibility = Visibility.Collapsed;
			}

			switch (pathType)
			{
				case PathType.Ellipse:
					this.ellipsePath.Visibility = Visibility.Visible;
					pathListBox.LayoutPaths[0].SourceElement = this.ellipsePath;
					break;
				case PathType.Wave:
					this.wavePath.Visibility = Visibility.Visible;
					pathListBox.LayoutPaths[0].SourceElement = this.wavePath;
					break;
				case PathType.Diagonal:
					this.diagonalPath.Visibility = Visibility.Visible;
					pathListBox.LayoutPaths[0].SourceElement = this.diagonalPath;
					break;
				case PathType.ZigZag:
					this.zigzagPath.Visibility = Visibility.Visible;
					pathListBox.LayoutPaths[0].SourceElement = this.zigzagPath;
					break;
				case PathType.Custom:
					pathListBox.LayoutPaths[0].SourceElement = CustomPathElement;
					break;

			}
		}

		private void CarouselControl_Loaded(object sender, RoutedEventArgs e)
		{
			BehaviorCollection behaviors = Interaction.GetBehaviors(pathListBox);
			pathListBoxScrollBehavior = (PathListBoxScrollBehavior)behaviors
				.Where(x => x.GetType() == typeof(PathListBoxScrollBehavior)).FirstOrDefault();
		}

		private void PathListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			RaiseSelectionChangedEvent(e);
		}

		private void PathListBoxItemTransformer_Loaded(object sender, RoutedEventArgs e)
		{
			PathListBoxItemTransformer trans = sender as PathListBoxItemTransformer;
			trans.Ease = AnimationEaseIn;
			trans.OpacityRange = OpacityRange;
			trans.ScaleRange = ScaleRange;
			trans.AngleRange = AngleRange;
			transformers.Add(trans);
		}

		/// <summary>
		/// The pathListBox transitioning did not seem to work when I used an ICommand
		/// could be it was firing to often and causing some mischief to happen
		/// </summary>
		private void PreviousButton_Click(object sender, RoutedEventArgs e)
		{
			if (pathListBox.ItemsSource == null ||
				(pathListBox.Items.Count == 0))
				return;

			if (pathListBox.SelectedIndex > 0)
			{
				pathListBox.SelectedIndex = pathListBox.SelectedIndex - 1;
			}
			else
			{
				if ((pathListBox.Items.Count - 1) < 0) return;
				pathListBox.SelectedIndex = pathListBox.Items.Count - 1;
			}

			pathListBox.SelectedItem = pathListBox.Items[pathListBox.SelectedIndex];
		}

		/// <summary>
		/// The pathListBox transitioning did not seem to work when I used an ICommand
		/// could be it was firing to often and causing some mischief to happen
		/// </summary>
		private void NextButton_Click(object sender, RoutedEventArgs e)
		{
			if (pathListBox.ItemsSource == null || (pathListBox.Items.Count == 0))
				return;

			if (pathListBox.SelectedIndex < pathListBox.Items.Count - 1)
			{
				pathListBox.SelectedIndex = pathListBox.SelectedIndex + 1;
			}
			else
			{
				pathListBox.SelectedIndex = 0;
			}
			pathListBox.SelectedItem = pathListBox.Items[pathListBox.SelectedIndex];
		}

		#endregion

		#region Events

		#region SelectionChanged

		/// <summary>
		/// SelectionChanged Routed Event, which is raised when
		/// the internal PathListBox.SelectionChanged event occurs
		/// </summary>
		public static readonly RoutedEvent SelectionChangedEvent =
			EventManager.RegisterRoutedEvent("SelectionChanged",
			RoutingStrategy.Bubble, typeof(SelectionChangedEventHandler),
			typeof(CarouselControl));


		public event SelectionChangedEventHandler SelectionChanged
		{
			add { AddHandler(SelectionChangedEvent, value); }
			remove { RemoveHandler(SelectionChangedEvent, value); }
		}

		private SelectionChangedEventArgs RaiseSelectionChangedEvent(SelectionChangedEventArgs arg)
		{
			arg.RoutedEvent = SelectionChangedEvent;
			RoutedEventHelper.RaiseEvent(this, arg);
			return arg;
		}

		#endregion

		#endregion

		#region DPs

		#region CustomPathElement

		/// <summary>
		/// The FrameworkElement to use as custom path for Carousel
		/// </summary>
		public static readonly DependencyProperty CustomPathElementProperty =
			DependencyProperty.Register("CustomPathElement", typeof(FrameworkElement), typeof(CarouselControl),
				new FrameworkPropertyMetadata((FrameworkElement)null,
					new PropertyChangedCallback(OnCustomPathElementChanged)));

		public FrameworkElement CustomPathElement
		{
			get { return (FrameworkElement)GetValue(CustomPathElementProperty); }
			set { SetValue(CustomPathElementProperty, value); }
		}

		private static void OnCustomPathElementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((CarouselControl)d).OnCustomPathElementChanged(e);
		}

		protected virtual void OnCustomPathElementChanged(DependencyPropertyChangedEventArgs e)
		{
			if (e.NewValue != null)
				SetVisibilityForPath(PathType.Custom);
		}

		#endregion

		#region NavigationButtonPosition

		/// <summary>
		/// The Navigation ButtonPosition to use for the Carousel
		/// </summary>
		public static readonly DependencyProperty NavigationButtonPositionProperty =
			DependencyProperty.Register("NavigationButtonPosition", typeof(ButtonPosition), typeof(CarouselControl),
				new FrameworkPropertyMetadata((ButtonPosition)ButtonPosition.BottomCenter,
					new PropertyChangedCallback(OnNavigationButtonPositionChanged)));

		public ButtonPosition NavigationButtonPosition
		{
			get { return (ButtonPosition)GetValue(NavigationButtonPositionProperty); }
			set { SetValue(NavigationButtonPositionProperty, value); }
		}

		private static void OnNavigationButtonPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((CarouselControl)d).OnNavigationButtonPositionChanged(e);
		}

		protected virtual void OnNavigationButtonPositionChanged(DependencyPropertyChangedEventArgs e)
		{
			if (e.NewValue != null)
				SetButtonPosition((ButtonPosition)e.NewValue);
		}

		#endregion

		#region PreviousButtonStyle

		/// <summary>
		/// Can be used to specify a new Style for Previous button to use for the Carousel
		/// </summary>
		public static readonly DependencyProperty PreviousButtonStyleProperty =
			DependencyProperty.Register("PreviousButtonStyle", typeof(Style), typeof(CarouselControl),
				new FrameworkPropertyMetadata((Style)null,
					new PropertyChangedCallback(OnPreviousButtonStyleChanged)));

		public Style PreviousButtonStyle
		{
			get { return (Style)GetValue(PreviousButtonStyleProperty); }
			set { SetValue(PreviousButtonStyleProperty, value); }
		}

		private static void OnPreviousButtonStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((CarouselControl)d).OnPreviousButtonStyleChanged(e);
		}

		protected virtual void OnPreviousButtonStyleChanged(DependencyPropertyChangedEventArgs e)
		{
			if (e.NewValue != null)
				this.previousButton.Style = (Style)e.NewValue;
		}

		#endregion

		#region NextButtonStyle

		/// <summary>
		/// Can be used to specify a new Style for Next button to use for the Carousel
		/// </summary>
		public static readonly DependencyProperty NextButtonStyleProperty =
			DependencyProperty.Register("NextButtonStyle", typeof(Style), typeof(CarouselControl),
				new FrameworkPropertyMetadata((Style)null,
					new PropertyChangedCallback(OnNextButtonStyleChanged)));

		public Style NextButtonStyle
		{
			get { return (Style)GetValue(NextButtonStyleProperty); }
			set { SetValue(NextButtonStyleProperty, value); }
		}

		private static void OnNextButtonStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((CarouselControl)d).OnNextButtonStyleChanged(e);
		}

		protected virtual void OnNextButtonStyleChanged(DependencyPropertyChangedEventArgs e)
		{
			if (e.NewValue != null)
				this.nextButton.Style = (Style)e.NewValue;
		}

		#endregion

		#region PathType

		/// <summary>
		/// PathType Dependency Property
		/// </summary>
		public static readonly DependencyProperty PathTypeProperty =
			DependencyProperty.Register("PathType", typeof(PathType), typeof(CarouselControl),
				new FrameworkPropertyMetadata((PathType)PathType.Custom,
					new PropertyChangedCallback(OnPathTypeChanged)));

		public PathType PathType
		{
			get { return (PathType)GetValue(PathTypeProperty); }
			set { SetValue(PathTypeProperty, value); }
		}

		private static void OnPathTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((CarouselControl)d).OnPathTypeChanged(e);
		}

		protected virtual void OnPathTypeChanged(DependencyPropertyChangedEventArgs e)
		{
			SetVisibilityForPath((PathType)e.NewValue);
		}

		#endregion

		#region AnimationEaseIn

		/// <summary>
		/// AnimationEaseIn Dependency Property
		/// </summary>
		public static readonly DependencyProperty AnimationEaseInProperty =
			DependencyProperty.Register("AnimationEaseIn", typeof(EasingFunctionBase), typeof(CarouselControl),
				new FrameworkPropertyMetadata((EasingFunctionBase)null,
					new PropertyChangedCallback(OnAnimationEaseInChanged)));

		public EasingFunctionBase AnimationEaseIn
		{
			get { return (EasingFunctionBase)GetValue(AnimationEaseInProperty); }
			set { SetValue(AnimationEaseInProperty, value); }
		}

		private static void OnAnimationEaseInChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((CarouselControl)d).OnAnimationEaseInChanged(e);
		}

		protected virtual void OnAnimationEaseInChanged(DependencyPropertyChangedEventArgs e)
		{
			if (e.NewValue != null)
			{
				foreach (PathListBoxItemTransformer pathListBoxItemTransformer in transformers)
				{
					pathListBoxItemTransformer.Ease = (EasingFunctionBase)e.NewValue;
				}
			}
		}

		#endregion

		#region AnimationEaseOut

		/// <summary>
		/// AnimationEaseOut Dependency Property
		/// </summary>
		public static readonly DependencyProperty AnimationEaseOutProperty =
			DependencyProperty.Register("AnimationEaseOut", typeof(EasingFunctionBase), typeof(CarouselControl),
				new FrameworkPropertyMetadata((EasingFunctionBase)null,
					new PropertyChangedCallback(OnAnimationEaseOutChanged)));

		public EasingFunctionBase AnimationEaseOut
		{
			get { return (EasingFunctionBase)GetValue(AnimationEaseOutProperty); }
			set { SetValue(AnimationEaseOutProperty, value); }
		}

		private static void OnAnimationEaseOutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((CarouselControl)d).OnAnimationEaseOutChanged(e);
		}

		protected virtual void OnAnimationEaseOutChanged(DependencyPropertyChangedEventArgs e)
		{
			if (e.NewValue != null)
			{
				if (pathListBoxScrollBehavior != null)
					pathListBoxScrollBehavior.Ease = (EasingFunctionBase)e.NewValue;
			}
		}

		#endregion

		#region OpacityRange

		/// <summary>
		/// OpacityRange to use for PathListBoxItemTransformer
		/// </summary>
		public static readonly DependencyProperty OpacityRangeProperty =
			DependencyProperty.Register("OpacityRange", typeof(Point), typeof(CarouselControl),
				new FrameworkPropertyMetadata((Point)new Point(0.7, 1.0),
					new PropertyChangedCallback(OnOpacityRangeChanged)));

		public Point OpacityRange
		{
			get { return (Point)GetValue(OpacityRangeProperty); }
			set { SetValue(OpacityRangeProperty, value); }
		}

		/// <summary>
		/// Handles changes to the OpacityRange property.
		/// </summary>
		private static void OnOpacityRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((CarouselControl)d).OnOpacityRangeChanged(e);
		}

		/// <summary>
		/// Provides derived classes an opportunity to handle changes to the OpacityRange property.
		/// </summary>
		protected virtual void OnOpacityRangeChanged(DependencyPropertyChangedEventArgs e)
		{
			foreach (PathListBoxItemTransformer pathListBoxItemTransformer in transformers)
			{
				pathListBoxItemTransformer.OpacityRange = (Point)e.NewValue;
			}

		}

		#endregion

		#region ScaleRange

		/// <summary>
		/// ScaleRange to use for PathListBoxItemTransformer
		/// </summary>
		public static readonly DependencyProperty ScaleRangeProperty =
			DependencyProperty.Register("ScaleRange", typeof(Point), typeof(CarouselControl),
				new FrameworkPropertyMetadata((Point)new Point(1.0, 3.0),
					new PropertyChangedCallback(OnScaleRangeChanged)));

		public Point ScaleRange
		{
			get { return (Point)GetValue(ScaleRangeProperty); }
			set { SetValue(ScaleRangeProperty, value); }
		}

		/// <summary>
		/// Handles changes to the ScaleRange property.
		/// </summary>
		private static void OnScaleRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((CarouselControl)d).OnScaleRangeChanged(e);
		}

		/// <summary>
		/// Provides derived classes an opportunity to handle changes to the ScaleRange property.
		/// </summary>
		protected virtual void OnScaleRangeChanged(DependencyPropertyChangedEventArgs e)
		{
			foreach (PathListBoxItemTransformer pathListBoxItemTransformer in transformers)
			{
				pathListBoxItemTransformer.ScaleRange = (Point)e.NewValue;
			}

		}

		#endregion

		#region AngleRange

		/// <summary>
		/// AngleRange to use for PathListBoxItemTransformer
		/// </summary>
		public static readonly DependencyProperty AngleRangeProperty =
			DependencyProperty.Register("AngleRange", typeof(Point), typeof(CarouselControl),
				new FrameworkPropertyMetadata((Point)new Point(0.0, 0.0),
					new PropertyChangedCallback(OnAngleRangeChanged)));

		public Point AngleRange
		{
			get { return (Point)GetValue(AngleRangeProperty); }
			set { SetValue(AngleRangeProperty, value); }
		}

		/// <summary>
		/// Handles changes to the AngleRange property.
		/// </summary>
		private static void OnAngleRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((CarouselControl)d).OnAngleRangeChanged(e);
		}

		/// <summary>
		/// Provides derived classes an opportunity to handle changes to the AngleRange property.
		/// </summary>
		protected virtual void OnAngleRangeChanged(DependencyPropertyChangedEventArgs e)
		{
			foreach (PathListBoxItemTransformer pathListBoxItemTransformer in transformers)
			{
				pathListBoxItemTransformer.AngleRange = (Point)e.NewValue;
			}

		}

		#endregion


		//Path ListBox wrapped properties


		#region DataTemplateToUse
		/// <summary>
		/// The DataTemplate to use for the Carousel
		/// </summary>
		[Bindable(true)]
		public static readonly DependencyProperty DataTemplateToUseProperty =
			DependencyProperty.Register("DataTemplateToUse", typeof(DataTemplate), typeof(CarouselControl),
				new FrameworkPropertyMetadata((DataTemplate)null,
					new PropertyChangedCallback(OnDataTemplateToUseChanged)));

		public DataTemplate DataTemplateToUse
		{
			get { return (DataTemplate)GetValue(DataTemplateToUseProperty); }
			set { SetValue(DataTemplateToUseProperty, value); }
		}

		private static void OnDataTemplateToUseChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((CarouselControl)d).OnDataTemplateToUseChanged(e);
		}

		protected virtual void OnDataTemplateToUseChanged(DependencyPropertyChangedEventArgs e)
		{
			if (e.NewValue != null)
				this.pathListBox.ItemTemplate = (DataTemplate)e.NewValue;
		}
		#endregion

		#region SelectedItem

		/// <summary>
		/// SelectedItem Dependency Property
		/// </summary>
		public static readonly DependencyProperty SelectedItemProperty =
			DependencyProperty.Register("SelectedItem", typeof(Object), typeof(CarouselControl),
				new FrameworkPropertyMetadata((Object)null,
					new PropertyChangedCallback(OnSelectedItemChanged)));

		public Object SelectedItem
		{
			get { return (Object)pathListBox.GetValue(SelectedItemProperty); }
			set { pathListBox.SetValue(SelectedItemProperty, value); }
		}

		private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((CarouselControl)d).OnSelectedItemChanged(e);
		}

		protected virtual void OnSelectedItemChanged(DependencyPropertyChangedEventArgs e)
		{
			object currentlySelected = pathListBox.SelectedItem;
			if (currentlySelected != e.NewValue)
			{
				pathListBox.SelectedItem = e.NewValue;
			}
		}



		#endregion

		#region ItemsSource

		/// <summary>
		/// The ItemsSource to use for the Carousel
		/// </summary>
		[Bindable(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public static readonly DependencyProperty ItemsSourceProperty =
			DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(CarouselControl),
				new FrameworkPropertyMetadata((IEnumerable)null,
					new PropertyChangedCallback(OnItemsSourceChanged)));

		public IEnumerable ItemsSource
		{
			get { return (IEnumerable)GetValue(ItemsSourceProperty); }
			set { SetValue(ItemsSourceProperty, value); }
		}

		private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((CarouselControl)d).OnItemsSourceChanged(e);
		}

		protected virtual void OnItemsSourceChanged(DependencyPropertyChangedEventArgs e)
		{
			if (e.NewValue != null)
			{
				transformers.Clear();
				this.pathListBox.ItemsSource = (IEnumerable)e.NewValue;
			}
		}

		#endregion

		#region NumberOfItemsOnPath



		/// <summary>
		/// NumberOfItemsOnPath
		/// </summary>
		public static readonly DependencyProperty NumberOfItemsOnPathProperty = DependencyProperty.Register(
			"NumberOfItemsOnPath",
			typeof(int),
			typeof(CarouselControl),
			new FrameworkPropertyMetadata(
				7,
				FrameworkPropertyMetadataOptions.None,
				new PropertyChangedCallback(OnNumberOfItemsOnPathChanged),
				new CoerceValueCallback(CoerceNumberOfItemsOnPath)
			),
			new ValidateValueCallback(IsValidNumberOfItemsOnPath)
		);

		//property accessors
		public int NumberOfItemsOnPath
		{
			get { return (int)GetValue(NumberOfItemsOnPathProperty); }
			set { SetValue(NumberOfItemsOnPathProperty, value); }
		}


		/// <summary>
		/// Coerce NumberOfItemsOnPath value if not within limits
		/// </summary>
		private static object CoerceNumberOfItemsOnPath(DependencyObject d, object value)
		{
			CarouselControl depObj = (CarouselControl)d;
			int current = (int)value;
			if (current < depObj.MinNumberOfItemsOnPath) current = depObj.MinNumberOfItemsOnPath;
			if (current > depObj.MaxNumberOfItemsOnPath) current = depObj.MaxNumberOfItemsOnPath;
			return current;
		}

		private static void OnNumberOfItemsOnPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			d.CoerceValue(MinNumberOfItemsOnPathProperty);  //invokes the CoerceValueCallback delegate ("CoerceMinNumberOfItemsOnPath")
			d.CoerceValue(MaxNumberOfItemsOnPathProperty);  //invokes the CoerceValueCallback delegate ("CoerceMaxNumberOfItemsOnPath")
			CarouselControl depObj = (CarouselControl)d;
			depObj.pathListBox.LayoutPaths[0].Capacity = (double)depObj.NumberOfItemsOnPath;


		}
		#endregion

		#region MinNumberOfItemsOnPath

		/// <summary>
		/// MinNumberOfItemsOnPath DP
		/// </summary>
		public static readonly DependencyProperty MinNumberOfItemsOnPathProperty = DependencyProperty.Register(
		"MinNumberOfItemsOnPath",
		typeof(int),
		typeof(CarouselControl),
		new FrameworkPropertyMetadata(
			3,
			FrameworkPropertyMetadataOptions.None,
			new PropertyChangedCallback(OnMinNumberOfItemsOnPathChanged),
			new CoerceValueCallback(CoerceMinNumberOfItemsOnPath)
		),
		new ValidateValueCallback(IsValidNumberOfItemsOnPath));

		//property accessors
		public int MinNumberOfItemsOnPath
		{
			get { return (int)GetValue(MinNumberOfItemsOnPathProperty); }
			set { SetValue(MinNumberOfItemsOnPathProperty, value); }
		}

		/// <summary>
		/// Coerce MinNumberOfItemsOnPath value if not within limits
		/// </summary>
		private static void OnMinNumberOfItemsOnPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			d.CoerceValue(MaxNumberOfItemsOnPathProperty);  //invokes the CoerceValueCallback delegate ("CoerceMaxNumberOfItemsOnPath")
			d.CoerceValue(NumberOfItemsOnPathProperty);  //invokes the CoerceValueCallback delegate ("CoerceNumberOfItemsOnPath")
		}

		private static object CoerceMinNumberOfItemsOnPath(DependencyObject d, object value)
		{
			CarouselControl depObj = (CarouselControl)d;
			int min = (int)value;
			if (min > depObj.MaxNumberOfItemsOnPath) min = depObj.MaxNumberOfItemsOnPath;
			return min;
		}
		#endregion

		#region MaxNumberOfItemsOnPath

		/// <summary>
		/// MaxNumberOfItemsOnPath
		/// </summary>
		public static readonly DependencyProperty MaxNumberOfItemsOnPathProperty = DependencyProperty.Register(
			"MaxNumberOfItemsOnPath",
			typeof(int),
			typeof(CarouselControl),
			new FrameworkPropertyMetadata(
				10,
				FrameworkPropertyMetadataOptions.None,
				new PropertyChangedCallback(OnMaxNumberOfItemsOnPathChanged),
				new CoerceValueCallback(CoerceMaxNumberOfItemsOnPath)
			),
			new ValidateValueCallback(IsValidNumberOfItemsOnPath)
		);

		//property accessors
		public int MaxNumberOfItemsOnPath
		{
			get { return (int)GetValue(MaxNumberOfItemsOnPathProperty); }
			set { SetValue(MaxNumberOfItemsOnPathProperty, value); }
		}

		/// <summary>
		/// Coerce MaxNumberOfItemsOnPath value if not within limits
		/// </summary>
		private static object CoerceMaxNumberOfItemsOnPath(DependencyObject d, object value)
		{
			CarouselControl depObj = (CarouselControl)d;
			int max = (int)value;
			if (max < depObj.MinNumberOfItemsOnPath) max = depObj.MinNumberOfItemsOnPath;
			return max;
		}

		private static void OnMaxNumberOfItemsOnPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			d.CoerceValue(MinNumberOfItemsOnPathProperty);  //invokes the CoerceValueCallback delegate ("CoerceMinNumberOfItemsOnPath")
			d.CoerceValue(NumberOfItemsOnPathProperty);  //invokes the CoerceValueCallback delegate ("CoerceNumberOfItemsOnPath")
		}
		#endregion

		#endregion
	}
}
