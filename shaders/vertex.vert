#version 460 core

layout(location = 0) in vec3 aPos;
layout(location = 1) in vec3 aNormal;
layout(location = 2) in vec2 aTexCoords;
layout(location = 3) in int aType;

uniform mat4 m;
uniform mat4 v;
uniform mat4 p;

out vec3 Normal;
out vec2 TexCoords;
out vec4 Color;

void main()
{
	Normal = aNormal;
	TexCoords = aTexCoords;
	gl_Position = p * v * m * vec4(aPos, 1.0);

	float diff = max(dot(normalize(aNormal), vec3(0.3, 1.0, 0.3)), 0.0);

	vec4 color;

	if(aType == 2)
	{
		color = vec4(1.0);
	}
	else if(aType == 1)
	{
		color = vec4(0, 0.714, 1, 0.4);
	}
	else
	{
		color = vec4(0.192, 0.671, 0.29, 1.0);
	}

	float ambient = 0.3;

	Color = vec4(color.rgb * (diff + ambient), color.a);
}

