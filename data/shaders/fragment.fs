#version 130 core
//in vec4 fColor; // must match name in vertex shader
out vec4 fragColor; // first out variable is automatically written to the screen
void main()
{
	fragColor = vec4(1.0f, 0.5f, 0.2f, 1.0f);
}