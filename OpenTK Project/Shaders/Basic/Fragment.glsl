#version 450 core

struct triangle 
{
    vec4 a, b, c, normal, origin, color, orientation;
};
layout(std430, binding = 0) buffer Triangles
{
    triangle data[];
};
in vec4 fColor;
in vec3 fPos;
uniform vec3 cameraPos;
uniform vec3 sunDirection;
uniform int triangleOffset;
out vec4 color;

vec3 rotate(vec4 q, vec3 v) {
    vec3 t = 2.0 * cross(q.xyz, v);
    return v + q.w * t + cross(q.xyz, t);
}

void main(){
    /*float lightRadius = 400;
    float darkFactor = clamp(distance(fPos, cameraPos) / lightRadius, 0.0, 1.0);
    if (darkFactor > 0.9) {
        darkFactor = 0.9;
    }
    vec4 darkFragColor = fColor * (1 - darkFactor);
    color = darkFragColor;*/
    float darkFactor = dot(normalize(rotate(data[gl_PrimitiveID + triangleOffset].orientation, data[gl_PrimitiveID + triangleOffset].normal.xyz)), sunDirection) + 0.9;
    color = fColor * clamp(darkFactor, 0.2, 1);
}