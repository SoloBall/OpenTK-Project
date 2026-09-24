using OpenTK.Mathematics;
using static OpenTK_Project.Game;

namespace OpenTK_Project
{
    public static class Mapper
    {
        public static void LoadSilentHill( List<Rectangle> objects, Vector3 position)
        {
            for ( int i = -10; i < 10; i++ )
            {
                for ( int j = -10; j < 10; j++ )
                {
                    Rectangle plane = new(10, 1000, 10, position + new Vector3(i * 100, j * 100, 1) - Vector3.UnitZ * 1010);
                    plane.Mesh = GrabMeshFromModels(plane.Color, plane.Position, "../../../Assets/Models/cube.obj", 10, 10, 1000);
                    objects.Add(plane);
                    for ( int k = 20; k < plane.Height; k++ )
                    {
                        if ( k % 20 == 0 )
                        {
                            Rectangle ring = new(12, 1, 12, plane.Position + Vector3.UnitZ * k);
                            ring.Mesh = GrabMeshFromModels(Color4.Red, ring.Position, "../../../Assets/Models/cube.obj", 12, 12, 1);
                            objects.Add(ring);
                        }
                    }
                }
            }
        }
        public static void LoadCube( List<Rectangle> objects, Vector3 position )
        {
            Rectangle plane = new(10, 1000, 10, position - Vector3.UnitZ * 1010);
            plane.Mesh = GrabMeshFromModels(plane.Color, plane.Position, "../../../Assets/Models/cube.obj", 10, 10, 1000);
            objects.Add(plane);
        }
    }
}
