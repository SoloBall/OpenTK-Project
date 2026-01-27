
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

void main(){
    vec3 sceneColor = texture(sceneTexture, uv).rgb;
    float center = length(texture(depthTexture, uv).r);
    vec2 pixel = 1.0 / screenSize;

    float outline = 1;
    float dist = (2.0 * nearPlane * farPlane) / (farPlane + nearPlane - center * (farPlane - nearPlane));
    if (dist > 0.2) {
        outline = 0.0;
    }
    vec3 outlineColorNew = outlineColor;
    if (selected > 0.5){
        outlineColorNew = vec3(1, 1, 0);
    }
    vec3 final = mix(sceneColor, outlineColorNew, outline);
    fragColor = vec4(final, 1);
}