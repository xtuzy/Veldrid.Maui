using System.Linq;
//using Vulkan;
//using static Vulkan.VulkanNative;
using static Veldrid.Vk.VulkanUtil;
using System;
using System.Runtime.InteropServices;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;

namespace Veldrid.Vk
{
    internal unsafe class VkSwapchain : Swapchain
    {
        private readonly VkGraphicsDevice _gd;
        private readonly VkSurfaceKHR _surface;
        private VkSwapchainKHR _deviceSwapchain;
        private KhrSwapchain _khrSwapchain;
        private readonly VkSwapchainFramebuffer _framebuffer;
        private VulkanVkFence _imageAvailableFence;
        private readonly uint _presentQueueIndex;
        private readonly VkQueue _presentQueue;
        private bool _syncToVBlank;
        private readonly SwapchainSource _swapchainSource;
        private readonly bool _colorSrgb;
        private bool? _newSyncToVBlank;
        private uint _currentImageIndex;
        private string _name;
        private bool _disposed;

        public override string Name { get => _name; set { _name = value; _gd.SetResourceName(this, value); } }
        public override Framebuffer Framebuffer => _framebuffer;
        public override bool SyncToVerticalBlank
        {
            get => _newSyncToVBlank ?? _syncToVBlank;
            set
            {
                if (_syncToVBlank != value)
                {
                    _newSyncToVBlank = value;
                }
            }
        }

        public override bool IsDisposed => _disposed;

        public VkSwapchainKHR DeviceSwapchain => _deviceSwapchain;
        public uint ImageIndex => _currentImageIndex;
        public VulkanVkFence ImageAvailableFence => _imageAvailableFence;
        public VkSurfaceKHR Surface => _surface;
        public VkQueue PresentQueue => _presentQueue;
        public uint PresentQueueIndex => _presentQueueIndex;
        public ResourceRefCount RefCount { get; }
        public KhrSwapchain KhrSwapchain => _khrSwapchain;

        public VkSwapchain(VkGraphicsDevice gd, ref SwapchainDescription description) : this(gd, ref description, VkNull.VkSurfaceKHRNull) { }

        public VkSwapchain(VkGraphicsDevice gd, ref SwapchainDescription description, VkSurfaceKHR existingSurface)
        {
            _gd = gd;
            _syncToVBlank = description.SyncToVerticalBlank;
            _swapchainSource = description.Source;
            _colorSrgb = description.ColorSrgb;

            if (Compare.IsEqual(existingSurface,VkNull.VkSurfaceKHRNull))
            {
                _surface = VkSurfaceUtil.CreateSurface(gd, gd.Instance, _swapchainSource);
            }
            else
            {
                _surface = existingSurface;
            }

            if (!GetPresentQueueIndex(out _presentQueueIndex))
            {
                throw new VeldridException($"The system does not support presenting the given Vulkan surface.");
            }
            vk.GetApi().GetDeviceQueue(_gd.Device, _presentQueueIndex, 0, out _presentQueue);

            _framebuffer = new VkSwapchainFramebuffer(gd, this, _surface, description.Width, description.Height, description.DepthFormat);

            CreateSwapchain(description.Width, description.Height);

            VkFenceCreateInfo fenceCI = new VkFenceCreateInfo();
            fenceCI.Flags = VkFenceCreateFlags.None;
            vk.GetApi().CreateFence(_gd.Device, ref fenceCI, null, out _imageAvailableFence);

            AcquireNextImage(_gd.Device, VkNull.VkSemaphoreNull, _imageAvailableFence);
            vk.GetApi().WaitForFences(_gd.Device, 1, ref _imageAvailableFence, true, ulong.MaxValue);
            vk.GetApi().ResetFences(_gd.Device, 1, ref _imageAvailableFence);

            RefCount = new ResourceRefCount(DisposeCore);
        }

        public override void Resize(uint width, uint height)
        {
            RecreateAndReacquire(width, height);
        }

        public bool AcquireNextImage(VkDevice device, VkSemaphore semaphore, VulkanVkFence fence)
        {
            if (_newSyncToVBlank != null)
            {
                _syncToVBlank = _newSyncToVBlank.Value;
                _newSyncToVBlank = null;
                RecreateAndReacquire(_framebuffer.Width, _framebuffer.Height);
                return false;
            }

            VkResult result = _khrSwapchain.AcquireNextImage(
                device,
                _deviceSwapchain,
                ulong.MaxValue,
                semaphore,
                fence,
                ref _currentImageIndex);
            _framebuffer.SetImageIndex(_currentImageIndex);
            if (result == VkResult.ErrorOutOfDateKhr || result == VkResult.SuboptimalKhr)
            {
                CreateSwapchain(_framebuffer.Width, _framebuffer.Height);
                return false;
            }
            else if (result != VkResult.Success)
            {
                throw new VeldridException("Could not acquire next image from the Vulkan swapchain.");
            }

            return true;
        }

        private void RecreateAndReacquire(uint width, uint height)
        {
            if (CreateSwapchain(width, height))
            {
                if (AcquireNextImage(_gd.Device, VkNull.VkSemaphoreNull, _imageAvailableFence))
                {
                    vk.GetApi().WaitForFences(_gd.Device, 1, ref _imageAvailableFence, true, ulong.MaxValue);
                    vk.GetApi().ResetFences(_gd.Device, 1, ref _imageAvailableFence);
                }
            }
        }

        private bool CreateSwapchain(uint width, uint height)
        {
            // Obtain the surface capabilities first -- this will indicate whether the surface has been lost.
            VkResult result = _gd.KhrSurface.GetPhysicalDeviceSurfaceCapabilities(_gd.PhysicalDevice, _surface, out VkSurfaceCapabilitiesKHR surfaceCapabilities);
            if (result == VkResult.ErrorSurfaceLostKhr)
            {
                throw new VeldridException($"The Swapchain's underlying surface has been lost.");
            }

            if (surfaceCapabilities.MinImageExtent.Width == 0 && surfaceCapabilities.MinImageExtent.Height == 0
                && surfaceCapabilities.MaxImageExtent.Width == 0 && surfaceCapabilities.MaxImageExtent.Height == 0)
            {
                return false;
            }

            if (Compare.IsNotEqual(_deviceSwapchain, VkNull.VkSwapchainKHRNull))
            {
                _gd.WaitForIdle();
            }

            _currentImageIndex = 0;
            uint surfaceFormatCount = 0;
            result = _gd.KhrSurface.GetPhysicalDeviceSurfaceFormats(_gd.PhysicalDevice, _surface, ref surfaceFormatCount, null);
            CheckResult(result);
            VkSurfaceFormatKHR[] formats = new VkSurfaceFormatKHR[surfaceFormatCount];
            result = _gd.KhrSurface.GetPhysicalDeviceSurfaceFormats(_gd.PhysicalDevice, _surface, ref surfaceFormatCount, out formats[0]);
            CheckResult(result);

            VkFormat desiredFormat = _colorSrgb
                ? VkFormat.B8G8R8A8Srgb
                : VkFormat.B8G8R8A8Unorm;

            VkSurfaceFormatKHR surfaceFormat = new VkSurfaceFormatKHR();
            if (formats.Length == 1 && formats[0].Format == VkFormat.Undefined)
            {
                surfaceFormat = new VkSurfaceFormatKHR { ColorSpace = VkColorSpaceKHR.ColorSpaceSrgbNonlinearKhr, Format = desiredFormat };
            }
            else
            {
                foreach (VkSurfaceFormatKHR format in formats)
                {
                    if (format.ColorSpace == VkColorSpaceKHR.ColorSpaceSrgbNonlinearKhr && format.Format == desiredFormat)
                    {
                        surfaceFormat = format;
                        break;
                    }
                }
                if (surfaceFormat.Format == VkFormat.Undefined)
                {
                    if (_colorSrgb && surfaceFormat.Format != VkFormat.R8G8B8A8Srgb)
                    {
                        throw new VeldridException($"Unable to create an sRGB Swapchain for this surface.");
                    }

                    surfaceFormat = formats[0];
                }
            }

            uint presentModeCount = 0;
            result = _gd.KhrSurface.GetPhysicalDeviceSurfacePresentModes(_gd.PhysicalDevice, _surface, ref presentModeCount, null);
            CheckResult(result);
            VkPresentModeKHR[] presentModes = new VkPresentModeKHR[presentModeCount];
            result = _gd.KhrSurface.GetPhysicalDeviceSurfacePresentModes(_gd.PhysicalDevice, _surface, ref presentModeCount, out presentModes[0]);
            CheckResult(result);

            VkPresentModeKHR presentMode = VkPresentModeKHR.FifoKhr;

            if (_syncToVBlank)
            {
                if (presentModes.Contains(VkPresentModeKHR.FifoRelaxedKhr))
                {
                    presentMode = VkPresentModeKHR.FifoRelaxedKhr;
                }
            }
            else
            {
                if (presentModes.Contains(VkPresentModeKHR.MailboxKhr))
                {
                    presentMode = VkPresentModeKHR.MailboxKhr;
                }
                else if (presentModes.Contains(VkPresentModeKHR.ImmediateKhr))
                {
                    presentMode = VkPresentModeKHR.ImmediateKhr;
                }
            }

            uint maxImageCount = surfaceCapabilities.MaxImageCount == 0 ? uint.MaxValue : surfaceCapabilities.MaxImageCount;
            uint imageCount = Math.Min(maxImageCount, surfaceCapabilities.MinImageCount + 1);

            VkSwapchainCreateInfoKHR swapchainCI = new VkSwapchainCreateInfoKHR();
            swapchainCI.Surface = _surface;
            swapchainCI.PresentMode = presentMode;
            swapchainCI.ImageFormat = surfaceFormat.Format;
            swapchainCI.ImageColorSpace = surfaceFormat.ColorSpace;
            uint clampedWidth = Util.Clamp(width, surfaceCapabilities.MinImageExtent.Width, surfaceCapabilities.MaxImageExtent.Width);
            uint clampedHeight = Util.Clamp(height, surfaceCapabilities.MinImageExtent.Height, surfaceCapabilities.MaxImageExtent.Height);
            swapchainCI.ImageExtent = new VkExtent2D { Width = clampedWidth, Height = clampedHeight };
            swapchainCI.MinImageCount = imageCount;
            swapchainCI.ImageArrayLayers = 1;
            swapchainCI.ImageUsage = VkImageUsageFlags.ColorAttachmentBit | VkImageUsageFlags.TransferDstBit;

            FixedArray2<uint> queueFamilyIndices = new FixedArray2<uint>(_gd.GraphicsQueueIndex, _gd.PresentQueueIndex);

            if (_gd.GraphicsQueueIndex != _gd.PresentQueueIndex)
            {
                swapchainCI.ImageSharingMode = VkSharingMode.Concurrent;
                swapchainCI.QueueFamilyIndexCount = 2;
                swapchainCI.PQueueFamilyIndices = &queueFamilyIndices.First;
            }
            else
            {
                swapchainCI.ImageSharingMode = VkSharingMode.Exclusive;
                swapchainCI.QueueFamilyIndexCount = 0;
            }

            swapchainCI.PreTransform = VkSurfaceTransformFlagsKHR.IdentityBitKhr;
            swapchainCI.CompositeAlpha = VkCompositeAlphaFlagsKHR.OpaqueBitKhr;
            swapchainCI.Clipped = true;

            VkSwapchainKHR oldSwapchain = _deviceSwapchain;
            swapchainCI.OldSwapchain = oldSwapchain;

            if (!vk.GetApi().TryGetDeviceExtension(_gd.Instance, _gd.Device, out _khrSwapchain))
            {
                throw new NotSupportedException("KHR_swapchain extension not found.");
            }

            result = _khrSwapchain.CreateSwapchain(_gd.Device, ref swapchainCI, null, out _deviceSwapchain);
            CheckResult(result);
            if (Compare.IsNotEqual(oldSwapchain, VkNull.VkSwapchainKHRNull))
            {
                _khrSwapchain.DestroySwapchain(_gd.Device, oldSwapchain, null);
            }

            _framebuffer.SetNewSwapchain(_deviceSwapchain, width, height, surfaceFormat, swapchainCI.ImageExtent);
            return true;
        }

        private bool GetPresentQueueIndex(out uint queueFamilyIndex)
        {
            uint graphicsQueueIndex = _gd.GraphicsQueueIndex;
            uint presentQueueIndex = _gd.PresentQueueIndex;

            if (QueueSupportsPresent(graphicsQueueIndex, _surface))
            {
                queueFamilyIndex = graphicsQueueIndex;
                return true;
            }
            else if (graphicsQueueIndex != presentQueueIndex && QueueSupportsPresent(presentQueueIndex, _surface))
            {
                queueFamilyIndex = presentQueueIndex;
                return true;
            }

            queueFamilyIndex = 0;
            return false;
        }

        private bool QueueSupportsPresent(uint queueFamilyIndex, VkSurfaceKHR surface)
        {
            VkResult result = _gd.KhrSurface.GetPhysicalDeviceSurfaceSupport(
                _gd.PhysicalDevice,
                queueFamilyIndex,
                surface,
                out VkBool32 supported);
            CheckResult(result);
            return supported;
        }

        public override void Dispose()
        {
            RefCount.Decrement();
        }

        private void DisposeCore()
        {
            vk.GetApi().DestroyFence(_gd.Device, _imageAvailableFence, null);
            _framebuffer.Dispose();
            _khrSwapchain.DestroySwapchain(_gd.Device, _deviceSwapchain, null);
            _gd.KhrSurface.DestroySurface(_gd.Instance, _surface, null);

            _disposed = true;
        }
    }
}
