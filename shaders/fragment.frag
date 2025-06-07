#version 460 core 

out vec4 FragColor;

in vec3 Normal;
in vec4 Color;
in vec2 TexCoords;

void main()
{
	FragColor = Color;
}