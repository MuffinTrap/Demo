using System;
using System.IO;
using System.Collections.Generic;

namespace MegaCars
{
	class Setting
	{
		public string SettingName { get; }
		public bool Found {get; set;}
		public float Value {get; set;}
		
		
		public Setting(string name)
		{
			SettingName = name;
			Found = false;
			Value = 0.0f;
		}
	}
	
	class SettingCollection
	{
		Dictionary<string, Setting> settings;

		public SettingCollection()
		{
			settings = new Dictionary<string, Setting>();	
		}
		
		public void AddSetting(string name)
		{
			settings.Add(name, new Setting(name));
		}
		
		public Setting GetSetting(string name)
		{
			return settings[name];
		}
	}
	
    class FileReader
    {
        public void ReadSettings(ref CarSettings settings)
		{
			StreamReader fileReader;
			fileReader = new StreamReader("settings.txt");

			// Find all settings in the file
			char [] split = { ':'};
			
			while (fileReader.EndOfStream == false)
			{
				string settingLine = fileReader.ReadLine();
				
				string[] nameAndSetting = null;
				nameAndSetting = settingLine.Split(split);

				if (nameAndSetting.Length == 2)
				{
					Setting s = settings.GetSetting(nameAndSetting[0].Trim());
					if (s != null)
					{
						float settingValue = Convert.ToSingle(nameAndSetting[1].Trim());
						s.Value = settingValue;
						
						Console.WriteLine("Read setting: " + settingLine);
					}
					else 
					{
						Console.WriteLine("Setting not found: " + nameAndSetting[0]);
					}
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
