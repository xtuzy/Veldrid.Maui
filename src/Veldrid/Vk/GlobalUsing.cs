global using VkInstance = Silk.NET.Vulkan.Instance;
global using VkPhysicalDevice = Silk.NET.Vulkan.PhysicalDevice;
global using VkPhysicalDeviceProperties = Silk.NET.Vulkan.PhysicalDeviceProperties;
global using VkPhysicalDeviceMemoryProperties = Silk.NET.Vulkan.PhysicalDeviceMemoryProperties;
global using VkPhysicalDeviceFeatures = Silk.NET.Vulkan.PhysicalDeviceFeatures;
global using VkDevice = Silk.NET.Vulkan.Device;
global using VkCommandPool = Silk.NET.Vulkan.CommandPool;
global using VkQueue = Silk.NET.Vulkan.Queue;
global using VkDebugReportCallbackEXT = Silk.NET.Vulkan.DebugReportCallbackEXT;
global using VkFormat = Silk.NET.Vulkan.Format;
global using VkFilter = Silk.NET.Vulkan.Filter;
global using VkCommandBuffer = Silk.NET.Vulkan.CommandBuffer;
global using VkSurfaceKHR = Silk.NET.Vulkan.SurfaceKHR;
global using VkSemaphore = Silk.NET.Vulkan.Semaphore;
global using VkSubmitInfo = Silk.NET.Vulkan.SubmitInfo;
global using VkPipelineStageFlags = Silk.NET.Vulkan.PipelineStageFlags;
global using VkMemoryPropertyFlags = Silk.NET.Vulkan.MemoryPropertyFlags;
global using PFN_vkDebugReportCallbackEXT = Silk.NET.Vulkan.DebugReportCallbackFunctionEXT;
global using VkResult = Silk.NET.Vulkan.Result;
global using VulkanVkFence = Silk.NET.Vulkan.Fence;
global using VkFenceCreateInfo = Silk.NET.Vulkan.FenceCreateInfo;
global using VkFenceCreateFlags = Silk.NET.Vulkan.FenceCreateFlags;

global using VkSwapchainKHR = Silk.NET.Vulkan.SwapchainKHR;
global using VkPresentInfoKHR = Silk.NET.Vulkan.PresentInfoKHR;
global using VulkanVkBuffer = Silk.NET.Vulkan.Buffer;
global using VkMemoryRequirements = Silk.NET.Vulkan.MemoryRequirements;
global using VkBufferUsageFlags = Silk.NET.Vulkan.BufferUsageFlags;
global using VkBufferCreateInfo = Silk.NET.Vulkan.BufferCreateInfo;
global using VkBufferMemoryRequirementsInfo2KHR = Silk.NET.Vulkan.BufferMemoryRequirementsInfo2KHR;
global using VkMemoryRequirements2KHR = Silk.NET.Vulkan.MemoryRequirements2KHR;
global using VkMemoryDedicatedRequirementsKHR = Silk.NET.Vulkan.MemoryDedicatedRequirementsKHR;
global using VkImage = Silk.NET.Vulkan.Image;
global using VkWin32SurfaceCreateInfoKHR = Silk.NET.Vulkan.Win32SurfaceCreateInfoKHR;
global using VkXlibSurfaceCreateInfoKHR = Silk.NET.Vulkan.XlibSurfaceCreateInfoKHR;
global using VkWaylandSurfaceCreateInfoKHR = Silk.NET.Vulkan.WaylandSurfaceCreateInfoKHR;
global using VkAndroidSurfaceCreateInfoKHR = Silk.NET.Vulkan.AndroidSurfaceCreateInfoKHR;
global using VkMacOSSurfaceCreateInfoMVK = Silk.NET.Vulkan.MacOSSurfaceCreateInfoMVK;
global using VkIOSSurfaceCreateInfoMVK = Silk.NET.Vulkan.IOSSurfaceCreateInfoMVK;
global using VkRect2D = Silk.NET.Vulkan.Rect2D;
global using VkClearValue = Silk.NET.Vulkan.ClearValue;
global using VkRenderPass = Silk.NET.Vulkan.RenderPass;
global using VkCommandPoolCreateInfo = Silk.NET.Vulkan.CommandPoolCreateInfo;
global using VkCommandPoolCreateFlags = Silk.NET.Vulkan.CommandPoolCreateFlags;
global using VkCommandBufferAllocateInfo = Silk.NET.Vulkan.CommandBufferAllocateInfo;
global using VkCommandBufferLevel = Silk.NET.Vulkan.CommandBufferLevel;
global using VkCommandBufferResetFlags = Silk.NET.Vulkan.CommandBufferResetFlags;
global using VkCommandBufferBeginInfo = Silk.NET.Vulkan.CommandBufferBeginInfo;
global using VkCommandBufferUsageFlags = Silk.NET.Vulkan.CommandBufferUsageFlags;
global using VkClearColorValue = Silk.NET.Vulkan.ClearColorValue;
global using VkClearAttachment = Silk.NET.Vulkan.ClearAttachment;
global using VkImageAspectFlags = Silk.NET.Vulkan.ImageAspectFlags;
global using VkClearRect = Silk.NET.Vulkan.ClearRect;
global using VkClearDepthStencilValue = Silk.NET.Vulkan.ClearDepthStencilValue;
global using VkPipelineBindPoint = Silk.NET.Vulkan.PipelineBindPoint;
global using VkImageLayout = Silk.NET.Vulkan.ImageLayout;
global using VkPipelineLayout = Silk.NET.Vulkan.PipelineLayout;
global using VkDescriptorSet = Silk.NET.Vulkan.DescriptorSet;
global using VkImageResolve = Silk.NET.Vulkan.ImageResolve;
global using VkExtent3D = Silk.NET.Vulkan.Extent3D;
global using VkImageSubresourceLayers = Silk.NET.Vulkan.ImageSubresourceLayers;
global using VkRenderPassBeginInfo = Silk.NET.Vulkan.RenderPassBeginInfo;
global using VulkanVkFramebuffer = Silk.NET.Vulkan.Framebuffer;
global using VkSubpassContents = Silk.NET.Vulkan.SubpassContents;
global using VkDependencyFlags = Silk.NET.Vulkan.DependencyFlags;

global using VkSamplerMipmapMode = Silk.NET.Vulkan.SamplerMipmapMode;
global using VkSamplerAddressMode = Silk.NET.Vulkan.SamplerAddressMode;
global using VkImageUsageFlags = Silk.NET.Vulkan.ImageUsageFlags;
global using VkImageType = Silk.NET.Vulkan.ImageType;
global using VkDescriptorType = Silk.NET.Vulkan.DescriptorType;
global using VkSampleCountFlags = Silk.NET.Vulkan.SampleCountFlags;
global using VkStencilOp = Silk.NET.Vulkan.StencilOp;
global using VkPolygonMode = Silk.NET.Vulkan.PolygonMode;
global using VkCullModeFlags = Silk.NET.Vulkan.CullModeFlags;
global using VkBlendOp = Silk.NET.Vulkan.BlendOp;
global using VkColorComponentFlags = Silk.NET.Vulkan.ColorComponentFlags;
global using VkPrimitiveTopology = Silk.NET.Vulkan.PrimitiveTopology;
global using VkBlendFactor = Silk.NET.Vulkan.BlendFactor;
global using VkShaderStageFlags = Silk.NET.Vulkan.ShaderStageFlags;
global using VkBorderColor = Silk.NET.Vulkan.BorderColor;
global using VkIndexType = Silk.NET.Vulkan.IndexType;
global using VkCompareOp = Silk.NET.Vulkan.CompareOp;

global using VkBufferCopy = Silk.NET.Vulkan.BufferCopy;
global using VkMemoryBarrier = Silk.NET.Vulkan.MemoryBarrier;
global using VkStructureType = Silk.NET.Vulkan.StructureType;
global using VkAccessFlags = Silk.NET.Vulkan.AccessFlags;
global using VkImageCopy = Silk.NET.Vulkan.ImageCopy;
global using VkOffset3D = Silk.NET.Vulkan.Offset3D;
global using VkSubresourceLayout = Silk.NET.Vulkan.SubresourceLayout;

global using VkImageCreateInfo = Silk.NET.Vulkan.ImageCreateInfo;
global using VkImageTiling = Silk.NET.Vulkan.ImageTiling;
global using VkImageCreateFlags = Silk.NET.Vulkan.ImageCreateFlags;
global using VkImageMemoryRequirementsInfo2KHR = Silk.NET.Vulkan.ImageMemoryRequirementsInfo2KHR;
global using VkImageSubresource = Silk.NET.Vulkan.ImageSubresource;
global using VkBufferImageCopy = Silk.NET.Vulkan.BufferImageCopy;
global using VkImageBlit = Silk.NET.Vulkan.ImageBlit;
global using SrcOffsetsBuffer = Silk.NET.Vulkan.ImageBlit.SrcOffsetsBuffer;
global using DstOffsetsBuffer = Silk.NET.Vulkan.ImageBlit.DstOffsetsBuffer;
global using AccessFlags = Silk.NET.Vulkan.AccessFlags;
global using PipelineStageFlags = Silk.NET.Vulkan.PipelineStageFlags;
global using VkDebugMarkerMarkerInfoEXT = Silk.NET.Vulkan.DebugMarkerMarkerInfoEXT;
global using VkViewport = Silk.NET.Vulkan.Viewport;
global using VulkanVkPipeline = Silk.NET.Vulkan.Pipeline;
global using VkDescriptorPool = Silk.NET.Vulkan.DescriptorPool;
global using VkDescriptorSetAllocateInfo = Silk.NET.Vulkan.DescriptorSetAllocateInfo;
global using VkDescriptorPoolSize = Silk.NET.Vulkan.DescriptorPoolSize;
global using VkDescriptorPoolCreateInfo = Silk.NET.Vulkan.DescriptorPoolCreateInfo;
global using VkDescriptorPoolCreateFlags = Silk.NET.Vulkan.DescriptorPoolCreateFlags;
global using VkMemoryAllocateInfo = Silk.NET.Vulkan.MemoryAllocateInfo;
global using VkMemoryDedicatedAllocateInfoKHR = Silk.NET.Vulkan.MemoryDedicatedAllocateInfoKHR;
global using VkDeviceMemory = Silk.NET.Vulkan.DeviceMemory;
global using VkImageView = Silk.NET.Vulkan.ImageView;
global using VkRenderPassCreateInfo = Silk.NET.Vulkan.RenderPassCreateInfo;
global using VkAttachmentDescription = Silk.NET.Vulkan.AttachmentDescription;
global using VkAttachmentReference = Silk.NET.Vulkan.AttachmentReference;
global using VkAttachmentLoadOp = Silk.NET.Vulkan.AttachmentLoadOp;
global using VkAttachmentStoreOp = Silk.NET.Vulkan.AttachmentStoreOp;
global using VkSubpassDescription = Silk.NET.Vulkan.SubpassDescription;
global using VkSubpassDependency = Silk.NET.Vulkan.SubpassDependency;
global using VkFramebufferCreateInfo = Silk.NET.Vulkan.FramebufferCreateInfo;
global using VkImageViewCreateInfo = Silk.NET.Vulkan.ImageViewCreateInfo;
global using VkImageViewType = Silk.NET.Vulkan.ImageViewType;
global using VkImageSubresourceRange = Silk.NET.Vulkan.ImageSubresourceRange;
global using VkDebugReportObjectTypeEXT = Silk.NET.Vulkan.DebugReportObjectTypeEXT;
global using VkDebugMarkerObjectNameInfoEXT = Silk.NET.Vulkan.DebugMarkerObjectNameInfoEXT;
global using VkInstanceCreateInfo = Silk.NET.Vulkan.InstanceCreateInfo;
global using VkApplicationInfo = Silk.NET.Vulkan.ApplicationInfo;
global using VkDebugReportFlagsEXT = Silk.NET.Vulkan.DebugReportFlagsEXT;
global using VkDebugReportCallbackCreateInfoEXT = Silk.NET.Vulkan.DebugReportCallbackCreateInfoEXT;
global using VkExtensionProperties = Silk.NET.Vulkan.ExtensionProperties;
global using VkDeviceQueueCreateInfo = Silk.NET.Vulkan.DeviceQueueCreateInfo;
global using VkDeviceCreateInfo = Silk.NET.Vulkan.DeviceCreateInfo;
global using VkPhysicalDeviceProperties2KHR = Silk.NET.Vulkan.PhysicalDeviceProperties2KHR;
global using VkQueueFamilyProperties = Silk.NET.Vulkan.QueueFamilyProperties;
global using VkQueueFlags = Silk.NET.Vulkan.QueueFlags;
global using VkImageFormatProperties = Silk.NET.Vulkan.ImageFormatProperties;
global using VkFormatProperties = Silk.NET.Vulkan.FormatProperties;
global using VkFormatFeatureFlags = Silk.NET.Vulkan.FormatFeatureFlags;
global using VkAllocationCallbacks = Silk.NET.Vulkan.AllocationCallbacks;
global using VkGraphicsPipelineCreateInfo = Silk.NET.Vulkan.GraphicsPipelineCreateInfo;
global using VkDescriptorSetLayout = Silk.NET.Vulkan.DescriptorSetLayout;
global using VkDescriptorSetLayoutCreateInfo = Silk.NET.Vulkan.DescriptorSetLayoutCreateInfo;
global using VkDescriptorSetLayoutBinding = Silk.NET.Vulkan.DescriptorSetLayoutBinding;
global using VkWriteDescriptorSet = Silk.NET.Vulkan.WriteDescriptorSet;
global using VkDescriptorBufferInfo = Silk.NET.Vulkan.DescriptorBufferInfo;
global using VkDescriptorImageInfo = Silk.NET.Vulkan.DescriptorImageInfo;
global using VulkanVkSampler = Silk.NET.Vulkan.Sampler;
global using VkSamplerCreateInfo = Silk.NET.Vulkan.SamplerCreateInfo;
global using VkShaderModule = Silk.NET.Vulkan.ShaderModule;
global using VkShaderModuleCreateInfo = Silk.NET.Vulkan.ShaderModuleCreateInfo;
global using VkSurfaceCapabilitiesKHR = Silk.NET.Vulkan.SurfaceCapabilitiesKHR;
global using VkSurfaceFormatKHR = Silk.NET.Vulkan.SurfaceFormatKHR;
global using VkColorSpaceKHR = Silk.NET.Vulkan.ColorSpaceKHR;
global using VkPresentModeKHR = Silk.NET.Vulkan.PresentModeKHR;
global using VkSwapchainCreateInfoKHR = Silk.NET.Vulkan.SwapchainCreateInfoKHR;
global using VkExtent2D = Silk.NET.Vulkan.Extent2D;
global using VkSharingMode = Silk.NET.Vulkan.SharingMode;
global using VkSurfaceTransformFlagsKHR = Silk.NET.Vulkan.SurfaceTransformFlagsKHR;
global using VkCompositeAlphaFlagsKHR = Silk.NET.Vulkan.CompositeAlphaFlagsKHR;
global using VkLayerProperties = Silk.NET.Vulkan.LayerProperties;
global using VkImageMemoryBarrier = Silk.NET.Vulkan.ImageMemoryBarrier;
global using VkMemoryType = Silk.NET.Vulkan.MemoryType;
global using VkPipelineColorBlendStateCreateInfo = Silk.NET.Vulkan.PipelineColorBlendStateCreateInfo;
global using VkPipelineColorBlendAttachmentState = Silk.NET.Vulkan.PipelineColorBlendAttachmentState;
global using VkPipelineRasterizationStateCreateInfo = Silk.NET.Vulkan.PipelineRasterizationStateCreateInfo;
global using VkFrontFace = Silk.NET.Vulkan.FrontFace;
global using VkPipelineDynamicStateCreateInfo = Silk.NET.Vulkan.PipelineDynamicStateCreateInfo;
global using VkDynamicState = Silk.NET.Vulkan.DynamicState;
global using VkPipelineDepthStencilStateCreateInfo = Silk.NET.Vulkan.PipelineDepthStencilStateCreateInfo;
global using VkPipelineMultisampleStateCreateInfo = Silk.NET.Vulkan.PipelineMultisampleStateCreateInfo;
global using VkPipelineInputAssemblyStateCreateInfo = Silk.NET.Vulkan.PipelineInputAssemblyStateCreateInfo;
global using VkPipelineVertexInputStateCreateInfo = Silk.NET.Vulkan.PipelineVertexInputStateCreateInfo;
global using VkVertexInputBindingDescription = Silk.NET.Vulkan.VertexInputBindingDescription;
global using VkVertexInputAttributeDescription = Silk.NET.Vulkan.VertexInputAttributeDescription;
global using VkVertexInputRate = Silk.NET.Vulkan.VertexInputRate;
global using VkSpecializationInfo = Silk.NET.Vulkan.SpecializationInfo;
global using VkSpecializationMapEntry = Silk.NET.Vulkan.SpecializationMapEntry;
global using VkPipelineShaderStageCreateInfo = Silk.NET.Vulkan.PipelineShaderStageCreateInfo;
global using VkPipelineViewportStateCreateInfo = Silk.NET.Vulkan.PipelineViewportStateCreateInfo;
global using VkPipelineLayoutCreateInfo = Silk.NET.Vulkan.PipelineLayoutCreateInfo;
global using VkPipelineCache = Silk.NET.Vulkan.PipelineCache;
global using VkComputePipelineCreateInfo = Silk.NET.Vulkan.ComputePipelineCreateInfo;
global using VkBool32 = Silk.NET.Core.Bool32;
global using vk = Veldrid.Vk.SilkNETVk;
using Silk.NET.Vulkan;
using System;

public static class Compare
{
    public static bool IsEqual(VkRect2D left, VkRect2D right)
    {
        if (right.Offset.Equals(left.Offset))
        {
            return right.Extent.Equals(left.Extent);
        }

        return false;
    }

    public static bool IsNotEqual(VkRect2D left, VkRect2D right)
    {
        return !IsEqual(left, right);
    }

    public static bool IsEqual(RenderPass left, RenderPass right)
    {
        return left.Handle == right.Handle;
    }

    public static bool IsNotEqual(RenderPass left, RenderPass right)
    {
        return !IsEqual(left, right);
    }

    public static bool IsEqual(VkCommandBuffer left, VkCommandBuffer right)
    {
        return left.Handle == right.Handle;
    }

    public static bool IsEqual(VulkanVkBuffer left, VulkanVkBuffer right)
    {
        return left.Handle == right.Handle;
    }

    public static bool IsNotEqual(VulkanVkBuffer left, VulkanVkBuffer right)
    {
        return !IsEqual(left, right);
    }

    public static bool IsEqual(VkSurfaceKHR left, VkSurfaceKHR right)
    {
        return left.Handle == right.Handle;
    }

    public static bool IsEqual(VkSwapchainKHR left, VkSwapchainKHR right)
    {
        return left.Handle == right.Handle;
    }

    public static bool IsNotEqual(VkSwapchainKHR left, VkSwapchainKHR right)
    {
        return !IsEqual(left, right);
    }

    public static bool IsEqual(VkImage left, VkImage right)
    {
        return left.Handle == right.Handle;
    }

    public static bool IsNotEqual(VkImage left, VkImage right)
    {
        return !IsEqual(left, right);
    }

    public static bool IsEqual(DeviceMemory left, DeviceMemory right)
    {
        return left.Handle == right.Handle;
    }

    public static bool IsEqual(DescriptorPool left, DescriptorPool right)
    {
        return left.Handle == right.Handle;
    }
}

public static class VkNull
{
    public static readonly VkImage VkImageNull = new VkImage();
    public static readonly VulkanVkBuffer VulkanVkBufferNull = new VulkanVkBuffer();
    public static readonly VkSurfaceKHR VkSurfaceKHRNull = new VkSurfaceKHR();
    public static readonly VkSemaphore VkSemaphoreNull = new VkSemaphore();
    public static readonly VkSwapchainKHR VkSwapchainKHRNull = new VkSwapchainKHR();
    public static readonly VkRenderPass VkRenderPassNull = new VkRenderPass();
    public static readonly VulkanVkFence VulkanVkFenceNull = new VulkanVkFence();
    public static readonly VkPipelineCache VkPipelineCacheNull = new VkPipelineCache();
}

namespace Veldrid.Vk
{ 
    public class SilkNETVk
    {
        static Silk.NET.Vulkan.Vk VK;

        public static void Init(Silk.NET.Vulkan.Vk vk = null)
        {
            VK = vk;
        }

        public static Silk.NET.Vulkan.Vk GetApi()
        {
            if(VK == null)
            {
                VK = Silk.NET.Vulkan.Vk.GetApi();
            }
            return VK;
        }
    }
}
