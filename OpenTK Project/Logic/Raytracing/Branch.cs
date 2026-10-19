using OpenTK.Mathematics;

namespace OpenTK_Project.Logic.Raytracing
{
    public class Branch
    {
        public Branch? LeftChild, RightChild;
        public TrianglePrimitive[]? Triangles;
        private int maxLeafSize = 4;
        private float traversalCost = 1;
        private float intersectionCost = 1;

        public void Populate( TrianglePrimitive[] primitives, Vector3 min, Vector3 max )
        {
            float rootArea = Area(min, max);
            float minArea = rootArea;
            int minAreaIndex = 0;
            for (int i = 0; i < primitives.Length; i++ )
            {
                Vector3 leftMin = Vector3.PositiveInfinity;
                Vector3 leftMax = Vector3.NegativeInfinity;
                for ( int k = 0; k < i; k++ )
                {
                    leftMin = GetMin(leftMin, primitives[k].Min.Xyz);
                    leftMax = GetMin(leftMax, primitives[k].Max.Xyz);
                }

                Vector3 rightMin = Vector3.PositiveInfinity;
                Vector3 rightMax = Vector3.NegativeInfinity;
                for ( int k = i; k < primitives.Length; k++ )
                {
                    rightMin = GetMin(rightMin, primitives[k].Min.Xyz);
                    rightMax = GetMax(rightMax, primitives[k].Max.Xyz);
                }

                float leftArea = Area(leftMin, leftMax);
                float rightArea = Area(rightMin, rightMax);

                float currentArea = leftArea * i + rightArea * ( primitives.Length - i);

                if (currentArea < minArea)
                {
                    minArea = currentArea;
                    minAreaIndex = i;
                }
            }
            float splitCost = traversalCost + minArea / rootArea * intersectionCost;
            float leafCost = primitives.Length * intersectionCost;
            if (minAreaIndex <= maxLeafSize && splitCost >= leafCost && primitives.Length - minAreaIndex <= maxLeafSize)
            {
                Triangles = primitives;
            }
            else
            {
                Vector3 leftMin = Vector3.PositiveInfinity;
                Vector3 leftMax = Vector3.NegativeInfinity;
                for ( int i = 0; i < minAreaIndex; i++ )
                {
                    leftMin = GetMin(leftMin, primitives[i].Min.Xyz);
                    leftMax = GetMin(leftMax, primitives[i].Max.Xyz);
                }
                LeftChild = new Branch();
                TrianglePrimitive[] leftPrimitives = new TrianglePrimitive[minAreaIndex];
                for (int i = 0; i <  minAreaIndex; i++ )
                {
                    leftPrimitives[i] = primitives[i];
                }


                Vector3 rightMin = Vector3.PositiveInfinity;
                Vector3 rightMax = Vector3.NegativeInfinity;
                for ( int i = minAreaIndex; i < primitives.Length; i++ )
                {
                    rightMin = GetMin(rightMin, primitives[i].Min.Xyz);
                    rightMax = GetMax(rightMax, primitives[i].Max.Xyz);
                }
                TrianglePrimitive[] rightPrimitives = new TrianglePrimitive[primitives.Length - minAreaIndex];
                for ( int i = minAreaIndex; i < primitives.Length; i++ )
                {
                    rightPrimitives[i - minAreaIndex] = primitives[i];
                }
            }
        }
        public List<TrianglePrimitive> GetPrimitives()
        {
            List<TrianglePrimitive> primitives = new List<TrianglePrimitive>();
            if ( Triangles != null )
            {
                primitives.AddRange(Triangles);
            }
            else
            {
                if (LeftChild != null )
                {
                    primitives.AddRange(LeftChild.GetPrimitives());
                }
                if ( RightChild != null )
                {
                    primitives.AddRange(RightChild.GetPrimitives());
                }
            }
            Console.WriteLine(primitives.Count());
            return primitives;
        }
        private float Area(Vector3 min, Vector3 max)
        {
            float w = max.X - min.X;
            float h = max.Y - min.Y;
            float d = max.Z - min.Z;
            return 2 * (w*h + w*d + h*d);
        }
        private Vector3 GetMin(Vector3 min1, Vector3 min2)
        {
            return new Vector3( Math.Min(min1.X, min2.X), Math.Min(min1.Y, min2.Y), Math.Min(min1.Z, min2.Z));
        }
        private Vector3 GetMax(Vector3 max1, Vector3 max2)
        {
            return  new Vector3(Math.Max(max1.X, max2.X), Math.Max(max1.Y, max2.Y), Math.Max(max1.Z, max2.Z));
        }
    }
}
