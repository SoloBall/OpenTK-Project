using OpenTK.Graphics.ES11;
using OpenTK.Mathematics;
using System.Reflection.Metadata.Ecma335;

namespace OpenTK_Project
{
    public class Camera
    {
        public Vector3 Front = Vector3.UnitX;
        public Vector3 Up = Vector3.UnitZ;
        public Vector3 Right => Vector3.Normalize(Vector3.Cross(Front, Up));


        public Vector3 Position = new(0, 0, 0);
        public float Yaw = 0;
        public float Pitch = 0;

        public float near = 0.1f;
        public float far = 10000f;

        public float speed = 5f;
        public float sensitivity = 0.1f;

        public void UpdateDirection()
        {
            float yawRads = MathHelper.DegreesToRadians(Yaw);
            float pitchRads = MathHelper.DegreesToRadians(Pitch);

            Vector3 direction = new(
                MathF.Cos(yawRads) * MathF.Cos(pitchRads),
                MathF.Cos(pitchRads) * MathF.Sin(yawRads),
                MathF.Sin(pitchRads)
                );

            Front = direction.Normalized();
        }

        // proposed position is calculating from min, change to interprit the entirety of the rectangle
        public bool IsLookingAtRectangle(Rectangle rectangle)
        {
            Vector3 proposedRectanglePosition = Position + Vector3.Distance(rectangle.Position, Position) * Front;
            Vector3 min = rectangle.Position;
            Vector3 max = min + new Vector3(rectangle.Length, rectangle.Width, rectangle.Height);

            if (proposedRectanglePosition.X > min.X && proposedRectanglePosition.X < max.X )
            {
                if (proposedRectanglePosition.Y > min.Y && proposedRectanglePosition.Y < max.Y )
                {
                    if (proposedRectanglePosition.Z > min.Z && proposedRectanglePosition.Z < max.Z )
                    {
                        return true;
                    }
                }
            }

            return false;

        }
        public Matrix4 GetMatrix()
        {
            return Matrix4.LookAt(Position, Position + Front, Up);
        }
    }
}
