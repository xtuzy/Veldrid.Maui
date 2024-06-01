using CoreAnimation;
using CoreGraphics;
using System.Diagnostics;
using UIKit;
namespace MauiDemo
{
    public class iOSGpuView : UIView
    {
        public iOSGpuView() : base() { }

        public iOSGpuView(CGRect frame) : base(frame)
        {
            this.Layer.MasksToBounds = true; // IMPORTANT
        }

        CGSize oldFrame = CGSize.Empty;
        /// <summary>
        /// 构建GraphicsDevice时大小不能为0, Maui会调用此方法计算UIView的大小,因此在该方法中判断何时大小不为0
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public override CGSize SizeThatFits(CGSize size)
        {
            Debug.WriteLine($"iOS:{nameof(SizeThatFits)}");
            var result = base.SizeThatFits(size);
            if (this.firstTimeLoad && result.Width > 0 && result.Height > 0)//初次有大小
            {
                ViewLoaded?.Invoke();
                firstTimeLoad = false;
                oldFrame = result;
            }
            else if (result != oldFrame)//大小更新
            {
                SizeChanged?.Invoke();
                oldFrame = result;
            }
            return result;
        }

        public override void LayoutSubviews()
        {
            Debug.WriteLine($"iOS:{nameof(LayoutSubviews)}");
            var result = this.Frame.Size;
            if (this.firstTimeLoad && result.Width > 0 && result.Height > 0)//初次有大小
            {
                ViewLoaded?.Invoke();
                firstTimeLoad = false;
                oldFrame = result;
            }
            else if (result != oldFrame)//大小更新
            {
                SizeChanged?.Invoke();
                oldFrame = result;
            }
            base.LayoutSubviews();
        }

        public Action SizeChanged;
        public Action ViewLoaded;
        public Action ViewRemoved;
        bool firstTimeLoad = true;
        public override void MovedToWindow()
        {
            Debug.WriteLine($"iOS:{nameof(MovedToWindow)}");
            var b = this.Bounds;
            base.MovedToWindow();
            /*if (firstTimeLoad)
            {
                ViewLoaded?.Invoke();
                firstTimeLoad = false;
            }
            else*/
            if (!firstTimeLoad)
            {
                ViewRemoved?.Invoke();
                firstTimeLoad = true;
            }
        }
    }
}
