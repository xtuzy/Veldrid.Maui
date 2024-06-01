using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace MauiDemo
{
    public abstract class GameBase : IDisposable
    {
        public IAppInfo VeldridAppInfo { get; internal set; }

        public GraphicsDevice GraphicsDevice => VeldridAppInfo?.GraphicsDevice;
        public ResourceFactory ResourceFactory => VeldridAppInfo?.GraphicsDevice.ResourceFactory;
        public Swapchain MainSwapchain => VeldridAppInfo?.GraphicsDevice.MainSwapchain;

        public GameBase()
        {
        }

        /// <summary>
        /// 如果添加Drawable到VeldridPlatformInterface时还没有创建设备,那么在创建设备时会调用这个方法,可在其中设置资源
        /// </summary>
        public virtual void OnGraphicsDeviceCreated()
        {
            CreateResources(ResourceFactory);
        }

        /// <summary>
        /// 子类可以在该方法中释放子类中使用的资源.
        /// </summary>
        public virtual void OnGraphicsDeviceDestroyed()
        {
        }

        /// <summary>
        /// 该方法被<see cref="OnGraphicsDeviceCreated"/>调用, 主要是为适配Veldrid的旧项目
        /// </summary>
        /// <param name="factory"></param>
        public abstract void CreateResources(ResourceFactory factory);

        public abstract void OnRender(float deltaMillisecond);

        /// <summary>
        /// 视图或者Window大小改变时
        /// </summary>
        public virtual void OnViewResize() { }

        public virtual void Dispose()
        {
            GraphicsDevice?.WaitForIdle();//貌似会减少引用数
            VeldridAppInfo = null;
        }
    }
}
