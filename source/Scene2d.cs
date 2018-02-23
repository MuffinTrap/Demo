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
	public class Scene2D : IScene 
	{
		ShaderProgram shaderProgram;

		DrawableMesh quadMesh;

		CameraComponent camera;

		public Scene2D()
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
			
			shaderProgram.setSamplerUniform("inputTexture", 0);

			quadMesh = new DrawableMesh(
				"Quad"
				, MeshDataGenerator.CreateTexturedQuadMesh(assetManager)
				, ShaderManager.getAttributes(new List<ShaderAttributeName> { ShaderAttributeName.Position, ShaderAttributeName.TexCoord },shaderProgram)
				, new TransformComponent()
				, assetManager.GetMaterial("konata")
				, shaderProgram);

			quadMesh.Transform.setLocationAndScale(new Vector3(0.0f, 11.0f, 0.0f), 1);

			GL.Enable(EnableCap.DepthTest);
			GL.DepthFunc(DepthFunction.Less);

			Error.checkGLError("Scene.loadScene");

		}


		public void drawScene() 
		{
			shaderProgram.Use();

			camera.setMatrices(shaderProgram);

			quadMesh.draw();

			Error.checkGLError("Scene2D.drawScene");
		}

		public void updateScene(KeyboardState keyState, MouseState mouseState) 
		{
			camera.Update(keyState, mouseState);
			quadMesh.Transform.rotateAroundY(0.04f);
		}
	}
}