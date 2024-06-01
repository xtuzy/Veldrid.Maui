using System;
using System.Diagnostics;
using Veldrid;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace MauiDemo
{
    public class VeldridApp : IAppInfo, IDisposable
    {
        WindowsGpuView View;

        ValueAnimator Animator;

        public override uint Width => (uint)(View.RenderSize.Width * View.CompositionScaleX);
        public override uint Height => (uint)(View.RenderSize.Height * View.CompositionScaleX);

        public VeldridApp(WindowsGpuView view)
        {
            View = view;
            View.CompositionScaleChanged += OnViewScaleChanged;
            View.SizeChanged += OnViewSizeChanged;

            View.Loaded += OnLoaded;
            View.Unloaded += OnUnloaded;
        }

        private void OnUnloaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            DestroyGraphicsDevice();
        }

        private void OnLoaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            CreateGraphicsDevice();
        }

        private void CreateGraphicsDevice()
        {
            var options = new GraphicsDeviceOptions(false, PixelFormat.R32_Float, true, ResourceBindingModel.Improved, true, true);
            var logicalDpi = 96.0f * View.CompositionScaleX;
            var renderWidth = View.RenderSize.Width;
            var renderHeight = View.RenderSize.Height;

            GraphicsDevice = GraphicsDevice.CreateD3D11(options, this.View, renderWidth, renderHeight, logicalDpi);

            Game?.OnGraphicsDeviceCreated();
            if (Animator == null)
            {
                Animator = new ValueAnimator();
                Animator.set(RenderLoop);
            }
            Animator.start();
        }

        private void DestroyGraphicsDevice()
        {
            if (GraphicsDevice != null)
            {
                Game?.OnGraphicsDeviceDestroyed();
                GraphicsDevice?.WaitForIdle();
                GraphicsDevice?.Dispose();
                GraphicsDevice = null;
                if (Animator != null)
                    Animator.cancel();
            }
        }

        /// <summary>
        /// View will still run it.
        /// </summary>
        private void RenderLoop()
        {
            if (GraphicsDevice != null)
            {
                try
                {
                    Game?.OnRender(16);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Encountered an error while rendering: " + ex);
                }
            }
        }

        private void OnViewSizeChanged(object sender, Microsoft.UI.Xaml.SizeChangedEventArgs e)
        {
            if (GraphicsDevice != null)
            {
                GraphicsDevice?.MainSwapchain?.Resize((uint)e.NewSize.Width, (uint)e.NewSize.Height);
                Game?.OnViewResize();
            }
        }

        private void OnViewScaleChanged(SwapChainPanel sender, object args)
        {
            if (GraphicsDevice != null)
            {
                GraphicsDevice?.MainSwapchain?.Resize((uint)sender.ActualSize.X, (uint)sender.ActualSize.Y);
                Game?.OnViewResize();
            }
        }

        public void Dispose()
        {
           View = null;
        }
    }
}
