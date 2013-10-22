using UnityEngine;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Linq;
using System;
using Ionic.Zip;

namespace RTS
{
	public struct Prefabs
	{
		public Dictionary<string, BuildingPrefab> buildingPrefabs;
		public Dictionary<string, UnitPrefab> unitPrefabs;
	}

	public struct BuildingPrefab
	{
		public struct Upgrade
		{
			public string ID;
			public string name;
			public int cost;
			public int power;
			public int health;
			public int buildTime;
			public float productionMulti;
			public Mesh model;
			public Texture2D texture;
		}

		public string ID;
		public string name;
		public int type;
		public int menuID;
		public int cost;
		public int powerUsage;
		public int buildTime;
		public int health;
		public Vector2 miniMapSize;
		public Mesh model;
		public Texture2D texture;
		public Texture2D cameo;
		public List<string> techReqs;
		public List<Upgrade> techUpgrades;
		public bool dataItem;
		public int replace;
	}

	public struct UnitPrefab
	{
		public struct Upgrade
		{
			public string ID;
			public string name;
			public int cost;
			public int health;
			public int buildTime;
			public Mesh model;
			public Texture2D texture;
		}

		public string ID;
		public string name;
		public string factory;
		public int type;
		public int menuID;
		public int cost;
		public int buildTime;
		public int health;
		public float speed;
		public float turnSpeed;
		public float accel;
		public Vector2 miniMapSize;
		public Mesh model;
		public Texture2D texture;
		public Texture2D cameo;
		public List<string> techReqs;
		public List<Upgrade> techUpgrades;
		public bool dataItem;
	}

	public static class FileParser
	{
		public static ZipFile m_dataFile;

		public static void ParseFiles()
		{
			if (Application.isEditor)
				Compress();
			
			string basePath = Application.dataPath;
			string[] mods = Directory.GetFiles(basePath + "/mods", "*.xml");
			
			CryptoProvider crypto = new CryptoProvider();
			MemoryStream baseData = crypto.DecryptFileToStream(basePath + "/BaseData.Data", "Rens");
			m_dataFile = ZipFile.Read(baseData);

			foreach (string name in mods)
			{
				ParseXML(name, false);
			}
			
			foreach (ZipEntry entry in m_dataFile)
			{
				if (entry.FileName.EndsWith(".xml"))
				{
					ParseXML(entry.FileName, true);
				}
			}
		}

		public static void ParseXML(string a_name, bool a_zip)
		{
			XmlDocument xml = new XmlDocument();
			List<ZipEntry> entries = null;
			if (a_zip)
			{		   
				entries = m_dataFile.SelectEntries(a_name).ToList();
				xml.Load(entries[0].OpenReader());
			}
			else
			{
				xml.Load(a_name);
			}
				
			XmlNode node = xml.FirstChild;
			while (node != null && node.Name != "Building" && node.Name != "Unit")
			{
				node = node.NextSibling;
			}
			
			if (node == null)
				goto XMLError;
			
			if (node.Name == "Building")
			{		   
				// Name
				XmlAttributeCollection attribs = node.Attributes;
				if (attribs.Count != 1)
					goto XMLError;
				XmlNode attrib = attribs.GetNamedItem("ID");
				if (attrib == null)
					goto XMLError;
				string ID = attrib.Value;
				
				// Duplicate check
				if (Main.m_res.prefabs.buildingPrefabs.ContainsKey(ID))
				{
					Debug.LogWarning("Prefab overwrite attempt detected - possible mod: " + ID);
					return;
				}
				
				// Create prefab
				BuildingPrefab prefab = new BuildingPrefab();
				prefab.dataItem = a_zip;
				prefab.ID = ID;
				prefab.type = Building.Type.DEFAULT;
				
				// Properties
				XmlNodeList nodes = node.SelectNodes("Properties");
				if (nodes.Count != 1)
					goto XMLError;
				attribs = nodes[0].Attributes;
				if (attribs.Count < 7 || attribs.Count > 8)
					goto XMLError;
				attrib = attribs.GetNamedItem("Name");
				if (attrib == null)
					goto XMLError;
				prefab.name = attrib.Value;
				attrib = attribs.GetNamedItem("MenuID");
				if (attrib == null)
					goto XMLError;
				prefab.menuID = Int32.Parse(attrib.Value);
				attrib = attribs.GetNamedItem("Cost");
				if (attrib == null)
					goto XMLError;
				prefab.cost = Int32.Parse(attrib.Value);
				attrib = attribs.GetNamedItem("PowerUsage");
				if (attrib == null)
					goto XMLError;
				prefab.powerUsage = Int32.Parse(attrib.Value);
				attrib = attribs.GetNamedItem("BuildTime");
				if (attrib == null)
					goto XMLError;
				prefab.buildTime = Int32.Parse(attrib.Value);
				attrib = attribs.GetNamedItem("Health");
				if (attrib == null)
					goto XMLError;
				prefab.health = Int32.Parse(attrib.Value);
				attrib = attribs.GetNamedItem("MiniMapIconSize");
				if (attrib == null)
					goto XMLError;
				if (attrib.Value.Split(',').Length != 2)
					goto XMLError;
				prefab.miniMapSize = new Vector2(Int32.Parse(attrib.Value.Split(',')[0]), Int32.Parse(attrib.Value.Split(',')[1]));
				attrib = attribs.GetNamedItem("BuildingType");
				if (attrib != null)
				{
					if (attrib.Value == "HEADQUARTERS")
					  prefab.type = Building.Type.HEADQUARTERS;
					else if (attrib.Value == "POWERFACTORY")
					  prefab.type = Building.Type.POWERFACTORY;
					else if (attrib.Value == "UNITFACTORY")
					  prefab.type = Building.Type.UNITFACTORY;
				}
				
				// Tech	 
				prefab.techReqs = new List<string>();
				prefab.techUpgrades = new List<BuildingPrefab.Upgrade>();
				nodes = node.SelectNodes("Tech");
				if (nodes.Count != 1)
					goto XMLError;
				attribs = nodes[0].Attributes;
				attrib = attribs.GetNamedItem("ReplaceModel");
				if (attrib == null)
				{
					if (attrib.Value == "1")
						prefab.replace = 1;
				}
				else
				{
					prefab.replace = 0;
				}
				
				nodes = node.SelectNodes("Tech/TechReq");
				foreach (XmlNode n in nodes)
				{
					attribs = n.Attributes;
					if (attribs.Count != 1)
						goto XMLError;
					attrib = attribs.GetNamedItem("Value");
					if (attrib == null)
						goto XMLError;
					if (prefab.techReqs == null)
						prefab.techReqs = new List<string>();
					prefab.techReqs.Add(attrib.Value);
				}
				
				nodes = node.SelectNodes("Tech/Upgrade");
				foreach (XmlNode n in nodes)
				{
					attribs = n.Attributes;
					if (attribs.Count < 4)
						goto XMLError;
					if (prefab.techUpgrades == null)
						prefab.techUpgrades = new List<BuildingPrefab.Upgrade>();
					BuildingPrefab.Upgrade upgrade = new BuildingPrefab.Upgrade();
					upgrade.health = 0;
					upgrade.power = 0;
					upgrade.productionMulti = 1f;
					attrib = attribs.GetNamedItem("Name");
					if (attrib == null)
						goto XMLError;
					upgrade.name = attrib.Value;
					attrib = attribs.GetNamedItem("Cost");
					if (attrib == null)
						goto XMLError;
					upgrade.cost = Int32.Parse(attrib.Value);
					attrib = attribs.GetNamedItem("UpgradeTime");
					if (attrib == null)
						goto XMLError;
					upgrade.buildTime = Int32.Parse(attrib.Value);
					attrib = attribs.GetNamedItem("AdditionalHealth");
					if (attrib != null)
						upgrade.health = Int32.Parse(attrib.Value);
					attrib = attribs.GetNamedItem("AdditionalPowerUsage");
					if (attrib != null)
						upgrade.power = Int32.Parse(attrib.Value);
					attrib = attribs.GetNamedItem("ModelFile");
					if (attrib != null && attrib.Value.Count() > 0)
					{
						upgrade.model = LoadObj(attrib.Value, a_zip);
						if (upgrade.model == null)
							goto XMLError;
					}
					attrib = attribs.GetNamedItem("TextureFile");
					if (attrib != null && attrib.Value.Count() > 0)
					{
						upgrade.texture = LoadImage(attrib.Value, a_zip);
						if (upgrade.texture == null)
							goto XMLError;
					}
					prefab.techUpgrades.Add(upgrade);
				}

				// Model
				nodes = node.SelectNodes("Model");
				if (nodes.Count != 1)
					goto XMLError;
				attribs = nodes[0].Attributes;
				if (attribs.Count != 2)
					goto XMLError;
				attrib = attribs.GetNamedItem("ModelFileName");
				if (attrib == null || attrib.Value.Count() == 0)
					goto XMLError;
				prefab.model = LoadObj(attrib.Value, a_zip);
				if (prefab.model == null)
					goto XMLError;
				attrib = attribs.GetNamedItem("TextureFileName");
				if (attrib != null && attrib.Value.Count() > 0)
				{
					prefab.texture = LoadImage(attrib.Value, a_zip);
					if (prefab.texture == null)
						goto XMLError;
				}

				// Cameo
				nodes = node.SelectNodes("Cameo");
				if (nodes.Count != 1)
					goto XMLError;
				attribs = nodes[0].Attributes;
				if (attribs.Count != 1)
					goto XMLError;
				attrib = attribs.GetNamedItem("CameoFileName");
				if (attrib == null || attrib.Value.Count() == 0)
					goto XMLError;
				prefab.cameo = LoadImage(attrib.Value, a_zip);
				if (prefab.cameo == null)
					goto XMLError;
				
				Main.m_res.prefabs.buildingPrefabs.Add(ID, prefab);
			}
			else
			{
				// Name
				XmlAttributeCollection attribs = node.Attributes;
				if (attribs.Count != 1)
					goto XMLError;
				XmlNode attrib = attribs.GetNamedItem("ID");
				if (attrib == null)
					goto XMLError;
				string ID = attrib.Value;
				
				// Duplicate check
				if (Main.m_res.prefabs.buildingPrefabs.ContainsKey(ID))
				{
					Debug.LogWarning("Prefab overwrite attempt detected - possible mod: " + ID);
					return;
				}
				
				// Create prefab
				UnitPrefab prefab = new UnitPrefab();
				prefab.dataItem = a_zip;
				prefab.ID = ID;
				prefab.type = Unit.Type.TANK;
				
				// Properties
				XmlNodeList nodes = node.SelectNodes("Properties");
				if (nodes.Count != 1)
					goto XMLError;
				attribs = nodes[0].Attributes;
				if (attribs.Count != 11)
					goto XMLError;
				attrib = attribs.GetNamedItem("Name");
				if (attrib == null)
					goto XMLError;
				prefab.name = attrib.Value;
				attrib = attribs.GetNamedItem("Factory");
				if (attrib == null)
					goto XMLError;
				prefab.factory = attrib.Value;
				attrib = attribs.GetNamedItem("MenuID");
				if (attrib == null)
					goto XMLError;
				prefab.menuID = Int32.Parse(attrib.Value);
				attrib = attribs.GetNamedItem("Cost");
				if (attrib == null)
					goto XMLError;
				prefab.cost = Int32.Parse(attrib.Value);
				attrib = attribs.GetNamedItem("BuildTime");
				if (attrib == null)
					goto XMLError;
				prefab.buildTime = Int32.Parse(attrib.Value);
				attrib = attribs.GetNamedItem("Health");
				if (attrib == null)
					goto XMLError;
				prefab.health = Int32.Parse(attrib.Value);
				attrib = attribs.GetNamedItem("Speed");
				if (attrib == null)
					goto XMLError;
				prefab.speed = float.Parse(attrib.Value);
				attrib = attribs.GetNamedItem("TurnSpeed");
				if (attrib == null)
					goto XMLError;
				prefab.turnSpeed = float.Parse(attrib.Value);
				attrib = attribs.GetNamedItem("Acceleration");
				if (attrib == null)
					goto XMLError;
				prefab.accel = float.Parse(attrib.Value);
				attrib = attribs.GetNamedItem("MiniMapIconSize");
				if (attrib == null)
					goto XMLError;
				if (attrib.Value.Split(',').Length != 2)
					goto XMLError;
				prefab.miniMapSize = new Vector2(Int32.Parse(attrib.Value.Split(',')[0]), Int32.Parse(attrib.Value.Split(',')[1]));
				attrib = attribs.GetNamedItem("UnitType");
				if (attrib == null)
					goto XMLError;
				if (attrib.Value == "DOZER")
				  prefab.type = Unit.Type.DOZER;
				else if (attrib.Value == "INFANTRY")
				  prefab.type = Unit.Type.INFANTRY;
				else if (attrib.Value == "TANK")
				  prefab.type = Unit.Type.TANK;

				// Tech	 
				prefab.techReqs = new List<string>();
				prefab.techUpgrades = new List<UnitPrefab.Upgrade>();
				
				nodes = node.SelectNodes("Tech/TechReq");
				foreach (XmlNode n in nodes)
				{
					attribs = n.Attributes;
					if (attribs.Count != 1)
						goto XMLError;
					attrib = attribs.GetNamedItem("Value");
					if (attrib == null)
						goto XMLError;
					if (prefab.techReqs == null)
						prefab.techReqs = new List<string>();
					prefab.techReqs.Add(attrib.Value);
				}
				
				nodes = node.SelectNodes("Tech/Upgrade");
				foreach (XmlNode n in nodes)
				{
					attribs = n.Attributes;
					if (attribs.Count < 3)
						goto XMLError;
					if (prefab.techUpgrades == null)
						prefab.techUpgrades = new List<UnitPrefab.Upgrade>();
					UnitPrefab.Upgrade upgrade = new UnitPrefab.Upgrade();
					upgrade.health = 0;
					attrib = attribs.GetNamedItem("Name");
					if (attrib == null)
						goto XMLError;
					upgrade.name = attrib.Value;
					attrib = attribs.GetNamedItem("Cost");
					if (attrib == null)
						goto XMLError;
					upgrade.cost = Int32.Parse(attrib.Value);
					attrib = attribs.GetNamedItem("UpgradeTime");
					if (attrib == null)
						goto XMLError;
					upgrade.buildTime = Int32.Parse(attrib.Value);
					attrib = attribs.GetNamedItem("AdditionalHealth");
					if (attrib != null)
						upgrade.health = Int32.Parse(attrib.Value);
					attrib = attribs.GetNamedItem("ModelFile");
					if (attrib != null && attrib.Value.Count() > 0)
					{
						upgrade.model = LoadObj(attrib.Value, a_zip);
						if (upgrade.model == null)
							goto XMLError;
					}
					attrib = attribs.GetNamedItem("TextureFile");
					if (attrib != null && attrib.Value.Count() > 0)
					{
						upgrade.texture = LoadImage(attrib.Value, a_zip);
						if (upgrade.texture == null)
							goto XMLError;
					}
					prefab.techUpgrades.Add(upgrade);
				}

				// Model
				nodes = node.SelectNodes("Model");
				if (nodes.Count != 1)
					goto XMLError;
				attribs = nodes[0].Attributes;
				if (attribs.Count != 2)
					goto XMLError;
				attrib = attribs.GetNamedItem("ModelFileName");
				if (attrib == null || attrib.Value.Count() == 0)
					goto XMLError;
				prefab.model = LoadObj(attrib.Value, a_zip);
				if (prefab.model == null)
					goto XMLError;
				attrib = attribs.GetNamedItem("TextureFileName");
				if (attrib != null && attrib.Value.Count() > 0)
				{
					prefab.texture = LoadImage(attrib.Value, a_zip);
					if (prefab.texture == null)
						goto XMLError;
				}

				// Cameo
				nodes = node.SelectNodes("Cameo");
				if (nodes.Count != 1)
					goto XMLError;
				attribs = nodes[0].Attributes;
				if (attribs.Count != 1)
					goto XMLError;
				attrib = attribs.GetNamedItem("CameoFileName");
				if (attrib == null || attrib.Value.Count() == 0)
					goto XMLError;
				prefab.cameo = LoadImage(attrib.Value, a_zip);
				if (prefab.cameo == null)
					goto XMLError;
				
				Main.m_res.prefabs.unitPrefabs.Add(ID, prefab);
			}
			
			return;
			
			XMLError:
			Debug.LogError("Invalid XML file: " + a_name);
		}

		public static void Compress()
		{
			string basePath = Application.dataPath;
			
			ZipFile zip = new ZipFile();
			zip.AddDirectory(basePath + "/BaseData/");
			zip.Save(basePath + "/BaseData.Data.temp");
			
			CryptoProvider crypto = new CryptoProvider();
			crypto.EncryptFile(basePath + "/BaseData.Data.temp", basePath + "/BaseData.Data", "Rens");
			File.Delete(basePath + "/BaseData.Data.temp");
		}
		// Load an OBJ.
		public static Mesh LoadObj(string a_path, bool a_dataFile)
		{
			Mesh mesh = null;
			if (a_path.Count() == 0)
				return mesh;

			if (a_dataFile)
			{
				List<ZipEntry> entries = m_dataFile.SelectEntries(a_path).ToList();
				if (entries.Count == 1)
				{
					mesh = ObjImporter.ImportStream(entries[0].OpenReader());
				}
			}
			else
			{
				mesh = ObjImporter.ImportFile(a_path);
			}

			return mesh;
		}
		// Load an image.
		public static Texture2D LoadImage(string a_path, bool a_dataFile)
		{
			Texture2D tex = null;
			
			if (a_dataFile)
			{
				List<ZipEntry> entries = m_dataFile.SelectEntries(a_path).ToList();
				if (entries.Count == 1)
				{
					Ionic.Crc.CrcCalculatorStream stream = entries[0].OpenReader();
					byte[] texBytes = new byte[stream.Length];
					stream.Read(texBytes, 0, (int)stream.Length);
					tex = new Texture2D(0, 0);
					tex.LoadImage(texBytes);
				}
			}
			else
			{
				FileStream texFile = File.OpenRead(Application.dataPath + "/mods/" + a_path);
				if (texFile != null)
				{
					byte[] texBytes = new byte[texFile.Length];
					texFile.Read(texBytes, 0, (int)texFile.Length);
					tex = new Texture2D(0, 0);
					tex.LoadImage(texBytes);
				}
			}
			
			return tex;
		}
	}
}