using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTK_Project
{
    internal static class ShaderControls
    {
        public static string GetVertexShaderSource( )
        {
            string source =
                @"
            #version 450 core

            layout (location = 0) in vec3 vPosition;
            layout (location = 1) in vec4 vColor;

            uniform mat4 projection; 
            uniform mat4 view;
            uniform mat4 model;

            out vec4 fColor;
            out vec3 fPos;

            void main() {
                fColor = vColor;
                gl_Position = projection * view * model * vec4(vPosition, 1.0);
                fPos = vPosition;
            }
            ";
            return source;
        }

        public static string GetFragmentShaderSource( )
        {
            string source =
               @"
            #version 450 core

            in vec4 fColor;
            in vec3 fPos;
            uniform vec3 cameraPos;
            out vec4 color;

            void main(){
                float lightRadius = 400;
                float darkFactor = clamp(distance(fPos, cameraPos) / lightRadius, 0.0, 1.0);
                if (darkFactor > 0.9) {
                    darkFactor = 0.9;
                }
                vec4 darkFragColor = fColor * (1 - darkFactor);
                color = darkFragColor;
            }
            ";
            return source;
        }

        public static string GetOutlineVertexShaderSource( )
        {
            string source = @"
                #version 330 core
                layout(location = 0) in vec3 aPosition;

                uniform mat4 uModel;
                uniform mat4 uView;

                out vec3 vViewPos;

                void main()
                {
                    vViewPos = vec3(uView * uModel * vec4(aPosition, 1.0));
                    // Projection is applied later, in the geometry shader, after we've
                    // built the thickened line quads in view space.
                    gl_Position = vec4(vViewPos, 1.0);
                }
                ";
            return source;
        }
        public static string GetOutlineGeometryShaderSource( )
        {
            string source = @"            

                #version 330 core
// Draw the mesh with PrimitiveType.TrianglesAdjacency and the index buffer
            // from AdjacencyMeshBuilder for this to receive 6 vertices per triangle.
            layout(triangles_adjacency) in;
            layout(triangle_strip, max_vertices = 12) out;

            in vec3 vViewPos[6];

            // ""clip space"" -> real 3d space with distance. It's distorted to look more 3d ish but it fucks math up
            uniform mat4 uProjection;

            uniform float uEdgeThickness;     // in view-space units, tune per scene scale
            uniform float uCreaseCosThreshold; // e.g. cos(35 degrees) ~= 0.82; lower = more sensitive

            vec3 faceNormal(vec3 a, vec3 b, vec3 c)
            {
                return normalize(cross(b - a, c - a));
            }

            // might want to make edgethickness relative to distance, as line size is otherwise not(i think) relative
            void emitEdgeQuad(vec3 p0, vec3 p1)
            {
                // Build a small quad along the edge, facing roughly toward the camera,
                // so it reads as a thick line from any angle.
                // viewdir is basically just the vector from the middle of the edge, to the camera
                vec3 viewDir = normalize(-(p0 + p1));
                vec3 edgeDir = normalize(p1 - p0);
                // edgedir alone would work, but we need to cross it with the camera's viewdir to make sure the outline is relative to the camera's position too. Otherwise, it'd probably mimic a jpg or something
                vec3 sideDir = normalize(cross(edgeDir, viewDir));
                vec3 offset = sideDir * uEdgeThickness;

                // the following just makes a rectangle... offset is just half of the height, Take a line, put an amount of border on it (offset in one direction, offset in the other) and you get a thicker line.
                // basically, p0 + offset is one corner and p1 - offset is the opposite corner
                gl_Position = uProjection * vec4(p0 + offset, 1.0); EmitVertex();
                gl_Position = uProjection * vec4(p0 - offset, 1.0); EmitVertex();
                gl_Position = uProjection * vec4(p1 + offset, 1.0); EmitVertex();
                gl_Position = uProjection * vec4(p1 - offset, 1.0); EmitVertex();

                // splits off the newly created vertices, so that the next set doesn't mix in with the previous set
                EndPrimitive();
            }

            void main()
            {
                // Matches the layout from AdjacencyMeshBuilder:
                // 0=v0, 1=adj across (v0,v1), 2=v1, 3=adj across (v1,v2), 4=v2, 5=adj across (v2,v0)
                vec3 p0 = vViewPos[0];
                vec3 p1 = vViewPos[1];
                vec3 p2 = vViewPos[2];
                vec3 p3 = vViewPos[3];
                vec3 p4 = vViewPos[4];
                vec3 p5 = vViewPos[5];

                vec3 mainN = faceNormal(p0, p2, p4);

                // Edge (p0, p2), neighbor triangle formed with p1
                vec3 n1 = faceNormal(p0, p1, p2);
                bool crease1 = acos(dot(mainN, n1)/(length(mainN)*length(n1))) > uCreaseCosThreshold;

                // delete silhouette if edges are too big, or at least change it so it doesn't use sign, as there are edge cases where it'd fuck up
                bool silhouette1 = sign(dot(mainN, -p0)) != sign(dot(n1, -p0));
                if (crease1 || silhouette1) emitEdgeQuad(p0, p2);

                // Edge (p2, p4), neighbor triangle formed with p3
                vec3 n2 = faceNormal(p2, p3, p4);
                bool crease2 = acos(dot(mainN, n2)) > uCreaseCosThreshold;
                bool silhouette2 = sign(dot(mainN, -p2)) != sign(dot(n2, -p2));
                if (crease2 || silhouette2) emitEdgeQuad(p2, p4);

                // Edge (p4, p0), neighbor triangle formed with p5
                vec3 n3 = faceNormal(p4, p5, p0);
                bool crease3 = acos(dot(mainN, n3)) > uCreaseCosThreshold;
                bool silhouette3 = sign(dot(mainN, -p4)) != sign(dot(n3, -p4));
                if (crease3 || silhouette3) emitEdgeQuad(p4, p0);
            }
";
            return source;
        }
        public static string GetOutlineFragmentShaderSource( )
        {
            string source = @"
                #version 330 core
                out vec4 FragColor;

                uniform vec3 uOutlineColor;

                void main()
                {
                    FragColor = vec4(uOutlineColor, 1.0);
                }
                ";
            return source;
        }
    }
}
