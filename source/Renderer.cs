namespace OpenTkConsole
{
	public class Renderer
	{
		private Renderer()
		{

		}

		private static Renderer singleton = null;
		public static Renderer GetSingleton()
		{
			if (singleton == null)
			{
				singleton = new Renderer();
			}
			return singleton;
		}

		// int lightAmount = 2;
		ShaderProgram activeProgram = null;

		public void RenderWithShader(ShaderProgram shader)
		{
			activeProgram = shader;
			activeProgram.Use();
		}

		public void RenderCamera(IShaderDataOwner camera)
		{
			if (activeProgram == null)
			{
				Logger.LogError(Logger.ErrorState.Critical, "RenderCamera, no active shader");
				return;
			}
			ShaderUniformManager man = ShaderUniformManager.GetSingleton();
			man.SetData(activeProgram, ShaderUniformName.ViewMatrix, camera);
			man.SetData(activeProgram, ShaderUniformName.ProjectionMatrix, camera);
		}
		
		public void RenderLight(IShaderDataOwner light, int lightIndex)
		{
			if (activeProgram == null)
			{
				Logger.LogError(Logger.ErrorState.Critical, "RenderLight, no active shader");
				return;
			}
			ShaderUniformManager man = ShaderUniformManager.GetSingleton();
			man.SetArrayData(activeProgram, ShaderUniformName.LightsArray, ShaderUniformName.LightPosition, light, lightIndex);
			//man.SetArrayData(activeProgram, ShaderUniformName.LightsArray, ShaderUniformName.LightDirection, light, lightIndex);
			man.SetArrayData(activeProgram, ShaderUniformName.LightsArray, ShaderUniformName.LightColor, light, lightIndex);
			man.SetArrayData(activeProgram, ShaderUniformName.LightsArray, ShaderUniformName.LinearAttenuation, light, lightIndex);
			man.SetArrayData(activeProgram, ShaderUniformName.LightsArray, ShaderUniformName.QuadraticAttenuation, light, lightIndex);
		}

		public void RenderMesh(DrawableMesh mesh)
		{
			if (activeProgram == null)
			{
				Logger.LogError(Logger.ErrorState.Critical, "RenderMesh, no active shader");
				return;
			}
			ShaderUniformManager man = ShaderUniformManager.GetSingleton();
			man.SetData(activeProgram, ShaderUniformName.WorldMatrix, mesh);
			MaterialManager matMan = MaterialManager.GetSingleton();
			matMan.SetMaterialToShader(mesh.BoundMaterial, activeProgram);

			mesh.draw();
		}
	}

}