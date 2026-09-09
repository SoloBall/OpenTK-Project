using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;

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
        private int SceneTexture;
        private int FrameBufferHandle;
        private int quadVertexArrayHandle;
        private int shaderStorageBufferHandle;

        private IntPtr MappedPtr;
        
        private Camera? camera;
        private float deltaTime;
        private Vector2 lastMousePos;
        private bool isFirstMouse;

        private Random? rand;

        private List<Rectangle>? rectangles;
        private Rectangle? selectedRectangle;
        private int rectangleStructSize = Marshal.SizeOf<InstanceStructs.RectangleInstance>();

        public Game(int width = 1280, int height = 768, string title = "Base Window") : base(GameWindowSettings.Default,
            new NativeWindowSettings()
            {
                Title = title,
                ClientSize = new(width, height),
                StartVisible = false,
                StartFocused = true,
                API = ContextAPI.OpenGL,
                Profile = ContextProfile.Core,
                APIVersion = new(4, 5)
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
            Rectangle plane = new(50, 1, 50, camera.Position - Vector3.UnitZ * 5);
            plane.Mesh = CreateRectangleMesh(plane.Position, plane.Width, plane.Length, plane.Height, plane.Color);
            rectangles.Add(plane);
            GenerateBuffers();

            base.OnLoad();
        }
        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

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
            if ( keyboardInput.IsKeyDown(Keys.Space) )
            {
                proposedPosition += camera.Up * velocity;
            }
            if (proposedPosition != camera.Position)
            {
                UpdateSelectedRectanglePosition();
                CameraCollidesWithRectangle(proposedPosition, velocity);
            }

            if (keyboardInput.IsKeyDown(Keys.Escape))
            {
                CursorState = CursorState.Normal;
            }
            if ( keyboardInput.IsKeyPressed(Keys.E) )
            {
                CreateGrid();
            }
            if ( keyboardInput.IsKeyPressed(Keys.X) )
            {
                rectangles!.Clear();
            }
            if (MouseState.IsButtonPressed(MouseButton.Left))
            {
                int length = rand!.Next(1, 5);
                int height = rand.Next(1, 5);
                int width = rand.Next(1, 5);
                Color4 randomColor = new((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble(),1f);
                Rectangle rectangle = new(length, height, width, camera.Position + camera.Front * 3, randomColor);
                rectangle.Mesh = CreateRectangleMesh(rectangle.Position, width, length, height, rectangle.Color);
                rectangles!.Add(rectangle);
            }
            if ( MouseState.IsButtonPressed(MouseButton.Right) )
            {
                for ( int i = 0; i < rectangles!.Count; i++ )
                {
                    if ( rectangles[i] != selectedRectangle && camera.IsLookingAtRectangle(rectangles[i]) )
                    {
                        Console.WriteLine("cube find: " + rectangles[i].Color.ToString());
                        if (selectedRectangle != null )
                        {
                            if (Vector3.Distance(camera.Position, selectedRectangle.Position) > Vector3.Distance(camera.Position, rectangles[i].Position) )
                            {
                                foreach (Rectangle rectangle in rectangles )
                                {
                                    if (rectangle == selectedRectangle )
                                    {
                                        rectangle.Selected = false;
                                        rectangle.Dirty = true;
                                    }
                                }
                                rectangles[i].Selected = true;
                                rectangles[i].Dirty = true;
                                selectedRectangle = rectangles[i];
                            }
                        }
                        else
                        {
                            rectangles[i].Selected = true;
                            rectangles[i].Dirty = true;
                            selectedRectangle = rectangles[i];
                        }
                    }
                }
            }
            if ( selectedRectangle != null )
            {
                if ( keyboardInput.IsKeyPressed(Keys.Q) )
                {
                    foreach (Rectangle rectangle in rectangles! )
                    {
                        if (rectangle == selectedRectangle )
                        {
                            rectangle.Dirty = true;
                            rectangle.Selected = false;
                        }
                    }
                    selectedRectangle = null;
                    return;
                }
                UpdateSelectedRectanglePosition();
            }
            if ( camera.Position.X > 5000 && camera.Position.Y > 5000 )
            {
                Vector3 oldPosition = camera.Position;
                camera.Position = camera.Position - oldPosition;
                foreach (Rectangle rectangle in rectangles! )
                {
                    rectangle.Dirty = true;
                    rectangle.Position = rectangle.Position - oldPosition;
                }
            }
        }
        
        void UpdateSelectedRectanglePosition( )
        {
            if (selectedRectangle != null )
            {
                float distance = Vector3.Distance(camera!.Position, selectedRectangle!.Position);
                var scroll = MouseState.ScrollDelta.Y;
                distance += scroll * 20 * (1 - deltaTime * 10);
                distance = float.Clamp(distance, 1f, 40f);
                Vector3 newPosition = Vector3.Lerp(selectedRectangle.Position, camera.Position + camera.Front * distance, 0.01f + deltaTime * 2);

                selectedRectangle.Position = newPosition;
                selectedRectangle.Dirty = true;
            }
        }
        void CreateGrid(int size = 25, int spread = 5 )
        {
            for ( float x = 0; x < size; x += spread )
            {
                for ( float y = 0; y < size; y += spread )
                {
                    for ( float z = 0; z < size; z += spread )
                    {
                        Rectangle point = new(1, 1, 1, (camera!.Position.X + x, camera.Position.Y + y, camera.Position.Z + z), new(x / size, z / size, y / size, 1));
                        point.Mesh = CreateRectangleMesh(point.Position, point.Width, point.Length, point.Height, point.Color);
                        rectangles!.Add(point);
                    }
                }
            }
        }
        protected override void OnRenderFrame(FrameEventArgs args)
        {
            int indicesCount = UpdateRectangles();

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FrameBufferHandle);
            GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.UseProgram(ShaderProgramHandle);
            GL.BindVertexArray(VertexArrayHandle);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndexBufferHandle);

            Matrix4 view = camera!.GetMatrix();
            int viewLocation = GL.GetUniformLocation(ShaderProgramHandle, "view");
            GL.UniformMatrix4(viewLocation, false, ref view);

            int cameraPosLocation = GL.GetUniformLocation(ShaderProgramHandle, "cameraPos");
            GL.Uniform3(cameraPosLocation, ref camera.Position);

            GL.DrawElements(PrimitiveType.Triangles, indicesCount, DrawElementsType.UnsignedInt, 0);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);
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
            Vector3 outlineColor = new Vector3(0.2f, 0.2f, 0.2f);
            GL.Uniform3(outlineColorLocaiton, outlineColor);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, SceneTexture);
            int sceneTextureLocation = GL.GetUniformLocation(PostProgramHandle, "sceneTexture");
            GL.Uniform1(sceneTextureLocation, 0);

            int nearPlaneLocation = GL.GetUniformLocation(PostProgramHandle, "nearPlane");
            GL.Uniform1(nearPlaneLocation, camera.near);

            int farPlaneLocation = GL.GetUniformLocation(PostProgramHandle, "farPlane");
            GL.Uniform1(farPlaneLocation, camera.far);

            GL.BindVertexArray(quadVertexArrayHandle);
            GL.DrawArraysInstanced(PrimitiveType.Triangles, 0, 6, rectangles.Count);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthMask(true);

            this.Context.SwapBuffers();
            base.OnRenderFrame(args);
        }
        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            // loop through each rectangle, if it's in between camera.position and camera.position + camera.front * 20, select it, select only the closest
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
            List<VertexPositionColor> vertices = [];
            List<int> indices = [];
            int vertexOffset = 0;
            // convert to foreach mesh and add indices per iteration
            for ( int i = 0; i < rectangles!.Count; i++ )
            {
                if ( rectangles[i].Selected )
                {
                    rectangles[i] = selectedRectangle!;
                }

                vertices.AddRange(rectangles[i].Mesh.Vertices);

                foreach ( int index in rectangles[i].Mesh.Indices )
                {
                    indices.Add(index + vertexOffset);
                }
                vertexOffset += rectangles[i].Mesh.Vertices.Count();
            }

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

            VertexAttribute vertexPositionColorAttrib0 = VertexPositionColor.VertexInfo.Attributes[0]; // try and take this value, use it as location later on.
            VertexAttribute vertexPositionColorAttrib1 = VertexPositionColor.VertexInfo.Attributes[1];

            GL.VertexAttribPointer(vertexPositionColorAttrib0.Index, vertexPositionColorAttrib0.Count, VertexAttribPointerType.Float, false, 7 * sizeof(float), vertexPositionColorAttrib0.Offset);
            GL.VertexAttribPointer(vertexPositionColorAttrib1.Index, vertexPositionColorAttrib1.Count, VertexAttribPointerType.Float, false, 7 * sizeof(float), vertexPositionColorAttrib1.Offset);

            GL.EnableVertexAttribArray(vertexPositionColorAttrib0.Index);
            GL.EnableVertexAttribArray(vertexPositionColorAttrib1.Index);


            DepthTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, DepthTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent32, this.ClientSize.X, this.ClientSize.Y, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);

            FrameBufferHandle = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FrameBufferHandle);
            GL.DrawBuffer(DrawBufferMode.ColorAttachment0);
            GL.ReadBuffer(ReadBufferMode.ColorAttachment0);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, DepthTexture, 0);

            GL.CreateTextures(TextureTarget.Texture2D, 1, out SceneTexture);
            GL.BindTexture(TextureTarget.Texture2D, SceneTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, ClientSize.X, ClientSize.Y, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureCompareMode, (int)All.None);

            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, SceneTexture, 0);


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


            string vertexShaderSource = ShaderControls.GetVertexShaderSource();

            string fragmentShaderSource = ShaderControls.GetFragmentShaderSource();

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

            shaderStorageBufferHandle = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, shaderStorageBufferHandle);
            int maxInstances = 5000;

            GL.BufferStorage(BufferTarget.ShaderStorageBuffer,
                maxInstances * rectangleStructSize,
                IntPtr.Zero,
                BufferStorageFlags.MapWriteBit |
                BufferStorageFlags.MapPersistentBit |
                BufferStorageFlags.MapCoherentBit);
            
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 0, shaderStorageBufferHandle);

            MappedPtr = GL.MapBufferRange(
                BufferTarget.ShaderStorageBuffer,
                IntPtr.Zero,
                maxInstances * rectangleStructSize,
                BufferAccessMask.MapWriteBit |
                BufferAccessMask.MapPersistentBit |
                BufferAccessMask.MapCoherentBit);


            string screenVertexShaderSource = ShaderControls.GetScreenVertexShaderSource();

            string screenFragmentShaderSource = ShaderControls.GetScreenFragmentShaderSource();

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
        List<int> GetRectangtyufgjleIndices(int verticesCount)
        {
            int[] rectangleIndices = {
                0,1,2,  2,3,0,
                1,4,7,  7,2,1,
                4,5,6,  6,7,4,
                5,0,3,  3,6,5,
                1,0,5,  5,4,1,
                3,2,7,  3,7,6
            };

            List<int> indices = new();
            for ( int i = 0; i < verticesCount; i++ )
            {
                foreach ( int index in rectangleIndices )
                {
                    indices.Add(index + i * 8);
                }
            }
            return indices;
        }
        // instead of going through all rectangles, have either a grid based indexing system where each area or chunk has "dirty", where if something is dirty there, only undirty there.
        // like have a list of 1000 chunks where each chunk has a list of 1000 objects aswell as a "dirty" property. Then for each dirty chunk, find the dirty object
        // return an array of meshes. Then run drawelements in a loop for each mesh
        // rectangles are currently being converted to meshes. But all are still rectangles, even with mesh. This would change later

        // Updates all objects (including camera) and returns amount of indices
        int UpdateRectangles()
        {
            List<VertexPositionColor> vertices = [];
            List<int> indices = [];
            int vertexOffset = 0;
            // convert to foreach mesh and add indices per iteration
            for (int i = 0; i < rectangles!.Count; i++ )
            {
                if (rectangles[i].Selected)
                {
                    rectangles[i] = selectedRectangle!;
                }

                vertices.AddRange(rectangles[i].Mesh.Vertices);

                foreach (int index in rectangles[i].Mesh.Indices )
                {
                    indices.Add(index + vertexOffset);
                }
                vertexOffset += rectangles[i].Mesh.Vertices.Count();
            }

            // using vertexbuffer to send data to GPU
            int sizeInBytes = VertexPositionColor.VertexInfo.SizeInBytes;
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferHandle);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Count() * sizeInBytes, vertices.ToArray(), BufferUsageHint.DynamicDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndexBufferHandle);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Count() * sizeof(int), indices.ToArray(), BufferUsageHint.DynamicDraw);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferHandle);

            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90f), ClientSize.X / ClientSize.Y, camera!.near, camera.far);
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
            return indices.Count();
        }
        // maybe convert so it can be made from a rectangle
        public static Mesh CreateRectangleMesh(Vector3 position, float width, float length, float height, Color4 color)
        {
            Mesh mesh = new Mesh();
            List<VertexPositionColor> vertices = new();
            // face 1
            vertices.Add(new VertexPositionColor(
                new Vector3(width + position.X, position.Y, position.Z),
                color));

            vertices.Add(new VertexPositionColor(
                new Vector3(width + position.X, length + position.Y, position.Z),
                color));

            vertices.Add(new VertexPositionColor(
                new Vector3(width + position.X, length + position.Y, height + position.Z),
                color));

            vertices.Add(new VertexPositionColor(
                new Vector3(width + position.X, position.Y, height + position.Z),
                color));

            // face 2
            vertices.Add(new VertexPositionColor(
                new Vector3(position.X, length + position.Y, position.Z),
                color));

            vertices.Add(new VertexPositionColor(
                new Vector3(position.X, position.Y, position.Z),
                color));

            vertices.Add(new VertexPositionColor(
                new Vector3(position.X, position.Y, height + position.Z),
                color));

            vertices.Add(new VertexPositionColor(
                new Vector3(position.X, length + position.Y, height + position.Z),
                color));

            int[] indices = {
                0,1,2,  2,3,0,
                1,4,7,  7,2,1,
                4,5,6,  6,7,4,
                5,0,3,  3,6,5,
                1,0,5,  5,4,1,
                3,2,7,  3,7,6
            };

            mesh.Vertices = vertices.ToArray();
            mesh.Indices = indices;

            return mesh;
        }
        public void CameraCollidesWithRectangle(Vector3 proposedPosition, float velocity)
        {
            float radius = 1f;
            foreach (Rectangle rectangle in rectangles!) 
            {
                if (rectangle.Position == Vector3.Zero) continue; // check for whether it's close enough for optimization
                if (rectangle.CollidesWithSphere(proposedPosition, radius)) //WIP
                {
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
            GL.DeleteVertexArray(quadVertexArrayHandle);


            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.DeleteBuffer(VertexBufferHandle);
            GL.DeleteBuffer(IndexBufferHandle);
            GL.DeleteBuffer(FrameBufferHandle);

            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.DeleteTexture(SceneTexture);
            GL.DeleteTexture(DepthTexture);

            GL.UseProgram(0);
            GL.DeleteProgram(ShaderProgramHandle);
            GL.DeleteProgram(ShaderProgramHandle);

            base.OnUnload();
        }
    }
}
