using Microsoft.Maui.Controls.Shapes;

namespace Wardrobe.Controls;

/// <summary>App-drawn page header (replaces the system navigation bar): back button, title, subtitle, optional action.</summary>
public sealed class PageHeader : ContentView
{
	public static readonly BindableProperty TitleProperty = BindableProperty.Create(
		nameof(Title), typeof(string), typeof(PageHeader), "",
		propertyChanged: (b, _, n) => ((PageHeader)b)._title.Text = (string)n);

	public static readonly BindableProperty SubtitleProperty = BindableProperty.Create(
		nameof(Subtitle), typeof(string), typeof(PageHeader), "",
		propertyChanged: (b, _, n) => ((PageHeader)b).UpdateSubtitle());

	public static readonly BindableProperty ShowBackProperty = BindableProperty.Create(
		nameof(ShowBack), typeof(bool), typeof(PageHeader), false,
		propertyChanged: (b, _, _) => ((PageHeader)b).UpdateBack());

	public static readonly BindableProperty ActionTextProperty = BindableProperty.Create(
		nameof(ActionText), typeof(string), typeof(PageHeader), "",
		propertyChanged: (b, _, _) => ((PageHeader)b).UpdateAction());

	public static readonly BindableProperty ActionIsDestructiveProperty = BindableProperty.Create(
		nameof(ActionIsDestructive), typeof(bool), typeof(PageHeader), false,
		propertyChanged: (b, _, _) => ((PageHeader)b).UpdateAction());

	readonly Border _back;
	readonly Label _title;
	readonly Label _subtitle;
	readonly Border _action;
	readonly Label _actionLabel;

	public event EventHandler? ActionTapped;

	public string Title { get => (string)GetValue(TitleProperty); set => SetValue(TitleProperty, value); }
	public string Subtitle { get => (string)GetValue(SubtitleProperty); set => SetValue(SubtitleProperty, value); }
	public bool ShowBack { get => (bool)GetValue(ShowBackProperty); set => SetValue(ShowBackProperty, value); }
	public string ActionText { get => (string)GetValue(ActionTextProperty); set => SetValue(ActionTextProperty, value); }
	public bool ActionIsDestructive { get => (bool)GetValue(ActionIsDestructiveProperty); set => SetValue(ActionIsDestructiveProperty, value); }

	public PageHeader()
	{
		_back = new Border
		{
			WidthRequest = 44,
			HeightRequest = 44,
			StrokeThickness = 0,
			StrokeShape = new RoundRectangle { CornerRadius = 22 },
			VerticalOptions = LayoutOptions.Center,
			IsVisible = false,
			Content = Icons.Create(Icons.Back, Theme.TextPrimary),
		}.SetColor(BackgroundColorProperty, Theme.Surface);
		_back.OnTapped(async () => await Shell.Current.Navigation.PopAsync());
		SemanticProperties.SetDescription(_back, "Back");

		_title = new Label
		{
			FontFamily = Theme.FontSemibold,
			FontSize = 28,
			LineBreakMode = LineBreakMode.TailTruncation,
			MaxLines = 1,
		};
		_subtitle = new Label { FontSize = 14, IsVisible = false }.SetColor(Label.TextColorProperty, Theme.TextSecondary);

		_actionLabel = new Label { FontFamily = Theme.FontSemibold, FontSize = 14, VerticalOptions = LayoutOptions.Center };
		_action = new Border
		{
			StrokeThickness = 0,
			StrokeShape = new RoundRectangle { CornerRadius = 18 },
			Padding = new Thickness(16, 9),
			VerticalOptions = LayoutOptions.Center,
			IsVisible = false,
			Content = _actionLabel,
		};
		_action.OnTapped(() => ActionTapped?.Invoke(this, EventArgs.Empty));

		var titles = new VerticalStackLayout { Spacing = 0, VerticalOptions = LayoutOptions.Center, Children = { _title, _subtitle } };

		var grid = new Grid
		{
			Padding = new Thickness(20, 16, 20, 12),
			ColumnSpacing = 14,
			ColumnDefinitions = { new(GridLength.Auto), new(GridLength.Star), new(GridLength.Auto) },
		};
		grid.Add(_back, 0);
		grid.Add(titles, 1);
		grid.Add(_action, 2);
		Content = grid;
	}

	void UpdateSubtitle()
	{
		_subtitle.Text = Subtitle;
		_subtitle.IsVisible = !string.IsNullOrEmpty(Subtitle);
	}

	void UpdateBack()
	{
		_back.IsVisible = ShowBack;
		_title.FontSize = ShowBack ? 22 : 28;
	}

	void UpdateAction()
	{
		_action.IsVisible = !string.IsNullOrEmpty(ActionText);
		_actionLabel.Text = ActionText;
		if (ActionIsDestructive)
		{
			_actionLabel.SetColor(Label.TextColorProperty, Theme.Danger);
			_action.SetColor(BackgroundColorProperty, Theme.Surface);
		}
		else
		{
			_actionLabel.SetColor(Label.TextColorProperty, Theme.Accent);
			_action.SetColor(BackgroundColorProperty, Theme.AccentSoft);
		}
	}
}
