using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Runtime.CompilerServices;

namespace OpenTK_Project
{
    public class Game : GameWindow
    {
        private int ShaderStorageBufferHandle;

        private Camera? camera;
        private float deltaTime;
        private Vector2 lastMousePos;
        private bool isFirstMouse;

        private bool Wireframe = false;

        private Random? rand;

        private List<SceneObject>? objects;
        private SceneObject? selectedRectangle;
        private Vector3 sunDirection;

        public Game(int width = 1280, int height = 720, string title = "Base Window") : base(GameWindowSettings.Default,
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

            camera = new(ClientSize);
            CursorState = CursorState.Grabbed;

            GL.Enable(EnableCap.DepthTest);
            //objects.AddRange(Mapper.CreateSilentHill());
            objects.AddRange(Mapper.CreateRectangle());
            camera.Position += Vector3.UnitX * 10 + Vector3.UnitZ * 3;

            ShaderStorageBufferHandle = GL.GenBuffer();
            sunDirection = new Vector3(0f, 0f, 1f).Normalized();

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
            if ( keyboardInput.IsKeyPressed(Keys.Y) )
            {
                Color4 randomColor = new((float)rand!.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble(), 1f);
                SceneObject rectangle = new(new Vector3(1, 1, 1), camera.Position + camera.Front * 3, randomColor, modelURL: "sphere");
                rectangle.Mesh.Rotate(Vector3.UnitX, 90);
                objects!.Add(rectangle);
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
                SceneObject rectangle = new(new Vector3(1, 1, 1), camera.Position + camera.Front * 3, randomColor);
                rectangle.Mesh.Rotate(Vector3.UnitZ, 30);
                //rectangle.Mesh = GrabMeshFromModels(rectangle.Color, rectangle.Position, "../../../Assets/Models/cube.obj", 1);
                objects!.Add(rectangle);
            }
            if ( MouseState.IsButtonPressed(MouseButton.Middle) )
            {
                SceneObject rectangle = new(new Vector3(3, 3, 3), camera.Position + camera.Front * 3, Color4.Yellow, modelURL: "Entity/cow");
                rectangle.Mesh.Rotate(Vector3.UnitX, 90);
                objects!.Add(rectangle);
            }
            if ( MouseState.IsButtonPressed(MouseButton.Right) )
            {
                // lazer
                //SceneObject obj = new(new Vector3(0.2f, 10, 0.2f), camera.Position + camera.Front * 12 + camera.Up * 2, Color4.Red);
                //objects!.Add(obj);
                for ( int i = 0; i < objects!.Count; i++ )
                {
                    if ( objects[i] != selectedRectangle && camera.IsLookingAtRectangle(objects[i]) )
                    {
                        Console.WriteLine("cube find: " + objects[i].Color.ToString());
                        if (selectedRectangle != null )
                        {
                            if (Vector3.Distance(camera.Position, selectedRectangle.Mesh.LocalOrigin) > Vector3.Distance(camera.Position, objects[i].Mesh.LocalOrigin) )
                            {
                                foreach (SceneObject rectangle in objects )
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
                    foreach (SceneObject rectangle in objects! )
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
                foreach (SceneObject obj in objects! )
                {
                    obj.Mesh.LocalOrigin = obj.Mesh.LocalOrigin - oldPosition;
                }
            }
        }

        void UpdateSelectedObjectPosition( )
        {
            if (selectedRectangle != null )
            {
                float distance = Vector3.Distance(camera!.Position, selectedRectangle!.Mesh.LocalOrigin);
                var scroll = MouseState.ScrollDelta.Y;
                distance += scroll * 20 * (1 - deltaTime * 10);
                distance = float.Clamp(distance, 1f, 40f);
                Vector3 newPosition = Vector3.Lerp(selectedRectangle.Mesh.LocalOrigin, camera.Position + camera.Front * distance, 0.01f + deltaTime * 2);
                selectedRectangle.Mesh.LocalOrigin = newPosition;
                selectedRectangle.Mesh.Rotate(Vector3.UnitY, 50*deltaTime);
            }
        }
        void CreateGrid(int size = 15, int spread = 3 )
        {
            for ( float x = 0; x < size; x += spread )
            {
                for ( float y = 0; y < size; y += spread )
                {
                    for ( float z = 0; z < size; z += spread )
                    {
                        SceneObject point = new(new Vector3(1, 1, 1), (camera!.Position.X + x, camera.Position.Y + y, camera.Position.Z + z), new(x / size, z / size, y / size, 1), modelURL: "Entity/cow");
                        point.Mesh.Rotate(Vector3.UnitX, 90);
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

        // instead of going through all rectangles, have either a grid based indexing system where each area or chunk has "dirty", where if something is dirty there, only undirty there.
        // like have a list of 1000 chunks where each chunk has a list of 1000 objects aswell as a "dirty" property. Then for each dirty chunk, find the dirty object

        // Updates all objects (including camera) and returns amount of indices

        // update so outline pass also works
        protected override void OnRenderFrame(FrameEventArgs args)
        {
            // for drawing in bulk
            /*List<VertexPositionColor> vertices = [];
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
            }*/
            // using vertexbuffer to send data to GPU
            GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            // switch to buffersubdata when too slow
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, ShaderStorageBufferHandle);

            // vertex 1, 2, 3, normal, position, color

            var triangles = new List<TriangleInfo>();
            foreach (SceneObject obj in objects )
            {
                for ( int i = 0; i < obj.Mesh.Indices.Length; i += 3 )
                {
                    Vector3 a = obj.Mesh.Vertices[obj.Mesh.Indices[i]].Position;
                    Vector3 b = obj.Mesh.Vertices[obj.Mesh.Indices[i + 1]].Position;
                    Vector3 c = obj.Mesh.Vertices[obj.Mesh.Indices[i + 2]].Position;
                    triangles.Add(new TriangleInfo
                    {
                        V0 = new Vector4(a, 0),
                        V1 = new Vector4(b, 0),
                        V2 = new Vector4(c, 0),
                        Normal = new Vector4(Vector3.Normalize(Vector3.Cross(b - a, c - a)), 0),
                        Origin = new Vector4(obj.Mesh.LocalOrigin, 0),
                        Color = obj.Color,
                        Orientation = obj.Mesh.Orientation
                    });
                }
            }
            GL.BufferData(BufferTarget.ShaderStorageBuffer, triangles.Count() * Unsafe.SizeOf<TriangleInfo>(), triangles.ToArray(), BufferUsageHint.DynamicDraw);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 0, ShaderStorageBufferHandle);
            int triangleOffset = 0;
            foreach (SceneObject obj in objects )
            {
                obj.Mesh.Render(ClientSize, camera, obj.Scale, sunDirection, triangleOffset, Wireframe, obj.Dirty, obj.Selected);
                triangleOffset += obj.Mesh.Indices.Length / 3;
            }

            this.Context.SwapBuffers();
            base.OnRenderFrame(args);
        }
        public void CameraCollidesWithRectangle(Vector3 proposedPosition, float velocity)
        {
            float radius = 1f;
            foreach (SceneObject rectangle in objects!) 
            {
                if (rectangle.Mesh.LocalOrigin == Vector3.Zero) continue; // check for whether it's close enough for optimization
                if (rectangle.CollidesWithSphere(proposedPosition, radius)) //WIP
                {
                    Vector3 min = rectangle.Mesh.LocalOrigin;
                    Vector3 max = rectangle.Mesh.LocalOrigin + rectangle.Scale;
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
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            GL.UseProgram(0);
            base.OnUnload();
        }
    }
}
