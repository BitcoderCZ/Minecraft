#version 460 core

out vec4 FragColor;

in vec3 fUv;

layout (location = 5, binding = 1) uniform sampler2DArray textures;

void main()
{
    FragColor = texture(textures, fUv, 0);
}