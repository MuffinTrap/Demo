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
	class Scene2D : IScene 
	{
		ShaderProgram shaderProgram;

		DrawableMesh quadMesh;

		Matrix4Uniform projectionMatrix;
		Matrix4Uniform viewMatrix;

		CameraComponent camera;

		Scene2D()
		{
			camera = new CameraComponent();
		}

		public void loadScene(AssetManager assetManager)
		{
			// Load program from single file...
			shaderProgram = new ShaderProgram(assetManager.GetShader("texturequad.vs")
			, assetManager.GetShader("texturequad.fs"));

			Error.checkGLError("Scene.loadScene");

			shaderProgram.Use();
			

			int diffuseColorLocation = shaderProgram.GetAttributeLocation("vDiffuseColor");
			shaderProgram.setSamplerUniform("inputTexture", 0);

			quadMesh = new DrawableMesh(
				"Quad"
				, MeshDataGenerator.CreateTexturedQuadMesh(assetManager)
				, ShaderManager.getDefaultAttributes(shaderProgram)
				, new TransformComponent()
				, null
				, shaderProgram);


			projectionMatrix = new Matrix4Uniform("projectionMatrix");
			projectionMatrix.Matrix = Matrix4.CreateOrthographic(1.0f, 1.0f, 0.1f, 100.0f);

			viewMatrix = new Matrix4Uniform("viewMatrix");
			viewMatrix.Matrix = camera.GetViewMatrix();

			GL.Enable(EnableCap.DepthTest);
			GL.DepthFunc(DepthFunction.Less);

			Error.checkGLError("Scene.loadScene");

		}


		public void drawScene() 
		{
			shaderProgram.Use();

			projectionMatrix.Set(shaderProgram);
			viewMatrix.Set(shaderProgram);

			quadMesh.draw();

			Error.checkGLError("Scene.drawScene");
		}

		public void updateScene(KeyboardState keyState) 
		{
			camera.Update(keyState);
			viewMatrix.Matrix = camera.GetViewMatrix();

		}
	}

}