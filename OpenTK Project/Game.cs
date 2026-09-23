using OpenTK.Compute.OpenCL;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Runtime.InteropServices;

// in shaders: make quad size equal to some function of triangle size
namespace OpenTK_Project
{
    public class Game : GameWindow
    {
        private int VertexBufferHandle;
        private int ShaderProgramHandle;
        private int OutlineShaderProgramHandle;
        private int VertexArrayHandle;
        private int IndexBufferHandle;
        private int AdjacentIndexBufferHandle;
        private int AdjacentVertexArrayHandle;

        private Camera? camera;
        private float deltaTime;
        private Vector2 lastMousePos;
        private bool isFirstMouse;

        private bool Wireframe = false;

        private Random? rand;

        private List<Rectangle>? objects;
        private Rectangle? selectedRectangle;

        public Game(int width = 1920, int height = 1080, string title = "Base Window") : base(GameWindowSettings.Default,
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

            objects = new();

            camera = new();
            CursorState = CursorState.Grabbed;

            GL.Enable(EnableCap.DepthTest);
            for (int i = -5; i < 5; i++)
            {
                for (int j = -2; j < 2; j++ )
                {
                    Rectangle plane = new(10, 1000, 10, camera.Position + new Vector3(i*100, j*100, 1) - Vector3.UnitZ * 1010);
                    plane.Mesh = GrabMeshFromModels(plane.Color, plane.Position, "../../../Assets/Models/cube.obj", 10, 10, 1000);
                    objects.Add(plane);
                    for (int k = 20; k < plane.Height; k++ )
                    {
                        if (k%20 == 0 )
                        {
                            Rectangle ring = new(12, 1, 12, plane.Position + Vector3.UnitZ * k);
                            ring.Mesh = GrabMeshFromModels(Color4.Red, ring.Position, "../../../Assets/Models/cube.obj", 12, 12, 1);
                            objects.Add(ring);
                        }
                    }
                }
            }
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
            if ( keyboardInput.IsKeyDown(Keys.LeftShift) )
            {
                velocity *= 4;
            }
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
                UpdateSelectedObjectPosition();
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
                objects!.Clear();
            }
            if ( keyboardInput.IsKeyPressed(Keys.K) )
            {
                if ( !Wireframe )
                {
                    GL.Disable(EnableCap.DepthTest);
                    GL.DepthMask(false);
                    Wireframe = true;
                }
                else
                {
                    GL.Enable(EnableCap.DepthTest);
                    GL.DepthMask(true);
                    Wireframe = false;
                }
            }
            if (MouseState.IsButtonPressed(MouseButton.Left))
            {
                Color4 randomColor = new((float)rand!.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble(),1f);
                Rectangle rectangle = new(1, 1, 1, camera.Position + camera.Front * 3, randomColor);
                rectangle.Mesh = GrabMeshFromModels(rectangle.Color, rectangle.Position, "../../../Assets/Models/cube.obj");
                //rectangle.Mesh = GrabMeshFromModels(rectangle.Color, rectangle.Position, "../../../Assets/Models/cube.obj", 1);
                objects!.Add(rectangle);
            }
            if ( MouseState.IsButtonPressed(MouseButton.Middle) )
            {
                Rectangle rectangle = new(3, 3, 3, camera.Position + camera.Front * 3, Color4.Yellow);
                rectangle.Mesh = GrabMeshFromModels(rectangle.Color, rectangle.Position);
                objects!.Add(rectangle);
            }
            if ( MouseState.IsButtonPressed(MouseButton.Right) )
            {
                for ( int i = 0; i < objects!.Count; i++ )
                {
                    if ( objects[i] != selectedRectangle && camera.IsLookingAtRectangle(objects[i]) )
                    {
                        Console.WriteLine("cube find: " + objects[i].Color.ToString());
                        if (selectedRectangle != null )
                        {
                            if (Vector3.Distance(camera.Position, selectedRectangle.Position) > Vector3.Distance(camera.Position, objects[i].Position) )
                            {
                                foreach (Rectangle rectangle in objects )
                                {
                                    if (rectangle == selectedRectangle )
                                    {
                                        rectangle.Selected = false;
                                        rectangle.Dirty = true;
                                    }
                                }
                                objects[i].Selected = true;
                                objects[i].Dirty = true;
                                selectedRectangle = objects[i];
                            }
                        }
                        else
                        {
                            objects[i].Selected = true;
                            objects[i].Dirty = true;
                            selectedRectangle = objects[i];
                        }
                    }
                }
            }
            if ( selectedRectangle != null )
            {
                if ( keyboardInput.IsKeyPressed(Keys.Q) )
                {
                    foreach (Rectangle rectangle in objects! )
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
                UpdateSelectedObjectPosition();
            }
            if ( camera.Position.X > 5000 && camera.Position.Y > 5000 )
            {
                Vector3 oldPosition = camera.Position;
                camera.Position = camera.Position - oldPosition;
                foreach (Rectangle rectangle in objects! )
                {
                    rectangle.Dirty = true;
                    rectangle.Position = rectangle.Position - oldPosition;
                }
            }
        }

        void UpdateSelectedObjectPosition( )
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
                objects[objects.Count()-1].Position = newPosition;
            }
        }
        void CreateGrid(int size = 24, int spread = 8 )
        {
            for ( float x = 0; x < size; x += spread )
            {
                for ( float y = 0; y < size; y += spread )
                {
                    for ( float z = 0; z < size; z += spread )
                    {
                        Rectangle point = new(1, 1, 1, (camera!.Position.X + x, camera.Position.Y + y, camera.Position.Z + z), new(x / size, z / size, y / size, 1));
                        point.Mesh = GrabMeshFromModels(point.Color, point.Position, "../../../Assets/Models/cube.obj");
                        objects!.Add(point);
                    }
                }
            }
        }
        // add outline pass and remove old uniforms
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
            GL.Enable(EnableCap.DepthTest);
            GL.DepthMask(true);
            List<VertexPositionColor> vertices = [];
            List<uint> indices = [];
            int vertexOffset = 0;
            for (int i = 0; i < objects!.Count; i++ )
            {
                if (objects[i].Selected)
                {
                    objects[i] = selectedRectangle!;
                }
                foreach (var vertex in objects[i].Mesh.Vertices )
                {
                    vertices.Add(new VertexPositionColor(vertex.Position + objects[i].Position, vertex.Color));
                }

                foreach (int index in objects[i].Mesh.Indices )
                {
                    indices.Add((uint)( index + vertexOffset ));
                }
                vertexOffset += objects[i].Mesh.Vertices.Count();
            }
            uint[] adjacentIndices = AdjacencyMeshBuilder.BuildAdjacencyIndices(indices.ToArray());
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

            AdjacentIndexBufferHandle = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, AdjacentIndexBufferHandle);
            GL.BufferData(BufferTarget.ElementArrayBuffer, adjacentIndices.Count() * sizeof(int), adjacentIndices.ToArray(), BufferUsageHint.DynamicDraw);
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

            GL.BindVertexArray(0);

            AdjacentVertexArrayHandle = GL.GenVertexArray();
            GL.BindVertexArray(AdjacentVertexArrayHandle);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferHandle);

            GL.VertexAttribPointer(vertexPositionColorAttrib0.Index, vertexPositionColorAttrib0.Count, VertexAttribPointerType.Float, false, 7 * sizeof(float), vertexPositionColorAttrib0.Offset);
            GL.VertexAttribPointer(vertexPositionColorAttrib1.Index, vertexPositionColorAttrib1.Count, VertexAttribPointerType.Float, false, 7 * sizeof(float), vertexPositionColorAttrib1.Offset);

            GL.EnableVertexAttribArray(vertexPositionColorAttrib0.Index);
            GL.EnableVertexAttribArray(vertexPositionColorAttrib1.Index);

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


            // outline pass shaders
            
            string outlineVertexShaderSource = ShaderControls.GetOutlineVertexShaderSource();
            int outlineVertexShaderHandle = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(outlineVertexShaderHandle, outlineVertexShaderSource);
            GL.CompileShader(outlineVertexShaderHandle);

            string outlineVertexShaderInfo = GL.GetShaderInfoLog(outlineVertexShaderHandle);
            if ( outlineVertexShaderInfo != string.Empty )
            {
                Console.WriteLine("Error during compilation of outline vertex shader: " + outlineVertexShaderInfo);
            }

            string outlineGeometryShaderSource = ShaderControls.GetOutlineGeometryShaderSource();
            int outlineGeometryShaderHandle = GL.CreateShader(ShaderType.GeometryShader);
            GL.ShaderSource(outlineGeometryShaderHandle, outlineGeometryShaderSource);
            GL.CompileShader(outlineGeometryShaderHandle);

            string outlineGeometryShaderInfo = GL.GetShaderInfoLog(outlineGeometryShaderHandle);
            if ( outlineGeometryShaderInfo != string.Empty )
            {
                Console.WriteLine("Error during compilation of outline geometry shader: " + outlineGeometryShaderInfo);
            }

            string outlineFragmentShaderSource = ShaderControls.GetOutlineFragmentShaderSource();
            int outlineFragmentShaderHandle = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(outlineFragmentShaderHandle, outlineFragmentShaderSource);
            GL.CompileShader(outlineFragmentShaderHandle);

            string outlineFragmentShaderInfo = GL.GetShaderInfoLog(outlineFragmentShaderHandle);
            if ( outlineFragmentShaderInfo != string.Empty )
            {
                Console.WriteLine("Error during compilation of outline fragment shader: " + outlineFragmentShaderInfo);
            }

            if ( OutlineShaderProgramHandle != 0 )
            {
                GL.DeleteProgram(OutlineShaderProgramHandle);
            }
            OutlineShaderProgramHandle = GL.CreateProgram();

            GL.AttachShader(OutlineShaderProgramHandle, outlineVertexShaderHandle);
            GL.AttachShader(OutlineShaderProgramHandle, outlineGeometryShaderHandle);
            GL.AttachShader(OutlineShaderProgramHandle, outlineFragmentShaderHandle);

            GL.LinkProgram(OutlineShaderProgramHandle);

            GL.DetachShader(OutlineShaderProgramHandle, outlineVertexShaderHandle);
            GL.DetachShader(OutlineShaderProgramHandle, outlineGeometryShaderHandle);
            GL.DetachShader(OutlineShaderProgramHandle, outlineFragmentShaderHandle);

            GL.DeleteShader(outlineVertexShaderHandle);
            GL.DeleteShader(outlineGeometryShaderHandle);
            GL.DeleteShader(outlineFragmentShaderHandle);
        }

        // instead of going through all rectangles, have either a grid based indexing system where each area or chunk has "dirty", where if something is dirty there, only undirty there.
        // like have a list of 1000 chunks where each chunk has a list of 1000 objects aswell as a "dirty" property. Then for each dirty chunk, find the dirty object

        // Updates all objects (including camera) and returns amount of indices

        // update so outline pass also works
        protected override void OnRenderFrame(FrameEventArgs args)
        {
            GL.UseProgram(ShaderProgramHandle);
            List<VertexPositionColor> vertices = [];
            List<uint> indices = [];
            int vertexOffset = 0;
            for ( int i = 0; i < objects!.Count; i++ )
            {
                if ( objects[i].Selected )
                {
                    objects[i] = selectedRectangle!;
                }
                foreach ( var vertex in objects[i].Mesh.Vertices )
                {
                    vertices.Add(new VertexPositionColor(vertex.Position + objects[i].Position, vertex.Color));
                }

                foreach ( int index in objects[i].Mesh.Indices )
                {
                    indices.Add((uint)( index + vertexOffset ));
                }
                vertexOffset += objects[i].Mesh.Vertices.Count();
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


            int projectionLocation = GL.GetUniformLocation(ShaderProgramHandle, "projection");
            int viewLocation = GL.GetUniformLocation(ShaderProgramHandle, "view");
            int modelLocation = GL.GetUniformLocation(ShaderProgramHandle, "model");

            int cameraPosLocation = GL.GetUniformLocation(ShaderProgramHandle, "cameraPos");

            GL.UniformMatrix4(projectionLocation, false, ref projection);
            GL.UniformMatrix4(viewLocation, false, ref view);
            GL.UniformMatrix4(modelLocation, false, ref model);

            GL.Uniform3(cameraPosLocation, camera.Position);

            GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.BindVertexArray(VertexArrayHandle);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndexBufferHandle);
            
            GL.DrawElements(PrimitiveType.Triangles, indices.Count, DrawElementsType.UnsignedInt, 0);


            // outline pass
            
            GL.UseProgram(OutlineShaderProgramHandle);

            uint[] adjacencedIndices = AdjacencyMeshBuilder.BuildAdjacencyIndices(indices.ToArray());
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferHandle);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Count() * sizeInBytes, vertices.ToArray(), BufferUsageHint.DynamicDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, AdjacentIndexBufferHandle);
            GL.BufferData(BufferTarget.ElementArrayBuffer, adjacencedIndices.Count() * sizeof(int), adjacencedIndices.ToArray(), BufferUsageHint.DynamicDraw);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferHandle);

            int outlinerojectionLocation = GL.GetUniformLocation(OutlineShaderProgramHandle, "uProjection");
            int outlineViewLocation = GL.GetUniformLocation(OutlineShaderProgramHandle, "uView");
            int outlineModelLocation = GL.GetUniformLocation(OutlineShaderProgramHandle, "uModel");

            int edgeThicknessLocation = GL.GetUniformLocation(OutlineShaderProgramHandle, "uEdgeThickness");
            int creaseCosThresholdLocation = GL.GetUniformLocation(OutlineShaderProgramHandle, "uCreaseCosThreshold");

            int outlineColorLocation = GL.GetUniformLocation(OutlineShaderProgramHandle, "uOutlineColor");

            GL.UniformMatrix4(outlinerojectionLocation, false, ref projection);
            GL.UniformMatrix4(outlineViewLocation, false, ref view);
            GL.UniformMatrix4(outlineModelLocation, false, ref model);

            GL.Uniform1(edgeThicknessLocation, 0.01f); // adjust
            GL.Uniform1(creaseCosThresholdLocation, float.DegreesToRadians(30f)); // minimum degrees

            GL.Uniform3(outlineColorLocation, new Vector3(0.2f, 0.2f, 0.2f));


            GL.BindVertexArray(AdjacentVertexArrayHandle);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, AdjacentIndexBufferHandle);
            if ( Wireframe )
            {
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            }
            GL.DrawElements(PrimitiveType.TrianglesAdjacency, adjacencedIndices.Count(), DrawElementsType.UnsignedInt, 0);

            this.Context.SwapBuffers();
            base.OnRenderFrame(args);
        }
        
        // maybe convert so it can be made from a rectangle
        public static Mesh GrabMeshFromModels( Color4 color, Vector3 position, string path = "../../../Assets/Models/Entity/cow.obj", float scaleX = 1, float scaleY = 1, float scaleZ = 1)
        {
            var (vertices, indices) = ObjLoader.Load(path, color, scaleX, scaleY, scaleZ);
            Mesh mesh = new();
            mesh.Vertices = vertices;
            mesh.Indices = indices;
            return mesh;
        }
        public void CameraCollidesWithRectangle(Vector3 proposedPosition, float velocity)
        {
            float radius = 1f;
            foreach (Rectangle rectangle in objects!) 
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


            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.DeleteBuffer(VertexBufferHandle);
            GL.DeleteBuffer(IndexBufferHandle);

            GL.BindTexture(TextureTarget.Texture2D, 0);

            GL.UseProgram(0);
            GL.DeleteProgram(ShaderProgramHandle);
            GL.DeleteProgram(ShaderProgramHandle);

            base.OnUnload();
        }
    }
}
