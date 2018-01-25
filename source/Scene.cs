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

public interface IScene
{
	void loadScene(AssetManager materialManager);
	void drawScene();
	void updateScene(KeyboardState keyState);
}

class EmptyScene : IScene
{
	public void loadScene(AssetManager assetManager) {}

	public void drawScene() {}

	public void updateScene(KeyboardState keyState) {}
}

class RotatingScene : IScene
{ 
	ShaderProgram shaderProgram;

	Mesh voxelMesh;
	
	Matrix4Uniform projectionMatrix;
	Matrix4Uniform viewMatrix;

	CameraComponent camera;

	public RotatingScene()
	{
		camera = new CameraComponent();
	}
	
	public void loadScene(AssetManager assetManager)
	{	
		// Load program from single file...
		shaderProgram = new ShaderProgram(assetManager.GetShader("objmesh.vs"), assetManager.GetShader("objmesh.fs"));
		
		Error.checkGLError("Scene.loadScene");

		shaderProgram.Use();
		Mesh.AttributeLocations locations = new Mesh.AttributeLocations();
		locations.positionLocation = shaderProgram.GetAttributeLocation("vPosition");
		locations.normalLocation = shaderProgram.GetAttributeLocation("vNormal");
		locations.texCoordLocation = shaderProgram.GetAttributeLocation("vTexCoord");
		shaderProgram.setSamplerUniform("inputTexture", 0);

		voxelMesh = assetManager.GetMesh("monu9.obj");
		voxelMesh.enableAttributes(locations, voxelMesh.VertexAmount, voxelMesh.getVertexSize());
		voxelMesh.setLocationAndScale(new Vector3(0.0f, 0.0f, 0.0f), 0.1f);

		projectionMatrix = new Matrix4Uniform("projectionMatrix");
		projectionMatrix.Matrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver2, 16.0f / 9.0f, 0.1f, 100f);

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
	
		drawMesh(voxelMesh);

		Error.checkGLError("Scene.drawScene");

	}

	public void updateScene(KeyboardState keyState)
	{
		camera.Update(keyState);
		viewMatrix.Matrix = camera.GetViewMatrix();

			//voxelMesh.rotate(0.01f);
	}
}
}