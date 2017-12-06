#version 130

uniform sampler2D inputTexture;

in vec2 fTexCoord;
in vec4 fLightColor; // must match name in vertex shader
out vec4 fragColor; // first out variable is automatically written to the screen
void main()
{
	vec4 textureColor = texture(inputTexture, fTexCoord);
	fragColor = fLightColor * textureColor;
}