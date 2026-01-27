using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTK_Project
{
    internal static class ShaderControls
    {
        public static string  GetVertexShaderSource( )
        {
            return File.ReadAllText("../../../Shaders/basic.vert");
        }
        // make colors fun
        public static string GetFragmentShaderSource( )
        {

            return File.ReadAllText("../../../Shaders/basic.frag");
        }
        public static string GetScreenVertexShaderSource( )
        {
            return File.ReadAllText("../../../Shaders/screen.vert");
        }
        public static string GetScreenFragmentShaderSource( )
        {
            return File.ReadAllText("../../../Shaders/screen.frag");
        }
    }
}
