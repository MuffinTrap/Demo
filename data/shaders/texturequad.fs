#version 130

uniform sampler2D inputTexture;

in vec2 fTexCoord;

out vec4 fragColor; // first out variable is automatically written to the screen
void main()
{
	vec4 textureColor = texture(inputTexture, fTexCoord);
	if (textureColor.a < 0.5)
	{
		discard;
	}
		
	fragColor = textureColor;
}