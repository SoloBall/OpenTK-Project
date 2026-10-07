using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTK_Project
{
    internal static class ShaderControls
    {
        public static string GetVertexShaderSource( )
        {
            string source = File.ReadAllText("../../../Shaders/Basic/Vertex.glsl");
            return source;
        }

        public static string GetFragmentShaderSource( )
        {
            string source = File.ReadAllText("../../../Shaders/Basic/Fragment.glsl");
            return source;
        }

        public static string GetOutlineVertexShaderSource( )
        {
            string source = File.ReadAllText("../../../Shaders/Outline/Vertex.glsl");
            return source;
        }
        public static string GetOutlineGeometryShaderSource( )
        {
            string source = File.ReadAllText("../../../Shaders/Outline/Geometry.glsl");
            return source;
        }
        public static string GetOutlineFragmentShaderSource( )
        {
            string source = File.ReadAllText("../../../Shaders/Outline/Fragment.glsl");
            return source;
        }
    }
}
