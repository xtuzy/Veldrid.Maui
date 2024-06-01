using System.Diagnostics;
using Veldrid;

namespace MauiDemo
{
    public class VeldridApp : IAppInfo, IDisposable
    {
        iOSGpuView View;

        ValueAnimator Animator;

        public override uint Width => (uint)(View.Frame.Width * DeviceDisplay.Current.MainDisplayInfo.Density);
        public override uint Height => (uint)(View.Frame.Height * DeviceDisplay.Current.MainDisplayInfo.Density);

        public VeldridApp(iOSGpuView view, GraphicsBackend backend = GraphicsBackend.Metal)
        {
            if (!(backend == GraphicsBackend.Metal || backend == GraphicsBackend.OpenGLES || backend == GraphicsBackend.Vulkan))
                throw new NotSupportedException($"Not support {backend} backend on iOS or Maccatalyst.");
            Backend = backend;

            View = view;
            View.ViewLoaded += CreateGraphicsDevice;
            View.SizeChanged += OnViewSizeChanged;
            View.ViewRemoved += DestroyGraphicsDevice;
        }

        private void RenderLoop()
        {
            if (GraphicsDevice != null)
            {
                try
                {
                    Game?.OnRender(16);
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Encountered an error while rendering: " + e);
                }
            }
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

        private void CreateGraphicsDevice()
        {
            var options = new GraphicsDeviceOptions(false, null, false, ResourceBindingModel.Improved, true, true);

            SwapchainSource ss = SwapchainSource.CreateUIView(View.Handle);
            SwapchainDescription scd = new SwapchainDescription(
                ss,
                (uint)View.Frame.Width,//MTLSwapchain内部自动转换成Pixel
                (uint)View.Frame.Height,
                PixelFormat.R32_Float,
                false);
            if (Backend == GraphicsBackend.Metal)
            {
                GraphicsDevice = GraphicsDevice.CreateMetal(options, scd);
            }
            else if (Backend == GraphicsBackend.OpenGLES)
            {
                GraphicsDevice = GraphicsDevice.CreateOpenGLES(options, scd);
            }
            else if (Backend == GraphicsBackend.Vulkan)
            {
                //need use MoltenVK nuget package
                GraphicsDevice = GraphicsDevice.CreateVulkan(options, scd);
            }

            Game?.OnGraphicsDeviceCreated();

            if (Animator == null)
            {
                Animator = new ValueAnimator();
                Animator.set(RenderLoop);
            }
            Animator.start();
        }

        private void OnViewSizeChanged()
        {
            if (GraphicsDevice != null)
            {
                //MTLSwapchain内部自动转换成Pixel
                GraphicsDevice?.MainSwapchain?.Resize((uint)View.Frame.Width, (uint)View.Frame.Height);
                Game?.OnViewResize();
            }
        }

        public void Dispose()
        {
            View = null;
        }
    }
}
