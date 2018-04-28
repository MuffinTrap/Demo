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

		CameraComponent camera;

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
		}
	}
}