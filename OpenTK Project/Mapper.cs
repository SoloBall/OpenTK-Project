using OpenTK.Mathematics;
using System.Text.Json;
using System.Text.Json.Serialization;
using static OpenTK_Project.Game;

namespace OpenTK_Project
{
    public static class Mapper
    {
        private static JsonSerializerOptions Options = new JsonSerializerOptions
        {
            IncludeFields = true,
            WriteIndented = true,
        };
        public static List<SceneObject> CreateSilentHill()
        {
            List<SceneObject> objects = new List<SceneObject>();
            Vector3 position = Vector3.Zero;
            SceneObject plane = new(new Vector3(1000, 1000, 0.1f), position, new Color4(0.15f, 0.15f, 0.15f, 1));
            objects.Add(plane);
            for ( int i = -10; i < 10; i++ )
            {
                for ( int j = -10; j < 10; j++ )
                {
                    SceneObject tower = new(new Vector3(10, 10, 100), position + new Vector3(i * 100, j * 100, 100));
                    objects.Add(tower);
                    for ( int k = 20; k < tower.Scale.Z*2; k += 20 )
                    {
                        SceneObject window0 = new(new Vector3(10.5f, 6f, 6), tower.Mesh.LocalOrigin - Vector3.UnitZ * tower.Mesh.LocalOrigin.Z + Vector3.UnitZ * k, color: Color4.LightBlue);
                        SceneObject window1 = new(new Vector3(6f, 10.5f, 6), tower.Mesh.LocalOrigin - Vector3.UnitZ * tower.Mesh.LocalOrigin.Z + Vector3.UnitZ * k, color: Color4.LightBlue);
                        objects.Add(window0);
                        objects.Add(window1);
                    }
                }
            }
            return objects;
        }
        public static List<SceneObject> CreateRectangle()
        {
            List<SceneObject> objects = new List<SceneObject>();
            Vector3 position = -Vector3.UnitZ * 1000;
            SceneObject plane = new(new Vector3(10, 10, 1000), position);
            objects.Add(plane);
            return objects;
        }
    }
}
