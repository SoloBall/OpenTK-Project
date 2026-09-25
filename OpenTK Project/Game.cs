using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace OpenTK_Project
{
    public class Game : GameWindow
    {
        private Camera? camera;
        private float deltaTime;
        private Vector2 lastMousePos;
        private bool isFirstMouse;

        private bool Wireframe = false;

        private Random? rand;

        private List<SceneObject>? objects;
        private SceneObject? selectedRectangle;

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

            camera = new(ClientSize);
            CursorState = CursorState.Grabbed;

            GL.Enable(EnableCap.DepthTest);
            Mapper.LoadCube(objects, camera.Position);
            //Mapper.LoadSilentHill(objects, camera.Position);


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
                SceneObject rectangle = new(1, 1, 1, camera.Position + camera.Front * 3, randomColor);
                rectangle.Mesh = GrabMeshFromModels(rectangle.Color, rectangle.Position, "../../../Assets/Models/sphere.obj");
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
                SceneObject rectangle = new(1, 1, 1, camera.Position + camera.Front * 3, randomColor);
                rectangle.Mesh = GrabMeshFromModels(rectangle.Color, rectangle.Position, "../../../Assets/Models/cube.obj");
                //rectangle.Mesh = GrabMeshFromModels(rectangle.Color, rectangle.Position, "../../../Assets/Models/cube.obj", 1);
                objects!.Add(rectangle);
            }
            if ( MouseState.IsButtonPressed(MouseButton.Middle) )
            {
                SceneObject rectangle = new(3, 3, 3, camera.Position + camera.Front * 3, Color4.Yellow);
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
                    obj.Position = obj.Position - oldPosition;
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
                objects[objects.Count()-1].Position = newPosition;
            }
        }
        void CreateGrid(int size = 50, int spread = 5 )
        {
            for ( float x = 0; x < size; x += spread )
            {
                for ( float y = 0; y < size; y += spread )
                {
                    for ( float z = 0; z < size; z += spread )
                    {
                        SceneObject point = new(1, 1, 1, (camera!.Position.X + x, camera.Position.Y + y, camera.Position.Z + z), new(x / size, z / size, y / size, 1));
                        point.Mesh = GrabMeshFromModels(point.Color, point.Position, "../../../Assets/Models/Entity/cow.obj");
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
            foreach (SceneObject obj in objects )
            {
                obj.Mesh.Render(ClientSize, camera, Wireframe, obj.Dirty);
            }

            this.Context.SwapBuffers();
            base.OnRenderFrame(args);
        }
        
        public static Mesh GrabMeshFromModels( Color4 color, Vector3 position, string path = "../../../Assets/Models/Entity/cow.obj", float scaleX = 1, float scaleY = 1, float scaleZ = 1)
        {
            var (vertices, indices) = ObjLoader.Load(path, color, position, scaleX, scaleY, scaleZ);
            Mesh mesh = new(vertices, indices);
            return mesh;
        }
        public void CameraCollidesWithRectangle(Vector3 proposedPosition, float velocity)
        {
            float radius = 1f;
            foreach (SceneObject rectangle in objects!) 
            {
                if (rectangle.Position == Vector3.Zero) continue; // check for whether it's close enough for optimization
                if (rectangle.CollidesWithSphere(proposedPosition, radius)) //WIP
                {
                    Vector3 min = rectangle.Position;
                    Vector3 max = rectangle.Position + new Vector3(rectangle.ScaleX, rectangle.ScaleZ, rectangle.ScaleY);
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
