#if __IOS__
using CoreGraphics;
using Microsoft.Extensions.DependencyModel;
using Silk.NET.Core.Contexts;
using Silk.NET.Core.Loader;
using Silk.NET.Vulkan;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UIKit;
using Veldrid.Vk;
namespace MauiDemo
{
    internal class MainController : UIViewController
    {
        public MainController(UIWindow window)
        {
            var vk = GetApi();
            SilkNETVk.Init(vk);

            var w = window!.Frame.Width;
            var scrollView = new UIScrollView(window!.Frame) { BackgroundColor = UIColor.Green };
            this.View = scrollView;

            var layout = new UIStackView(new CGRect(0, 0, w, 20 + 300 + 20 + 300)) { BackgroundColor = UIColor.Red };
            layout.Axis = UILayoutConstraintAxis.Vertical;
            layout.Alignment = UIStackViewAlignment.Center;
            scrollView.AddSubview(layout);

            var gpuViewTitle = new UILabel(new CGRect(0, 0, w, 20)) { Text = "Render to UIView" };
            layout.AddSubview(gpuViewTitle);

            var gpuView = new iOSGpuView(new CGRect((w -300) / 2, 20, 300, 300)) { BackgroundColor = UIColor.Brown };
            layout.AddSubview(gpuView);

            var app = new VeldridApp(gpuView, Veldrid.GraphicsBackend.Metal);
            app.Game = new HelloTriangle();

            var imageViewTitle = new UILabel(new CGRect(0, 20+300, w, 20)) { Text = "Headless Vulkan" };
            layout.AddSubview(imageViewTitle);

            var imageView = new SkiaSharp.Views.iOS.SKCanvasView(new CGRect((w - 300) / 2, 20 + 300 + 20, 300, 300)) { };
            layout.AddSubview(imageView);

            SKBitmap bitmap = null;
            try
            {
                var headless = new HeadlessHelloTriangle((int)(300 * DeviceDisplay.Current.MainDisplayInfo.Density), (int)(300 * DeviceDisplay.Current.MainDisplayInfo.Density));
                headless.CreateResources();
                bitmap = headless.SaveRgba32ToSKBitmap(headless.OnRender());
                headless.Dispose();
            }
            catch (Exception ex)
            {
            }
            imageView.PaintSurface += (sender, e) =>
            {
                e.Surface.Canvas.Clear(SKColors.White);
                if (bitmap != null)
                {
                    e.Surface.Canvas.DrawBitmap(bitmap, 0, 0);
                }
            };
        }

        Vk GetApi()
        {
            var context = CreateIOSContext();
            Vk ret = new Vk(context);

            return ret;
        }

        INativeContext CreateIOSContext(string text = "__Internal")
        {
            var _foldSilkNETVulkanVkPInvokeOverride0 = AppContext.TryGetSwitch("SILK_NET_VULKAN_VK_ENABLE_PINVOKE_OVERRIDE_0", out var isEnabled) && isEnabled;
            if (_foldSilkNETVulkanVkPInvokeOverride0 && text == "__Internal")
            {
                return null;
            }
            if (IOSNativeContext.TryCreate(text, out var context))
            {
                return context;
            }

            throw new FileNotFoundException("Could not load from any of the possible library names! Please make sure that the library is installed and in the right place!");
        }

        class IOSNativeContext : INativeContext, IDisposable
        {
            private readonly IntPtr _libraryHandle;

            public IntPtr NativeHandle => _libraryHandle;

            public static bool TryCreate(string name, out IOSNativeContext context)
            {
                var library = ObjCRuntime.Dlfcn.dlopen(null, 0x002);//主程序 copy from xamarin-macios

                context = new IOSNativeContext(library);
                return true;
            }

            IOSNativeContext(IntPtr library)
            {
                _libraryHandle = library;
            }

            public nint GetProcAddress(string proc, int? slot = null)
            {
                return ObjCRuntime.Dlfcn.dlsym(NativeHandle, proc);
            }

            public bool TryGetProcAddress(string proc, out nint addr, int? slot = null)
            {
                addr = ObjCRuntime.Dlfcn.dlsym(NativeHandle, proc);
                return true;
            }

            public void Dispose()
            {
                ObjCRuntime.Dlfcn.dlclose(NativeHandle);
            }
        }
    }
}
#endif
