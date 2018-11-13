
using OpenTK;
namespace OpenTkConsole
{
	public class Light : IShaderDataOwner
	{
		public enum LightType
		{
			Point,
			Directional,
			Spot
		}

		public TransformComponent transform;
		public LightType type;
		public Vector3 color;
		public float ambientStrength;

		public float linearAttenuation;
		public float quadraticAttenuation;


		private static class AttenuationArray
		{
			public class AttenuationRecord
			{
				public float distance;
				public float linear;
				public float quadratic;

				public AttenuationRecord(float d, float l, float q)
				{
					distance = d;
					linear = l;
					quadratic = q;
				}
			}
			static AttenuationRecord[] attenuations = {
					new AttenuationRecord(0.0f, 1.0f, 2.0f),
					new AttenuationRecord(7.0f, 0.7f, 1.8f),
					new AttenuationRecord(13f,  0.35f , 0.44f),
					new AttenuationRecord(20f , 0.22f , 0.20f),
					new AttenuationRecord(32f , 0.14f , 0.07f),
					new AttenuationRecord(50f , 0.09f , 0.032f),
					new AttenuationRecord(65f , 0.07f , 0.017f),
					new AttenuationRecord(100f, 0.045f, 0.0075f),
					new AttenuationRecord(160f, 0.027f, 0.0028f),
					new AttenuationRecord(200f, 0.022f, 0.0019f),
					new AttenuationRecord(325f, 0.014f, 0.0007f),
					new AttenuationRecord(600f, 0.007f, 0.0002f),
					new AttenuationRecord(3250f, 0.0014f,   0.000007f ) };

			public static AttenuationRecord getAttenuationForDistance(float distance)
			{
				for (int index = 0; index < attenuations.Length; index++)
				{
					AttenuationRecord r = attenuations[index];
					if (index + 1 < attenuations.Length)
					{
						AttenuationRecord r2 = attenuations[index + 1];
						if (distance >= r.distance && distance < r2.distance)
						{
							float over = distance - r.distance;
							float between = r2.distance - r.distance;
							float s = over / between;
							float f = 1.0f - s;
							return new AttenuationRecord(distance, r.linear * f + r2.linear * s, r.quadratic * f + r2.quadratic * s);
						}
					}
					else
					{
						return r;
					}
				}
				return new AttenuationRecord(distance, 0.0f, 0.0f);
			}

		}

		private Light(Vector3 colorParam, float ambientParam, float distanceParam, Vector3 positionParam, Vector3 directionParam)
		{
			transform = new TransformComponent(positionParam);
			transform.Direction = directionParam;
			color = colorParam;
			ambientStrength = ambientParam;
			AttenuationArray.AttenuationRecord r = AttenuationArray.getAttenuationForDistance(distanceParam);
			linearAttenuation = r.linear;
			quadraticAttenuation = r.quadratic;
		}

		public static Light createDirectionalLight(Vector3 colorParam, float ambientParam, Vector3 directionParam)
		{
			Light dirLight = new Light(colorParam, ambientParam, 0.0f, new Vector3(0,0,0), directionParam);
			dirLight.type = LightType.Directional;

			return dirLight;
		}

		public static Light createPointLight(Vector3 colorParam, float ambientParam, float distanceParam, Vector3 positionParam)
		{
			Light pointLight = new Light(colorParam, ambientParam, distanceParam, positionParam, new Vector3(0,0,0));
			pointLight.type = LightType.Point;

			return pointLight;
		}
		public bool SetUniform(ShaderProgram shaderProgram, int location, ShaderUniformName dataName)
		{
			switch(dataName)
			{
				case ShaderUniformName.LightColor: shaderProgram.SetVec3Uniform(location, color);
					break;
				case ShaderUniformName.LightDirection: shaderProgram.SetVec3Uniform(location, transform.Direction);
					break;
				case ShaderUniformName.LightPosition: shaderProgram.SetVec3Uniform(location, transform.Position);
					break;
				case ShaderUniformName.AmbientStrength:
					shaderProgram.SetFloatUniform(location, ambientStrength);
					break;
				case ShaderUniformName.LinearAttenuation:
					shaderProgram.SetFloatUniform(location, linearAttenuation);
					break;
				case ShaderUniformName.QuadraticAttenuation:
					shaderProgram.SetFloatUniform(location, quadraticAttenuation);
					break;
				default:
					return false;
			}
			return true;
		}
	}
}