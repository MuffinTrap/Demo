using System;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace MuffinSpace
{
	public class Scene
	{
		public string name;
		List<DrawableMesh> drawables;
		List<Light> lights;

		public Scene()
		{
			drawables = new List<DrawableMesh>();
			lights = new List<Light>();
		}

		public void AddDrawable(DrawableMesh drawableItem)
		{
			if (!drawables.Contains(drawableItem))
			{
				drawables.Add(drawableItem);
			}
		}

		public void AddLight(Light light)
		{
			if (!lights.Contains(light))
			{
				lights.Add(light);
			}
		}
	}
}