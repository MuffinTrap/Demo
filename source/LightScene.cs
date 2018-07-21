using System;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace OpenTkConsole
{
	public class LightScene : IScene
	{
		ShaderProgram shaderProgram;
		DrawableMesh quadMesh;
		CameraComponent camera;

		DirectionalLight sunLight;

		public LightScene(CameraComponent mainCamera)
		{
			camera = mainCamera;
			sunLight = new DirectionalLight(new Vector3(1.0f, 1.0f, 0.9f), new Vector3(-0.3f, -1.0f, -0.1f), 0.3f);
		}

		public void setCameraFrames(List<PosAndDir> frames) { }

		public void loadScene(AssetManager assetManager)
		{
			shaderProgram = assetManager.GetShaderProgram("litobjmesh");

			quadMesh = assetManager.GetMesh("monu9"
			, assetManager.getMeshData("monu9.obj")
			, "monu9"
			, shaderProgram
			, new Vector3(4.0f, 0.0f, 0.0f)
			, 0.2f);

			// Lighting
			sunLight.ActivateForDrawing();

			GL.Enable(EnableCap.DepthTest);
			GL.DepthFunc(DepthFunction.Less);

			Error.checkGLError("LightScene.loadScene");
		}


		public void drawScene(float cameraFrame) 
		{
			ShaderUniformManager uniformManager = ShaderUniformManager.GetSingleton();

			camera.ActivateForDrawing();
			quadMesh.ActivateForDrawing();
			uniformManager.ActivateShader(shaderProgram);
			quadMesh.draw();

			Error.checkGLError("LightScene.drawScene");
		}

		public void updateScene(KeyboardState keyState, MouseState mouseState) 
		{
			//quadMesh.Transform.rotateAroundY(0.01f);
			camera.Update(keyState, mouseState);
		}
	}
}