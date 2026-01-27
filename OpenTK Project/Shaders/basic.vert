
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