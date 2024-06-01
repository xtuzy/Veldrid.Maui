//using Vulkan;
//using static Vulkan.VulkanNative;
using static Veldrid.Vk.VulkanUtil;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Veldrid.Vk
{
    internal unsafe class VkPipeline : Pipeline
    {
        private readonly VkGraphicsDevice _gd;
        private readonly VulkanVkPipeline _devicePipeline;
        private readonly VkPipelineLayout _pipelineLayout;
        private readonly VkRenderPass _renderPass;
        private bool _destroyed;
        private string _name;

        public VulkanVkPipeline DevicePipeline => _devicePipeline;

        public VkPipelineLayout PipelineLayout => _pipelineLayout;

        public uint ResourceSetCount { get; }
        public int DynamicOffsetsCount { get; }
        public bool ScissorTestEnabled { get; }

        public override bool IsComputePipeline { get; }

        public ResourceRefCount RefCount { get; }

        public override bool IsDisposed => _destroyed;

        public VkPipeline(VkGraphicsDevice gd, ref GraphicsPipelineDescription description)
            : base(ref description)
        {
            _gd = gd;
            IsComputePipeline = false;
            RefCount = new ResourceRefCount(DisposeCore);

            VkGraphicsPipelineCreateInfo pipelineCI = new VkGraphicsPipelineCreateInfo();

            // Blend State
            VkPipelineColorBlendStateCreateInfo blendStateCI = new VkPipelineColorBlendStateCreateInfo();
            int attachmentsCount = description.BlendState.AttachmentStates.Length;
            VkPipelineColorBlendAttachmentState* attachmentsPtr
                = stackalloc VkPipelineColorBlendAttachmentState[attachmentsCount];
            for (int i = 0; i < attachmentsCount; i++)
            {
                BlendAttachmentDescription vdDesc = description.BlendState.AttachmentStates[i];
                VkPipelineColorBlendAttachmentState attachmentState = new VkPipelineColorBlendAttachmentState();
                attachmentState.SrcColorBlendFactor = VkFormats.VdToVkBlendFactor(vdDesc.SourceColorFactor);
                attachmentState.DstColorBlendFactor = VkFormats.VdToVkBlendFactor(vdDesc.DestinationColorFactor);
                attachmentState.ColorBlendOp = VkFormats.VdToVkBlendOp(vdDesc.ColorFunction);
                attachmentState.SrcAlphaBlendFactor = VkFormats.VdToVkBlendFactor(vdDesc.SourceAlphaFactor);
                attachmentState.DstAlphaBlendFactor = VkFormats.VdToVkBlendFactor(vdDesc.DestinationAlphaFactor);
                attachmentState.AlphaBlendOp = VkFormats.VdToVkBlendOp(vdDesc.AlphaFunction);
                attachmentState.ColorWriteMask = VkFormats.VdToVkColorWriteMask(vdDesc.ColorWriteMask.GetOrDefault());
                attachmentState.BlendEnable = vdDesc.BlendEnabled;
                attachmentsPtr[i] = attachmentState;
            }

            blendStateCI.AttachmentCount = (uint)attachmentsCount;
            blendStateCI.PAttachments = attachmentsPtr;
            RgbaFloat blendFactor = description.BlendState.BlendFactor;
            blendStateCI.BlendConstants[0] = blendFactor.R;
            blendStateCI.BlendConstants[1] = blendFactor.G;
            blendStateCI.BlendConstants[2] = blendFactor.B;
            blendStateCI.BlendConstants[3] = blendFactor.A;

            pipelineCI.PColorBlendState = &blendStateCI;

            // Rasterizer State
            RasterizerStateDescription rsDesc = description.RasterizerState;
            VkPipelineRasterizationStateCreateInfo rsCI = new VkPipelineRasterizationStateCreateInfo();
            rsCI.CullMode = VkFormats.VdToVkCullMode(rsDesc.CullMode);
            rsCI.PolygonMode = VkFormats.VdToVkPolygonMode(rsDesc.FillMode);
            rsCI.DepthClampEnable = !rsDesc.DepthClipEnabled;
            rsCI.FrontFace = rsDesc.FrontFace == FrontFace.Clockwise ? VkFrontFace.Clockwise : VkFrontFace.CounterClockwise;
            rsCI.LineWidth = 1f;

            pipelineCI.PRasterizationState = &rsCI;

            ScissorTestEnabled = rsDesc.ScissorTestEnabled;

            // Dynamic State
            VkPipelineDynamicStateCreateInfo dynamicStateCI = new VkPipelineDynamicStateCreateInfo();
            VkDynamicState* dynamicStates = stackalloc VkDynamicState[2];
            dynamicStates[0] = VkDynamicState.Viewport;
            dynamicStates[1] = VkDynamicState.Scissor;
            dynamicStateCI.DynamicStateCount = 2;
            dynamicStateCI.PDynamicStates = dynamicStates;

            pipelineCI.PDynamicState = &dynamicStateCI;

            // Depth Stencil State
            DepthStencilStateDescription vdDssDesc = description.DepthStencilState;
            VkPipelineDepthStencilStateCreateInfo dssCI = new VkPipelineDepthStencilStateCreateInfo();
            dssCI.DepthWriteEnable = vdDssDesc.DepthWriteEnabled;
            dssCI.DepthTestEnable = vdDssDesc.DepthTestEnabled;
            dssCI.DepthCompareOp = VkFormats.VdToVkCompareOp(vdDssDesc.DepthComparison);
            dssCI.StencilTestEnable = vdDssDesc.StencilTestEnabled;

            dssCI.Front.FailOp = VkFormats.VdToVkStencilOp(vdDssDesc.StencilFront.Fail);
            dssCI.Front.PassOp = VkFormats.VdToVkStencilOp(vdDssDesc.StencilFront.Pass);
            dssCI.Front.DepthFailOp = VkFormats.VdToVkStencilOp(vdDssDesc.StencilFront.DepthFail);
            dssCI.Front.CompareOp = VkFormats.VdToVkCompareOp(vdDssDesc.StencilFront.Comparison);
            dssCI.Front.CompareMask = vdDssDesc.StencilReadMask;
            dssCI.Front.WriteMask = vdDssDesc.StencilWriteMask;
            dssCI.Front.Reference = vdDssDesc.StencilReference;

            dssCI.Back.FailOp = VkFormats.VdToVkStencilOp(vdDssDesc.StencilBack.Fail);
            dssCI.Back.PassOp = VkFormats.VdToVkStencilOp(vdDssDesc.StencilBack.Pass);
            dssCI.Back.DepthFailOp = VkFormats.VdToVkStencilOp(vdDssDesc.StencilBack.DepthFail);
            dssCI.Back.CompareOp = VkFormats.VdToVkCompareOp(vdDssDesc.StencilBack.Comparison);
            dssCI.Back.CompareMask = vdDssDesc.StencilReadMask;
            dssCI.Back.WriteMask = vdDssDesc.StencilWriteMask;
            dssCI.Back.Reference = vdDssDesc.StencilReference;

            pipelineCI.PDepthStencilState = &dssCI;

            // Multisample
            VkPipelineMultisampleStateCreateInfo multisampleCI = new VkPipelineMultisampleStateCreateInfo();
            VkSampleCountFlags vkSampleCount = VkFormats.VdToVkSampleCount(description.Outputs.SampleCount);
            multisampleCI.RasterizationSamples = vkSampleCount;
            multisampleCI.AlphaToCoverageEnable = description.BlendState.AlphaToCoverageEnabled;

            pipelineCI.PMultisampleState = &multisampleCI;

            // Input Assembly
            VkPipelineInputAssemblyStateCreateInfo inputAssemblyCI = new VkPipelineInputAssemblyStateCreateInfo();
            inputAssemblyCI.Topology = VkFormats.VdToVkPrimitiveTopology(description.PrimitiveTopology);

            pipelineCI.PInputAssemblyState = &inputAssemblyCI;

            // Vertex Input State
            VkPipelineVertexInputStateCreateInfo vertexInputCI = new VkPipelineVertexInputStateCreateInfo();

            VertexLayoutDescription[] inputDescriptions = description.ShaderSet.VertexLayouts;
            uint bindingCount = (uint)inputDescriptions.Length;
            uint attributeCount = 0;
            for (int i = 0; i < inputDescriptions.Length; i++)
            {
                attributeCount += (uint)inputDescriptions[i].Elements.Length;
            }
            VkVertexInputBindingDescription* bindingDescs = stackalloc VkVertexInputBindingDescription[(int)bindingCount];
            VkVertexInputAttributeDescription* attributeDescs = stackalloc VkVertexInputAttributeDescription[(int)attributeCount];

            int targetIndex = 0;
            int targetLocation = 0;
            for (int binding = 0; binding < inputDescriptions.Length; binding++)
            {
                VertexLayoutDescription inputDesc = inputDescriptions[binding];
                bindingDescs[binding] = new VkVertexInputBindingDescription()
                {
                    Binding = (uint)binding,
                    InputRate = (inputDesc.InstanceStepRate != 0) ? VkVertexInputRate.Instance : VkVertexInputRate.Vertex,
                    Stride = inputDesc.Stride
                };

                uint currentOffset = 0;
                for (int location = 0; location < inputDesc.Elements.Length; location++)
                {
                    VertexElementDescription inputElement = inputDesc.Elements[location];

                    attributeDescs[targetIndex] = new VkVertexInputAttributeDescription()
                    {
                        Format = VkFormats.VdToVkVertexElementFormat(inputElement.Format),
                        Binding = (uint)binding,
                        Location = (uint)(targetLocation + location),
                        Offset = inputElement.Offset != 0 ? inputElement.Offset : currentOffset
                    };

                    targetIndex += 1;
                    currentOffset += FormatSizeHelpers.GetSizeInBytes(inputElement.Format);
                }

                targetLocation += inputDesc.Elements.Length;
            }

            vertexInputCI.VertexBindingDescriptionCount = bindingCount;
            vertexInputCI.PVertexBindingDescriptions = bindingDescs;
            vertexInputCI.VertexAttributeDescriptionCount = attributeCount;
            vertexInputCI.PVertexAttributeDescriptions = attributeDescs;

            pipelineCI.PVertexInputState = &vertexInputCI;

            // Shader Stage

            VkSpecializationInfo specializationInfo;
            SpecializationConstant[] specDescs = description.ShaderSet.Specializations;
            if (specDescs != null)
            {
                uint specDataSize = 0;
                foreach (SpecializationConstant spec in specDescs)
                {
                    specDataSize += VkFormats.GetSpecializationConstantSize(spec.Type);
                }
                byte* fullSpecData = stackalloc byte[(int)specDataSize];
                int specializationCount = specDescs.Length;
                VkSpecializationMapEntry* mapEntries = stackalloc VkSpecializationMapEntry[specializationCount];
                uint specOffset = 0;
                for (int i = 0; i < specializationCount; i++)
                {
                    ulong data = specDescs[i].Data;
                    byte* srcData = (byte*)&data;
                    uint dataSize = VkFormats.GetSpecializationConstantSize(specDescs[i].Type);
                    Unsafe.CopyBlock(fullSpecData + specOffset, srcData, dataSize);
                    mapEntries[i].ConstantID = specDescs[i].ID;
                    mapEntries[i].Offset = specOffset;
                    mapEntries[i].Size = (UIntPtr)dataSize;
                    specOffset += dataSize;
                }
                specializationInfo.DataSize = (UIntPtr)specDataSize;
                specializationInfo.PData = fullSpecData;
                specializationInfo.MapEntryCount = (uint)specializationCount;
                specializationInfo.PMapEntries = mapEntries;
            }

            Shader[] shaders = description.ShaderSet.Shaders;
            StackList<VkPipelineShaderStageCreateInfo> stages = new StackList<VkPipelineShaderStageCreateInfo>();
            foreach (Shader shader in shaders)
            {
                VkShader vkShader = Util.AssertSubtype<Shader, VkShader>(shader);
                VkPipelineShaderStageCreateInfo stageCI = new VkPipelineShaderStageCreateInfo();
                stageCI.Module = vkShader.ShaderModule;
                stageCI.Stage = VkFormats.VdToVkShaderStages(shader.Stage);
                // stageCI.pName = CommonStrings.main; // Meh
                stageCI.PName = new FixedUtf8String(shader.EntryPoint); // TODO: DONT ALLOCATE HERE
                stageCI.PSpecializationInfo = &specializationInfo;
                stages.Add(stageCI);
            }

            pipelineCI.StageCount = stages.Count;
            pipelineCI.PStages = (VkPipelineShaderStageCreateInfo*)stages.Data;

            // ViewportState
            VkPipelineViewportStateCreateInfo viewportStateCI = new VkPipelineViewportStateCreateInfo();
            viewportStateCI.ViewportCount = 1;
            viewportStateCI.ScissorCount = 1;

            pipelineCI.PViewportState = &viewportStateCI;

            // Pipeline Layout
            ResourceLayout[] resourceLayouts = description.ResourceLayouts;
            VkPipelineLayoutCreateInfo pipelineLayoutCI = new VkPipelineLayoutCreateInfo();
            pipelineLayoutCI.SetLayoutCount = (uint)resourceLayouts.Length;
            VkDescriptorSetLayout* dsls = stackalloc VkDescriptorSetLayout[resourceLayouts.Length];
            for (int i = 0; i < resourceLayouts.Length; i++)
            {
                dsls[i] = Util.AssertSubtype<ResourceLayout, VkResourceLayout>(resourceLayouts[i]).DescriptorSetLayout;
            }
            pipelineLayoutCI.PSetLayouts = dsls;

            vk.GetApi().CreatePipelineLayout(_gd.Device, ref pipelineLayoutCI, null, out _pipelineLayout);
            pipelineCI.Layout = _pipelineLayout;

            // Create fake RenderPass for compatibility.

            VkRenderPassCreateInfo renderPassCI = new VkRenderPassCreateInfo();
            OutputDescription outputDesc = description.Outputs;
            StackList<VkAttachmentDescription, Size512Bytes> attachments = new StackList<VkAttachmentDescription, Size512Bytes>();

            // TODO: A huge portion of this next part is duplicated in VkFramebuffer.cs.

            StackList<VkAttachmentDescription> colorAttachmentDescs = new StackList<VkAttachmentDescription>();
            StackList<VkAttachmentReference> colorAttachmentRefs = new StackList<VkAttachmentReference>();
            for (uint i = 0; i < outputDesc.ColorAttachments.Length; i++)
            {
                colorAttachmentDescs[i].Format = VkFormats.VdToVkPixelFormat(outputDesc.ColorAttachments[i].Format);
                colorAttachmentDescs[i].Samples = vkSampleCount;
                colorAttachmentDescs[i].LoadOp = VkAttachmentLoadOp.DontCare;
                colorAttachmentDescs[i].StoreOp = VkAttachmentStoreOp.Store;
                colorAttachmentDescs[i].StencilLoadOp = VkAttachmentLoadOp.DontCare;
                colorAttachmentDescs[i].StencilStoreOp = VkAttachmentStoreOp.DontCare;
                colorAttachmentDescs[i].InitialLayout = VkImageLayout.Undefined;
                colorAttachmentDescs[i].FinalLayout = VkImageLayout.ShaderReadOnlyOptimal;
                attachments.Add(colorAttachmentDescs[i]);

                colorAttachmentRefs[i].Attachment = i;
                colorAttachmentRefs[i].Layout = VkImageLayout.ColorAttachmentOptimal;
            }

            VkAttachmentDescription depthAttachmentDesc = new VkAttachmentDescription();
            VkAttachmentReference depthAttachmentRef = new VkAttachmentReference();
            if (outputDesc.DepthAttachment != null)
            {
                PixelFormat depthFormat = outputDesc.DepthAttachment.Value.Format;
                bool hasStencil = FormatHelpers.IsStencilFormat(depthFormat);
                depthAttachmentDesc.Format = VkFormats.VdToVkPixelFormat(outputDesc.DepthAttachment.Value.Format, toDepthFormat: true);
                depthAttachmentDesc.Samples = vkSampleCount;
                depthAttachmentDesc.LoadOp = VkAttachmentLoadOp.DontCare;
                depthAttachmentDesc.StoreOp = VkAttachmentStoreOp.Store;
                depthAttachmentDesc.StencilLoadOp = VkAttachmentLoadOp.DontCare;
                depthAttachmentDesc.StencilStoreOp = hasStencil ? VkAttachmentStoreOp.Store : VkAttachmentStoreOp.DontCare;
                depthAttachmentDesc.InitialLayout = VkImageLayout.Undefined;
                depthAttachmentDesc.FinalLayout = VkImageLayout.DepthStencilAttachmentOptimal;

                depthAttachmentRef.Attachment = (uint)outputDesc.ColorAttachments.Length;
                depthAttachmentRef.Layout = VkImageLayout.DepthStencilAttachmentOptimal;
            }

            VkSubpassDescription subpass = new VkSubpassDescription();
            subpass.PipelineBindPoint = VkPipelineBindPoint.Graphics;
            subpass.ColorAttachmentCount = (uint)outputDesc.ColorAttachments.Length;
            subpass.PColorAttachments = (VkAttachmentReference*)colorAttachmentRefs.Data;
            for (int i = 0; i < colorAttachmentDescs.Count; i++)
            {
                attachments.Add(colorAttachmentDescs[i]);
            }

            if (outputDesc.DepthAttachment != null)
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

            VkResult creationResult = vk.GetApi().CreateRenderPass(_gd.Device, ref renderPassCI, null, out _renderPass);
            CheckResult(creationResult);

            pipelineCI.RenderPass = _renderPass;

            VkResult result = vk.GetApi().CreateGraphicsPipelines(_gd.Device, new VkPipelineCache(), 1, ref pipelineCI, null, out _devicePipeline);
            CheckResult(result);

            ResourceSetCount = (uint)description.ResourceLayouts.Length;
            DynamicOffsetsCount = 0;
            foreach (VkResourceLayout layout in description.ResourceLayouts)
            {
                DynamicOffsetsCount += layout.DynamicBufferCount;
            }
        }

        public VkPipeline(VkGraphicsDevice gd, ref ComputePipelineDescription description)
            : base(ref description)
        {
            _gd = gd;
            IsComputePipeline = true;
            RefCount = new ResourceRefCount(DisposeCore);

            VkComputePipelineCreateInfo pipelineCI = new VkComputePipelineCreateInfo();

            // Pipeline Layout
            ResourceLayout[] resourceLayouts = description.ResourceLayouts;
            VkPipelineLayoutCreateInfo pipelineLayoutCI = new VkPipelineLayoutCreateInfo();
            pipelineLayoutCI.SetLayoutCount = (uint)resourceLayouts.Length;
            VkDescriptorSetLayout* dsls = stackalloc VkDescriptorSetLayout[resourceLayouts.Length];
            for (int i = 0; i < resourceLayouts.Length; i++)
            {
                dsls[i] = Util.AssertSubtype<ResourceLayout, VkResourceLayout>(resourceLayouts[i]).DescriptorSetLayout;
            }
            pipelineLayoutCI.PSetLayouts = dsls;

            vk.GetApi().CreatePipelineLayout(_gd.Device, ref pipelineLayoutCI, null, out _pipelineLayout);
            pipelineCI.Layout = _pipelineLayout;

            // Shader Stage

            VkSpecializationInfo specializationInfo;
            SpecializationConstant[] specDescs = description.Specializations;
            if (specDescs != null)
            {
                uint specDataSize = 0;
                foreach (SpecializationConstant spec in specDescs)
                {
                    specDataSize += VkFormats.GetSpecializationConstantSize(spec.Type);
                }
                byte* fullSpecData = stackalloc byte[(int)specDataSize];
                int specializationCount = specDescs.Length;
                VkSpecializationMapEntry* mapEntries = stackalloc VkSpecializationMapEntry[specializationCount];
                uint specOffset = 0;
                for (int i = 0; i < specializationCount; i++)
                {
                    ulong data = specDescs[i].Data;
                    byte* srcData = (byte*)&data;
                    uint dataSize = VkFormats.GetSpecializationConstantSize(specDescs[i].Type);
                    Unsafe.CopyBlock(fullSpecData + specOffset, srcData, dataSize);
                    mapEntries[i].ConstantID = specDescs[i].ID;
                    mapEntries[i].Offset = specOffset;
                    mapEntries[i].Size = (UIntPtr)dataSize;
                    specOffset += dataSize;
                }
                specializationInfo.DataSize = (UIntPtr)specDataSize;
                specializationInfo.PData = fullSpecData;
                specializationInfo.MapEntryCount = (uint)specializationCount;
                specializationInfo.PMapEntries = mapEntries;
            }

            Shader shader = description.ComputeShader;
            VkShader vkShader = Util.AssertSubtype<Shader, VkShader>(shader);
            VkPipelineShaderStageCreateInfo stageCI = new VkPipelineShaderStageCreateInfo();
            stageCI.Module = vkShader.ShaderModule;
            stageCI.Stage = VkFormats.VdToVkShaderStages(shader.Stage);
            stageCI.PName = CommonStrings.main; // Meh
            stageCI.PSpecializationInfo = &specializationInfo;
            pipelineCI.Stage = stageCI;

            VkResult result = vk.GetApi().CreateComputePipelines(
                _gd.Device,
                VkNull.VkPipelineCacheNull,
                1,
                ref pipelineCI,
                null,
                out _devicePipeline);
            CheckResult(result);

            ResourceSetCount = (uint)description.ResourceLayouts.Length;
            DynamicOffsetsCount = 0;
            foreach (VkResourceLayout layout in description.ResourceLayouts)
            {
                DynamicOffsetsCount += layout.DynamicBufferCount;
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

        public override void Dispose()
        {
            RefCount.Decrement();
        }

        private void DisposeCore()
        {
            if (!_destroyed)
            {
                _destroyed = true;
                vk.GetApi().DestroyPipelineLayout(_gd.Device, _pipelineLayout, null);
                vk.GetApi().DestroyPipeline(_gd.Device, _devicePipeline, null);
                if (!IsComputePipeline)
                {
                    vk.GetApi().DestroyRenderPass(_gd.Device, _renderPass, null);
                }
            }
        }
    }
}
