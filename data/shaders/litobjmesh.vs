#version 330

// OpenGL 3.0 -> GLSL 1.30
// a projection transformation to apply to the vertex' position
uniform mat4 uProjectionMatrix;
uniform mat4 uWorldMatrix;
uniform mat4 uViewMatrix;

// attributes of vertex
in vec3 aPosition;
in vec3 aNormal;
in vec2 aTexCoord;

out vec3 fNormal;
out vec3 fPosition;
out vec2 fTexCoord;

void main()
{
	// gl_Position is a special variable of OpenGL that must be set
	gl_Position =  uProjectionMatrix * uViewMatrix * uWorldMatrix * vec4(aPosition, 1.0);
	fNormal = mat3(transpose(inverse(uViewMatrix * uWorldMatrix))) * aNormal;
	fPosition = vec3(uViewMatrix * uWorldMatrix * vec4(aPosition, 1.0));
	fTexCoord = vec2(aTexCoord.x, 1 - aTexCoord.y);
}

