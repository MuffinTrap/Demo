#version 330

#include light.ss;

// Textures
uniform sampler2D uDiffuseMap;
uniform sampler2D uIlluminationMap;
uniform sampler2D uRoughnessMap;
uniform sampler2D uMetallicMap;

in vec3 fNormal;
in vec3 fPosition;
in vec2 fTexCoord;

out vec4 fragColor;

void main()
{	
	vec4 textureColor = texture(uDiffuseMap, fTexCoord);
	vec4 roughnessValue = texture(uRoughnessMap, fTexCoord);
	vec4 metallicValue = texture(uMetallicMap, fTexCoord);
	
	Material mat = getMaterial(metallicValue.x, roughnessValue.x);
	
	vec4 lightColor = calculateLightColor(textureColor, fPosition, fNormal, mat);
	
	vec4 illuminationValue = texture(uIlluminationMap, fTexCoord);
	float illuminationStrength = illuminationValue.x;
	vec4 innerLightColor = vec4(1,1,1,0);
	vec4 illuminationColor = getIlluminationColor(fPosition, fNormal, mat, innerLightColor);
	
    fragColor = lightColor + (illuminationStrength * illuminationColor) + (illuminationStrength * textureColor);
}

vec4 getIlluminationColor(vec3 fragmentPosition, vec3 fragmentNormal, Material mat, vec4 innerLightColor)
{
	vec3 viewDir = normalize(-fragmentPosition);
	vec3 norm = normalize(fragmentNormal);
	float specular = pow(max(dot(viewDir, norm), 0.0), mat.specularPower);
	return specular * innerLightColor;
}

