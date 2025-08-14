using OpenTK.Graphics.ES11;
using OpenTK.Mathematics;

namespace OpenTK_Project
{
    public class Rectangle
    {
        public int Length, Width, Height;
        public Vector3 Position;
        public Color4 Color;

        public Rectangle(int length, int width, int height, Vector3? position = null, Color4? color = null)
        {
            Length = length;
            Width = width;
            Height = height;
            Position = position ?? new Vector3 (0, 0, 0);
            Color = color ?? new Color4(1f, 1f, 1f, 1f);
        }
    }
}
