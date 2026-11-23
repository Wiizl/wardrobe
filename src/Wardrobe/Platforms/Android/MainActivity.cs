using Android.App;
using Android.Content.PM;
using Android.Content.Res;
using Android.OS;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidX.Core.View;

namespace Wardrobe;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
	protected override void OnCreate(Bundle? savedInstanceState)
	{
		base.OnCreate(savedInstanceState);
		ApplySystemBars();
	}

	public override void OnConfigurationChanged(Configuration newConfig)
	{
		base.OnConfigurationChanged(newConfig);
		ApplySystemBars();
	}

	/// <summary>Tapping anywhere outside the focused text field closes the keyboard.</summary>
	public override bool DispatchTouchEvent(MotionEvent? e)
	{
		if (e?.Action == MotionEventActions.Down && CurrentFocus is EditText edit)
		{
			var bounds = new Android.Graphics.Rect();
			edit.GetGlobalVisibleRect(bounds);
			if (!bounds.Contains((int)e.RawX, (int)e.RawY))
			{
				edit.ClearFocus();
				var keyboard = (InputMethodManager?)GetSystemService(InputMethodService);
				keyboard?.HideSoftInputFromWindow(edit.WindowToken, HideSoftInputFlags.None);
			}
		}
		return base.DispatchTouchEvent(e);
	}

	/// <summary>Match the status/navigation bars to the app background (colors from Colors.xaml).</summary>
	void ApplySystemBars()
	{
		var window = Window;
		if (window is null)
			return;

		bool dark = (Resources?.Configuration?.UiMode & UiMode.NightMask) == UiMode.NightYes;

		// Before Android 15 the bar colors are set explicitly; on 15+ the app draws edge-to-edge
		// and the page background shows through the bars.
		if (!OperatingSystem.IsAndroidVersionAtLeast(35))
		{
			var color = Android.Graphics.Color.ParseColor(dark ? "#0F1115" : "#F5F6FA");
			window.SetStatusBarColor(color);
			window.SetNavigationBarColor(color);
		}

		var controller = WindowCompat.GetInsetsController(window, window.DecorView);
		if (controller is null)
			return;
		controller.AppearanceLightStatusBars = !dark;
		controller.AppearanceLightNavigationBars = !dark;
	}
}
