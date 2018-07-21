#version 330

// Light properties
uniform vec3 uLightColor;

// Material properties
uniform float uAmbientStrength;
uniform float uDiffuseStrength;
uniform float uSpecularStrength;
uniform float uSpecularPower;

// Textures
uniform sampler2D uDiffuseMap;

in vec3 fNormal;
in vec3 fPosition;
in vec2 fTexCoord;
in vec3 fLightDirection;

out vec4 fragColor;

void main()
{
	// Ambient color
	vec4 lightColor4 = vec4(uLightColor, 1.0f);
	vec4 ambientResult = lightColor4 * uAmbientStrength;
	
	// Diffuse color
	vec3 norm = normalize(fNormal);
	vec3 toLight = fLightDirection * -1;
	float diffuse = max(dot(norm, toLight), 0.0);
	vec4 diffuseResult = uDiffuseStrength * diffuse * lightColor4;
	
	// Specular color
	// In view space the view position is (0,0,0)
	vec3 viewDir = normalize(-fPosition);
	vec3 reflectDir = reflect(-toLight, norm);
	float specular = pow(max(dot(viewDir, reflectDir), 0.0), uSpecularPower);
	vec4 specularResult = uSpecularStrength * specular * lightColor4;
	
	vec4 textureColor = texture(uDiffuseMap, fTexCoord);
    fragColor = (ambientResult + diffuseResult + specularResult) * textureColor;
}