#version 330

uniform mat4 uViewMatrix;
uniform mat4 uModelMatrix;
uniform mat4 uProjectionMatrix;

in vec3 vPosition;
in vec3 vNormal;
in vec2 vTexCoords;

out vec4 oNormal;
out vec4 oSurfacePosition;
out vec2 oTexCoords;

void main()
{
	gl_Position = vec4(vPosition, 1) * uModelMatrix * uViewMatrix * uProjectionMatrix;

	oSurfacePosition = vec4(vPosition, 1) * uModelMatrix * uViewMatrix;
	oNormal = vec4(normalize(vNormal * mat3(transpose(inverse(uModelMatrix * uViewMatrix)))), 1);

	oTexCoords = vTexCoords;
}