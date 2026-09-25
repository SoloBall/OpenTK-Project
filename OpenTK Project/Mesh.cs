using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace OpenTK_Project
{
    // add garbage collection
    // parse position into vertex shader
    // parse outline color to outline fragment shader
    public class Mesh
    {
        private int VertexBufferObject, VertexArrayObject, AdjacentVertexArrayObject, IndexBufferObject, AdjacentIndexBufferObject, BasicShader, OutlineShader;
        public VertexPositionColor[] Vertices { get; set; }
        public uint[] Indices { get; set; }
        private uint[] AdjacentIndices;
        private int sizeInBytes = VertexPositionColor.VertexInfo.SizeInBytes;

        public Mesh( VertexPositionColor[] vertices, uint[] indices)
        {
            Vertices = vertices;
            Indices = indices;
            AdjacentIndices = AdjacencyMeshBuilder.BuildAdjacencyIndices(Indices);

            if ( VertexBufferObject != 0 )
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                GL.BindVertexArray(0);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
                GL.DeleteBuffer(VertexBufferObject);
                GL.DeleteBuffer(IndexBufferObject);
                GL.DeleteVertexArray(VertexArrayObject);
                VertexBufferObject = IndexBufferObject = VertexArrayObject = 0;
            }
            VertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, Vertices.Count() * sizeInBytes, Vertices.ToArray(), BufferUsageHint.DynamicDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            IndexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndexBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, Indices.Count() * sizeof(int), Indices.ToArray(), BufferUsageHint.DynamicDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            AdjacentIndexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, AdjacentIndexBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, AdjacentIndices.Count() * sizeof(int), AdjacentIndices.ToArray(), BufferUsageHint.DynamicDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            VertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(VertexArrayObject);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);

            VertexAttribute vertexPositionColorAttrib0 = VertexPositionColor.VertexInfo.Attributes[0]; // try and take this value, use it as location later on.
            VertexAttribute vertexPositionColorAttrib1 = VertexPositionColor.VertexInfo.Attributes[1];

            GL.VertexAttribPointer(vertexPositionColorAttrib0.Index, vertexPositionColorAttrib0.Count, VertexAttribPointerType.Float, false, 7 * sizeof(float), vertexPositionColorAttrib0.Offset);
            GL.VertexAttribPointer(vertexPositionColorAttrib1.Index, vertexPositionColorAttrib1.Count, VertexAttribPointerType.Float, false, 7 * sizeof(float), vertexPositionColorAttrib1.Offset);

            GL.EnableVertexAttribArray(vertexPositionColorAttrib0.Index);
            GL.EnableVertexAttribArray(vertexPositionColorAttrib1.Index);

            GL.BindVertexArray(0);

            AdjacentVertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(AdjacentVertexArrayObject);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);

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

            if ( BasicShader != 0 )
            {
                GL.DeleteProgram(BasicShader);
            }
            BasicShader = GL.CreateProgram();

            GL.AttachShader(BasicShader, vertexShaderHandle);
            GL.AttachShader(BasicShader, fragmentShaderHandle);

            GL.LinkProgram(BasicShader);

            GL.DetachShader(BasicShader, vertexShaderHandle);
            GL.DetachShader(BasicShader, fragmentShaderHandle);

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

            if ( OutlineShader != 0 )
            {
                GL.DeleteProgram(OutlineShader);
            }
            OutlineShader = GL.CreateProgram();

            GL.AttachShader(OutlineShader, outlineVertexShaderHandle);
            GL.AttachShader(OutlineShader, outlineGeometryShaderHandle);
            GL.AttachShader(OutlineShader, outlineFragmentShaderHandle);

            GL.LinkProgram(OutlineShader);

            GL.DetachShader(OutlineShader, outlineVertexShaderHandle);
            GL.DetachShader(OutlineShader, outlineGeometryShaderHandle);
            GL.DetachShader(OutlineShader, outlineFragmentShaderHandle);

            GL.DeleteShader(outlineVertexShaderHandle);
            GL.DeleteShader(outlineGeometryShaderHandle);
            GL.DeleteShader(outlineFragmentShaderHandle);
        }
        public void Render(Vector2i ClientSize, Camera camera, bool wireframe = false, bool dirty = false )
        {
            
            GL.UseProgram(BasicShader);
            if ( dirty ) 
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
                GL.BufferData(BufferTarget.ArrayBuffer, Vertices.Count() * sizeInBytes, Vertices.ToArray(), BufferUsageHint.DynamicDraw);

                GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndexBufferObject);
                GL.BufferData(BufferTarget.ElementArrayBuffer, Indices.Count() * sizeof(int), Indices.ToArray(), BufferUsageHint.DynamicDraw);
                GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
            }

            Matrix4 view = camera!.GetMatrix();
            Matrix4 model = Matrix4.Identity;


            int projectionLocation = GL.GetUniformLocation(BasicShader, "projection");
            int viewLocation = GL.GetUniformLocation(BasicShader, "view");
            int modelLocation = GL.GetUniformLocation(BasicShader, "model");
            int testLocation = GL.GetUniformLocation(BasicShader, "test");

            int cameraPosLocation = GL.GetUniformLocation(BasicShader, "cameraPos");

            GL.UniformMatrix4(projectionLocation, false, ref camera.projection);
            GL.UniformMatrix4(viewLocation, false, ref view);
            GL.UniformMatrix4(modelLocation, false, ref model);

            GL.Uniform3(cameraPosLocation, camera.Position);
            GL.BindVertexArray(VertexArrayObject);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndexBufferObject);

            if ( !wireframe )
            {
                GL.DrawElements(PrimitiveType.Triangles, Indices.Length, DrawElementsType.UnsignedInt, 0);
            }


            // outline pass

            GL.UseProgram(OutlineShader);

            if ( dirty ) 
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
                GL.BufferData(BufferTarget.ArrayBuffer, Vertices.Count() * sizeInBytes, Vertices.ToArray(), BufferUsageHint.DynamicDraw);

                GL.BindBuffer(BufferTarget.ElementArrayBuffer, AdjacentIndexBufferObject);
                GL.BufferData(BufferTarget.ElementArrayBuffer, AdjacentIndices.Count() * sizeof(int), AdjacentIndices.ToArray(), BufferUsageHint.DynamicDraw);

                GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
            }

            int outlinerojectionLocation = GL.GetUniformLocation(OutlineShader, "uProjection");
            int outlineViewLocation = GL.GetUniformLocation(OutlineShader, "uView");
            int outlineModelLocation = GL.GetUniformLocation(OutlineShader, "uModel");

            int edgeThicknessLocation = GL.GetUniformLocation(OutlineShader, "uEdgeThickness");
            int creaseCosThresholdLocation = GL.GetUniformLocation(OutlineShader, "uCreaseCosThreshold");

            int outlineColorLocation = GL.GetUniformLocation(OutlineShader, "uOutlineColor");

            GL.UniformMatrix4(outlinerojectionLocation, false, ref camera.projection);
            GL.UniformMatrix4(outlineViewLocation, false, ref view);
            GL.UniformMatrix4(outlineModelLocation, false, ref model);

            GL.Uniform1(edgeThicknessLocation, 0.002f);
            GL.Uniform1(creaseCosThresholdLocation, float.DegreesToRadians(30f));

            GL.Uniform3(outlineColorLocation, new Vector3(0.2f, 0.2f, 0.2f));


            GL.BindVertexArray(AdjacentVertexArrayObject);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, AdjacentIndexBufferObject);
            GL.DrawElements(PrimitiveType.TrianglesAdjacency, AdjacentIndices.Count(), DrawElementsType.UnsignedInt, 0);
        }
    }
}
