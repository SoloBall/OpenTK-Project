using OpenTK.Graphics.ES11;
using OpenTK.Mathematics;

namespace OpenTK_Project
{
    // position is in mesh due to redundancy cleanup
    public class SceneObject
    {
        public Vector3 Scale;
        public Color4 Color;
        public bool Selected;
        public bool Dirty;
        public Mesh Mesh;

        public SceneObject(Vector3 scale, Vector3 position, Color4? color = null, bool selected = false, bool dirty = true, string modelURL = "cube" )
        {
            Scale = scale;
            Color = color ?? new Color4(1f, 1f, 1f, 1f);
            Selected = selected;
            Dirty = dirty;
            Mesh = GrabMeshFromModels(position, $"../../../Assets/Models/{modelURL}.obj");
         }
        private Mesh GrabMeshFromModels(Vector3 position, string path = "../../../Assets/Models/Entity/cow.obj")
        {
            var (vertices, indices) = ObjLoader.Load(path, Color);
            Mesh mesh = new(vertices, indices, position, Scale);
            return mesh;
        }
        // is missing radius
        public bool CollidesWithSphere(Vector3 sphereCenter, float radius)
        {
            Vector3 min = Mesh.LocalOrigin;
            Vector3 max = Mesh.LocalOrigin + Scale;

            bool result = true;
            if ( min.X > sphereCenter.X || sphereCenter.X > max.X || min.Y > sphereCenter.Y || sphereCenter.Y > max.Y || min.Z > sphereCenter.Z || sphereCenter.Z > max.Z)
            {
                result = false;
            }
            return result;
        }
    }
}
