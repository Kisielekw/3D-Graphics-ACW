#version 330

struct Light
{
	vec4 position;
	vec3 colour;
};

struct Material
{
	vec3 ambient;
	vec3 diffuse;
	vec3 specular;
	float shininess;
};

uniform Light uLights[3];
uniform Material uMaterial;

uniform vec4 uEyePosition;

uniform sampler2D uTexture;

in vec4 oNormal;
in vec4 oSurfacePosition;
in vec2 oTexCoords;

out vec4 FragColour;

void main()
{
	vec3 sumColour = vec3(0, 0, 0);
	
	vec4 eyeDir = normalize(uEyePosition - oSurfacePosition);
	
	for(int i = 0; i < 3; i++)
	{
		vec4 lightDir = normalize(uLights[i].position - oSurfacePosition);

		vec4 reflection = reflect(-lightDir, oNormal);

		float diffuse = max(dot(oNormal, lightDir), 0.0);
		float specular = pow(max(dot(reflection, eyeDir), 0.0), uMaterial.shininess);

		float ambient = 0.1;

		vec3 diffuseLight = uLights[i].colour * diffuse;
		vec3 specularLight = uLights[i].colour * specular;
		vec3 ambientLight = uLights[i].colour * ambient;

		sumColour += vec3(ambientLight * uMaterial.ambient + diffuseLight * uMaterial.diffuse + specularLight * uMaterial.specular);
	}

	FragColour = vec4(sumColour, 1);
}