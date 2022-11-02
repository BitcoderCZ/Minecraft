#version 460 core
layout (location = 0) in vec3 aPosition;

layout (location = 0) out vec3 fragCoord;

layout (location = 0) uniform mat4 uTransform;
layout (location = 1) uniform mat4 uProjection;
layout (location = 2) uniform mat4 uView;

void main()
{
    fragCoord = (uView * vec4(aPosition, 1.0)).xyz;
    gl_Position = uProjection * uView * (uTransform * vec4(aPosition, 1.0));
}