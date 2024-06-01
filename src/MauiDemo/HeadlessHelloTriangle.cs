using SkiaSharp;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using Veldrid;

namespace MauiDemo
{
    public class HeadlessHelloTriangle: 
        IDisposable
    {
        public HeadlessHelloTriangle(int pixelW = 300, int pixelH = 300)
        {
            PixelWidth = pixelW; PixelHeight = pixelH;
            var isSupported = GraphicsDevice.IsBackendSupported(GraphicsBackend.Vulkan);
            var options = new GraphicsDeviceOptions(true, null, false, ResourceBindingModel.Improved, true, true);

            var graphicsDevice = GraphicsDevice.CreateVulkan(options);
            GraphicsDevice = graphicsDevice;
            ResourceFactory = GraphicsDevice.ResourceFactory;
        }

        #region  Add for headless
        GraphicsDevice GraphicsDevice;
        ResourceFactory ResourceFactory;
        int PixelWidth;
        int PixelHeight;
        Texture _offscreenReadOut;
        Texture _offscreenColor;
        Framebuffer _offscreenFB;
        #endregion

        private DeviceBuffer _vertexBuffer;
        private Pipeline _pipeline;
        private CommandList _commandList;
        private Shader[] _shaders;
        private DeviceBuffer _indexBuffer;
        ushort[] triangleIndices;
        public unsafe void CreateResources()
        {
            ResourceFactory factory = ResourceFactory;
            //vertices data of a triangle
            Vector3[] vertices = new Vector3[]
            {
                new Vector3( -0.5f, -0.5f, 0.0f),
                new Vector3(0.5f, -0.5f, 0.0f),
                new Vector3(0.0f, 0.5f, 0.0f),
            };

            // create Buffer for vertices data
            BufferDescription vbDescription = new BufferDescription(
                (uint)(vertices.Length * sizeof(Vector3)),
                BufferUsage.VertexBuffer);
            _vertexBuffer = factory.CreateBuffer(vbDescription);
            GraphicsDevice.UpdateBuffer(_vertexBuffer, 0, vertices);// update data to Buffer

            // Index data
            triangleIndices = new ushort[] { 0, 1, 2 };// CounterClockwise
            // create IndexBuffer
            BufferDescription ibDescription = new BufferDescription(
                (uint)(triangleIndices.Length * sizeof(ushort)),
                BufferUsage.IndexBuffer);
            _indexBuffer = factory.CreateBuffer(ibDescription);
            GraphicsDevice.UpdateBuffer(_indexBuffer, 0, triangleIndices);// update data to Buffer

            string vertexCode = @"
#version 460

layout(location = 0) in vec3 aPos;

void main()
{
    gl_Position = vec4(aPos.x, aPos.y, aPos.z, 1.0);
}";

            string fragmentCode = @"
#version 460

layout(location = 0) out vec4 FragColor;

void main()
{
    FragColor = vec4(1.0f, 0.5f, 0.2f, 1.0f);
}";

            (byte[] vertexBytes, byte[] fragmentBytes) = HelloTriangle.GetShaderBytes(factory.BackendType);
            string entryPoint = factory.BackendType == GraphicsBackend.Metal ? "main0" : "main";
            var vertexShaderDesc = new ShaderDescription(ShaderStages.Vertex, vertexBytes, entryPoint);
            var fragmentShaderDesc = new ShaderDescription(ShaderStages.Fragment, fragmentBytes, entryPoint);

            var vertexShader = factory.CreateShader(vertexShaderDesc);
            var fragmentShader = factory.CreateShader(fragmentShaderDesc);
            _shaders = new Shader[] { vertexShader, fragmentShader };

            // VertexLayout tell Veldrid we store what in Vertex Buffer, it need match with vertex.glsl
            VertexLayoutDescription vertexLayout = new VertexLayoutDescription(
               new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3));

            #region Add for Headless 
            _offscreenReadOut = factory.CreateTexture(TextureDescription.Texture2D((uint)PixelWidth, (uint)PixelHeight, 1, 1, PixelFormat.R32_G32_B32_A32_Float, TextureUsage.Staging));
            _offscreenColor = factory.CreateTexture(TextureDescription.Texture2D((uint)PixelWidth, (uint)PixelHeight, 1, 1, PixelFormat.R32_G32_B32_A32_Float, TextureUsage.RenderTarget));
            _offscreenFB = factory.CreateFramebuffer(new FramebufferDescription(null, _offscreenColor));
            #endregion

            // create GraphicsPipeline
            GraphicsPipelineDescription pipelineDescription = new GraphicsPipelineDescription();
            pipelineDescription.BlendState = BlendStateDescription.SingleOverrideBlend;
            pipelineDescription.DepthStencilState = DepthStencilStateDescription.Disabled;
            pipelineDescription.RasterizerState = new RasterizerStateDescription(
                cullMode: FaceCullMode.Back,
                fillMode: PolygonFillMode.Solid,//draw outline or fill
                frontFace: FrontFace.CounterClockwise,//order of drawing point, see Indices array.
                depthClipEnabled: true,
                scissorTestEnabled: false);
            pipelineDescription.PrimitiveTopology = PrimitiveTopology.TriangleStrip;//basis graphics is point,line,or triangle
            pipelineDescription.ResourceLayouts = System.Array.Empty<ResourceLayout>();
            pipelineDescription.ShaderSet = new ShaderSetDescription(
                vertexLayouts: new VertexLayoutDescription[] { vertexLayout },
                shaders: _shaders);
            pipelineDescription.Outputs = _offscreenFB.OutputDescription;

            _pipeline = factory.CreateGraphicsPipeline(pipelineDescription);
            // create CommandList
            _commandList = factory.CreateCommandList();
        }

        public byte[] OnRender()
        {
            // Begin() must be called before commands can be issued.
            _commandList.Begin();

            _commandList.SetFramebuffer(_offscreenFB);
            _commandList.ClearColorTarget(0, RgbaFloat.Black);

            _commandList.SetVertexBuffer(0, _vertexBuffer);
            _commandList.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);
            _commandList.SetPipeline(_pipeline);
            _commandList.DrawIndexed(
                indexCount: (uint)triangleIndices.Length,
                instanceCount: 1,
                indexStart: 0,
                vertexOffset: 0,
                instanceStart: 0);

            #region Add for Headless
            //transfer GPU drawing to CPU readable one
            _commandList.CopyTexture(_offscreenColor, _offscreenReadOut);
            #endregion

            // End() must be called before commands can be submitted for execution.
            _commandList.End();
            GraphicsDevice?.SubmitCommands(_commandList);
            // Once commands have been submitted, the rendered image can be presented to the application window.
            //GraphicsDevice?.SwapBuffers(MainSwapchain);
            GraphicsDevice?.WaitForIdle();
            
            #region Add for Headless
            MappedResourceView<byte> view = GraphicsDevice.Map<byte>(_offscreenReadOut, MapMode.Read);
            byte[] tmp = new byte[view.SizeInBytes];
            Marshal.Copy(view.MappedResource.Data, tmp, 0, (int)view.SizeInBytes);
            GraphicsDevice.Unmap(_offscreenReadOut);
            #endregion

            return tmp;
        }

        #region Add for Headless
        public SKBitmap SaveRgba32ToSKBitmap(byte[] bytes)
        {
            var flipVertical = GraphicsDevice.BackendType == GraphicsBackend.Vulkan;
            using SKImage img = SKImage.FromPixelCopy(new SKImageInfo(bytes.Length / 16 / PixelHeight, PixelHeight, SKColorType.RgbaF32), bytes);
            SKBitmap bmp = new SKBitmap(img.Width, img.Height);
            using SKCanvas surface = new SKCanvas(bmp);
            surface.Scale(1, flipVertical ? -1 : 1, 0, flipVertical ? PixelHeight / 2f : 0);
            surface.DrawImage(img, 0, 0);
            return bmp;
        }
        #endregion

        public void Dispose()
        {
            GraphicsDevice = null;

            _indexBuffer?.Dispose();
            _vertexBuffer?.Dispose();
            _pipeline?.Dispose();
            _commandList?.Dispose();
            foreach (var shader in _shaders)
                shader?.Dispose();
        }
    }
}
