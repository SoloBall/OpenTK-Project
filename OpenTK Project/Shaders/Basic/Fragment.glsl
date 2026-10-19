#version 450 core

struct Triangle 
{
    vec4 a, b, c, normal, origin, minPoint, maxPoint, centroidPoint, color, orientation;
};
layout(std430, binding = 0) buffer Triangles
{
    Triangle data[];
};
in vec4 fColor;
in vec3 fPos;
uniform vec3 cameraPos;
uniform vec3 sunDirection;
uniform int triangleOffset;
uniform vec4 sunOrigin;
out vec4 color;


void main(){
    vec3 lightPos = cameraPos.xyz;
    float lightRadius = 10000;
    if (sunOrigin.w != 0)
    {
        lightPos = sunOrigin.xyz;
    }
    float darkFactor = clamp(distance(fPos, lightPos) / lightRadius, 0.0, 1.0);
    if (darkFactor > 0.9) {
        darkFactor = 0.9;
    }
    vec4 darkFragColor = fColor * (1 - darkFactor);
    vec3 directionToSun = vec3(0, 0, 0);
    if (sunOrigin.w != 0) 
    {
        directionToSun = sunOrigin.xyz - fPos;
        if (length(directionToSun) < 0.9) // a little more than the max distance inside the cube sun 
        {
            color = fColor;
            return;
        }
        directionToSun = normalize(directionToSun);
    }
    else  
    {
        directionToSun = sunDirection;
    }
    if (dot(data[gl_PrimitiveID + triangleOffset].normal.xyz, directionToSun) > 0 && dot(data[gl_PrimitiveID + triangleOffset].normal.xyz, cameraPos) > 0) 
    {
        float lowestT = 3.4e38;
        for (int i = 0; i < data.length(); i++) 
        {
            if (i == gl_PrimitiveID + triangleOffset)
            {
                continue;
            }
            Triangle triangle = data[i];
            if (sunOrigin.w != 0)
            {
                if (length(sunOrigin.xyz - triangle.origin.xyz)  <= 0.9f)
                {
                    continue;
                }
            }
            vec3 planeOriginVector = triangle.a.xyz + triangle.origin.xyz;
            vec3 planeConnectionVector1 = triangle.b.xyz - planeOriginVector + triangle.origin.xyz;
            vec3 planeConnectionVector2 = triangle.c.xyz - planeOriginVector + triangle.origin.xyz;
            vec3 normal = triangle.normal.xyz;
            float tDenom = dot(normal, directionToSun);
            if (abs(tDenom) < 1e-8) continue; 
            float t = dot((planeOriginVector - fPos), normal) / (tDenom);
            if (t < 1)
            {
                continue;
            }
            vec3 point = fPos + t * directionToSun;
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
            if (t < lowestT) 
            {
                lowestT = t;
            }
        }
        if (lowestT < 3.4e38) {
            color = darkFragColor * clamp(lowestT / 10, 0.4f, 0.95f);
        }
        else 
        {
            color = darkFragColor;
        }
    }
    else 
    {
        float nigga = dot(data[gl_PrimitiveID + triangleOffset].normal.xyz, directionToSun);
        color = darkFragColor * clamp(((1 - nigga*-1)), 0.4, 1);
    }
}