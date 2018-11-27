#version 330

struct Light
{
	vec3 position;
	vec3 direction;
	vec3 color;
	float linearAttenuation;
	float quadraticAttenuation;
};

struct Material
{
	float diffuseStrength;
	float specularPower;
	float specularStrength;
	float ambientStrength;
};

float getAttenuation(float distanceToLight, float linear, float quadratic);

vec4 getPointLight(
	  vec3 fragmentPosition
	, vec3 fragmentNormal
	, Light l
	, Material m);
	
vec4 getDirectionalLight(
	  vec3 fragmentPosition
	, vec3 fragmentNormal
	, Light l
	, Material m);

uniform mat4 uViewMatrix;
	
// Light properties
#define LIGHT_AMOUNT 2
uniform Light uLights[LIGHT_AMOUNT];

// Textures
uniform sampler2D uDiffuseMap;
uniform sampler2D uIlluminationMap;

in vec3 fNormal;
in vec3 fPosition;
in vec2 fTexCoord;

out vec4 fragColor;

void main()
{
	Material mat;
	mat.diffuseStrength = 0.7f;
	mat.specularPower = 64;
	mat.specularStrength = 0.2f;
	mat.ambientStrength = 0.1f;
	
	vec4 illuminationValue = texture(uIlluminationMap, fTexCoord);
	float illuminationPercentage = illuminationValue.x;
	vec4 textureColor = texture(uDiffuseMap, fTexCoord);
	vec4 lightColor = vec4(0,0,0,0);
	for (int i = 0; i < LIGHT_AMOUNT; i++)
	{
		lightColor += textureColor * getPointLight(fPosition, fNormal, uLights[i], mat);
	}
    fragColor = ((1.0f - illuminationPercentage) * lightColor) + ((illuminationPercentage) * textureColor);
}

vec4 getPointLight(
	  vec3 fragmentPosition
	, vec3 fragmentNormal
	, Light l
	, Material m)
{
	// Transform light direction and position to view space.
	vec3 position = vec3(uViewMatrix * vec4(l.position, 1.0f));
	
	vec3 toLight = (position - fragmentPosition);
	float distanceToLight = length(toLight);
	toLight = normalize(toLight);
	vec3 toFragment = toLight * -1.0f;
	
	float attenuation = getAttenuation(distanceToLight, l.linearAttenuation, l.quadraticAttenuation);
	
	// Ambient color
	vec4 color4 = vec4(l.color, 1.0f);
	vec4 ambientResult = color4 * m.ambientStrength * attenuation;
	
	// Diffuse color
	vec3 norm = normalize(fragmentNormal);
	float diffuse = max(dot(norm, toLight), 0.0);
	vec4 diffuseResult = m.diffuseStrength * diffuse * color4 * attenuation;
	
	// Specular color
	// In view space the view position is (0,0,0)
	vec3 viewDir = normalize(-fragmentPosition);
	vec3 reflectDir = reflect(toFragment, norm);
	float specular = pow(max(dot(viewDir, reflectDir), 0.0), m.specularPower);
	vec4 specularResult = m.specularStrength * specular * color4 * attenuation;
	
	return (ambientResult + diffuseResult + specularResult);
}

vec4 getDirectionalLight(
	  vec3 fragmentPosition
	, vec3 fragmentNormal
	, Light l
	, Material m)
{
	vec3 direction = vec3(uViewMatrix * vec4(l.direction, 1.0f));
	
	float attenuation = 1.0f;
	vec3 toFragment = direction;
	vec3 toLight = toFragment * -1.0f;
	// Ambient color
	vec4 lightColor4 = vec4(l.color, 1.0f);
	vec4 ambientResult = lightColor4 * m.ambientStrength * attenuation;
	
	// Diffuse color
	vec3 norm = normalize(fragmentNormal);
	float diffuse = max(dot(norm, toLight), 0.0);
	vec4 diffuseResult = m.diffuseStrength * diffuse * lightColor4 * attenuation;
	
	// Specular color
	// In view space the view position is (0,0,0)
	vec3 viewDir = normalize(-fragmentPosition);
	vec3 reflectDir = reflect(toFragment, norm);
	float specular = pow(max(dot(viewDir, reflectDir), 0.0), m.specularPower);
	vec4 specularResult = m.specularStrength * specular * lightColor4 * attenuation;
	
	return (ambientResult + diffuseResult + specularResult);
}

float getAttenuation(float distanceToLight, float linear, float quadratic)
{
	return 1.0f / (1.0f + linear * distanceToLight + quadratic * (distanceToLight * distanceToLight));
}