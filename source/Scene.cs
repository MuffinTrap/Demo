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

	Mesh origoTriangle;
	Mesh xTriangle;
	Mesh zTriangle;
	Mesh yTriangle;

	Mesh voxelMesh;
	
	Matrix4Uniform projectionMatrix;
	Matrix4Uniform viewMatrix;

	// Camera
	Vector3 cameraPosition;
	Vector3 cameraDirection;
	Vector3 sceneUp;

	public RotatingScene()
	{
		
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

		origoTriangle = addMesh(Mesh.CreateTriangleMesh(assetManager), new Vector3(0, 0, 0), 1.0f, locations);
		xTriangle = addMesh(Mesh.CreateTriangleMesh(assetManager), new Vector3(1, 0, 0),  1.0f, locations);
		zTriangle = addMesh(Mesh.CreateTriangleMesh(assetManager), new Vector3(0, 0, 1),  1.0f, locations);
		yTriangle = addMesh(Mesh.CreateTriangleMesh(assetManager), new Vector3(0, 1, 0), 1.0f, locations);

		Mesh monuMesh = assetManager.GetMesh("monu9.obj");
		voxelMesh = addMesh(monuMesh, new Vector3(0.0f, 0.0f, 0.0f), 0.1f, locations);

		projectionMatrix = new Matrix4Uniform("projectionMatrix");
		projectionMatrix.Matrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver2, 16.0f / 9.0f, 0.1f, 100f);

		cameraPosition = new Vector3(0.0f, 0.0f, -10.0f);
		cameraDirection = new Vector3(0, 0, 1.0f);
		sceneUp = new Vector3(0.0f, 1.0f, 0.0f);

		viewMatrix = new Matrix4Uniform("viewMatrix");
		viewMatrix.Matrix = Matrix4.CreateTranslation(cameraPosition);

		GL.Enable(EnableCap.DepthTest);
		GL.DepthFunc(DepthFunction.Less);

		Error.checkGLError("Scene.loadScene");
	}

	public Mesh addMesh(Mesh meshData, Vector3 position, float scale, Mesh.AttributeLocations locations)
	{
		Mesh newMesh = meshData;
		newMesh.bufferData(locations);
		newMesh.WorldPosition = position;
		newMesh.Scale = scale;

		return newMesh;
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

		drawMesh(zTriangle);
		drawMesh(origoTriangle);
		drawMesh(xTriangle);
		
		drawMesh(yTriangle);
		//
		drawMesh(voxelMesh);

		Error.checkGLError("Scene.drawScene");

	}

	public void updateScene(KeyboardState keyState)
	{
		float cameraSpeed = 0.01f;
		if (keyState.IsKeyDown(key: Key.Up) || keyState.IsKeyDown(Key.W))
		{
			cameraPosition += cameraDirection * cameraSpeed;
		}
		else if (keyState.IsKeyDown(Key.Down) || keyState.IsKeyDown(Key.S))
		{
	        cameraPosition -= cameraDirection * cameraSpeed;
		}

		if (keyState.IsKeyDown(key: Key.Left) || keyState.IsKeyDown(Key.A))
		{
			cameraPosition -= Vector3.Cross(cameraDirection, sceneUp) * cameraSpeed;
		}
		else if (keyState.IsKeyDown(Key.Right) || keyState.IsKeyDown(Key.D))
		{
			cameraPosition += Vector3.Cross(cameraDirection, sceneUp) * cameraSpeed;
		}

		if (keyState.IsKeyDown(key: Key.R) )
		{
			cameraPosition -=  sceneUp * cameraSpeed;
		}
		else if (keyState.IsKeyDown(Key.F) )
		{
			cameraPosition += sceneUp * cameraSpeed;
		}

		viewMatrix.Matrix = Matrix4.CreateTranslation(cameraPosition);

		float rotationSpeed = 0.01f;
		voxelMesh.rotate(rotationSpeed);

	}
}
}