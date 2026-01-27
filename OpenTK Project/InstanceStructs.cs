using System.Runtime.InteropServices;
using OpenTK.Mathematics;

namespace OpenTK_Project
{
    public class InstanceStructs
    {
        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct RectangleInstance
        {
            public float Length;
            public float Height;
            public float Width;
            private float _pad0; // padding for vec3 alignment

            public Vector3 Position;
            private float _pad1; // vec3 -> 16 bytes

            public Color4 Color;

            public float Selected;
            private Vector3 _pad2; // struct size multiple of 16
        }

    }

}
