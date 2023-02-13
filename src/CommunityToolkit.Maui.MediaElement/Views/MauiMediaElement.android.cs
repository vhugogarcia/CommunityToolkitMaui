using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.CoordinatorLayout.Widget;
using Com.Google.Android.Exoplayer2.UI;
using CommunityToolkit.Maui.Views;

namespace CommunityToolkit.Maui.Core.Views;

/// <summary>
/// The user-interface element that represents the <see cref="MediaElement"/> on Android.
/// </summary>
public class MauiMediaElement : CoordinatorLayout
{
	readonly Context context;
	readonly StyledPlayerView playerView;
	readonly FrameLayout relativeLayout;
	int originalUiOptions;
	int originalHeight;

	/// <summary>
	/// Initializes a new instance of the <see cref="MauiMediaElement"/> class.
	/// </summary>
	/// <param name="context">The application's <see cref="Context"/>.</param>
	/// <param name="playerView">The <see cref="StyledPlayerView"/> that acts as the platform media player.</param>
	public MauiMediaElement(Context context, StyledPlayerView playerView) : base(context)
	{
		this.playerView = playerView;
		this.context = context;

		playerView.FullscreenButtonClick += PlayerView_FullscreenButtonClick;

		// Create a RelativeLayout for sizing the video
		relativeLayout = new(context)
		{
			LayoutParameters = new FrameLayout.LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent)
		};

		relativeLayout.AddView(playerView);
		AddView(relativeLayout);
	}

	/// <summary>
	/// Allows the video to enter or exist a full screen mode
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	void PlayerView_FullscreenButtonClick(object? sender, StyledPlayerView.FullscreenButtonClickEventArgs e)
	{
		// Ensure there is a player view
		if (this.playerView is null)
		{
			return;
		}

		// Ensure current activity exists
		var currentActivity = Platform.CurrentActivity;
		if (currentActivity is null)
		{
			return;
		}

		// Ensure current window is available
		var currentWindow = currentActivity.Window;
		if (currentWindow is null)
		{
			return;
		}

		// Ensure current resources exist
		var currentResources = currentActivity.Resources;
		if (currentResources is null)
		{
			return;
		}

		
		if (e.IsFullScreen)
		{
			// Get the original values
			originalHeight = this.playerView.Height;

			// Force the landscape on the device
			currentActivity.RequestedOrientation = Android.Content.PM.ScreenOrientation.Landscape;

			// Hide the status bar and enter a full screen immerse mode
			if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.R)
			{
				currentWindow.SetDecorFitsSystemWindows(false);
				currentWindow.InsetsController?.Hide(WindowInsets.Type.NavigationBars());
			}
			else
			{
				originalUiOptions = (int)currentWindow.DecorView.SystemUiVisibility;
				var newUiOptions = originalUiOptions | (int)SystemUiFlags.LayoutStable | (int)SystemUiFlags.LayoutHideNavigation | (int)SystemUiFlags.LayoutHideNavigation |
								(int)SystemUiFlags.LayoutFullscreen | (int)SystemUiFlags.HideNavigation | (int)SystemUiFlags.Fullscreen | (int)SystemUiFlags.Immersive;

				currentWindow.DecorView.SystemUiVisibility = (StatusBarVisibility)newUiOptions;
			}

			// We update the player layout
			if (currentWindow.DecorView is FrameLayout)
			{
				var currentDisplayMetrics = currentResources.DisplayMetrics;
				if (currentDisplayMetrics is not null)
				{
					var customHeight = currentDisplayMetrics.WidthPixels;
					var customWidth = currentDisplayMetrics.HeightPixels;

					var frameLayoutParameters = new FrameLayout.LayoutParams(customWidth, customHeight, GravityFlags.CenterHorizontal);
					this.playerView.LayoutParameters = frameLayoutParameters;
				}
			}
		}
		else
		{
			currentActivity.RequestedOrientation = Android.Content.PM.ScreenOrientation.Portrait;

			if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.R)
			{
				currentWindow.SetDecorFitsSystemWindows(false);
				currentWindow.InsetsController?.Show(WindowInsets.Type.NavigationBars());
			}
			else
			{
				currentWindow.DecorView.SystemUiVisibility = (StatusBarVisibility)originalUiOptions;
			}

			var frameLayoutParameters = new FrameLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, originalHeight);
			playerView.LayoutParameters = frameLayoutParameters;
		}
	}

	/// <summary>
	/// Releases the unmanaged resources used by the <see cref="MediaElement"/> and optionally releases the managed resources.
	/// </summary>
	/// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release only unmanaged resources.</param>
	protected override void Dispose(bool disposing)
{
	if (disposing)
	{
		if (playerView is not null)
		{
			// https://github.com/google/ExoPlayer/issues/1855#issuecomment-251041500
			playerView.Player?.Release();
			playerView.Player?.Dispose();
			playerView.Dispose();
		}
	}

	base.Dispose(disposing);
}
}