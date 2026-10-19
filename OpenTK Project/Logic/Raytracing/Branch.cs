using OpenTK.Mathematics;

namespace OpenTK_Project.Logic.Raytracing
{
    public class Branch
    {
        public Branch? LeftChild, RightChild;
        public Vector3? Min, Max;
        public Leaf? Triangles;

        public void Populate( TrianglePrimitive[] primitives )
        {
            for (int i = 0; i < primitives.Length; i++ )
            {
                // check area for everything before i and add everything to the right, check if it's the lowest area
            }
        }
    }
}
