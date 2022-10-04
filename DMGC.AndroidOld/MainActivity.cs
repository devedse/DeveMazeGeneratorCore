using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using DeveMazeGeneratorCore.MonoGame.Core.HelperObjects;
using DeveMazeGeneratorMonoGame;

namespace DeveMazeGeneratorCore.MonoGame.Android
{
    [Activity(Label = "DeveMazeGeneratorCoreMonoGame"
        , MainLauncher = true
        , Theme = "@style/Theme.Splash"
        , AlwaysRetainTaskState = true
        , LaunchMode = LaunchMode.SingleInstance
        , ScreenOrientation = ScreenOrientation.FullSensor
        , ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize)]
    [IntentFilter(
        new[] { "android.intent.action.MAIN" }, 
        AutoVerify = true,
        Categories = new[] { 
            "android.intent.category.LAUNCHER", 
            "android.intent.category.LEANBACK_LAUNCHER" 
        })]
    public class MainActivity : Microsoft.Xna.Framework.AndroidGameActivity
    {
        private void FixUiOptions()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
            {
                Window.InsetsController.Hide(WindowInsets.Type.StatusBars());
                Window.InsetsController.Hide(WindowInsets.Type.SystemBars());
            }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            var g = new TheGame(Platform.Android);
            SetContentView((View)g.Services.GetService(typeof(View)));

            FixUiOptions();
            g.Run();
        }
    }
}

