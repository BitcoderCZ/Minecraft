#version 460 core

out vec4 FragColor;

layout (location = 0) in vec2 fUv;

layout (location = 5, binding = 0) uniform sampler2D uTexture;

void main()
{
    FragColor = texture(uTexture, fUv);
}