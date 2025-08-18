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
        public bool CollidesWithSphere(Vector3 sphereCenter, float radius)
        {
            Vector3 min = Position;
            Vector3 max = Position + new Vector3(Length, Width, Height);

            float x = Math.Clamp(sphereCenter.X, min.X, max.X);
            float y = Math.Clamp(sphereCenter.Y, min.Y, max.Y);
            float z = Math.Clamp(sphereCenter.Z, min.Z, max.Z);

            float distanceSquared =
                MathF.Pow((x - sphereCenter.X), 2) +
                MathF.Pow((y - sphereCenter.Y), 2) +
                MathF.Pow((z - sphereCenter.Z), 2);

            return distanceSquared < MathF.Pow(radius, 2);
        }
    }
}
