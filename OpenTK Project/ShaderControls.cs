using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTK_Project
{
    internal static class ShaderControls
    {
        public static string  GetVertexShaderSource( )
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
                float lightRadius = 40;
                float darkFactor = clamp(distance(fPos, cameraPos) / lightRadius, 0.0, 1.0);
                if (darkFactor < 0.1) {
                    darkFactor = 0.1;
                }
                vec4 darkFragColor = fColor * (1 - darkFactor);
                color = (floor(darkFragColor * 16) / 16);
            }
            ";
            return source;
        }
        public static string GetScreenVertexShaderSource( )
        {
            string source =
                @"
            #version 450 core

            layout (location = 0) in vec2 vPosition;
            
            struct RectangleInstance {
                float Length;
                float Height;
                float Width;
                vec3 Position;
                vec4 Color;
                float Selected;
            };
            
            layout(std430, binding = 0) buffer InstanceBuffer {
                RectangleInstance data[];
            };

            out vec2 uv;
            out float selected;
            out vec3 rectanglePosition;

            void main() {
                RectangleInstance rectangle = data[gl_InstanceID];

                uv = vPosition * 0.5 + 0.5;
                gl_Position = vec4(vPosition, 0.0, 1.0);

                selected = rectangle.Selected;
                rectanglePosition = rectangle.Position;
            }
            ";
            return source;
        }
        public static string GetScreenFragmentShaderSource( )
        {
            string source =
                @"
            #version 450 core

            uniform sampler2D sceneTexture;
            uniform sampler2D depthTexture;
            uniform vec3 cameraPos;
            uniform vec2 screenSize;
            uniform vec3 outlineColor;
            uniform float nearPlane;
            uniform float farPlane;
            in vec2 uv;
            in float selected;
            in vec3 rectanglePosition;
            out vec4 fragColor;

            float linearDepth(float raw) {
                float z = raw * 2.0 - 1.0; // drop this if your projection already outputs 0..1 NDC directly
                return (2.0 * nearPlane * farPlane) / (farPlane + nearPlane - raw * (farPlane - nearPlane));
            }

            void main(){
                vec3 sceneColor = texture(sceneTexture, uv).rgb;
                vec2 pixel = 1.0 / screenSize;

               float center = linearDepth(texture(depthTexture, uv).r);
               float right  = linearDepth(texture(depthTexture, uv + vec2(pixel.x, 0)).r);
               float left   = linearDepth(texture(depthTexture, uv + vec2(-pixel.x, 0)).r);
               float up     = linearDepth(texture(depthTexture, uv + vec2(0, pixel.y)).r);
               float down   = linearDepth(texture(depthTexture, uv + vec2(0, -pixel.y)).r);


                float difference =
                    abs(center - right) + 
                    abs(center - left) + 
                    abs(center - up) + 
                    abs(center - down);

                float outline = difference  > 0.1 ? 1.0 : 0.0;
                float dist = (2.0 * nearPlane * farPlane) / (farPlane + nearPlane - center * (farPlane - nearPlane));
                if (center > 50.0) {
                    outline = 0.0;
                }
                vec3 outlineColorNew = selected > 0.5 ? vec3(0.0, 1.0, 1.0) : outlineColor;
                vec3 final = mix(sceneColor, outlineColorNew, outline);
                fragColor = vec4(final, 1.0);
            }
            ";
            return source;
        }
    }
}
