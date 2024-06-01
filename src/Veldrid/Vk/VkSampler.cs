//using Vulkan;
//using static Vulkan.VulkanNative;

namespace Veldrid.Vk
{
    internal unsafe class VkSampler : Sampler
    {
        private readonly VkGraphicsDevice _gd;
        private readonly VulkanVkSampler _sampler;
        private bool _disposed;
        private string _name;

        public VulkanVkSampler DeviceSampler => _sampler;

        public ResourceRefCount RefCount { get; }

        public override bool IsDisposed => _disposed;

        public VkSampler(VkGraphicsDevice gd, ref SamplerDescription description)
        {
            _gd = gd;
            VkFormats.GetFilterParams(description.Filter, out VkFilter minFilter, out VkFilter magFilter, out VkSamplerMipmapMode mipmapMode);

            VkSamplerCreateInfo samplerCI = new VkSamplerCreateInfo
            {
                SType = VkStructureType.SamplerCreateInfo,
                AddressModeU = VkFormats.VdToVkSamplerAddressMode(description.AddressModeU),
                AddressModeV = VkFormats.VdToVkSamplerAddressMode(description.AddressModeV),
                AddressModeW = VkFormats.VdToVkSamplerAddressMode(description.AddressModeW),
                MinFilter = minFilter,
                MagFilter = magFilter,
                MipmapMode = mipmapMode,
                CompareEnable = description.ComparisonKind != null,
                CompareOp = description.ComparisonKind != null
                    ? VkFormats.VdToVkCompareOp(description.ComparisonKind.Value)
                    : VkCompareOp.Never,
                AnisotropyEnable = description.Filter == SamplerFilter.Anisotropic,
                MaxAnisotropy = description.MaximumAnisotropy,
                MinLod = description.MinimumLod,
                MaxLod = description.MaximumLod,
                MipLodBias = description.LodBias,
                BorderColor = VkFormats.VdToVkSamplerBorderColor(description.BorderColor)
            };

            vk.GetApi().CreateSampler(_gd.Device, ref samplerCI, null, out _sampler);
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
            if (!_disposed)
            {
                vk.GetApi().DestroySampler(_gd.Device, _sampler, null);
                _disposed = true;
            }
        }
    }
}
