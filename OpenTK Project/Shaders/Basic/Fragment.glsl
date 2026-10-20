#version 450 core

struct Triangle 
{
    vec4 a, b, c, normal, origin, minPoint, maxPoint, centroidPoint, color, orientation;
};
struct Node
{
    Triangle Triangles[];
    int Left, Right, Parent, Self;
    vec4 Min, Max;
};

layout(std430, binding = 0) buffer Nodes
{
    Node nodes[];
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
    // if current primitive points in the same direction as either the sun or camera, as in dot() > 0, skip. If point is same direction as sun, go to other shadow part.
        float shortestRayLength = 3.4e38;
        Triangle triangle;
        int index = 0;
        bool hit = true;
        while (hit = true)
        {
            // ray origin = fpos, ray direction = directionToSun find scale
            vec3 t0 = (nodes[index].Min.xyz - fPos) / directionToSun;
            vec3 t1 = (nodes[index].Max.xyz - fPos) / directionToSun;

            vec3 tMin = min(t0, t1);
            vec3 tMax = max(t0, t1);

            float tEnter = max(max(tMin.x, tMin.y), tMin.z);
            float tExit  = min(min(tMax.x, tMax.y), tMax.z);

            bool hit = tEnter <= tExit && tExit >= 0.0;
            if (nodes[index].Triangles.length() > 0)
            {
                // whats the shortest intersecting triangle?
                // plane intersection test
                vec3 planeOriginVector = triangle.a.xyz + triangle.origin.xyz;
                vec3 planeConnectionVector1 = triangle.b.xyz - planeOriginVector + triangle.origin.xyz;
                vec3 planeConnectionVector2 = triangle.c.xyz - planeOriginVector + triangle.origin.xyz;
                vec3 normal = triangle.normal.xyz;
                float tDenom = dot(normal, directionToSun);
                if (abs(tDenom) < 1e-8) continue; 
                float rayLength = dot((planeOriginVector - fPos), normal) / (tDenom);
                if (rayLength < 1)
                {
                    continue;
                }
                vec3 point = fPos + rayLength * directionToSun;

                // triangle intersection test
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
                if (rayLength < shortestRayLength) 
                {
                    shortestRayLength = rayLength;
                }
            }
         }
    if (shortestRayLength < 3.4e38) {
        color = darkFragColor * clamp(shortestRayLength / 10, 0.4f, 0.95f);
    }
    else 
    {
        color = darkFragColor;
    }
}