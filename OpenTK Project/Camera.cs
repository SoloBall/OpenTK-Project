using OpenTK.Graphics.ES11;
using OpenTK.Mathematics;

namespace OpenTK_Project
{
    public class Camera
    {
        public Vector3 Front = Vector3.UnitX;
        public Vector3 Up = Vector3.UnitZ;
        public Vector3 Right => Vector3.Normalize(Vector3.Cross(Front, Up));


        public Vector3 Position = new(50000, 50000, 50000);
        public float Yaw = 0;
        public float Pitch = 0;

        public float speed = 5f;
        public float sensitivity = 0.1f;
        public float near = 0.1f;
        public float far = 100f;

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

        public Matrix4 GetMatrix()
        {
            return Matrix4.LookAt(Position, Position + Front, Up);
        }
    }
}
