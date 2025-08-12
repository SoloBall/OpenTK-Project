using OpenTK;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Runtime.CompilerServices;

namespace OpenTK_Project
{
    public class Game : GameWindow
    {
        private int VertexBufferHandle;
        private int ShaderProgramHandle;
        private int VertexArrayHandle;
        private int IndexBufferHandle;

        private Camera camera;
        private float deltaTime;
        private Vector2 lastMousePos;
        private bool isFirstMouse;
        private int indicesCount;
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
            IsVisible = true;
            isFirstMouse = true;
            GL.ClearColor(new Color4(0.3f, 0.3f, 0.3f, 1f));

            camera = new();
            CursorState = CursorState.Grabbed;

            base.OnLoad();
        }
        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            deltaTime = (float)args.Time;
            float velocity = camera.speed * deltaTime;

            var keyboardInput = KeyboardState;
            var mouseInput = MouseState;

            if (keyboardInput.IsKeyDown(Keys.W))
            {
                camera.Position += camera.Front * velocity;
            }
            if (keyboardInput.IsKeyDown(Keys.A))
            {
                camera.Position -= camera.Right * velocity;
            }
            if (keyboardInput.IsKeyDown(Keys.S))
            {
                camera.Position -= camera.Front * velocity;
            }
            if (keyboardInput.IsKeyDown(Keys.D))
            {
                camera.Position += camera.Right * velocity;
            }
            if (keyboardInput.IsKeyDown(Keys.Escape))
            {
                CursorState = CursorState.Normal;
            }

            if (MouseState.IsButtonPressed(MouseButton.Left))
            {
                CreateRectangle(1, 1, 1, camera.Position);
            }
        }
        protected override void OnRenderFrame(FrameEventArgs args)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.UseProgram(ShaderProgramHandle);
            GL.BindVertexArray(VertexArrayHandle);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndexBufferHandle);

            Matrix4 view = camera.GetMatrix();
            int viewLocation = GL.GetUniformLocation(ShaderProgramHandle, "view");
            GL.UniformMatrix4(viewLocation, false, ref view);

            GL.DrawElements(PrimitiveType.Triangles, indicesCount, DrawElementsType.UnsignedInt, 0);

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

            camera.Pitch -= deltaMousePos.Y * camera.sensitivity;
            camera.Yaw += deltaMousePos.X * camera.sensitivity;
            Console.WriteLine(camera.Pitch);
            Console.WriteLine(camera.Yaw);
            camera.Pitch = Math.Clamp(camera.Pitch, -89f, 89f);

            camera.UpdateDirection();
        }
        protected override void OnResize(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, e.Width, e.Height);
            base.OnResize(e);
        }

        void CreateRectangle(int length, int height, int width, Vector3 position = new(), Color4 color = new())
        {
            if (color == new Color4())
            {
                color = new Color4(1f, 1f, 1f, 1f);
            }

            VertexPositionColor[] vertices =                                                //hellish code :/
            {
                // face 1
                new VertexPositionColor(
                    new Vector3(length + position.X, position.Y, position.Z),
                    color),

                new VertexPositionColor(
                    new Vector3(length + position.X, height + position.Y, position.Z),
                    color),

                new VertexPositionColor(
                    new Vector3(length + position.X, height + position.Y, width + position.Z),
                    color),

                new VertexPositionColor(
                    new Vector3(length + position.X, position.Y, width + position.Z),
                    color),

                // face 2
                new VertexPositionColor(
                    new Vector3(position.X, height + position.Y, position.Z),
                    color),

                new VertexPositionColor(
                    new Vector3(position.X, position.Y, position.Z),
                    color),

                new VertexPositionColor(
                    new Vector3(position.X, position.Y, width + position.Z),
                    color),

                new VertexPositionColor(
                    new Vector3(position.X, height + position.Y, width + position.Z),
                    color)
            };

            int[] indices = {
                0,1,2,  2,3,0,   // right face
                1,4,7,  7,2,1,   // top face
                4,5,6,  6,7,4,   // left face
                5,0,3,  3,6,5,   // bottom face
                1,0,5,  5,4,1,   // front face
                3,2,7,  3,7,6    // back face 
            };

            indicesCount = indices.Length;
            // using vertexbuffer to send data to GPU
            int sizeInBytes = VertexPositionColor.VertexInfo.SizeInBytes;

            VertexBufferHandle = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferHandle);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeInBytes, vertices, BufferUsageHint.DynamicDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            IndexBufferHandle = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndexBufferHandle);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(int), indices, BufferUsageHint.DynamicDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            VertexArrayHandle = GL.GenVertexArray();
            GL.BindVertexArray(VertexArrayHandle);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferHandle);

            VertexAttribute vertexPositionColorAttrib0 = VertexPositionColor.VertexInfo.Attributes[0];
            VertexAttribute vertexPositionColorAttrib1 = VertexPositionColor.VertexInfo.Attributes[1];

            GL.VertexAttribPointer(vertexPositionColorAttrib0.Index, vertexPositionColorAttrib0.Count, VertexAttribPointerType.Float, false, 7 * sizeof(float), vertexPositionColorAttrib0.Offset);
            GL.VertexAttribPointer(vertexPositionColorAttrib1.Index, vertexPositionColorAttrib1.Count, VertexAttribPointerType.Float, false, 7 * sizeof(float), vertexPositionColorAttrib1.Offset);

            GL.EnableVertexAttribArray(vertexPositionColorAttrib0.Index);
            GL.EnableVertexAttribArray(vertexPositionColorAttrib1.Index);


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

            void main() {
                fColor = vColor;
                gl_Position = projection * view * model * vec4(vPosition, 1.0);
            }
            ";

            string fragmentShaderSource =
            @"
            #version 330 core

            in vec4 fColor;
            out vec4 color;

            void main(){
                color = fColor;
            }
            ";


            int vertexShaderHandle = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShaderHandle, vertexShaderSource);
            GL.CompileShader(vertexShaderHandle);

            string vertexShaderInfo = GL.GetShaderInfoLog(vertexShaderHandle);
            if (vertexShaderInfo != string.Empty)
            {
                Console.WriteLine("Error during compilation of vertex shader: " + vertexShaderInfo);
            }

            int fragmentShaderHandle = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShaderHandle, fragmentShaderSource);
            GL.CompileShader(fragmentShaderHandle);

            string fragmentShaderInfo = GL.GetShaderInfoLog(fragmentShaderHandle);
            if (fragmentShaderInfo != string.Empty)
            {
                Console.WriteLine("Error during compilation of fragment shader: " + fragmentShaderInfo);
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
            int[] viewport = new int[4];
            GL.GetInteger(GetPName.Viewport, viewport);
            int viewportSizeLocation = GL.GetUniformLocation(ShaderProgramHandle, "viewportSize");
            GL.Uniform2(viewportSizeLocation, (float)viewport[2], (float)viewport[3]);

            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90f), Size.X / Size.Y, 0.1f, 100f);
            Matrix4 view = camera.GetMatrix();
            Matrix4 model = Matrix4.Identity;

            int projectionLocation = GL.GetUniformLocation(ShaderProgramHandle, "projection");
            int viewLocation = GL.GetUniformLocation(ShaderProgramHandle, "view");
            int modelLocation = GL.GetUniformLocation(ShaderProgramHandle, "model");

            GL.UniformMatrix4(projectionLocation, false, ref projection);
            GL.UniformMatrix4(viewLocation, false, ref view);
            GL.UniformMatrix4(modelLocation, false, ref model);

            GL.UseProgram(0);
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
