//using Vulkan;
//using static Vulkan.VulkanNative;

namespace Veldrid.Vk
{
    internal unsafe class VkFence : Fence
    {
        private readonly VkGraphicsDevice _gd;
        private VulkanVkFence _fence;
        private string _name;
        private bool _destroyed;

        public VulkanVkFence DeviceFence => _fence;

        public VkFence(VkGraphicsDevice gd, bool signaled)
        {
            _gd = gd;
            VkFenceCreateInfo fenceCI = new VkFenceCreateInfo();
            fenceCI.Flags = signaled ? VkFenceCreateFlags.SignaledBit : VkFenceCreateFlags.None;
            VkResult result = vk.GetApi().CreateFence(_gd.Device, ref fenceCI, null, out _fence);
            VulkanUtil.CheckResult(result);
        }

        public override void Reset()
        {
            _gd.ResetFence(this);
        }

        public override bool Signaled => vk.GetApi().GetFenceStatus(_gd.Device, _fence) == VkResult.Success;
        public override bool IsDisposed => _destroyed;

        public override string Name
        {
            get => _name;
            set
            {
                _name = value; _gd.SetResourceName(this, value);
            }
        }

        public override void Dispose()
        {
            if (!_destroyed)
            {
                vk.GetApi().DestroyFence(_gd.Device, _fence, null);
                _destroyed = true;
            }
        }
    }
}
