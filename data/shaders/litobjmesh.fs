#version 330

in vec3 fNormal;
in vec3 fPosition;
in vec3 fLightPosition;
out vec4 fragColor;
  
uniform vec4 objectColor;
uniform vec4 lightColor;

uniform float ambientStrength;
uniform float diffuseStrength;
uniform float specularStrength;
uniform int specularPower;


void main()
{
	// Ambient color
	vec4 ambientResult = lightColor * ambientStrength;
	
	// Diffuse color
	vec3 norm = normalize(fNormal);
	vec3 lightDir = normalize(fLightPosition - fPosition);
	float diffuse = max(dot(norm, lightDir), 0.0);
	vec4 diffuseResult = diffuseStrength * diffuse * lightColor;
	
	// Specular color
	// In view space the view position is (0,0,0)
	vec3 viewDir = normalize(-fPosition);
	vec3 reflectDir = reflect(-lightDir, norm);
	float specular = pow(max(dot(viewDir, reflectDir), 0.0), specularPower);
	vec4 specularResult = specularStrength * specular * lightColor;
	
    fragColor = (ambientResult + diffuseResult + specularResult) * objectColor;
}