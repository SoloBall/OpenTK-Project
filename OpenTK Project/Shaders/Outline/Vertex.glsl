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