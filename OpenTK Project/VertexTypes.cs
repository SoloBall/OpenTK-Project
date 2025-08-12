using OpenTK.Graphics.ES11;
using OpenTK.Mathematics;
using System.Security.Cryptography.X509Certificates;

namespace OpenTK_Project
{
    public readonly struct VertexPositionColor
    {
        public readonly Vector3 Position;
        public readonly Color4 Color;

        public static readonly VertexInfo VertexInfo = new VertexInfo(
            typeof(VertexPositionColor),
            new VertexAttribute("Position", 0, 3, 0),
            new VertexAttribute("Color", 1, 4, 3 * sizeof(float))
        );

        public VertexPositionColor(Vector3 position, Color4 color)
        {
            Position = position;
            Color = color;
        }
    }


    public class VertexInfo
    {
        public int SizeInBytes;
        public Type Type;
        public VertexAttribute[] Attributes;

        public VertexInfo(Type type, params VertexAttribute[] attributes)
        {
            Type = type;
            SizeInBytes = 0;
            Attributes = attributes;

            for (int i = 0; i < Attributes.Length; i++)
            {
                VertexAttribute attribute = Attributes[i];
                SizeInBytes += attribute.Count * sizeof(float);
            }
        }
    }

    public struct VertexAttribute
    {
        public string Name;
        public int Index;
        public int Count;
        public int Offset;

        public VertexAttribute(string name,int index, int count, int offset)
        {
            Name = name;
            Index = index;
            Count = count;
            Offset = offset;
        }
    }
}
