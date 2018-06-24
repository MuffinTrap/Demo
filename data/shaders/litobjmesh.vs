#version 130

// OpenGL 3.0 -> GLSL 1.30
// a projection transformation to apply to the vertex' position
uniform mat4 projectionMatrix;
uniform mat4 worldMatrix;
uniform mat4 viewMatrix;


// attributes of our vertex
in vec3 vPosition;
in vec3 vNormal;

out vec3 fNormal;
out vec3 fPosition;

void main()
{
	// gl_Position is a special variable of OpenGL that must be set
	 gl_Position =  projectionMatrix * viewMatrix * worldMatrix * vec4(vPosition, 1.0);
	 fNormal = vec3(worldMatrix * vec4(vNormal, 0.0f));
	 fPosition = vec3(worldMatrix * vec4(vPosition, 1.0));
}

