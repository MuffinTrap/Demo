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
class Scene
{
	Shader vertexShader;
	Shader fragmentShader;
	
	ShaderProgram shaderProgram;
	
	Mesh testMesh;
	Mesh objMesh;
	
	Matrix4Uniform projectionMatrix;
	Matrix4Uniform viewMatrix;
	
	public void loadScene()
	{
		vertexShader = Shader.CreateFromFile(ShaderType.VertexShader, "../shaders/vertex.vs");
		
		fragmentShader = Shader.CreateFromFile(ShaderType.FragmentShader, "../shaders/fragment.fs");
		
		shaderProgram = new ShaderProgram(vertexShader, fragmentShader);
		
		Error.checkGLError("Scene.loadScene");
		
		testMesh = Mesh.CreateTriangleMesh();
		
		objMesh = Mesh.CreateFromFile("../models/testbox.obj");

        shaderProgram.Use();
        testMesh.bufferData(shaderProgram.GetAttributeLocation("vPosition"));
		
		projectionMatrix = new Matrix4Uniform("projectionMatrix");
		projectionMatrix.Matrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver2, 16.0f / 9.0f, 0.1f, 100f);
		
		viewMatrix = new Matrix4Uniform("viewMatrix");
		viewMatrix.Matrix = Matrix4.CreateTranslation(0.0f, 0.0f, -10.0f);
		
		Error.checkGLError("Scene.loadScene");
	}
	
	public void drawScene()
	{
		shaderProgram.Use();
		
		projectionMatrix.Set(shaderProgram);
		viewMatrix.Set(shaderProgram);
		objMesh.worldMatrix.Set(shaderProgram);
		
		Error.checkGLError("Scene.drawScene");
		
		
		GL.BindVertexArray(objMesh.VAOHandle);
        GL.DrawArrays(PrimitiveType.Triangles, 0, objMesh.VertexAmount);
		
	}

}
}