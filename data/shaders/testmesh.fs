#version 130

in  vec3 fDiffuseColor;
out vec4 fragColor; // first out variable is automatically written to the screen
void main()
{
	float r = fDiffuseColor.x >=0 ? 1.0f : 0.5f;
	float g = 0.5f;
	float b = fDiffuseColor.z >=0 ? 1.0f : 0.5f;
	
	fragColor = vec4(r, g, b, 1.0f);
}