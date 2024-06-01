#if WINDOWS
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SkiaSharp;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MauiDemo
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainController : Microsoft.UI.Xaml.Window
    {
        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern int GetDpiForWindow(IntPtr hwnd);

        public MainController()
        {
            //WinUI Set window special size
            IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            Microsoft.UI.WindowId windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            Microsoft.UI.Windowing.AppWindow appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
            appWindow.Resize(new Windows.Graphics.SizeInt32(500, 500));
            var density = GetDpiForWindow(hWnd) / 96.0;

            var scrollView = new Microsoft.UI.Xaml.Controls.ScrollViewer()
            {
                Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.Maui.Graphics.Colors.Blue.ToWindowsColor()),
            };
            this.Content = scrollView;

            var layout = new StackPanel();
            scrollView.Content = layout;

            layout.Children.Add(new Microsoft.UI.Xaml.Controls.TextBlock()
            {
                HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Center,
                VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center,
                Text = "Render to SwapChainPanel",
            });

            var gpuView = new WindowsGpuView() { Width = 300, Height = 300 };
            layout.Children.Add(gpuView);

            var app = new VeldridApp(gpuView);
            app.Game = new HelloTriangle();

            layout.Children.Add(new Microsoft.UI.Xaml.Controls.TextBlock()
            {
                HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Center,
                VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center,
                Text = "Headless Vulkan",
            });

            var imageView = new SkiaSharp.Views.Windows.SKXamlCanvas() { Width = 300, Height = 300};
            layout.Children.Add(imageView);

            SKBitmap bitmap = null;
            try
            {
                var headless = new HeadlessHelloTriangle((int)(300 * density), (int)(300 * density));
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
