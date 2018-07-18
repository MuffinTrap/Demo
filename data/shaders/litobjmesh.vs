#version 330

// OpenGL 3.0 -> GLSL 1.30
// a projection transformation to apply to the vertex' position
uniform mat4 projectionMatrix;
uniform mat4 worldMatrix;
uniform mat4 viewMatrix;
uniform vec3 lightPosition;


// attributes of our vertex
in vec3 vPosition;
in vec3 vNormal;

out vec3 fNormal;
out vec3 fPosition;
out vec3 fLightPosition;

void main()
{
	// gl_Position is a special variable of OpenGL that must be set
	 gl_Position =  projectionMatrix * viewMatrix * worldMatrix * vec4(vPosition, 1.0);
	 fNormal = mat3(transpose(inverse(viewMatrix * worldMatrix))) * vNormal;
	 fPosition = vec3(viewMatrix * worldMatrix * vec4(vPosition, 1.0));
	 fLightPosition = vec3(viewMatrix * vec4(lightPosition, 1.0f));
}

