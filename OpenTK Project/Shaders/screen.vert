
#version 450 core

layout (location = 0) in vec2 vPosition;
            
struct RectangleInstance {
    float Length;
    float Height;
    float Width;
    vec3 Position;
    vec4 Color;
    float Selected;
};
            
layout(std430, binding = 0) buffer InstanceBuffer {
    RectangleInstance data[];
};

out vec2 uv;
out float selected;
out vec3 rectanglePosition;

void main() {
    RectangleInstance rectangle = data[gl_InstanceID];

    uv = vPosition * 0.5 + 0.5;
    gl_Position = vec4(vPosition, 1.0, 1.0);
    // wallah wrong
    selected = rectangle.Selected;
    rectanglePosition = rectangle.Position;
}