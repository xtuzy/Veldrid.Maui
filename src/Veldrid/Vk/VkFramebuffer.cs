using System.Collections.Generic;
//using Vulkan;
//using static Vulkan.VulkanNative;
using static Veldrid.Vk.VulkanUtil;
using System;
using System.Diagnostics;

namespace Veldrid.Vk
{
    internal unsafe class VkFramebuffer : VkFramebufferBase
    {
        private readonly VkGraphicsDevice _gd;
        private readonly VulkanVkFramebuffer _deviceFramebuffer;
        private readonly VkRenderPass _renderPassNoClearLoad;
        private readonly VkRenderPass _renderPassNoClear;
        private readonly VkRenderPass _renderPassClear;
        private readonly List<VkImageView> _attachmentViews = new List<VkImageView>();
        private bool _destroyed;
        private string _name;

        public override VulkanVkFramebuffer CurrentFramebuffer => _deviceFramebuffer;
        public override VkRenderPass RenderPassNoClear_Init => _renderPassNoClear;
        public override VkRenderPass RenderPassNoClear_Load => _renderPassNoClearLoad;
        public override VkRenderPass RenderPassClear => _renderPassClear;

        public override uint RenderableWidth => Width;
        public override uint RenderableHeight => Height;

        public override uint AttachmentCount { get; }

        public override bool IsDisposed => _destroyed;

        public VkFramebuffer(VkGraphicsDevice gd, ref FramebufferDescription description, bool isPresented)
            : base(description.DepthTarget, description.ColorTargets)
        {
            _gd = gd;

            VkRenderPassCreateInfo renderPassCI = new VkRenderPassCreateInfo();

            StackList<VkAttachmentDescription> attachments = new StackList<VkAttachmentDescription>();

            uint colorAttachmentCount = (uint)ColorTargets.Count;
            StackList<VkAttachmentReference> colorAttachmentRefs = new StackList<VkAttachmentReference>();
            for (int i = 0; i < colorAttachmentCount; i++)
            {
                VkTexture vkColorTex = Util.AssertSubtype<Texture, VkTexture>(ColorTargets[i].Target);
                VkAttachmentDescription colorAttachmentDesc = new VkAttachmentDescription();
                colorAttachmentDesc.Format = vkColorTex.VkFormat;
                colorAttachmentDesc.Samples = vkColorTex.VkSampleCount;
                colorAttachmentDesc.LoadOp = VkAttachmentLoadOp.Load;
                colorAttachmentDesc.StoreOp = VkAttachmentStoreOp.Store;
                colorAttachmentDesc.StencilLoadOp = VkAttachmentLoadOp.DontCare;
                colorAttachmentDesc.StencilStoreOp = VkAttachmentStoreOp.DontCare;
                colorAttachmentDesc.InitialLayout = isPresented
                    ? VkImageLayout.PresentSrcKhr
                    : ((vkColorTex.Usage & TextureUsage.Sampled) != 0)
                        ? VkImageLayout.ShaderReadOnlyOptimal
                        : VkImageLayout.ColorAttachmentOptimal;
                colorAttachmentDesc.FinalLayout = VkImageLayout.ColorAttachmentOptimal;
                attachments.Add(colorAttachmentDesc);

                VkAttachmentReference colorAttachmentRef = new VkAttachmentReference();
                colorAttachmentRef.Attachment = (uint)i;
                colorAttachmentRef.Layout = VkImageLayout.ColorAttachmentOptimal;
                colorAttachmentRefs.Add(colorAttachmentRef);
            }

            VkAttachmentDescription depthAttachmentDesc = new VkAttachmentDescription();
            VkAttachmentReference depthAttachmentRef = new VkAttachmentReference();
            if (DepthTarget != null)
            {
                VkTexture vkDepthTex = Util.AssertSubtype<Texture, VkTexture>(DepthTarget.Value.Target);
                bool hasStencil = FormatHelpers.IsStencilFormat(vkDepthTex.Format);
                depthAttachmentDesc.Format = vkDepthTex.VkFormat;
                depthAttachmentDesc.Samples = vkDepthTex.VkSampleCount;
                depthAttachmentDesc.LoadOp = VkAttachmentLoadOp.Load;
                depthAttachmentDesc.StoreOp = VkAttachmentStoreOp.Store;
                depthAttachmentDesc.StencilLoadOp = VkAttachmentLoadOp.DontCare;
                depthAttachmentDesc.StencilStoreOp = hasStencil
                    ? VkAttachmentStoreOp.Store
                    : VkAttachmentStoreOp.DontCare;
                depthAttachmentDesc.InitialLayout = ((vkDepthTex.Usage & TextureUsage.Sampled) != 0)
                    ? VkImageLayout.ShaderReadOnlyOptimal
                    : VkImageLayout.DepthStencilAttachmentOptimal;
                depthAttachmentDesc.FinalLayout = VkImageLayout.DepthStencilAttachmentOptimal;

                depthAttachmentRef.Attachment = (uint)description.ColorTargets.Length;
                depthAttachmentRef.Layout = VkImageLayout.DepthStencilAttachmentOptimal;
            }

            VkSubpassDescription subpass = new VkSubpassDescription();
            subpass.PipelineBindPoint = VkPipelineBindPoint.Graphics;
            if (ColorTargets.Count > 0)
            {
                subpass.ColorAttachmentCount = colorAttachmentCount;
                subpass.PColorAttachments = (VkAttachmentReference*)colorAttachmentRefs.Data;
            }

            if (DepthTarget != null)
            {
                subpass.PDepthStencilAttachment = &depthAttachmentRef;
                attachments.Add(depthAttachmentDesc);
            }

            VkSubpassDependency subpassDependency = new VkSubpassDependency();
            subpassDependency.SrcSubpass = Silk.NET.Vulkan.Vk.SubpassExternal;
            subpassDependency.SrcStageMask = VkPipelineStageFlags.ColorAttachmentOutputBit;
            subpassDependency.DstStageMask = VkPipelineStageFlags.ColorAttachmentOutputBit;
            subpassDependency.DstAccessMask = VkAccessFlags.ColorAttachmentReadBit | VkAccessFlags.ColorAttachmentWriteBit;

            renderPassCI.AttachmentCount = attachments.Count;
            renderPassCI.PAttachments = (VkAttachmentDescription*)attachments.Data;
            renderPassCI.SubpassCount = 1;
            renderPassCI.PSubpasses = &subpass;
            renderPassCI.DependencyCount = 1;
            renderPassCI.PDependencies = &subpassDependency;

            VkResult creationResult = vk.GetApi().CreateRenderPass(_gd.Device, ref renderPassCI, null, out _renderPassNoClear);
            CheckResult(creationResult);

            for (int i = 0; i < colorAttachmentCount; i++)
            {
                attachments[i].LoadOp = VkAttachmentLoadOp.Load;
                attachments[i].InitialLayout = VkImageLayout.ColorAttachmentOptimal;
            }
            if (DepthTarget != null)
            {
                attachments[attachments.Count - 1].LoadOp = VkAttachmentLoadOp.Load;
                attachments[attachments.Count - 1].InitialLayout = VkImageLayout.DepthStencilAttachmentOptimal;
                bool hasStencil = FormatHelpers.IsStencilFormat(DepthTarget.Value.Target.Format);
                if (hasStencil)
                {
                    attachments[attachments.Count - 1].StencilLoadOp = VkAttachmentLoadOp.Load;
                }

            }
            creationResult = vk.GetApi().CreateRenderPass(_gd.Device, ref renderPassCI, null, out _renderPassNoClearLoad);
            CheckResult(creationResult);


            // Load version

            if (DepthTarget != null)
            {
                attachments[attachments.Count - 1].LoadOp = VkAttachmentLoadOp.Clear;
                attachments[attachments.Count - 1].InitialLayout = VkImageLayout.Undefined;
                bool hasStencil = FormatHelpers.IsStencilFormat(DepthTarget.Value.Target.Format);
                if (hasStencil)
                {
                    attachments[attachments.Count - 1].StencilLoadOp = VkAttachmentLoadOp.Clear;
                }
            }

            for (int i = 0; i < colorAttachmentCount; i++)
            {
                attachments[i].LoadOp = VkAttachmentLoadOp.Clear;
                attachments[i].InitialLayout = VkImageLayout.Undefined;
            }

            creationResult = vk.GetApi().CreateRenderPass(_gd.Device, ref renderPassCI, null, out _renderPassClear);
            CheckResult(creationResult);

            VkFramebufferCreateInfo fbCI = new VkFramebufferCreateInfo();
            uint fbAttachmentsCount = (uint)description.ColorTargets.Length;
            if (description.DepthTarget != null)
            {
                fbAttachmentsCount += 1;
            }

            VkImageView* fbAttachments = stackalloc VkImageView[(int)fbAttachmentsCount];
            for (int i = 0; i < colorAttachmentCount; i++)
            {
                VkTexture vkColorTarget = Util.AssertSubtype<Texture, VkTexture>(description.ColorTargets[i].Target);
                VkImageViewCreateInfo imageViewCI = new VkImageViewCreateInfo();
                imageViewCI.Image = vkColorTarget.OptimalDeviceImage;
                imageViewCI.Format = vkColorTarget.VkFormat;
                imageViewCI.ViewType = VkImageViewType.ImageViewType2D;
                imageViewCI.SubresourceRange = new VkImageSubresourceRange(
                    VkImageAspectFlags.ColorBit,
                    description.ColorTargets[i].MipLevel,
                    1,
                    description.ColorTargets[i].ArrayLayer,
                    1);
                VkImageView* dest = (fbAttachments + i);
                VkResult result = vk.GetApi().CreateImageView(_gd.Device, ref imageViewCI, null, dest);
                CheckResult(result);
                _attachmentViews.Add(*dest);
            }

            // Depth
            if (description.DepthTarget != null)
            {
                VkTexture vkDepthTarget = Util.AssertSubtype<Texture, VkTexture>(description.DepthTarget.Value.Target);
                bool hasStencil = FormatHelpers.IsStencilFormat(vkDepthTarget.Format);
                VkImageViewCreateInfo depthViewCI = new VkImageViewCreateInfo();
                depthViewCI.Image = vkDepthTarget.OptimalDeviceImage;
                depthViewCI.Format = vkDepthTarget.VkFormat;
                depthViewCI.ViewType = description.DepthTarget.Value.Target.ArrayLayers == 1
                    ? VkImageViewType.ImageViewType2D
                    : VkImageViewType.ImageViewType2DArray;
                depthViewCI.SubresourceRange = new VkImageSubresourceRange(
                    hasStencil ? VkImageAspectFlags.DepthBit | VkImageAspectFlags.StencilBit : VkImageAspectFlags.DepthBit,
                    description.DepthTarget.Value.MipLevel,
                    1,
                    description.DepthTarget.Value.ArrayLayer,
                    1);
                VkImageView* dest = (fbAttachments + (fbAttachmentsCount - 1));
                VkResult result = vk.GetApi().CreateImageView(_gd.Device, ref depthViewCI, null, dest);
                CheckResult(result);
                _attachmentViews.Add(*dest);
            }

            Texture dimTex;
            uint mipLevel;
            if (ColorTargets.Count > 0)
            {
                dimTex = ColorTargets[0].Target;
                mipLevel = ColorTargets[0].MipLevel;
            }
            else
            {
                Debug.Assert(DepthTarget != null);
                dimTex = DepthTarget.Value.Target;
                mipLevel = DepthTarget.Value.MipLevel;
            }

            Util.GetMipDimensions(
                dimTex,
                mipLevel,
                out uint mipWidth,
                out uint mipHeight,
                out _);

            fbCI.Width = mipWidth;
            fbCI.Height = mipHeight;

            fbCI.AttachmentCount = fbAttachmentsCount;
            fbCI.PAttachments = fbAttachments;
            fbCI.Layers = 1;
            fbCI.RenderPass = _renderPassNoClear;

            creationResult = vk.GetApi().CreateFramebuffer(_gd.Device, ref fbCI, null, out _deviceFramebuffer);
            CheckResult(creationResult);

            if (DepthTarget != null)
            {
                AttachmentCount += 1;
            }
            AttachmentCount += (uint)ColorTargets.Count;
        }

        public override void TransitionToIntermediateLayout(VkCommandBuffer cb)
        {
            for (int i = 0; i < ColorTargets.Count; i++)
            {
                FramebufferAttachment ca = ColorTargets[i];
                VkTexture vkTex = Util.AssertSubtype<Texture, VkTexture>(ca.Target);
                vkTex.SetImageLayout(ca.MipLevel, ca.ArrayLayer, VkImageLayout.ColorAttachmentOptimal);
            }
            if (DepthTarget != null)
            {
                VkTexture vkTex = Util.AssertSubtype<Texture, VkTexture>(DepthTarget.Value.Target);
                vkTex.SetImageLayout(
                    DepthTarget.Value.MipLevel,
                    DepthTarget.Value.ArrayLayer,
                    VkImageLayout.DepthStencilAttachmentOptimal);
            }
        }

        public override void TransitionToFinalLayout(VkCommandBuffer cb)
        {
            for (int i = 0; i < ColorTargets.Count; i++)
            {
                FramebufferAttachment ca = ColorTargets[i];
                VkTexture vkTex = Util.AssertSubtype<Texture, VkTexture>(ca.Target);
                if ((vkTex.Usage & TextureUsage.Sampled) != 0)
                {
                    vkTex.TransitionImageLayout(
                        cb,
                        ca.MipLevel, 1,
                        ca.ArrayLayer, 1,
                        VkImageLayout.ShaderReadOnlyOptimal);
                }
            }
            if (DepthTarget != null)
            {
                VkTexture vkTex = Util.AssertSubtype<Texture, VkTexture>(DepthTarget.Value.Target);
                if ((vkTex.Usage & TextureUsage.Sampled) != 0)
                {
                    vkTex.TransitionImageLayout(
                        cb,
                        DepthTarget.Value.MipLevel, 1,
                        DepthTarget.Value.ArrayLayer, 1,
                        VkImageLayout.ShaderReadOnlyOptimal);
                }
            }
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

        protected override void DisposeCore()
        {
            if (!_destroyed)
            {
                vk.GetApi().DestroyFramebuffer(_gd.Device, _deviceFramebuffer, null);
                vk.GetApi().DestroyRenderPass(_gd.Device, _renderPassNoClear, null);
                vk.GetApi().DestroyRenderPass(_gd.Device, _renderPassNoClearLoad, null);
                vk.GetApi().DestroyRenderPass(_gd.Device, _renderPassClear, null);
                foreach (VkImageView view in _attachmentViews)
                {
                    vk.GetApi().DestroyImageView(_gd.Device, view, null);
                }

                _destroyed = true;
            }
        }
    }
}
