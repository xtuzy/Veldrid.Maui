#if ANDROID
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using SkiaSharp;
using System;
using System.Diagnostics;
using ScrollView = Android.Widget.ScrollView;

namespace MauiDemo
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class MainController : AppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Set our view from the "main" layout resource
            var scrollView = new ScrollView(this) {};
            SetContentView(scrollView);

            var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };
            layout.SetGravity(GravityFlags.CenterHorizontal);
            layout.SetBackgroundColor(Android.Graphics.Color.Blue);
            scrollView.AddView(layout);

            var gpuViewTitle = new TextView(this) { Text = "Render to SurfaceView" };
            layout.AddView(gpuViewTitle);

            var gpuView = new AndroidGpuView(this);
            layout.AddView(gpuView, new ViewGroup.LayoutParams(300, 300));
            
            var app = new VeldridApp(gpuView, Veldrid.GraphicsBackend.OpenGLES);
            app.Game = new HelloTriangle();

            var imageViewTitle = new TextView(this) { Text = "Headless Vulkan" };
            layout.AddView(imageViewTitle);

            var imageView = new SkiaSharp.Views.Android.SKCanvasView(this) {};
            layout.AddView(imageView, new ViewGroup.LayoutParams(300, 300));

            SKBitmap bitmap = null;
            try
            {
                var headless = new HeadlessHelloTriangle(300, 300);
                headless.CreateResources();
                bitmap = headless.SaveRgba32ToSKBitmap(headless.OnRender());
                headless.Dispose();
            }
            catch (Exception ex)
            {
            }
            imageView.PaintSurface += (sender, e) =>
            {
                if (bitmap != null)
                {
                    e.Surface.Canvas.DrawBitmap(bitmap, 0, 0);
                }
            };
        }
    }
}
#endif
