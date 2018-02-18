#version 130

// OpenGL 3.0 -> GLSL 1.30

uniform mat4 worldMatrix;
uniform mat4 viewMatrix;
uniform mat4 projectionMatrix;

// attributes of our vertex
in vec3 vPosition;
out vec3 fDiffuseColor;

void main()
{
	// gl_Position is a special variable of OpenGL that must be set
	 gl_Position =  projectionMatrix * viewMatrix * worldMatrix * vec4(vPosition, 1.0);
	 vec4 transformed = worldMatrix * vec4(vPosition, 1.0);
	 fDiffuseColor = transformed.xyz;
}

