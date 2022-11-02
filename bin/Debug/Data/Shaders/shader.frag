#version 460 core

out vec4 FragColor;

layout (location = 0) in vec3 fragCoord;

void main()
{
    FragColor = vec4(fragCoord, 1.0);
}