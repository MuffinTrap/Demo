
using OpenTK;
namespace OpenTkConsole
{
	public class DirectionalLight : IShaderDataOwner
	{
		public void ActivateForDrawing()
		{
			ShaderUniformManager uniMan = ShaderUniformManager.GetSingleton();
			uniMan.RegisterDataOwner(this, ShaderUniformName.LightColor);
			uniMan.RegisterDataOwner(this, ShaderUniformName.LightDirection);
			uniMan.RegisterDataOwner(this, ShaderUniformName.AmbientStrength);
		}
		public void SetUniform(ShaderProgram shaderProgram, int location, ShaderUniformName dataName)
		{
			switch(dataName)
			{
				case ShaderUniformName.LightColor: shaderProgram.SetVec3Uniform(location, color);
					break;
				case ShaderUniformName.LightDirection: shaderProgram.SetVec3Uniform(location, direction);
					break;
				case ShaderUniformName.AmbientStrength:
					shaderProgram.SetFloatUniform(location, ambientStrength);
					break;
				default:
					break;
			}
		}

		private Vector3 color = new Vector3(1,1,1);
		private Vector3 direction = new Vector3(0, -1, 0);
		private float ambientStrength = 0.2f;

		public DirectionalLight(Vector3 lightColor, Vector3 lightDirection, float ambient)
		{
			color = lightColor;
			direction = Vector3.Normalize(lightDirection);
			ambientStrength = MathHelper.Clamp(ambient, 0.0f, 1.0f);
		}

	}

}