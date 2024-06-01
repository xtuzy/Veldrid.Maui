//using Vulkan;
using static Veldrid.Vk.VulkanUtil;
//using static Vulkan.VulkanNative;

namespace Veldrid.Vk
{
    internal unsafe class VkTextureView : TextureView
    {
        private readonly VkGraphicsDevice _gd;
        private readonly VkImageView _imageView;
        private bool _destroyed;
        private string _name;

        public VkImageView ImageView => _imageView;

        public new VkTexture Target => (VkTexture)base.Target;

        public ResourceRefCount RefCount { get; }

        public override bool IsDisposed => _destroyed;

        public VkTextureView(VkGraphicsDevice gd, ref TextureViewDescription description)
            : base(ref description)
        {
            _gd = gd;
            VkImageViewCreateInfo imageViewCI = new VkImageViewCreateInfo();
            VkTexture tex = Util.AssertSubtype<Texture, VkTexture>(description.Target);
            imageViewCI.Image = tex.OptimalDeviceImage;
            imageViewCI.Format = VkFormats.VdToVkPixelFormat(Format, (Target.Usage & TextureUsage.DepthStencil) != 0);

            VkImageAspectFlags aspectFlags;
            if ((description.Target.Usage & TextureUsage.DepthStencil) == TextureUsage.DepthStencil)
            {
                aspectFlags = VkImageAspectFlags.DepthBit;
            }
            else
            {
                aspectFlags = VkImageAspectFlags.ColorBit;
            }

            imageViewCI.SubresourceRange = new VkImageSubresourceRange(
                aspectFlags,
                description.BaseMipLevel,
                description.MipLevels,
                description.BaseArrayLayer,
                description.ArrayLayers);

            if ((tex.Usage & TextureUsage.Cubemap) == TextureUsage.Cubemap)
            {
                imageViewCI.ViewType = description.ArrayLayers == 1 ? VkImageViewType.TypeCube : VkImageViewType.TypeCubeArray;
                imageViewCI.SubresourceRange.LayerCount *= 6;
            }
            else
            {
                switch (tex.Type)
                {
                    case TextureType.Texture1D:
                        imageViewCI.ViewType = description.ArrayLayers == 1
                            ? VkImageViewType.Type1D
                            : VkImageViewType.Type1DArray;
                        break;
                    case TextureType.Texture2D:
                        imageViewCI.ViewType = description.ArrayLayers == 1
                            ? VkImageViewType.Type2D
                            : VkImageViewType.Type2DArray;
                        break;
                    case TextureType.Texture3D:
                        imageViewCI.ViewType = VkImageViewType.Type3D;
                        break;
                }
            }

            vk.GetApi().CreateImageView(_gd.Device, ref imageViewCI, null, out _imageView);
            RefCount = new ResourceRefCount(DisposeCore);
        }

        public override string Name
        {
            get => _name;
            set
            {
                _name = value;
                _gd.SetResourceName(this, value);
            }
        }

        public override void Dispose()
        {
            RefCount.Decrement();
        }

        private void DisposeCore()
        {
            if (!_destroyed)
            {
                _destroyed = true;
                vk.GetApi().DestroyImageView(_gd.Device, ImageView, null);
            }
        }
    }
}
