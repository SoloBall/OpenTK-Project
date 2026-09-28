using OpenTK.Mathematics;
using static OpenTK_Project.Game;

namespace OpenTK_Project
{
    public static class Mapper
    {
        public static void LoadSilentHill( List<SceneObject> objects, Vector3 position)
        {
            for ( int i = -10; i < 10; i++ )
            {
                for ( int j = -10; j < 10; j++ )
                {
                    SceneObject plane = new(new Vector3(10, 1000, 10), position + new Vector3(i * 100, j * 100, 1) - Vector3.UnitZ * 1010);
                    objects.Add(plane);
                    for ( int k = 20; k < plane.Scale.Y; k++ )
                    {
                        if ( k % 20 == 0 )
                        {
                            SceneObject ring = new(new Vector3(12, 1, 12), plane.Mesh.LocalOrigin + Vector3.UnitZ * k);
                            objects.Add(ring);
                        }
                    }
                }
            }
        }
        public static void LoadCube( List<SceneObject> objects, Vector3 position )
        {
            SceneObject plane = new(new Vector3(10, 10, 1000), position);
            objects.Add(plane);
        }
    }
}
