using System;
using System.Collections.Generic;
using OpenTK;

namespace MuffinSpace
{
	public class TextGenerator
	{
		public TextGenerator GetSingleton()
		{
			if (singleton == null)
			{
				singleton = new TextGenerator();
			}
			return singleton;
		}
		private static TextGenerator singleton = null;

		static Dictionary<char, Vector2> uvCoords;

		private static int column = 0;
		private static int row = 0;

		private TextGenerator()
		{
			column = 0;
			row = 0;
			GenerateUVs();
		}

		public List<Vector2> GetUVsOfString(string message)
		{
			List<Vector2> uvs = new List<Vector2>();
			foreach(char c in message)
			{
				uvs.Add(uvCoords[c]);
			}
			return uvs;
		}

		public static void GenerateUVs()
		{
			uvCoords = new Dictionary<char, Vector2>();
			Add('!');
			Add('\"');
			Add('#');
			Add('$');
			Add('%');
			Add('&');
			Add('\'');
			Add('(');
			Add(')');
			Add('*');

			column = 0;
			row++;

			Add('+');
			Add(',');
			Add('-');
			Add('.');
			Add('/');
			Add('0');
			Add('1');
			Add('2');
			Add('3');
			Add('4');
			Add('5');
			Add('6');
			Add('7');
			Add('8');

			column = 0;
			row++;

			Add('9');
			Add(':');
			Add(';');
			Add('<');
			Add('=');
			Add('>');
			Add('?');
			Add('@');

			column = 0;
			row++;

			Add('A');
			Add('B');
			Add('C');
			Add('D');
			Add('E');
			Add('F');
			Add('G');
			Add('H');
			Add('I');
			Add('J');
			Add('K');
			Add('L');
			Add('M');
			Add('N');

			column = 0;
			row++;

			Add('O');
			Add('P');
			Add('Q');
			Add('R');
			Add('S');
			Add('T');
			Add('U');
			Add('V');
			Add('W');
			Add('X');
			Add('Y');
			Add('Z');

			column = 0;
			row++;

			Add('[');
			Add('\\');
			Add(']');
			Add('^');
			Add('_');
			Add('`');

			Add('a');
			Add('b');
			Add('c');
			Add('d');
			Add('e');
			Add('f');
			Add('g');
			Add('h');

			column = 0;
			row++;

			Add('i');
			Add('j');
			Add('k');
			Add('l');
			Add('m');
			Add('n');
			Add('o');
			Add('p');
			Add('q');
			Add('r');
			Add('s');
			Add('t');
			Add('u');
			Add('v');

			column = 0;
			row++;

			Add('w');
			Add('x');
			Add('y');
			Add('z');

			Add('{');
			Add('|');
			Add('}');
			Add('~');

			// Following are all empty character
			row++;
			column = 0;
			Add(' ');
			Vector2 emptyUV = uvCoords[' '];
			uvCoords.Add('\t', emptyUV);
			uvCoords.Add('\r', emptyUV);
			uvCoords.Add('\r', emptyUV);
			uvCoords.Add('\f', emptyUV);
			uvCoords.Add('\r', emptyUV);
		}
		private static void Add(char cha)
		{
			
			float imageWidth = 1024;
			float imageHeigth = 1024;
			float letterSize = 90;

			Vector2 uv = new Vector2((column * letterSize) / imageWidth
			, 1.0f - row / imageHeigth);

			column++;

			uvCoords.Add(cha, uv);
		}
	}
}