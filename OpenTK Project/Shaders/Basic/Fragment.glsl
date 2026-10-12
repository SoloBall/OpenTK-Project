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


void main(){
    float lightRadius = 400;
    float darkFactor = clamp(distance(fPos, cameraPos) / lightRadius, 0.0, 1.0);
    if (darkFactor > 0.9) {
        darkFactor = 0.9;
    }
    vec4 darkFragColor = fColor * (1 - darkFactor);
    if (dot(data[gl_PrimitiveID + triangleOffset].normal.xyz, sunDirection) > 0) 
    {
        float highestT = -1;
        for (int i = 0; i < data.length(); i++) 
        {
            if (i == gl_PrimitiveID + triangleOffset)
            {
                continue;
            }
            triangle triangle = data[i];
            vec3 planeOriginVector = triangle.a.xyz + triangle.origin.xyz;
            vec3 planeConnectionVector1 = triangle.b.xyz - planeOriginVector + triangle.origin.xyz;
            vec3 planeConnectionVector2 = triangle.c.xyz - planeOriginVector + triangle.origin.xyz;
            vec3 normal = triangle.normal.xyz;
            float tDenom = dot(normal, sunDirection);
            if (abs(tDenom) < 1e-8) continue; 
            float t = dot((planeOriginVector - fPos), normal) / tDenom;
            if (t < 1e-5)
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
            if (t > highestT) 
            {
                highestT = t;
            }
        }
        color = darkFragColor;
        if (highestT > 0) 
        {
            color = darkFragColor * clamp(0.3 + highestT / 50, 0.4f, 0.95f);
        }
    }
    else 
    {
        float nigga = dot(data[gl_PrimitiveID + triangleOffset].normal.xyz, sunDirection);
        color = darkFragColor * clamp(((1 - nigga*-1)), 0.4f, 1f);
    }
}