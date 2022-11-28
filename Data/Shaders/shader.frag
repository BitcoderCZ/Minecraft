#version 450 core

out vec4 FragColor;

in vec3 fUv;
in vec4 fCol;

layout (binding = 1) uniform sampler2DArray textures;

layout (location = 6) uniform float globalLight;
layout (location = 7) uniform float minGlobalLightLevel;
layout (location = 8) uniform float maxGlobalLightLevel;

void main()
{
    vec4 texColor = texture(textures, fUv, 0);

    if (texColor.a == 0.0) {
    	discard;
	}

    float shade = clamp(1.0 - (((maxGlobalLightLevel - minGlobalLightLevel) * globalLight + minGlobalLightLevel) * (fCol.a)), minGlobalLightLevel, maxGlobalLightLevel);

    texColor = mix(texColor, vec4(0.0, 0.0, 0.0, 1.0), shade);

	FragColor = texColor;
}