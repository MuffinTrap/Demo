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
in vec3 vNormal;
in vec2 vTexCoord;

out vec2 fTexCoord;
out vec4 fDiffuseColor; // must match name in fragment shader

vec4 directLight(vec4 normal, vec4 lightDir)
{
	vec4 lightColor = vec4(1.0f, 0.4f, 0.4f, 1.0f);
	float lightDot = dot(normal, -1.0f * lightDir);
	
	float clampDot = clamp(lightDot, 0.0f, 1.0f);
	
	return clampDot * lightColor;
}


void main()
{
	// gl_Position is a special variable of OpenGL that must be set
	 gl_Position =  projectionMatrix * viewMatrix * modelMatrix * vec4(vPosition * uScale,	1.0);
	
	// 
	vec4 transNormal = viewMatrix * modelMatrix * vec4(vNormal, 0.0f);
	vec4 lightDir = vec4(0.3f, -0.3f, 0.0f, 0.0f);
	vec4 transLight = viewMatrix * lightDir;
	
	
	fTexCoord = vTexCoord;
	fDiffuseColor = (0.1f * uDiffuseColor) + directLight(transNormal, lightDir);
}

