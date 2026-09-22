using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTK_Project
{
    // Converts a standard triangle index buffer into the layout OpenGL expects
    // for PrimitiveType.TrianglesAdjacency: 6 indices per triangle instead of 3.
    //
    // Per triangle, the layout is: v0, adj01, v1, adj12, v2, adj20
    //   - v0, v1, v2   = the actual triangle's own vertices (drawn/visible)
    //   - adj01        = the far vertex of the neighboring triangle across edge (v0,v1)
    //   - adj12        = the far vertex of the neighboring triangle across edge (v1,v2)
    //   - adj20        = the far vertex of the neighboring triangle across edge (v2,v0)
    //
    // This must match the vertex order the geometry shader expects (see outline.geom).
    public static class AdjacencyMeshBuilder
    {
        public static uint[] BuildAdjacencyIndices( uint[] triangleIndices )
        {
            int triCount = triangleIndices.Length / 3;

            // Maps an unordered edge -> list of (triangle index, opposite vertex) that use it.
            // For a closed/manifold mesh each edge should be used by exactly 2 triangles.
            var edgeMap = new Dictionary<(uint, uint), List<(int tri, uint opp)>>();

            static (uint, uint) Key( uint a, uint b ) => a < b ? (a, b) : (b, a);

            void AddEdge( uint a, uint b, uint opp, int tri )
            {
                var key = Key(a, b);
                if ( !edgeMap.TryGetValue(key, out var list) )
                    edgeMap[key] = list = new List<(int, uint)>();
                list.Add((tri, opp));
            }

            for ( int t = 0; t < triCount; t++ )
            {
                uint i0 = triangleIndices[t * 3 + 0];
                uint i1 = triangleIndices[t * 3 + 1];
                uint i2 = triangleIndices[t * 3 + 2];

                AddEdge(i0, i1, i2, t);
                AddEdge(i1, i2, i0, t);
                AddEdge(i2, i0, i1, t);
            }

            uint FindOpposite( uint a, uint b, int tri, uint selfOpp )
            {
                var key = Key(a, b);
                foreach ( var (t, opp) in edgeMap[key] )
                {
                    if ( t != tri ) return opp;
                }
                // No second triangle on this edge -> open/boundary edge (e.g. a flat plane's rim).
                // Duplicating the triangle's own opposite vertex makes the geometry shader's
                // crease/silhouette test degenerate to "same normal on both sides", i.e. it
                // will NOT auto-outline open boundary edges. If you have open meshes you want
                // outlined on their rim, special-case this (e.g. sentinel index + always-draw
                // flag) rather than relying on the crease test.
                return selfOpp;
            }

            var result = new uint[triCount * 6];
            for ( int t = 0; t < triCount; t++ )
            {
                uint i0 = triangleIndices[t * 3 + 0];
                uint i1 = triangleIndices[t * 3 + 1];
                uint i2 = triangleIndices[t * 3 + 2];

                uint adj01 = FindOpposite(i0, i1, t, i2);
                uint adj12 = FindOpposite(i1, i2, t, i0);
                uint adj20 = FindOpposite(i2, i0, t, i1);

                result[t * 6 + 0] = i0;
                result[t * 6 + 1] = adj01;
                result[t * 6 + 2] = i1;
                result[t * 6 + 3] = adj12;
                result[t * 6 + 4] = i2;
                result[t * 6 + 5] = adj20;
            }

            return result;
        }
    }
}
