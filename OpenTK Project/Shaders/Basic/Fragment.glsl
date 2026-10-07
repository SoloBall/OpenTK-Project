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