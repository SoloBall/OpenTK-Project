using OpenTK.Graphics.ES11;
using OpenTK.Mathematics;

namespace OpenTK_Project
{
    public class Rectangle
    {
        public int Length, Width, Height;
        public Vector3 Position;
        public Color4 Color;
        public bool Selected;

        public Rectangle(int length, int height, int width, Vector3? position = null, Color4? color = null, bool selected = false )
        {
            Length = length;
            Width = width;
            Height = height;
            Width = width;
            Position = position ?? new Vector3(0, 0, 0);
            Color = color ?? new Color4(1f, 1f, 1f, 1f);
            Selected = selected;
         }
        // is missing radius
        public bool CollidesWithSphere(Vector3 sphereCenter, float radius)
        {
            Vector3 min = Position;
            Vector3 max = Position + new Vector3(Length, Width, Height);

            bool result = true;
            if ( min.X > sphereCenter.X || sphereCenter.X > max.X || min.Y > sphereCenter.Y || sphereCenter.Y > max.Y || min.Z > sphereCenter.Z || sphereCenter.Z > max.Z)
            {
                result = false;
            }
            return result;
            /*Vector3 min = Position;
            Vector3 max = Position + new Vector3(Length, Width, Height);

            Console.Write("sphere: ");
            Console.WriteLine(sphereCenter);
            Console.Write("Square min: ");
            Console.WriteLine(min);
            Console.Write("square max: ");
            Console.WriteLine(max);
            Console.WriteLine();
            float x = Math.Clamp(sphereCenter.X, min.X, max.X);
            float y = Math.Clamp(sphereCenter.Y, min.Y, max.Y);
            float z = Math.Clamp(sphereCenter.Z, min.Z, max.Z);

            float distanceSquared =
                MathF.Pow((x - sphereCenter.X), 2) +
                MathF.Pow((y - sphereCenter.Y), 2) +
                MathF.Pow((z - sphereCenter.Z), 2);

            return distanceSquared < MathF.Pow(radius, 2);*/
        }
    }
}
