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
	void loadScene();
	void drawScene();
	void updateScene(KeyboardState keyState);
}

class EmptyScene : IScene
{
	public void loadScene()
	{

	}
	public void drawScene()
	{

	}
	public void updateScene(KeyboardState keyState)
	{

	}
}

class RotatingScene : IScene
{
	Shader vertexShader;
	Shader fragmentShader;
	
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
	
	public void loadScene()
	{
		vertexShader = Shader.CreateFromFile(ShaderType.VertexShader, "../data/shaders/vertex.vs");
		
		fragmentShader = Shader.CreateFromFile(ShaderType.FragmentShader, "../data/shaders/fragment.fs");
		
		shaderProgram = new ShaderProgram(vertexShader, fragmentShader);
		
		Error.checkGLError("Scene.loadScene");

		shaderProgram.Use();
		Mesh.PositionDataIndex = shaderProgram.GetAttributeLocation("vPosition");
		Mesh.NormalDataIndex = shaderProgram.GetAttributeLocation("vNormal");
		Mesh.TexCoordDataIndex = shaderProgram.GetAttributeLocation("vTexCoord");
		Mesh.ColorDataIndex = shaderProgram.GetUniformLocation("uDiffuseColor");
		Mesh.ScaleDataIndex = shaderProgram.GetUniformLocation("uScale");
		shaderProgram.setSamplerUniform("inputTexture", 0);

		//origoTriangle = addMesh(Mesh.CreateTriangleMesh(), new Vector3(0, 0, 0), new Color4(1.0f, 1.0f, 1.0f, 1.0f), 0.1f);
		//xTriangle = addMesh(Mesh.CreateTriangleMesh(), new Vector3(1, 0, 0), new Color4(1, 0.2f, 0.2f, 1), 0.1f);
		//zTriangle = addMesh(Mesh.CreateTriangleMesh(), new Vector3(0, 0, 1), new Color4(0.2f, 1, 0.2f, 1), 0.1f);
		//yTriangle = addMesh(Mesh.CreateTriangleMesh(), new Vector3(0, 1, 0), new Color4(0.2f, 0.2f, 1, 1), 0.1f);

		voxelMesh = addMesh(Mesh.CreateFromFile("../data/models/monu9.obj"), new Vector3(0.0f, 0.0f, 0.0f), new Color4(1.0f, 1.0f, 1.0f, 1.0f), 0.1f);

		projectionMatrix = new Matrix4Uniform("projectionMatrix");
		projectionMatrix.Matrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver2, 16.0f / 9.0f, 0.1f, 100f);

		cameraPosition = new Vector3(0, -5, -10);
		cameraDirection = new Vector3(0, 0, 1.0f);
		sceneUp = new Vector3(0.0f, 1.0f, 0.0f);

		viewMatrix = new Matrix4Uniform("viewMatrix");
		viewMatrix.Matrix = Matrix4.CreateTranslation(cameraPosition);

		GL.Enable(EnableCap.DepthTest);
		GL.DepthFunc(DepthFunction.Less);

		Error.checkGLError("Scene.loadScene");
	}

	public Mesh addMesh(Mesh meshData, Vector3 position, Color4 color, float scale)
	{
			Mesh newMesh = meshData;
			newMesh.bufferData();
			newMesh.WorldPosition = position;
			newMesh.DiffuseColor = color;
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

		//drawMesh(zTriangle);
		//drawMesh(origoTriangle);
		//drawMesh(xTriangle);
		//
		//drawMesh(yTriangle);
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