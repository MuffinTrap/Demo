
using OpenTK;
namespace MuffinSpace
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

		public float linearAttenuation; // Ambient for directional
		public float quadraticAttenuation; // Intensity for directional

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

		public Vector3 GetPosDir()
		{
			if (type == LightType.Directional)
			{
				return transform.Direction;
			}
			else
			{
				return transform.Position;
			}
		}

		public string GetInfoString()
		{
			return "Color: " + Logger.PrintVec3(color) + " PosDir: " + Logger.PrintVec3(GetPosDir()) + " Attenuation: " + linearAttenuation+ ", " + quadraticAttenuation;
		}

		private Light(LightType typeParam, Vector3 colorParam, Vector3 positionParam, Vector3 directionParam)
		{
			type = typeParam;
			color = colorParam;

			transform = new TransformComponent(positionParam);
			transform.Direction = directionParam.Normalized();
		}



		public static Light CreatePointLight(Vector3 colorParam, float distanceParam, Vector3 positionParam)
		{
			Light pointLight = new Light(LightType.Point, colorParam, positionParam, new Vector3(-1,0,0));

			AttenuationArray.AttenuationRecord r = AttenuationArray.getAttenuationForDistance(distanceParam);
			pointLight.linearAttenuation = r.linear;
			pointLight.quadraticAttenuation = r.quadratic;

			return pointLight;
		}

		public static Light CreateDirectionalLight(Vector3 colorParam, float ambientParam, float intensityParam, Vector3 directionParam)
		{
			Light dirLight = new Light(LightType.Directional, colorParam, new Vector3(-1, 0, 0), directionParam);
			dirLight.linearAttenuation = ambientParam;
			dirLight.quadraticAttenuation = intensityParam;

			return dirLight;
		}

		public static Light CreateBlackLight(LightType type)
		{
			Light black = new Light(type, new Vector3(0, 0, 0), new Vector3(-1, 0, 0), new Vector3(-1, 0, 0));
			black.type = type;
			black.linearAttenuation = 0.0f;
			black.quadraticAttenuation = 0.0f;
			return black;
		}
		
		public void SetToBlack()
		{
			color = new Vector3(0, 0, 0);
			linearAttenuation = 0.0f;
			quadraticAttenuation = 0.0f;
		}

		public void SetTo(Light other)
		{
			color = other.color;
			linearAttenuation = other.linearAttenuation;
			quadraticAttenuation = other.quadraticAttenuation;
			transform.Position = other.transform.Position;
			transform.Direction = other.transform.Direction;
		}

		public bool SetUniform(ShaderProgram shaderProgram, int location, ShaderUniformName dataName)
		{
			switch(dataName)
			{
				case ShaderUniformName.LightPositionOrDirection:
					shaderProgram.SetVec3Uniform(location, GetPosDir());
					break;
				case ShaderUniformName.LightColor: shaderProgram.SetVec3Uniform(location, color);
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