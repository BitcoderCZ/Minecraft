#version 460 core

out vec4 FragColor;

layout (location = 0) in vec2 fUv;

uniform sampler2D uTexture;

void main()
{
    FragColor = /*vec4(fUv.x, fUv.y, 0.0, 0.0);*/texture(uTexture, fUv);
}