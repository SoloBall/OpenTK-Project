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
            #version 330 core

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
        // make colors fun
        public static string GetFragmentShaderSource( )
        {
            string source =
               @"
            #version 330 core

            in vec4 fColor;
            in vec3 fPos;
            uniform vec3 cameraPos;
            out vec4 color;

            void main(){
                float lightRadius = 40;
                float darkFactor = clamp(distance(fPos, cameraPos) / lightRadius, 0.0, 1.0);
                if (darkFactor < 0.3) {
                    darkFactor = 0.0;
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
            #version 330 core

            layout (location = 0) in vec2 vPosition;
            out vec2 uv;

            void main() {
                uv = vPosition * 0.5 + 0.5;
                gl_Position = vec4(vPosition, 0.0, 1.0);
            }
            ";
            return source;
        }
        public static string GetScreenFragmentShaderSource( )
        {
            string source =
                @"
            #version 330 core

            uniform sampler2D sceneTexture;
            uniform sampler2D depthTexture;
            uniform vec3 cameraPos;
            uniform vec2 screenSize;
            uniform vec3 outlineColor;
            uniform float nearPlane;
            uniform float farPlane;
            in vec2 uv;
            out vec4 fragColor;

            void main(){
                vec3 sceneColor = texture(sceneTexture, uv).rgb;
                float center = length(texture(depthTexture, uv).rgb);
                vec2 pixel = 1.0 / screenSize;

                float right = length(texture(depthTexture, uv + vec2(pixel.x, 0)).rgb);
                float left = length(texture(depthTexture, uv + vec2(-pixel.x, 0)).rgb);
                float up = length(texture(depthTexture, uv + vec2(0, pixel.y)).rgb);
                float down = length(texture(depthTexture, uv + vec2(0, -pixel.y)).rgb);
                float rightUp = texture(depthTexture, uv + vec2(pixel.x, pixel.y)).r;
                float rightDown = texture(depthTexture, uv + vec2(pixel.x, -pixel.y)).r;
                float leftUp = texture(depthTexture, uv + vec2(-pixel.x, pixel.y)).r;
                float leftDown = texture(depthTexture, uv + vec2(-pixel.x, -pixel.y)).r;

                float difference =
                    abs(center - right) + 
                    abs(center - left) + 
                    abs(center - up) + 
                    abs(center - down);

                float outline = difference  > 0.004 ? 1.0 : 0.0;
                float dist = (2.0 * nearPlane * farPlane) / (farPlane + nearPlane - center * (farPlane - nearPlane));
                if (dist > 0.2) {
                    outline = 0.0;
                }
                //vec3 outlineColorNew = vec3(1 - sceneColor.r, 1 - sceneColor.g, 1 - sceneColor.b);
                vec3 final = mix(sceneColor, outlineColor, outline);
                fragColor = vec4(final, 1);
            }
            ";
            return source;
        }
    }
}
