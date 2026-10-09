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
    vec4 n = normalize(q);
    vec3 t = 2.0 * cross(n.xyz, v);
    return v + n.w * t + cross(n.xyz, t);
}



void main(){
    float lightRadius = 400;
    float darkFactor = clamp(distance(fPos, cameraPos) / lightRadius, 0.0, 1.0);
    if (darkFactor > 0.9) {
        darkFactor = 0.9;
    }
    float shadowFactor = 1;
    vec4 darkFragColor = fColor * (1 - darkFactor);
    for (int i = 0; i < data.length(); i++) 
    {
        if (i == gl_PrimitiveID + triangleOffset)
        {
            continue;
        }
        triangle triangle = data[i];
        vec3 planeOriginVector = rotate(vec4(triangle.orientation.xyz, triangle.orientation.w), triangle.a.xyz) + triangle.origin.xyz;
        vec3 planeConnectionVector1 = rotate(vec4(triangle.orientation.xyz, triangle.orientation.w), triangle.b.xyz) - planeOriginVector + triangle.origin.xyz;
        vec3 planeConnectionVector2 = rotate(vec4(triangle.orientation.xyz, triangle.orientation.w), triangle.c.xyz) - planeOriginVector + triangle.origin.xyz;
        if (dot(sunDirection, triangle.normal.xyz) == 0)
        {
            continue;
        }
        vec3 normal = cross(planeConnectionVector1, planeConnectionVector2);
        float tDenom = dot(normal, sunDirection);
        if (abs(tDenom) < 1e-8) continue; 
        float t = dot((planeOriginVector - fPos), normal) / tDenom;
        if (t < 0)
        {
            continue;
        }
        vec3 point = fPos + t * sunDirection;
        vec3 v0 = planeConnectionVector1;
        vec3 v1 = planeConnectionVector2;
        vec3 v2 = point - planeOriginVector;

        float d00 = dot(v0, v0);
        float d01 = dot(v0, v1);
        float d11 = dot(v1, v1);
        float d20 = dot(v2, v0);
        float d21 = dot(v2, v1);

        float denom = d00 * d11 - d01 * d01; // zero only for a truly degenerate triangle

        float scaler1 = (d11 * d20 - d01 * d21) / denom;
        float scaler2 = (d00 * d21 - d01 * d20) / denom;

        if (scaler1 < 0.0 || scaler2 < 0.0 || scaler1 + scaler2 > 1.0 || denom == 0)
        {
            continue;
        }
        shadowFactor = 2 - t;
    }
    color = darkFragColor * clamp(shadowFactor, 0.4, 1);
}