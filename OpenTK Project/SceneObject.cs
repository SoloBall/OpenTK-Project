using OpenTK.Graphics.ES11;
using OpenTK.Mathematics;

namespace OpenTK_Project
{
    public class SceneObject
    {
        public int ScaleX, ScaleZ, ScaleY;
        public Vector3 Position;
        public Color4 Color;
        public bool Selected;
        public bool Dirty;
        public Mesh Mesh;

        public SceneObject(int scaleX, int scaleY, int scaleZ, Vector3? position = null, Color4? color = null, bool selected = false, bool dirty = true )
        {
            ScaleX = scaleX;
            ScaleY = scaleY;
            ScaleZ = scaleZ;
            ScaleZ = scaleZ;
            Position = position ?? new Vector3(0, 0, 0);
            Color = color ?? new Color4(1f, 1f, 1f, 1f);
            Selected = selected;
            Dirty = dirty;
         }
        // is missing radius
        public bool CollidesWithSphere(Vector3 sphereCenter, float radius)
        {
            Vector3 min = Position;
            Vector3 max = Position + new Vector3(ScaleX, ScaleZ, ScaleY);

            bool result = true;
            if ( min.X > sphereCenter.X || sphereCenter.X > max.X || min.Y > sphereCenter.Y || sphereCenter.Y > max.Y || min.Z > sphereCenter.Z || sphereCenter.Z > max.Z)
            {
                result = false;
            }
            return result;
        }
    }
}
