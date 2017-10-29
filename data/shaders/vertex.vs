#version 130

// OpenGL 3.0 -> GLSL 1.30
// a projection transformation to apply to the vertex' position
uniform mat4 projectionMatrix;
uniform mat4 modelMatrix;
uniform mat4 viewMatrix;

uniform vec4 uDiffuseColor;
uniform float uScale;

// attributes of our vertex
in vec3 vPosition;
in vec2 vTexCoord;

out vec2 fTexCoord;
out vec4 fDiffuseColor; // must match name in fragment shader
void main()
{
	// gl_Position is a special variable of OpenGL that must be set
	 gl_Position =   projectionMatrix * viewMatrix * modelMatrix * vec4(vPosition * uScale,	1.0);
	
	// 
	fTexCoord = vTexCoord;
	fDiffuseColor = uDiffuseColor;
}