#version 130

// OpenGL 3.0 -> GLSL 1.30
// a projection transformation to apply to the vertex' position
uniform mat4 projectionMatrix;
uniform mat4 worldMatrix;
uniform mat4 viewMatrix;


// attributes of our vertex
in vec3 vPosition;
in vec3 vNormal;
in vec2 vTexCoord;

out vec2 fTexCoord;
out vec4 fLightColor; // must match name in fragment shader

vec4 directLight(vec4 normal, vec4 lightDir)
{
	vec4 lightColor = vec4(1.0f, 1.0f, 1.0f, 1.0f);
	float lightDot = dot(normal, -1.0f * lightDir);
	
	float clampDot = clamp(lightDot, 1.0f, 1.0f);
	
	return clampDot * lightColor;
}


void main()
{
	// gl_Position is a special variable of OpenGL that must be set
	 gl_Position =  projectionMatrix * viewMatrix * worldMatrix * vec4(vPosition, 1.0);

	// 
	vec4 transNormal = viewMatrix * worldMatrix * vec4(vNormal, 0.0f);
	vec4 lightDir = normalize(vec4(0.0f, -1.0f, 0.0f, 0.0f));
	vec4 transLight = viewMatrix * lightDir;
	
	fTexCoord = vTexCoord;
	fLightColor = directLight(transNormal, lightDir);
}

