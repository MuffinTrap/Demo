#version 130

in vec3 fNormal;
in vec3 fPosition;
out vec4 fragColor;
  
uniform vec4 objectColor;
uniform vec4 lightColor;
uniform vec3 lightPosition;
uniform float ambientStrength;

void main()
{
	// Ambient color
	vec4 ambientResult = lightColor * ambientStrength;
	
	// Diffuse color
	vec3 norm = normalize(fNormal);
	vec3 lightDir = normalize(lightPosition - fPosition);
	float diffuse = max(dot(norm, lightDir), 0.0);
	vec4 diffuseResult = diffuse * lightColor;
	
    fragColor = (ambientResult + diffuseResult) * objectColor;
}