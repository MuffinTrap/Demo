using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace MuffinSpace
{
	public class Renderer
	{

		ShaderProgram activeProgram = null;

		// OpenGL state
		Vector3 clearingColor;
		int width;
		int height;

		CameraComponent camera;
		List<PosAndDir> cameraFrames;

		// Light amount is set when shaders are loaded
		int maxPointLightIndex = 2;
		List<Light> activeLights;

		// Skybox material when rendering sky meshes
		Material activeSkybox = null;

		private Renderer()
		{
			clearingColor = new Vector3(0, 0, 0);
			camera = new CameraComponent();
			cameraFrames = null;

			activeLights = new List<Light>();
			activeLights.Add(Light.CreateBlackLight(Light.LightType.Directional));
			for (int l = 1; l <= maxPointLightIndex; l++)
			{
				activeLights.Add(Light.CreateBlackLight(Light.LightType.Point));
			}

			// Default state
			GL.FrontFace(FrontFaceDirection.Ccw); // This is the default of OpenGL

			GL.Enable(EnableCap.DepthTest);
			GL.DepthFunc(DepthFunction.Less);

			GL.Enable(EnableCap.CullFace);
			GL.CullFace(CullFaceMode.Back);

			GL.Enable(EnableCap.Blend);
			GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
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

		public void UpdateInput(KeyboardState keyState, MouseState mouseState)
		{
			camera.UpdateInput(keyState, mouseState);
		}

		private void ResetLights()
		{
			foreach( Light l in activeLights)
			{
				l.SetToBlack();
			}
		}
	

		public void SetClearColor(Vector3 color)
		{
			if (clearingColor != color)
			{
				clearingColor = color;
				Color4 ccolor = new Color4(color.X, color.Y, color.Z, 1.0f);
				GL.ClearColor(ccolor);
			}
		}

		public void ClearScreen()
		{
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
		}

		public void StartFrame()
		{
			ClearScreen();
			ResetLights();
			activeProgram = null;
		}

		public void EndFrame()
		{
		}

		public void ResizeScreen(int widthParam, int heightParam)
		{
			width = widthParam;
			height = heightParam;
			camera.AspectRatio = (float)width / height;
            GL.Viewport(0, 0, widthParam, heightParam);
		}

		public void SetActiveSkybox(Material skyboxMaterial)
		{
			activeSkybox = skyboxMaterial;
		}

		public void SetActiveShader(ShaderProgram shader)
		{
			if (shader == null)
			{
				Logger.LogError(Logger.ErrorState.Critical, "SetActiveShader, no shader given");
				return;
			}
			if (activeProgram != shader)
			{
				activeProgram = shader;
				activeProgram.Use();

				ShaderUniformManager man = ShaderUniformManager.GetSingleton();
				if (man.DoesShaderUseCamera(activeProgram))
				{
					RenderCamera();
				}
				if (man.DoesShaderUseLights(activeProgram))
				{
					RenderActiveLights();
				}
				if (man.DoesShaderSupportUniform(activeProgram, ShaderUniformName.CubeMap))
				{
					MaterialManager matMan = MaterialManager.GetSingleton();
					matMan.SetFromMaterialToShader(activeSkybox, ShaderUniformName.CubeMap, activeProgram);
				}
			}
		}

		public int GetMaxLights()
		{
			return maxPointLightIndex + 1;
		}

		public CameraComponent GetCamera()
		{
			return camera;
		}

		public void SetCameraFrames(List<PosAndDir> frames)
		{
			cameraFrames = frames;
		}

		public void RenderCamera()
		{
			if (activeProgram == null)
			{
				Logger.LogError(Logger.ErrorState.Critical, "RenderCamera, no active shader");
				return;
			}

			if (cameraFrames != null && !camera.FreeMode)
			{
				int frame = SyncSystem.GetSingleton().Frame;
				float frameProg = SyncSystem.GetSingleton().FrameProgress;
				camera.SetFrame(frame, frameProg, cameraFrames);
			}

			ShaderUniformManager man = ShaderUniformManager.GetSingleton();
			man.TrySetData(activeProgram, ShaderUniformName.ViewMatrix, camera);
			man.TrySetData(activeProgram, ShaderUniformName.ProjectionMatrix, camera);
		}

		public void ActivateLight(Light light, int index)
		{
			if (index < activeLights.Count)
			{
				activeLights[index].SetTo(light);
			}
		}

		public void DeactivateLight(int index)
		{
			if (index < activeLights.Count)
			{
				activeLights[index].SetToBlack();
			}
		}

		public void RenderActiveLights()
		{
			int pointlightIndex = 1;	
			for (int i = 0; i < activeLights.Count; i++)
			{
				Light activeLight = activeLights[i];
				if (activeLight.type == Light.LightType.Directional)
				{
					RenderDirectionalLight(activeLight, 0);
				}
				else if (activeLight.type == Light.LightType.Point)
				{
					if (pointlightIndex <= maxPointLightIndex)
					{
						RenderPointLight(activeLight, pointlightIndex);
						pointlightIndex++;
					}
				}
			}
		}

		public void RenderDirectionalLight(IShaderDataOwner light, int lightIndex)
		{
			if (activeProgram == null)
			{
				Logger.LogError(Logger.ErrorState.Critical, "RenderLight, no active shader");
				return;
			}
			if (lightIndex != 0)
			{
				Logger.LogError(Logger.ErrorState.Limited, "RenderLight, directional lights must have index 0");
			}
			PassLightData(light, lightIndex);
		}
		
		public void RenderPointLight(IShaderDataOwner light, int lightIndex)
		{
			if (activeProgram == null)
			{
				Logger.LogError(Logger.ErrorState.Critical, "RenderLight, no active shader");
				return;
			}
			if (lightIndex == 0)
			{
				Logger.LogError(Logger.ErrorState.Limited, "RenderLight, point lights must have index greater than 0");
			}
			PassLightData(light, lightIndex);
		}

		private void PassLightData(IShaderDataOwner light, int lightIndex)
		{
			ShaderUniformManager man = ShaderUniformManager.GetSingleton();
			man.SetArrayData(activeProgram, ShaderUniformName.LightsArray, ShaderUniformName.LightPositionOrDirection, light, lightIndex);
			man.SetArrayData(activeProgram, ShaderUniformName.LightsArray, ShaderUniformName.LightColor, light, lightIndex);
			man.SetArrayData(activeProgram, ShaderUniformName.LightsArray, ShaderUniformName.LinearAttenuation, light, lightIndex);
			man.SetArrayData(activeProgram, ShaderUniformName.LightsArray, ShaderUniformName.QuadraticAttenuation, light, lightIndex);
		}

		public void RenderMesh(DrawableMesh mesh)
		{
			if (mesh == null)
			{
				Logger.LogError(Logger.ErrorState.Critical, "RenderMesh, no mesh given");
				return;
			}
			SetActiveShader(mesh.ShaderProgram);
			if (activeProgram == null)
			{
				Logger.LogError(Logger.ErrorState.Critical, "RenderMesh, no active shader");
				return;
			}

			ShaderUniformManager man = ShaderUniformManager.GetSingleton();
			man.TrySetData(activeProgram, ShaderUniformName.WorldMatrix, mesh);
			if (mesh.BoundMaterial != null)
			{
				MaterialManager matMan = MaterialManager.GetSingleton();
				matMan.SetMaterialToShader(mesh.BoundMaterial, activeProgram);
			}

			mesh.draw();
		}

		public void RenderObject(DrawableMesh mesh)
		{
			GL.DepthFunc(DepthFunction.Less);
			RenderMesh(mesh);

		}

		public void RenderSky(DrawableMesh mesh)
		{
			GL.DepthFunc(DepthFunction.Lequal);

			mesh.Transform.Position = camera.Position;
			RenderMesh(mesh);
			int cubeSamplerLocation = mesh.ShaderProgram.GetUniformLocation(ShaderUniformName.CubeMap);
			int cubeSamplerValue = mesh.ShaderProgram.GetSamplerUniform(cubeSamplerLocation);
			int defaultUnit = MaterialManager.GetSingleton().GetTextureUnitForMap(ShaderUniformName.CubeMap);
			if (cubeSamplerValue != defaultUnit)
			{
				Logger.LogInfo("Sampler cube at :" + cubeSamplerLocation + " has value " + cubeSamplerValue
				+ " default is " + defaultUnit);
			}
		}

		public void RenderGui(DrawableMesh mesh)
		{
			GL.DepthFunc(DepthFunction.Always);
			RenderMesh(mesh);
		}
	}

}