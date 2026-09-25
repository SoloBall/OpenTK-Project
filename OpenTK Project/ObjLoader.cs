using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTK_Project
{
    public static class ObjLoader
    {
        public static (VertexPositionColor[] vertices, uint[] indices) Load( string path, Color4 color, Vector3 worldSpacePosition, float scaleX = 1, float scaleY = 1, float scaleZ = 1 )
        {
            var positions = new List<Vector3>();
            var indices = new List<int>();

            foreach ( var rawLine in File.ReadLines(path) )
            {
                var line = rawLine.Trim();
                if ( line.Length == 0 || line.StartsWith("#") ) continue;

                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                if ( parts[0] == "v" )
                { 
                    float x = float.Parse(parts[1], CultureInfo.InvariantCulture);
                    float y = float.Parse(parts[2], CultureInfo.InvariantCulture);
                    float z = float.Parse(parts[3], CultureInfo.InvariantCulture);
                    positions.Add(new Vector3(x, y, z));
                }
                else if ( parts[0] == "f" )
                {
                    var faceIndices = new List<int>();
                    for ( int i = 1; i < parts.Length; i++ )
                    {
                        // a face part can be "5", "5/2", "5/2/1", or "5//1" — we only need the position index
                        string posPart = parts[i].Split('/')[0];
                        int idx = int.Parse(posPart, CultureInfo.InvariantCulture);
                        if ( idx < 0 ) idx = positions.Count + idx + 1; // handle relative indices
                        faceIndices.Add(idx - 1); // OBJ is 1-based
                    }

                    // fan-triangulate (handles tris and quads)
                    for ( int i = 1; i < faceIndices.Count - 1; i++ )
                    {
                        indices.Add(faceIndices[0]);
                        indices.Add(faceIndices[i]);
                        indices.Add(faceIndices[i + 1]);
                    }
                }
            }

            var vertices = new VertexPositionColor[positions.Count];
            for ( int i = 0; i < positions.Count; i++ )
            {
                vertices[i] = new VertexPositionColor(positions[i] * new Vector3(scaleX, scaleY, scaleZ) + worldSpacePosition, color);
            }
            uint[] uintIndices = new uint[indices.Count];
            for (int i = 0; i < indices.Count; i++ )
            {
                uintIndices[i] = (uint)indices[i];
            }
            return (vertices, uintIndices);
        }
    }
}
