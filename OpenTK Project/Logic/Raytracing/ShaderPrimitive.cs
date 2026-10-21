using OpenTK.Mathematics;

namespace OpenTK_Project.Logic.Raytracing
{
    public struct ShaderPrimitive
    {
        public Vector4 V0, V1, V2, Normal, Origin;
        public ShaderPrimitive( )
        {
            V0 = new(0, 0, 0, 1);
            V1 = new(0, 0, 0, 1);
            V2 = new(0, 0, 0, 1);
            Normal = new(0, 0, 0, 1);
            Origin = new(0, 0, 0, 1);
        }
    }
}
