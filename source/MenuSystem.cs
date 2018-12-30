
using System.Collections.Generic;
using OpenTK;
using RocketNet;

namespace MuffinSpace
{
	public class Tunable
	{
		public string name;
		private float tunableValue;
		public float Value
		{
			get
			{
				return tunableValue;

			}
			set
			{
				value = MathHelper.Clamp(value, min, max);
			}
		}
		public float min;
		public float max;

		public Tunable(string nameParam, float minParam, float valueParam, float maxParam)
		{
			name = nameParam;
			min = minParam;
			max = maxParam;
			tunableValue = MathHelper.Clamp(valueParam, min, max);
		}
	}

	public class TunableGroup
	{
		int id = 0;
		public int Id
		{
			get
			{
				return id;
			}
			private set
			{
				if (value > 1)
				{
					id = value;
				}
			}
		}
		int parentId = 0;
		private string name;
		public string Name
		{
			get
			{
				return name;
			}
			private set {
				if (value.Length > 0)
				{
					name = value;
				}
			}
		}
		List<Tunable> tunables;
		public List<int> subGroups;

		public TunableGroup(int idParam, int parentIdParam, string nameParam)
		{
			id = idParam;
			parentId = parentIdParam;
			name = nameParam;

			tunables = new List<Tunable>();
			subGroups = new List<int>();
		}


		public Tunable AddTunable(string name, float min, float startValue, float max)
		{
			// Check if exists. because could be loaded from file
			foreach (Tunable t in tunables)
			{
				if (t.name == name)
				{
					return t;
				}
			}
			Tunable newT = new Tunable(name, min, startValue, max);
			tunables.Add(newT);
			return newT;
		}
	
		public void AddGroup(int groupId)
		{
			if (!subGroups.Contains(groupId))
			{
				subGroups.Add(groupId);
			}
		}
	}

	public class MenuSystem
	{
		private static MenuSystem singleton = null;
		private int rootGroupId = 1;
		private int idCounter = 2;
		public static int GetInvalidId()
		{
			return -1;
		}

		private Dictionary<int, TunableGroup> groups;

		public int GetRootGroupId()
		{
			return rootGroupId;
		}

		private MenuSystem()
		{
			groups = new Dictionary<int, TunableGroup>();
			TunableGroup rootGroup = new TunableGroup(rootGroupId, 0, "Root");
			groups.Add(rootGroupId, rootGroup);
		}

		public static MenuSystem GetSingleton()
		{
			if (singleton == null)
			{
				singleton = new MenuSystem();
			}
			return singleton;
		}

		public Tunable CreateTunable(int parentGroup, string name, float min, float startValue, float max)
		{
			if (groups.ContainsKey(parentGroup))
			{
				TunableGroup parent = groups[parentGroup];
				return parent.AddTunable(name, min, startValue, max);
			}
			return null;
		}

		public bool ContainsGroup(int groupId, string groupName)
		{
			return (GetGroupId(groupId, groupName) != GetInvalidId());
		}

		public int GetGroupId(int parentGroup, string groupName)
		{
			TunableGroup parent = groups[parentGroup];
			foreach(int id in parent.subGroups)
			{
				TunableGroup subGroup = groups[id];
				if (subGroup.Name == groupName)
				{
					return subGroup.Id;
				}
			}
			return GetInvalidId();
		}

		public int CreateTunableGroup(int parentGroup, string name)
		{
			if (groups.ContainsKey(parentGroup))
			{
				int extId = GetGroupId(parentGroup, name);
				if (extId == GetInvalidId())
				{
					int newId = idCounter;
					idCounter += 1;
					TunableGroup newGroup = new TunableGroup(newId, parentGroup, name);
					groups.Add(newId, newGroup);
					TunableGroup parent = groups[parentGroup];
					parent.AddGroup(newId);
					return newId;
				}
				else
				{
					return extId;
				}
			}
			return -1;
		}

		// Writes all tunables and their values and relationships
		// to a file
		public bool WriteToFile(string filenameAndPath)
		{
			return false;
		}

		// Reads a file and creates tunables defined therein 
		public bool ReadFromFile(string filenameAndPath)
		{
			return false;
		}
	}
}