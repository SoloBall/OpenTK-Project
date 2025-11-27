using OpenTK;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace OpenTK_Project
{
    public class Game : GameWindow
    {
        private int VertexBufferHandle;
        private int ShaderProgramHandle;
        private int PostProgramHandle;
        private int VertexArrayHandle;
        private int IndexBufferHandle;
        private int DepthTexture;
        private int FrameBufferHandle;
        private int quadVertexArrayHandle;

        private Camera? camera;
        private float deltaTime;
        private Vector2 lastMousePos;
        private bool isFirstMouse;
        private int indicesCount;

        private Random? rand;

        private List<Rectangle>? rectangles;
        public Game(int width = 1280, int height = 768, string title = "Base Window") : base(GameWindowSettings.Default,
            new NativeWindowSettings()
            {
                Title = title,
                ClientSize = new(width, height),
                StartVisible = false,
                StartFocused = true,
                API = ContextAPI.OpenGL,
                Profile = ContextProfile.Core,
                APIVersion = new(3, 3)
            })
        {
            this.CenterWindow();
        }
        protected override void OnLoad()
        {
            rand = new Random();
            IsVisible = true;
            isFirstMouse = true;
            GL.ClearColor(new Color4(0.3f, 0.3f, 0.3f, 1f));

            rectangles = new();

            camera = new();
            CursorState = CursorState.Grabbed;

            GL.Enable(EnableCap.DepthTest);

            Rectangle glitchyPlane = new(50, 50, 1);
            for (int i = 0; i < 200; i++)
            {
                rectangles.Add(glitchyPlane);
            }
            Rectangle plane = new(50, 1, 50, camera.Position - Vector3.UnitY * 2);
            rectangles.Add(plane);
            GenerateBuffers();

            base.OnLoad();
        }
        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            UpdateRectangles();
            deltaTime = (float)args.Time;
            float velocity = camera!.speed * deltaTime;

            var keyboardInput = KeyboardState;
            var mouseInput = MouseState;
            Vector3 proposedPosition = camera.Position;
            if (keyboardInput.IsKeyDown(Keys.W))
            {
                proposedPosition += camera.Front * velocity;
            }
            if (keyboardInput.IsKeyDown(Keys.A))
            {
                proposedPosition -= camera.Right * velocity;
            }
            if (keyboardInput.IsKeyDown(Keys.S))
            {
                proposedPosition -= camera.Front * velocity;
            }
            if (keyboardInput.IsKeyDown(Keys.D))
            {
                proposedPosition += camera.Right * velocity;
            }
            if (proposedPosition != camera.Position)
            {
                CameraCollidesWithRectangle(proposedPosition, velocity);
            }

            if (keyboardInput.IsKeyDown(Keys.Escape))
            {
                CursorState = CursorState.Normal;
            }

            if (MouseState.IsButtonPressed(MouseButton.Left))
            {
                int length = rand!.Next(1, 5);
                int width = rand.Next(1, 5);
                int height = rand.Next(1, 5);
                Color4 randomColor = new((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble(),1f);
                Rectangle rectangle = new(length, width, height, camera.Position + camera.Front * 3, randomColor);
                rectangles!.Add(rectangle);
            }
        }
        
        protected override void OnRenderFrame(FrameEventArgs args)
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FrameBufferHandle);
            GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.UseProgram(ShaderProgramHandle);
            GL.BindVertexArray(VertexArrayHandle);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndexBufferHandle);

            Matrix4 view = camera!.GetMatrix();
            int viewLocation = GL.GetUniformLocation(ShaderProgramHandle, "view");
            GL.UniformMatrix4(viewLocation, false, ref view);

            GL.DrawElements(PrimitiveType.Triangles, indicesCount, DrawElementsType.UnsignedInt, 0);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.Disable(EnableCap.DepthTest);
            GL.DepthMask(false);
            GL.UseProgram(PostProgramHandle);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, DepthTexture);

            int depthTextureLocation = GL.GetUniformLocation(PostProgramHandle, "depthTexture");
            GL.Uniform1(depthTextureLocation, 0);

            Vector2 screenSize = new Vector2(ClientSize.X, ClientSize.Y);
            int screenSizeLocation = GL.GetUniformLocation(PostProgramHandle, "screenSize");
            GL.Uniform2(screenSizeLocation, ref screenSize);

            int outlineColorLocaiton = GL.GetUniformLocation(PostProgramHandle, "outlineColor");
            GL.Uniform3(outlineColorLocaiton, (1, 1, 1)); // literally does nothing

            GL.BindVertexArray(quadVertexArrayHandle);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthMask(true);
            this.Context.SwapBuffers();
            base.OnRenderFrame(args);
        }
        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            base.OnMouseMove(e);

            if (isFirstMouse)
            {
                lastMousePos = e.Position;
                isFirstMouse = false;
            }

            Vector2 deltaMousePos = e.Position - lastMousePos;
            lastMousePos = e.Position;

            camera!.Pitch -= deltaMousePos.Y * camera.sensitivity;
            camera.Yaw -= deltaMousePos.X * camera.sensitivity;
            camera.Pitch = Math.Clamp(camera.Pitch, -89f, 89f);

            camera.UpdateDirection();
        }
        protected override void OnResize(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, e.Width, e.Height);
            base.OnResize(e);
        }

        void GenerateBuffers( )
        {
            List<VertexPositionColor> vertices = new();
            for ( int i = 0; i < rectangles!.Count(); i++ )
            {
                Rectangle rectangle = rectangles![i];

                // face 1
                vertices.Add(
                new VertexPositionColor(
                    new Vector3(rectangle.Length + rectangle.Position.X, rectangle.Position.Z, rectangle.Position.Y),
                    rectangle.Color));

                vertices.Add(
                new VertexPositionColor(
                    new Vector3(rectangle.Length + rectangle.Position.X, rectangle.Width + rectangle.Position.Z, rectangle.Position.Y),
                    rectangle.Color));

                vertices.Add(
                new VertexPositionColor(
                    new Vector3(rectangle.Length + rectangle.Position.X, rectangle.Width + rectangle.Position.Z, rectangle.Height + rectangle.Position.Y),
                    rectangle.Color));

                vertices.Add(
                new VertexPositionColor(
                    new Vector3(rectangle.Length + rectangle.Position.X, rectangle.Position.Z, rectangle.Height + rectangle.Position.Y),
                    rectangle.Color));

                // face 2
                vertices.Add(
                new VertexPositionColor(
                    new Vector3(rectangle.Position.X, rectangle.Width + rectangle.Position.Z, rectangle.Position.Y),
                    rectangle.Color));

                vertices.Add(
                new VertexPositionColor(
                    new Vector3(rectangle.Position.X, rectangle.Position.Z, rectangle.Position.Y),
                    rectangle.Color));

                vertices.Add(
                new VertexPositionColor(
                    new Vector3(rectangle.Position.X, rectangle.Position.Z, rectangle.Height + rectangle.Position.Y),
                    rectangle.Color));

                vertices.Add(
                new VertexPositionColor(
                    new Vector3(rectangle.Position.X, rectangle.Width + rectangle.Position.Z, rectangle.Height + rectangle.Position.Y),
                    rectangle.Color));
            }


            int[] rectangleIndices = {
                0,1,2,  2,3,0,
                1,4,7,  7,2,1,
                4,5,6,  6,7,4,
                5,0,3,  3,6,5,
                1,0,5,  5,4,1,
                3,2,7,  3,7,6
            };

            List<int> indices = new();
            for ( int i = 0; i < vertices.Count(); i++ )
            {
                foreach ( int index in rectangleIndices )
                {
                    indices.Add(index + i * 8);
                }
            }

            indicesCount = indices.Count();
            // using vertexbuffer to send data to GPU
            int sizeInBytes = VertexPositionColor.VertexInfo.SizeInBytes;

            if ( VertexBufferHandle != 0 )
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                GL.BindVertexArray(0);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
                GL.DeleteBuffer(VertexBufferHandle);
                GL.DeleteBuffer(IndexBufferHandle);
                GL.DeleteVertexArray(VertexArrayHandle);
                VertexBufferHandle = IndexBufferHandle = VertexArrayHandle = 0;
            }
            VertexBufferHandle = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferHandle);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Count() * sizeInBytes, vertices.ToArray(), BufferUsageHint.DynamicDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            IndexBufferHandle = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndexBufferHandle);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Count() * sizeof(int), indices.ToArray(), BufferUsageHint.DynamicDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            VertexArrayHandle = GL.GenVertexArray();
            GL.BindVertexArray(VertexArrayHandle);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferHandle);

            VertexAttribute vertexPositionColorAttrib0 = VertexPositionColor.VertexInfo.Attributes[0];
            VertexAttribute vertexPositionColorAttrib1 = VertexPositionColor.VertexInfo.Attributes[1];

            GL.VertexAttribPointer(vertexPositionColorAttrib0.Index, vertexPositionColorAttrib0.Count, VertexAttribPointerType.Float, false, 7 * sizeof(float), vertexPositionColorAttrib0.Offset);
            GL.VertexAttribPointer(vertexPositionColorAttrib1.Index, vertexPositionColorAttrib1.Count, VertexAttribPointerType.Float, false, 7 * sizeof(float), vertexPositionColorAttrib1.Offset);

            GL.EnableVertexAttribArray(vertexPositionColorAttrib0.Index);
            GL.EnableVertexAttribArray(vertexPositionColorAttrib1.Index);


            DepthTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, DepthTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent32, this.ClientSize.X, this.ClientSize.Y, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Nearest);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureCompareMode, (int)All.None);



            FrameBufferHandle = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FrameBufferHandle);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, DepthTexture, 0);

            //optional, until i add colors to the shit
            GL.DrawBuffer(DrawBufferMode.None);
            GL.ReadBuffer(ReadBufferMode.None);

            var status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (status != FramebufferErrorCode.FramebufferComplete )
            {
                Console.WriteLine($"FBO not complete: {status}");
            }
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);


            float[] quadVerts = {
                // positions (clip space)
                -1f, -1f,
                 1f, -1f,
                 1f,  1f,

                -1f, -1f,
                 1f,  1f,
                -1f,  1f    
            };

            quadVertexArrayHandle = GL.GenVertexArray();
            int quadVertexBufferHandle = GL.GenBuffer();

            GL.BindVertexArray(quadVertexArrayHandle);
            GL.BindBuffer(BufferTarget.ArrayBuffer, quadVertexBufferHandle);
            GL.BufferData(BufferTarget.ArrayBuffer, quadVerts.Length * sizeof(float), quadVerts, BufferUsageHint.StaticDraw);

            // vPosition is location = 0 in the post-process vertex shader
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.BindVertexArray(0);


            //defining the code for shaders -- what they do
            string vertexShaderSource =
            @"
            #version 330 core

            layout (location = 0) in vec3 vPosition;
            layout (location = 1) in vec4 vColor;

            uniform mat4 projection; 
            uniform mat4 view;
            uniform mat4 model;

            out vec4 fColor;
            out vec3 fPos;

            void main() {
                fColor = vColor;
                gl_Position = projection * view * model * vec4(vPosition, 1.0);
                fPos = vPosition;
            }
            ";

            string fragmentShaderSource =
            @"
            #version 330 core

            in vec4 fColor;
            in vec3 fPos;
            uniform vec3 cameraPos;
            out vec4 color;

            void main(){
                float lightRadius = 50;
                float darkFactor = clamp(distance(fPos, cameraPos) / lightRadius, 0.0, 1.0);
                color = fColor * (1 - darkFactor);
            }
            ";

            int vertexShaderHandle = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShaderHandle, vertexShaderSource);
            GL.CompileShader(vertexShaderHandle);

            string vertexShaderInfo = GL.GetShaderInfoLog(vertexShaderHandle);
            if ( vertexShaderInfo != string.Empty )
            {
                Console.WriteLine("Error during compilation of vertex shader: " + vertexShaderInfo);
            }

            int fragmentShaderHandle = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShaderHandle, fragmentShaderSource);
            GL.CompileShader(fragmentShaderHandle);

            string fragmentShaderInfo = GL.GetShaderInfoLog(fragmentShaderHandle);
            if ( fragmentShaderInfo != string.Empty )
            {
                Console.WriteLine("Error during compilation of fragment shader: " + fragmentShaderInfo);
            }

            if ( ShaderProgramHandle != 0 )
            {
                GL.DeleteProgram(ShaderProgramHandle);
            }
            ShaderProgramHandle = GL.CreateProgram();

            GL.AttachShader(ShaderProgramHandle, vertexShaderHandle);
            GL.AttachShader(ShaderProgramHandle, fragmentShaderHandle);

            GL.LinkProgram(ShaderProgramHandle);

            GL.DetachShader(ShaderProgramHandle, vertexShaderHandle);
            GL.DetachShader(ShaderProgramHandle, fragmentShaderHandle);

            GL.DeleteShader(vertexShaderHandle);
            GL.DeleteShader(fragmentShaderHandle);

            GL.UseProgram(ShaderProgramHandle);

            string screenVertexShaderSource =
            @"
            #version 330 core

            layout (location = 0) in vec2 vPosition;
            out vec2 uv;

            void main() {
                uv = vPosition * 0.5 + 0.5;
                gl_Position = vec4(vPosition, 0.0, 1.0);
            }
            ";

            string screenFragmentShaderSource =
            @"
            #version 330 core

            uniform sampler2D depthTexture;
            uniform vec2 screenSize;
            uniform vec3 outlineColor;
            in vec2 uv;
            out vec4 fragColor;

            void main(){

                float center = texture(depthTexture, uv).r;
                vec2 pixel = 1.0 / screenSize;

                float right = texture(depthTexture, uv + vec2(pixel.x, 0)).r;
                float left = texture(depthTexture, uv + vec2(-pixel.x, 0)).r;
                float up = texture(depthTexture, uv + vec2(0, pixel.y)).r;
                float down = texture(depthTexture, uv + vec2(0, -pixel.y)).r;

                float difference =
                    abs(center - right) + 
                    abs(center - left) + 
                    abs(center - up) + 
                    abs(center - down); 

                float outline = difference  > 0.0005 ? 1.0 : 0.0;
                
                vec3 base = vec3(0.2, 0.2, 0.2);
                vec3 final = mix(base, outlineColor, outline);
                fragColor = vec4(final, 1.0);
            }
            ";

            int screenVertexShaderHandle = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(screenVertexShaderHandle, screenVertexShaderSource);
            GL.CompileShader(screenVertexShaderHandle);

            string screenVertexShaderInfo = GL.GetShaderInfoLog(screenVertexShaderHandle);
            if ( screenVertexShaderInfo != string.Empty )
            {
                Console.WriteLine("Error during compilation of screen vertex shader: " + screenVertexShaderInfo);
            }

            int screenFragmentShaderHandle = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(screenFragmentShaderHandle, screenFragmentShaderSource);
            GL.CompileShader(screenFragmentShaderHandle);

            string screenFragmentShaderInfo = GL.GetShaderInfoLog(screenFragmentShaderHandle);
            if ( screenFragmentShaderInfo != string.Empty )
            {
                Console.WriteLine("Error during compilation of screen fragment shader: " + screenFragmentShaderInfo);
            }

            if ( PostProgramHandle != 0 )
            {
                GL.DeleteProgram(PostProgramHandle);
            }
            PostProgramHandle = GL.CreateProgram();

            GL.AttachShader(PostProgramHandle, screenVertexShaderHandle);
            GL.AttachShader(PostProgramHandle, screenFragmentShaderHandle);

            GL.LinkProgram(PostProgramHandle);

            GL.DetachShader(PostProgramHandle, screenVertexShaderHandle);
            GL.DetachShader(PostProgramHandle, screenFragmentShaderHandle);

            GL.DeleteShader(screenVertexShaderHandle);
            GL.DeleteShader(screenFragmentShaderHandle);
        }
        void UpdateRectangles()
        {
            List<VertexPositionColor> vertices = new();
            for ( int i = 0; i < rectangles!.Count(); i++ )
            {
                Rectangle rectangle = rectangles![i];

                // face 1
                vertices.Add(
                new VertexPositionColor(
                    new Vector3(rectangle.Length + rectangle.Position.X, rectangle.Position.Z, rectangle.Position.Y),
                    rectangle.Color));

                vertices.Add(
                new VertexPositionColor(
                    new Vector3(rectangle.Length + rectangle.Position.X, rectangle.Width + rectangle.Position.Z, rectangle.Position.Y),
                    rectangle.Color));

                vertices.Add(
                new VertexPositionColor(
                    new Vector3(rectangle.Length + rectangle.Position.X, rectangle.Width + rectangle.Position.Z, rectangle.Height + rectangle.Position.Y),
                    rectangle.Color));

                vertices.Add(
                new VertexPositionColor(
                    new Vector3(rectangle.Length + rectangle.Position.X, rectangle.Position.Z, rectangle.Height + rectangle.Position.Y),
                    rectangle.Color));

                // face 2
                vertices.Add(
                new VertexPositionColor(
                    new Vector3(rectangle.Position.X, rectangle.Width + rectangle.Position.Z, rectangle.Position.Y),
                    rectangle.Color));

                vertices.Add(
                new VertexPositionColor(
                    new Vector3(rectangle.Position.X, rectangle.Position.Z, rectangle.Position.Y),
                    rectangle.Color));

                vertices.Add(
                new VertexPositionColor(
                    new Vector3(rectangle.Position.X, rectangle.Position.Z, rectangle.Height + rectangle.Position.Y),
                    rectangle.Color));

                vertices.Add(
                new VertexPositionColor(
                    new Vector3(rectangle.Position.X, rectangle.Width + rectangle.Position.Z, rectangle.Height + rectangle.Position.Y),
                    rectangle.Color));
            }

            int[] rectangleIndices = {
                0,1,2,  2,3,0,
                1,4,7,  7,2,1,
                4,5,6,  6,7,4,
                5,0,3,  3,6,5,
                1,0,5,  5,4,1,
                3,2,7,  3,7,6
            };

            List<int> indices = new();
            for ( int i = 0; i < vertices.Count(); i++ )
            {
                foreach ( int index in rectangleIndices )
                {
                    indices.Add(index + i * 8);
                }
            }

            indicesCount = indices.Count();
            // using vertexbuffer to send data to GPU
            int sizeInBytes = VertexPositionColor.VertexInfo.SizeInBytes;
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferHandle);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Count() * sizeInBytes, vertices.ToArray(), BufferUsageHint.DynamicDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndexBufferHandle);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Count() * sizeof(int), indices.ToArray(), BufferUsageHint.DynamicDraw);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferHandle);

            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90f), Size.X / Size.Y, 0.1f, 100f);
            Matrix4 view = camera!.GetMatrix();
            Matrix4 model = Matrix4.Identity;

            int cameraPosLocation = GL.GetUniformLocation(ShaderProgramHandle, "cameraPos");

            int projectionLocation = GL.GetUniformLocation(ShaderProgramHandle, "projection");
            int viewLocation = GL.GetUniformLocation(ShaderProgramHandle, "view");
            int modelLocation = GL.GetUniformLocation(ShaderProgramHandle, "model");

            GL.UniformMatrix4(projectionLocation, false, ref projection);
            GL.UniformMatrix4(viewLocation, false, ref view);
            GL.UniformMatrix4(modelLocation, false, ref model);

            GL.Uniform3(cameraPosLocation, camera.Position);
        }
        public void CameraCollidesWithRectangle(Vector3 proposedPosition, float velocity)
        {
            float radius = 1f;
            foreach (Rectangle rectangle in rectangles!) 
            {
                if (rectangle.Position == Vector3.Zero) continue; // check for whether it's close enough for optimization
                if (rectangle.CollidesWithSphere(proposedPosition, radius)) //WIP
                {
                    Console.WriteLine("collides");
                    Vector3 min = rectangle.Position;
                    Vector3 max = rectangle.Position + new Vector3(rectangle.Length, rectangle.Width, rectangle.Height);
                    float distToLeft = Math.Abs(camera!.Position.X - min.X);
                    float distToRight = Math.Abs(camera.Position.X - max.X);
                    float distToBack = Math.Abs(camera.Position.Z - min.Z);
                    float distToFront = Math.Abs(camera.Position.Z - max.Z);
                    float distToTop = Math.Abs(camera.Position.Y - max.Y);
                    float distToBottom = Math.Abs(camera.Position.Y - min.Y);

                    Vector3 rectangleNormal = new(-1, 0, 0);
                    float minDist = distToLeft;
                    if (distToRight < minDist)
                    {
                        minDist = distToRight;
                        rectangleNormal = new(1, 0, 0);
                    }
                    if (distToBack < minDist)
                    {
                        minDist = distToBack;
                        rectangleNormal = new(0, 0, -1);
                    }
                    if (distToFront < minDist)
                    {
                        minDist = distToFront;
                        rectangleNormal = new(0, 0, 1);
                    }
                    if (distToTop < minDist)
                    {
                        minDist = distToTop;
                        rectangleNormal = new(0, 1, 0);
                    }
                    if (distToBottom < minDist)
                    {
                        rectangleNormal = new(0, -1, 0);
                    }
                    Vector3 moveDir = velocity * Vector3.Normalize(proposedPosition);
                    Vector3 slideDir = moveDir - Vector3.Dot(moveDir, rectangleNormal) * rectangleNormal;

                    proposedPosition = camera.Position + slideDir;
                    break;
                }
            }
            camera!.Position = proposedPosition;
        }

        protected override void OnUnload() // garbage collection
        {
            GL.BindVertexArray(0);
            GL.DeleteVertexArray(VertexArrayHandle);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.DeleteBuffer(VertexBufferHandle);
            GL.DeleteBuffer(IndexBufferHandle);

            GL.UseProgram(0);

            GL.DeleteProgram(ShaderProgramHandle);

            base.OnUnload();
        }
    }
}
