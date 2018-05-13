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
	public class UnicornScene : IScene
	{
		ShaderProgram modelShader;
		//ShaderProgram particleShader;

		DrawableMesh unicornMesh;
		//DrawableMesh hillMesh;

		Vector3 cameraPos = new Vector3(0, 5, 10);
		Vector3 cameraTarget = new Vector3(0, 0, 0);
		float cameraAngle = 0.0f;
		const float fullCircle = (float)(Math.PI * 2.0f);
		float angleSpeed = fullCircle / (float)DemoSettings.GetDefaults().UpdatesPerSecond;

		CameraComponent camera;
		public void setCameraFrames(List<PosAndDir> frames) { }

		public UnicornScene()
		{
			camera = new CameraComponent();
		}

		public void loadScene(AssetManager assetManager)
		{
			modelShader = assetManager.GetShaderProgram("objmesh");
			modelShader.Use();
			modelShader.setSamplerUniform("inputTexture", 0);

			unicornMesh = assetManager.GetMesh(
				name: "Unicorn"
				, modelFile: "unicorn.obj"
				, material: "unicorn"
				, shader: modelShader
				, position: new Vector3(0, 0, 0)
				, scale: 0.1f);

			GL.Enable(EnableCap.DepthTest);
			GL.DepthFunc(DepthFunction.Less);

			Error.checkGLError("Scene.loadScene");

			camera.Position = cameraPos;
			camera.SetTarget(cameraTarget);

		}

		public void drawScene(float cameraFrame)
		{
			modelShader.Use();

			camera.setMatrices(modelShader);

			//hillMesh.draw();
			unicornMesh.draw();

			//particleShader.Use();
			//camera.setMatrices(particleShader);

			// Draw particle

			Error.checkGLError("UnicornScene.drawScene");
		}

		public void updateScene(KeyboardState keyState, MouseState mouseState)
		{
			camera.Update(keyState, mouseState);

			// Orbiting camera around unicorn mesh
			cameraAngle += angleSpeed;
			if (cameraAngle > fullCircle)
			{
				cameraAngle -= fullCircle;
			}
			Matrix4 rot = Matrix4.CreateRotationY(cameraAngle);
			camera.Position = Vector3.TransformVector(cameraPos, rot);
			camera.SetTarget(cameraTarget);
		}
	}
}