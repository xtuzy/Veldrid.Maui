using Silk.NET.Vulkan;
using System;
using System.Diagnostics;
//using Vulkan;
//using static Vulkan.VulkanNative;

namespace Veldrid.Vk
{
    internal unsafe static class VulkanUtil
    {
        private static Lazy<bool> s_isVulkanLoaded = new Lazy<bool>(TryLoadVulkan);
        private static readonly Lazy<string[]> s_instanceExtensions = new Lazy<string[]>(EnumerateInstanceExtensions);

        [Conditional("DEBUG")]
        public static void CheckResult(VkResult result)
        {
            if (result != VkResult.Success)
            {
                throw new VeldridException("Unsuccessful VkResult: " + result);
            }
        }

        public static bool TryFindMemoryType(VkPhysicalDeviceMemoryProperties memProperties, uint typeFilter, VkMemoryPropertyFlags properties, out uint typeIndex)
        {
            typeIndex = 0;

            for (int i = 0; i < memProperties.MemoryTypeCount; i++)
            {
                if (((typeFilter & (1 << i)) != 0)
                    && (memProperties.MemoryTypes[i].PropertyFlags & properties) == properties)
                {
                    typeIndex = (uint)i;
                    return true;
                }
            }

            return false;
        }

        public static string[] EnumerateInstanceLayers()
        {
            uint propCount = 0;
            VkResult result = vk.GetApi().EnumerateInstanceLayerProperties(ref propCount, null);
            CheckResult(result);
            if (propCount == 0)
            {
                return Array.Empty<string>();
            }

            VkLayerProperties[] props = new VkLayerProperties[propCount];
            vk.GetApi().EnumerateInstanceLayerProperties(ref propCount, ref props[0]);

            string[] ret = new string[propCount];
            for (int i = 0; i < propCount; i++)
            {
                fixed (byte* layerNamePtr = props[i].LayerName)
                {
                    ret[i] = Util.GetString(layerNamePtr);
                }
            }

            return ret;
        }

        public static string[] GetInstanceExtensions() => s_instanceExtensions.Value;

        private static string[] EnumerateInstanceExtensions()
        {
            if (!IsVulkanLoaded())
            {
                return Array.Empty<string>();
            }

            uint propCount = 0;
            VkResult result = vk.GetApi().EnumerateInstanceExtensionProperties((byte*)null, ref propCount, null);
            if (result != VkResult.Success)
            {
                return Array.Empty<string>();
            }

            if (propCount == 0)
            {
                return Array.Empty<string>();
            }

            VkExtensionProperties[] props = new VkExtensionProperties[propCount];
            vk.GetApi().EnumerateInstanceExtensionProperties((byte*)null, ref propCount, ref props[0]);

            string[] ret = new string[propCount];
            for (int i = 0; i < propCount; i++)
            {
                fixed (byte* extensionNamePtr = props[i].ExtensionName)
                {
                    ret[i] = Util.GetString(extensionNamePtr);
                }
            }

            return ret;
        }

        public static bool IsVulkanLoaded() => s_isVulkanLoaded.Value;
        private static bool TryLoadVulkan()
        {
            try
            {
                uint propCount;
                vk.GetApi().EnumerateInstanceExtensionProperties((byte*)null, &propCount, null);
                return true;
            }
            catch { return false; }
        }

        public static void TransitionImageLayout(
            VkCommandBuffer cb,
            VkImage image,
            uint baseMipLevel,
            uint levelCount,
            uint baseArrayLayer,
            uint layerCount,
            VkImageAspectFlags aspectMask,
            VkImageLayout oldLayout,
            VkImageLayout newLayout)
        {
            Debug.Assert(oldLayout != newLayout);
            VkImageMemoryBarrier barrier = new VkImageMemoryBarrier();
            barrier.OldLayout = oldLayout;
            barrier.NewLayout = newLayout;
            barrier.SrcQueueFamilyIndex = Silk.NET.Vulkan.Vk.QueueFamilyIgnored;
            barrier.DstQueueFamilyIndex = Silk.NET.Vulkan.Vk.QueueFamilyIgnored;
            barrier.Image = image;
            barrier.SubresourceRange.AspectMask = aspectMask;
            barrier.SubresourceRange.BaseMipLevel = baseMipLevel;
            barrier.SubresourceRange.LevelCount = levelCount;
            barrier.SubresourceRange.BaseArrayLayer = baseArrayLayer;
            barrier.SubresourceRange.LayerCount = layerCount;

            VkPipelineStageFlags srcStageFlags = VkPipelineStageFlags.None;
            VkPipelineStageFlags dstStageFlags = VkPipelineStageFlags.None;

            if ((oldLayout == VkImageLayout.Undefined || oldLayout == VkImageLayout.Preinitialized) && newLayout == VkImageLayout.TransferDstOptimal)
            {
                barrier.SrcAccessMask = VkAccessFlags.None;
                barrier.DstAccessMask = VkAccessFlags.TransferWriteBit;
                srcStageFlags = VkPipelineStageFlags.TopOfPipeBit;
                dstStageFlags = VkPipelineStageFlags.TransferBit;
            }
            else if (oldLayout == VkImageLayout.ShaderReadOnlyOptimal && newLayout == VkImageLayout.TransferSrcOptimal)
            {
                barrier.SrcAccessMask = VkAccessFlags.ShaderReadBit;
                barrier.DstAccessMask = VkAccessFlags.TransferReadBit;
                srcStageFlags = VkPipelineStageFlags.FragmentShaderBit;
                dstStageFlags = VkPipelineStageFlags.TransferBit;
            }
            else if (oldLayout == VkImageLayout.ShaderReadOnlyOptimal && newLayout == VkImageLayout.TransferDstOptimal)
            {
                barrier.SrcAccessMask = VkAccessFlags.ShaderReadBit;
                barrier.DstAccessMask = VkAccessFlags.TransferWriteBit;
                srcStageFlags = VkPipelineStageFlags.FragmentShaderBit;
                dstStageFlags = VkPipelineStageFlags.TransferBit;
            }
            else if (oldLayout == VkImageLayout.Preinitialized && newLayout == VkImageLayout.TransferSrcOptimal)
            {
                barrier.SrcAccessMask = VkAccessFlags.None;
                barrier.DstAccessMask = VkAccessFlags.TransferReadBit;
                srcStageFlags = VkPipelineStageFlags.TopOfPipeBit;
                dstStageFlags = VkPipelineStageFlags.TransferBit;
            }
            else if (oldLayout == VkImageLayout.Preinitialized && newLayout == VkImageLayout.General)
            {
                barrier.SrcAccessMask = VkAccessFlags.None;
                barrier.DstAccessMask = VkAccessFlags.ShaderReadBit;
                srcStageFlags = VkPipelineStageFlags.TopOfPipeBit;
                dstStageFlags = VkPipelineStageFlags.ComputeShaderBit;
            }
            else if (oldLayout == VkImageLayout.Preinitialized && newLayout == VkImageLayout.ShaderReadOnlyOptimal)
            {
                barrier.SrcAccessMask = VkAccessFlags.None;
                barrier.DstAccessMask = VkAccessFlags.ShaderReadBit;
                srcStageFlags = VkPipelineStageFlags.TopOfPipeBit;
                dstStageFlags = VkPipelineStageFlags.FragmentShaderBit;
            }
            else if (oldLayout == VkImageLayout.General && newLayout == VkImageLayout.ShaderReadOnlyOptimal)
            {
                barrier.SrcAccessMask = VkAccessFlags.TransferReadBit;
                barrier.DstAccessMask = VkAccessFlags.ShaderReadBit;
                srcStageFlags = VkPipelineStageFlags.TransferBit;
                dstStageFlags = VkPipelineStageFlags.FragmentShaderBit;
            }
            else if (oldLayout == VkImageLayout.ShaderReadOnlyOptimal && newLayout == VkImageLayout.General)
            {
                barrier.SrcAccessMask = VkAccessFlags.ShaderReadBit;
                barrier.DstAccessMask = VkAccessFlags.ShaderReadBit;
                srcStageFlags = VkPipelineStageFlags.FragmentShaderBit;
                dstStageFlags = VkPipelineStageFlags.ComputeShaderBit;
            }

            else if (oldLayout == VkImageLayout.TransferSrcOptimal && newLayout == VkImageLayout.ShaderReadOnlyOptimal)
            {
                barrier.SrcAccessMask = VkAccessFlags.TransferReadBit;
                barrier.DstAccessMask = VkAccessFlags.ShaderReadBit;
                srcStageFlags = VkPipelineStageFlags.TransferBit;
                dstStageFlags = VkPipelineStageFlags.FragmentShaderBit;
            }
            else if (oldLayout == VkImageLayout.TransferDstOptimal && newLayout == VkImageLayout.ShaderReadOnlyOptimal)
            {
                barrier.SrcAccessMask = VkAccessFlags.TransferWriteBit;
                barrier.DstAccessMask = VkAccessFlags.ShaderReadBit;
                srcStageFlags = VkPipelineStageFlags.TransferBit;
                dstStageFlags = VkPipelineStageFlags.FragmentShaderBit;
            }
            else if (oldLayout == VkImageLayout.TransferSrcOptimal && newLayout == VkImageLayout.TransferDstOptimal)
            {
                barrier.SrcAccessMask = VkAccessFlags.TransferReadBit;
                barrier.DstAccessMask = VkAccessFlags.TransferWriteBit;
                srcStageFlags = VkPipelineStageFlags.TransferBit;
                dstStageFlags = VkPipelineStageFlags.TransferBit;
            }
            else if (oldLayout == VkImageLayout.TransferDstOptimal && newLayout == VkImageLayout.TransferSrcOptimal)
            {
                barrier.SrcAccessMask = VkAccessFlags.TransferWriteBit;
                barrier.DstAccessMask = VkAccessFlags.TransferReadBit;
                srcStageFlags = VkPipelineStageFlags.TransferBit;
                dstStageFlags = VkPipelineStageFlags.TransferBit;
            }
            else if (oldLayout == VkImageLayout.ColorAttachmentOptimal && newLayout == VkImageLayout.TransferSrcOptimal)
            {
                barrier.SrcAccessMask = VkAccessFlags.ColorAttachmentWriteBit;
                barrier.DstAccessMask = VkAccessFlags.TransferReadBit;
                srcStageFlags = VkPipelineStageFlags.ColorAttachmentOutputBit;
                dstStageFlags = VkPipelineStageFlags.TransferBit;
            }
            else if (oldLayout == VkImageLayout.ColorAttachmentOptimal && newLayout == VkImageLayout.TransferDstOptimal)
            {
                barrier.SrcAccessMask = VkAccessFlags.ColorAttachmentWriteBit;
                barrier.DstAccessMask = VkAccessFlags.TransferWriteBit;
                srcStageFlags = VkPipelineStageFlags.ColorAttachmentOutputBit;
                dstStageFlags = VkPipelineStageFlags.TransferBit;
            }
            else if (oldLayout == VkImageLayout.ColorAttachmentOptimal && newLayout == VkImageLayout.ShaderReadOnlyOptimal)
            {
                barrier.SrcAccessMask = VkAccessFlags.ColorAttachmentWriteBit;
                barrier.DstAccessMask = VkAccessFlags.ShaderReadBit;
                srcStageFlags = VkPipelineStageFlags.ColorAttachmentOutputBit;
                dstStageFlags = VkPipelineStageFlags.FragmentShaderBit;
            }
            else if (oldLayout == VkImageLayout.DepthStencilAttachmentOptimal && newLayout == VkImageLayout.ShaderReadOnlyOptimal)
            {
                barrier.SrcAccessMask = VkAccessFlags.DepthStencilAttachmentWriteBit;
                barrier.DstAccessMask = VkAccessFlags.ShaderReadBit;
                srcStageFlags = VkPipelineStageFlags.LateFragmentTestsBit;
                dstStageFlags = VkPipelineStageFlags.FragmentShaderBit;
            }
            else if (oldLayout == VkImageLayout.ColorAttachmentOptimal && newLayout == VkImageLayout.PresentSrcKhr)
            {
                barrier.SrcAccessMask = VkAccessFlags.ColorAttachmentWriteBit;
                barrier.DstAccessMask = VkAccessFlags.MemoryReadBit;
                srcStageFlags = VkPipelineStageFlags.ColorAttachmentOutputBit;
                dstStageFlags = VkPipelineStageFlags.BottomOfPipeBit;
            }
            else if (oldLayout == VkImageLayout.TransferDstOptimal && newLayout == VkImageLayout.PresentSrcKhr)
            {
                barrier.SrcAccessMask = VkAccessFlags.TransferWriteBit;
                barrier.DstAccessMask = VkAccessFlags.MemoryReadBit;
                srcStageFlags = VkPipelineStageFlags.TransferBit;
                dstStageFlags = VkPipelineStageFlags.BottomOfPipeBit;
            }
            else if (oldLayout == VkImageLayout.TransferDstOptimal && newLayout == VkImageLayout.ColorAttachmentOptimal)
            {
                barrier.SrcAccessMask = VkAccessFlags.TransferWriteBit;
                barrier.DstAccessMask = VkAccessFlags.ColorAttachmentWriteBit;
                srcStageFlags = VkPipelineStageFlags.TransferBit;
                dstStageFlags = VkPipelineStageFlags.ColorAttachmentOutputBit;
            }
            else if (oldLayout == VkImageLayout.TransferDstOptimal && newLayout == VkImageLayout.DepthStencilAttachmentOptimal)
            {
                barrier.SrcAccessMask = VkAccessFlags.TransferWriteBit;
                barrier.DstAccessMask = VkAccessFlags.DepthStencilAttachmentWriteBit;
                srcStageFlags = VkPipelineStageFlags.TransferBit;
                dstStageFlags = VkPipelineStageFlags.LateFragmentTestsBit;
            }
            else if (oldLayout == VkImageLayout.General && newLayout == VkImageLayout.TransferSrcOptimal)
            {
                barrier.SrcAccessMask = VkAccessFlags.ShaderWriteBit;
                barrier.DstAccessMask = VkAccessFlags.TransferReadBit;
                srcStageFlags = VkPipelineStageFlags.ComputeShaderBit;
                dstStageFlags = VkPipelineStageFlags.TransferBit;
            }
            else if (oldLayout == VkImageLayout.General && newLayout == VkImageLayout.TransferDstOptimal)
            {
                barrier.SrcAccessMask = VkAccessFlags.ShaderWriteBit;
                barrier.DstAccessMask = VkAccessFlags.TransferWriteBit;
                srcStageFlags = VkPipelineStageFlags.ComputeShaderBit;
                dstStageFlags = VkPipelineStageFlags.TransferBit;
            }
            else if (oldLayout == VkImageLayout.PresentSrcKhr && newLayout == VkImageLayout.TransferSrcOptimal)
            {
                barrier.SrcAccessMask = VkAccessFlags.MemoryReadBit;
                barrier.DstAccessMask = VkAccessFlags.TransferReadBit;
                srcStageFlags = VkPipelineStageFlags.BottomOfPipeBit;
                dstStageFlags = VkPipelineStageFlags.TransferBit;
            }
            else
            {
                Debug.Fail("Invalid image layout transition.");
            }

            vk.GetApi().CmdPipelineBarrier(
                cb,
                srcStageFlags,
                dstStageFlags,
                VkDependencyFlags.None,
                0, null,
                0, null,
                1, &barrier);
        }
    }

    internal unsafe static class VkPhysicalDeviceMemoryPropertiesEx
    {
        public static VkMemoryType GetMemoryType(this VkPhysicalDeviceMemoryProperties memoryProperties, uint index)
        {
            return (&memoryProperties.MemoryTypes.Element0)[index];
        }
    }
}
