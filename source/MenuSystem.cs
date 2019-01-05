
using System.IO;
using System.Collections.Generic;
using System;
using OpenTK;
using RocketNet;

namespace MuffinSpace
{
	public class Tunable
	{
		public string Name { get; set; }
		public string Value { get; set; }

		public Tunable(string nameParam, string valueParam)
		{
			Name = nameParam;
			Value = valueParam;
		}
	}

	public class TunableObject
	{
		public string Name { get; set; }
		public List<Tunable> tunables;

		public TunableObject(string nameParam)
		{
			Name = nameParam;
			tunables = new List<Tunable>();
		}

		public void AddTunable(string name, string value)
		{
			// Check if exists. because could be loaded from file
			foreach (Tunable t in tunables)
			{
				if (t.Name == name)
				{
					return;
				}
			}
			Tunable newT = new Tunable(name, value);
			tunables.Add(newT);
		}
	}

	public class TunableManager
	{
		private static TunableManager singleton = null;

		private List<TunableObject> tunables;

		private TunableManager()
		{
			tunables = new List<TunableObject>();
		}

		public static TunableManager GetSingleton()
		{
			if (singleton == null)
			{
				singleton = new TunableManager();
			}
			return singleton;
		}

		public void AddObject(string objectName)
		{
			tunables.Add(new TunableObject(objectName));
		}

		public void AddTunable(string name, string value)
		{
			tunables[tunables.Count - 1].AddTunable(name, value);
		}

		public void ReloadValues()
		{
			tunables.Clear();
			ReadSettings();
		}


		public float GetFloat(string name)
		{
			return Single.Parse(GetValue(name));
		}
		public Vector3 GetVec3(string name)
		{
			return readVector3(GetValue(name));
		}

		private string GetValue(string name)
		{
			string[] pathParts = name.Split('.');
			foreach (TunableObject o in tunables)
			{
				if (o.Name == pathParts[0])
				{
					foreach(Tunable t in o.tunables)
					{
						if (t.Name == pathParts[1])
						{
							return t.Value;
						}
					}
				}
			}
			Logger.LogError(Logger.ErrorState.Limited, "No such setting as: " + name);
			return "";
		}

		public static Vector3 readVector3(string vectorLine)
		{
			char split = ',';
			string[] values = vectorLine.Split(split);

			Vector3 vec = new Vector3();

			Logger.LogInfo("Reading vector3: " + vectorLine);
			if (values.Length == 3)  // #, #, #
			{
				// use . as separator instead of system default
				System.Globalization.NumberFormatInfo nfi = new System.Globalization.CultureInfo("en-US", false).NumberFormat;

				try
				{
					vec.X = Single.Parse(values[0], nfi);
					vec.Y = Single.Parse(values[1], nfi);
					vec.Z = Single.Parse(values[2], nfi);
				} catch (FormatException e)
				{
					Logger.LogError(Logger.ErrorState.Limited, string.Format("Value {0} Caught Formatexception:" + e.Message, values[1]));
				}
			}

			return vec;
		}

        private void ReadSettings()
		{
			StreamReader fileReader;
			fileReader = new StreamReader("settings.cfg");

			// Find all settings in the file
			char [] settingSplit = {' '};
			char [] valueSplit = {':'};

			string activeHeader = "";
			
			while (fileReader.EndOfStream == false)
			{
				string settingLine = fileReader.ReadLine();
				if (settingLine.Contains(";"))
				{
					// Comment
					continue;
				}

				// Header or value?
				if (settingLine.Contains("{"))
				{
					string[] headerLine = settingLine.Split(settingSplit);
					string headerName = headerLine[0].Trim();
					activeHeader = headerName;
					AddObject(activeHeader);
					Logger.LogInfo("Added setting group: " + settingLine);
				}
				
				string[] nameAndSetting = null;
				nameAndSetting = settingLine.Split(valueSplit);

				if (nameAndSetting.Length == 2)
				{
					string settingName = nameAndSetting[0].Trim();
					string settingValue = nameAndSetting[1].Trim();

					AddTunable(settingName, settingValue);
					Logger.LogInfo("Read setting: " + settingLine);
				}
				else if (nameAndSetting.Length > 0 && nameAndSetting[0].Length != 0)
				{
					Console.WriteLine("Invalid setting: " + nameAndSetting[0]);
				}
			}
				
			//
			fileReader.Close();
		}
	}
}