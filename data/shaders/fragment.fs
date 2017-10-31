#version 130

uniform sampler2D inputTexture;

in vec3 fNormal;
in vec2 fTexCoord;
in vec4 fDiffuseColor; // must match name in vertex shader
out vec4 fragColor; // first out variable is automatically written to the screen
void main()
{
	vec4 textureColor = texture(inputTexture, fTexCoord);
	fragColor = fDiffuseColor * textureColor;
}