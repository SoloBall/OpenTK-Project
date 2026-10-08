using OpenTK.Mathematics;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
struct TriangleInfo
{
    public Vector4 V0, V1, V2, Normal, Origin;
    public Color4 Color; // 16 bytes, matches vec4 in std430
    public Quaternion Orientation;
}