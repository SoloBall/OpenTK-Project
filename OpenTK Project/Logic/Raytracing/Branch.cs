using OpenTK.Mathematics;
using System.Text.Json;

namespace OpenTK_Project.Logic.Raytracing
{
    public class Branch
    {
        public Branch? LeftChild, RightChild;
        public uint? LeftChildIndex, RightChildIndex, ParentIndex, SelfIndex;
        public TrianglePrimitive[]? Triangles;
        private Vector3 min, max;
        private int maxLeafSize = 4;
        private float traversalCost = 1;
        private float intersectionCost = 1;

        public uint Populate( TrianglePrimitive[] primitives, Vector3 min, Vector3 max, uint index = 0 )
        {
            index++;
            float rootArea = Area(min, max);
            float minCost = float.PositiveInfinity;
            int minCostIndex = 0;
            for (int i = 1; i < primitives.Length; i++ )
            {
                Vector3 leftMin = Vector3.PositiveInfinity;
                Vector3 leftMax = Vector3.NegativeInfinity;
                for ( int k = 0; k < i; k++ )
                {
                    leftMin = GetMin(leftMin, primitives[k].Min.Xyz);
                    leftMax = GetMax(leftMax, primitives[k].Max.Xyz);
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

                float currentCost = leftArea * i + rightArea * ( primitives.Length - i);

                if (currentCost < minCost)
                {
                    minCost = currentCost;
                    minCostIndex = i;
                }
            }
            float splitCost = traversalCost + minCost / rootArea * intersectionCost;
            float leafCost = primitives.Length * intersectionCost;
            if (minCostIndex <= maxLeafSize && primitives.Length - minCostIndex <= maxLeafSize)
            {
                Triangles = primitives;
            }
            else
            {
                Vector3 leftMin = Vector3.PositiveInfinity;
                Vector3 leftMax = Vector3.NegativeInfinity;
                for ( int i = 0; i < minCostIndex; i++ )
                {
                    leftMin = GetMin(leftMin, primitives[i].Min.Xyz);
                    leftMax = GetMax(leftMax, primitives[i].Max.Xyz);
                }
                TrianglePrimitive[] leftPrimitives = new TrianglePrimitive[minCostIndex];
                for (int i = 0; i <  minCostIndex; i++ )
                {
                    leftPrimitives[i] = primitives[i];
                }
                LeftChild = new Branch();

                Vector3 rightMin = Vector3.PositiveInfinity;
                Vector3 rightMax = Vector3.NegativeInfinity;
                for ( int i = minCostIndex; i < primitives.Length; i++ )
                {
                    rightMin = GetMin(rightMin, primitives[i].Min.Xyz);
                    rightMax = GetMax(rightMax, primitives[i].Max.Xyz);
                }
                TrianglePrimitive[] rightPrimitives = new TrianglePrimitive[primitives.Length - minCostIndex];
                for ( int i = minCostIndex; i < primitives.Length; i++ )
                {
                    rightPrimitives[i - minCostIndex] = primitives[i];
                }
                RightChild = new Branch();

                LeftChildIndex = LeftChild.Populate(leftPrimitives, leftMin, leftMax, index);
                RightChildIndex = RightChild.Populate(rightPrimitives, rightMin, rightMax, index);
                ParentIndex = index - 1;
                SelfIndex = index;
                this.min = min;
                this.max = max;
            }
            return index;
        }
        public List<ShaderNode> GetNodes()
        {
            List<ShaderNode> nodes = new List<ShaderNode>();
            ShaderNode node = new ShaderNode();
            node.Min = new Vector4(min, 0);
            node.Max = new Vector4(max, 0);
            node.Self = (uint)SelfIndex!;
            if (ParentIndex != null )
            {
                node.Parent = (uint)ParentIndex;
            }
            if ( Triangles != null )
            {
                node.Triangles = Triangles.ToArray();
            }
            else
            {
                if (LeftChild != null )
                {
                    node.Left = (uint)LeftChild.SelfIndex!;
                    nodes.AddRange(LeftChild.GetNodes());
                }
                if ( RightChild != null )
                {
                    node.Left = (uint)RightChild.SelfIndex!;
                    nodes.AddRange(RightChild.GetNodes());
                }
            }
            nodes.Add(node);
            return nodes;
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
