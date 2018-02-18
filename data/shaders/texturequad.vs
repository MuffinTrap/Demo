#version 130

// OpenGL 3.0 -> GLSL 1.30

uniform mat4 projectionMatrix;
uniform mat4 worldMatrix;
uniform mat4 viewMatrix;


// attributes of our vertex
in vec3 vPosition;
in vec2 vTexCoord;

out vec2 fTexCoord;


void main()
{
	// gl_Position is a special variable of OpenGL that must be set
	 gl_Position =  projectionMatrix * viewMatrix * worldMatrix * vec4(vPosition, 1.0);

	 
	// Tex coord adjustments here !
	fTexCoord = vec2(vTexCoord.x, -1 * vTexCoord.y);
}

