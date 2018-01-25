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

		Mesh quadMesh;

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
			Mesh.AttributeLocations locations = new Mesh.AttributeLocations();
			locations.positionLocation = shaderProgram.GetAttributeLocation("vPosition");
			locations.texCoordLocation = shaderProgram.GetAttributeLocation("vTexCoord");
			locations.normalLocation = -1;
			locations.diffuseColorLocation = shaderProgram.GetAttributeLocation("vDiffuseColor");
			shaderProgram.setSamplerUniform("inputTexture", 0);

			quadMesh = Mesh.CreateTexturedQuadMesh(assetManager, locations);

			projectionMatrix = new Matrix4Uniform("projectionMatrix");
			projectionMatrix.Matrix = Matrix4.CreateOrthographic(1.0f, 1.0f, 0.1f, 100.0f);

			viewMatrix = new Matrix4Uniform("viewMatrix");
			viewMatrix.Matrix = camera.GetViewMatrix();

			GL.Enable(EnableCap.DepthTest);
			GL.DepthFunc(DepthFunction.Less);

			Error.checkGLError("Scene.loadScene");

		}

		public void drawMesh(Mesh mesh)
		{
			mesh.updateUniforms(shaderProgram);
			GL.BindVertexArray(mesh.VAOHandle);
			GL.DrawArrays(PrimitiveType.Triangles, 0, mesh.VertexAmount);
			GL.BindVertexArray(0);
			GL.BindTexture(TextureTarget.Texture2D, 0);
		}

		public void drawScene() 
		{
			shaderProgram.Use();

			projectionMatrix.Set(shaderProgram);
			viewMatrix.Set(shaderProgram);

			drawMesh(quadMesh);

			Error.checkGLError("Scene.drawScene");
		}

		public void updateScene(KeyboardState keyState) 
		{
			camera.Update(keyState);
			viewMatrix.Matrix = camera.GetViewMatrix();

		}


	}

}