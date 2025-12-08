using System.Runtime.InteropServices;
using OpenTK.Mathematics;

namespace OpenTK_Project
{
    public class InstanceStructs
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct RectangleInstance
        {
            public int Length, Height, Width;
            public Vector3 Position;
            public Color4 Color;
            public float Selected;
        }
    }

}
