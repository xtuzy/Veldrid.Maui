using Android.Runtime;
using Android.Views;
using Veldrid;

namespace MauiDemo
{
    public class VeldridApp : IAppInfo, IDisposable
    {
        AndroidGpuView View;

        ValueAnimator Animator;

        /// <summary>
        /// Android的View在代码中宽高本身使用像素为单位, 无需转换
        /// </summary>
        public override uint Width => (uint)View.Width;
        public override uint Height => (uint)View.Height;

        public VeldridApp(AndroidGpuView view, GraphicsBackend backend = GraphicsBackend.OpenGLES)
        {
            if (!(backend == GraphicsBackend.Vulkan || backend == GraphicsBackend.OpenGLES))
            {
                throw new NotSupportedException($"{backend} is not supported on Android.");
            }
            Backend = backend;

            View = view;
            View.AndroidSurfaceCreated += CreateGraphicsDevice;
            View.AndroidSurfaceChanged += OnViewSizeChanged;
            View.AndroidSurfaceDestoryed += DestroyGraphicsDevice;
        }

        private void CreateGraphicsDevice(ISurfaceHolder holder)
        {
            var options = new GraphicsDeviceOptions(false, PixelFormat.R16_UNorm, false, ResourceBindingModel.Improved, true, true);

            if (GraphicsDevice == null)
            {
                SwapchainSource ss = SwapchainSource.CreateAndroidSurface(holder.Surface.Handle, JNIEnv.Handle);
                SwapchainDescription sd = new SwapchainDescription(
                    ss,
                    (uint)Width,
                    (uint)Height,
                    options.SwapchainDepthFormat,
                    options.SyncToVerticalBlank);
                if (Backend == GraphicsBackend.Vulkan)
                    GraphicsDevice = GraphicsDevice.CreateVulkan(options, sd);
                else
                    GraphicsDevice = GraphicsDevice.CreateOpenGLES(options, sd);
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
                GraphicsDevice?.MainSwapchain?.Resize((uint)Width, (uint)Height);
                Game?.OnViewResize();
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

        private void RenderLoop()
        {
            if (GraphicsDevice != null)
                Game?.OnRender(16);
        }

        public void Dispose()
        {
            View = null;
        }
    }
}
