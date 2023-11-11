#version 330

uniform mat4 uViewMatrix;
uniform mat4 uModelMatrix;
uniform mat4 uProjectionMatrix;

uniform vec3 uLightDirection;

in vec3 vPosition;
in vec3 vNormal;

out vec4 oColour;

void main()
{
	gl_Position = vec4(vPosition, 1) * uModelMatrix * uViewMatrix * uProjectionMatrix; 
	vec3 inverseTransposeNormal = normalize(vNormal * mat3(transpose(inverse(uModelMatrix * uViewMatrix))));
	vec3 lightDir = normalize(-uLightDirection * mat3(uViewMatrix));
	oColour = vec4(vec3(max(dot(inverseTransposeNormal, lightDir), 0)), 1);
}