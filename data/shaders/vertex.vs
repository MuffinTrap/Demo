#version 130 core

// OpenGL 3.0 -> GLSL 1.30
// a projection transformation to apply to the vertex' position
uniform mat4 projectionMatrix;
uniform mat4 modelMatrix;
uniform mat4 viewMatrix;
// attributes of our vertex
in vec3 vPosition;
//  in vec4 vColor;
// out vec4 fColor; // must match name in fragment shader
void main()
{
	// gl_Position is a special variable of OpenGL that must be set
	 gl_Position =   projectionMatrix * viewMatrix * modelMatrix * vec4(vPosition,	1.0);
	
	// 
	// fColor = vColor;
}