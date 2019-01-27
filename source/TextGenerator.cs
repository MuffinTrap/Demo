using System;
using System.Collections.Generic;
using OpenTK;

namespace MuffinSpace
{
	public class TextGenerator
	{
		public static TextGenerator GetSingleton()
		{
			if (singleton == null)
			{
				singleton = new TextGenerator();
			}
			return singleton;
		}
		private static TextGenerator singleton = null;

      private TextGenerator()
      {
          fonts = new List<PixelFont>();
		  fonts.Add(new PixelFont("lucida", 512f, 256, 32, 32));
      }

      private List<PixelFont> fonts;

      public PixelFont GetFont(string fontName)
      {
          foreach (PixelFont f in fonts)
          {
              if (f.name == fontName)
              {
                  return f;
              }
          }
			Logger.LogError(Logger.ErrorState.Critical, "No font found with name " + fontName);
          return null;
      }

  }

    public class PixelFont
    {
		public string name;
        private Dictionary<char, Vector2> uvCoords;

        private static int column = 0;
        private static int row = 0;
        float imageWidth;
        float imageHeight;
        float letterWidth;
        float letterHeight;

        private Vector2 letterUVSize;

        public PixelFont(string nameParam, float textureWidth, float textureHeight, float letterWidthParam, float letterHeightParam)
        {
            name = nameParam;
            imageWidth = textureWidth;
            imageHeight = textureHeight;
			letterWidth = letterWidthParam;
			letterHeight = letterHeightParam;

			letterUVSize = new Vector2(letterWidth / imageWidth, letterHeight / imageHeight);
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

        public Vector2 GetLetterUVSize()
        {
			return letterUVSize;
        }

		public void GenerateUVs()
		{
			Logger.LogInfo("Generating UVs for characters");
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
			Add('+');
			Add(',');
			Add('-');
			Add('.');
			Add('/');
			column = 0;
			row++;

			Add('0');
			Add('1');
			Add('2');
			Add('3');
			Add('4');
			Add('5');
			Add('6');
			Add('7');
			Add('8');
			Add('9');
			Add(':');
			Add(';');
			Add('<');
			Add('=');
			Add('>');
			Add('?');
			column = 0;
			row++;

			Add('@');
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
			Add('O');
			column = 0;
			row++;

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
			Add('[');
			Add('\\');
			Add(']');
			Add('^');
			Add('_');
			column = 0;
			row++;

			Add('`');
			Add('a');
			Add('b');
			Add('c');
			Add('d');
			Add('e');
			Add('f');
			Add('g');
			Add('h');
			Add('i');
			Add('j');
			Add('k');
			Add('l');
			Add('m');
			Add('n');
			Add('o');
			column = 0;
			row++;


			Add('p');
			Add('q');
			Add('r');
			Add('s');
			Add('t');
			Add('u');
			Add('v');
			Add('w');
			Add('x');
			Add('y');
			Add('z');
			Add('{');
			Add('|');
			Add('}');
			Add('~');

			Add(' ');
		}
      private void Add(char cha)
      {
			float U = (column * letterWidth) / imageWidth; 
			float V = 1.0f - (row * letterHeight) / imageHeight;
			Vector2 uv = new Vector2(U, V);

        column++;
		
		if (uvCoords.ContainsKey(cha))
		{
			Logger.LogError(Logger.ErrorState.Critical, "Character '" + cha + "' is already added to font");
			return;
		}
			//Logger.LogInfo("Char " + cha + " : (" + uv.X + ", " + uv.Y + ")");
        uvCoords.Add(cha, uv);
      }
    }
}
