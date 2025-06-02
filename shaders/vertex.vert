#version 330 core

layout(location = 0) in vec3 aPos;
layout(location = 1) in vec3 aNormal;
layout(location = 2) in vec2 aTexCoords;
layout(location = 3) in vec3 aWorldPos;

uniform mat4 m;
uniform mat4 v;
uniform mat4 p;

out vec3 Normal;
out vec2 TexCoords;
out vec3 Color;

void main()
{
	Normal = aNormal;
	TexCoords = aTexCoords;
	gl_Position = p * v * m * vec4(aPos + aWorldPos, 1.0);

	float diff = max(dot(normalize(aNormal), vec3(0.3, 1.0, 0.3)), 0.0);

	vec3 color;

	if(aWorldPos.y > 58)
	{
		color = vec3(1.0);
	}
	else if(aWorldPos.y < 28)
	{
		color = vec3(0, 0.714, 1);
	}
	else
	{
		color = vec3(0.192, 0.671, 0.29);
	}

	vec3 ambient = vec3(0.3);

	Color = color * (diff + ambient);
}

