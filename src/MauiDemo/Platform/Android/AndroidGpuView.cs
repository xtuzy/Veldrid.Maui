using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Views;
using System.Diagnostics;

namespace MauiDemo
{
    public class AndroidGpuView : SurfaceView, ISurfaceHolderCallback
    {
        public event Action<ISurfaceHolder> AndroidSurfaceCreated;
        public event Action AndroidSurfaceDestoryed;
        public event Action AndroidSurfaceChanged;

        public AndroidGpuView(Context context):base(context)
        {
            Holder.AddCallback(this);
        }

        public void SurfaceCreated(ISurfaceHolder holder)
        {
            AndroidSurfaceCreated?.Invoke(holder);
        }

        public void SurfaceDestroyed(ISurfaceHolder holder)
        {
            AndroidSurfaceDestoryed?.Invoke();
        }

        public void SurfaceChanged(ISurfaceHolder holder, [GeneratedEnum] Format format, int width, int height)
        {
            AndroidSurfaceChanged?.Invoke();
        }
    }
}
