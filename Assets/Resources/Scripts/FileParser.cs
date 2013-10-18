using UnityEngine;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Linq;
using System;
using Ionic.Zip;

namespace RTS
{	
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
			public string modelPath;
			public string texturePath;
		}
		
		public string name;
		public int cost;
		public int powerUsage;
		public int buildTime;
		public int health;
		public Vector2 miniMapSize;
		public Vector3 bounds;
		public string modelPath;
		public string texturePath;
		public string cameoPath;
		public List<string> techReqs;
		public List<Upgrade> techUpgrades;
		public bool dataItem;
		public int replace;
	}
	
	struct UnitPrefab
	{
	}
	
	public partial class Main : MonoBehaviour 
	{
		ZipFile m_dataFile;
		
		Dictionary<string, BuildingPrefab> m_buildingPrefabs = new Dictionary<string, BuildingPrefab>();		
		Dictionary<string, UnitPrefab> m_unitPrefabs = new Dictionary<string, UnitPrefab>();
		
		public void ParseFiles()
		{
			if (Application.isEditor)
				Compress();
			
			string basePath = Application.dataPath;
			string[] mods = Directory.GetFiles(basePath + "/mods", "*.xml");
			
			CryptoProvider crypto = new CryptoProvider();
			MemoryStream baseData = crypto.DecryptFileToStream(basePath + "/BaseData.Data", "Rens");
			m_dataFile = ZipFile.Read(baseData);
			//baseData.Flush();
			//baseData.Close();
			
			foreach(string name in mods)
			{
				ParseXML(name, false);
			}
			
			foreach(ZipEntry entry in m_dataFile)
			{
				if (entry.FileName.EndsWith(".xml"))
				{
					ParseXML(entry.FileName, true);
				}
			}
		}
		
		public void ParseXML(string a_name, bool a_zip)
		{
			XmlDocument xml = new XmlDocument();
			if (a_zip)
			{			
				List<ZipEntry> entries = m_dataFile.SelectEntries(a_name).ToList();
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
			
			if (node == null) goto XMLError;
			
			if (node.Name == "Building")
			{
				BuildingPrefab prefab = new BuildingPrefab();
				prefab.dataItem = a_zip;
				
				// Name
				XmlAttributeCollection attribs = node.Attributes;
				if (attribs.Count != 1) goto XMLError;
				XmlNode attrib = attribs.GetNamedItem("ID");
				if (attrib == null) goto XMLError;
				string ID = attrib.Value;
				
				// Duplicate check
				if (m_buildingPrefabs.ContainsKey(ID))
				{
					Debug.LogWarning("Prefab overwrite attempt detected - possible mod: " + ID);
					return;
				}
				
				// Properties
				XmlNodeList nodes = node.SelectNodes("Properties");
				if (nodes.Count != 1) goto XMLError;
				attribs = nodes[0].Attributes;
				if (attribs.Count != 7) goto XMLError;
				attrib = attribs.GetNamedItem("Name");
				if (attrib == null) goto XMLError;
				prefab.name = attrib.Value;
				attrib = attribs.GetNamedItem("Cost");
				if (attrib == null) goto XMLError;
				prefab.cost = Int32.Parse(attrib.Value);
				attrib = attribs.GetNamedItem("PowerUsage");
				if (attrib == null) goto XMLError;
				prefab.powerUsage = Int32.Parse(attrib.Value);
				attrib = attribs.GetNamedItem("BuildTime");
				if (attrib == null) goto XMLError;
				prefab.buildTime = Int32.Parse(attrib.Value);
				attrib = attribs.GetNamedItem("Health");
				if (attrib == null) goto XMLError;
				prefab.health = Int32.Parse(attrib.Value);
				attrib = attribs.GetNamedItem("MiniMapIconSize");
				if (attrib == null) goto XMLError;
				if (attrib.Value.Split(',').Length != 2) goto XMLError;
				prefab.miniMapSize = new Vector2(Int32.Parse(attrib.Value.Split(',')[0]), Int32.Parse(attrib.Value.Split(',')[1]));
				attrib = attribs.GetNamedItem("Bounds");
				if (attrib == null) goto XMLError;
				if (attrib.Value.Split(',').Length != 3) goto XMLError;
				prefab.bounds = new Vector3(float.Parse(attrib.Value.Split(',')[0]), float.Parse(attrib.Value.Split(',')[1]), float.Parse(attrib.Value.Split(',')[2]));
				
				// Tech		
				nodes = node.SelectNodes("Tech");
				if (nodes.Count != 1) goto XMLError;
				attribs = nodes[0].Attributes;
				attrib = attribs.GetNamedItem("ReplaceModel");
				if (attrib == null) {
					if (attrib.Value == "1") prefab.replace = 1;
				} else {
					prefab.replace = 0;
				}
				
				nodes = node.SelectNodes("Tech/TechReqs");
				foreach (XmlNode n in nodes)
				{
					attribs = n.Attributes;
					if (attribs.Count != 1) goto XMLError;
					attrib = attribs.GetNamedItem("Value");
					if (attrib == null) goto XMLError;
					if (prefab.techReqs == null)
						prefab.techReqs = new List<string>();
					prefab.techReqs.Add(attrib.Value);
				}
				
				nodes = node.SelectNodes("Tech/Upgrade");
				foreach (XmlNode n in nodes)
				{
					attribs = n.Attributes;
					if (attribs.Count < 4) goto XMLError;
					if (prefab.techUpgrades == null)
						prefab.techUpgrades = new List<BuildingPrefab.Upgrade>();
					BuildingPrefab.Upgrade upgrade = new BuildingPrefab.Upgrade();
					attrib = attribs.GetNamedItem("Name");
					if (attrib == null) goto XMLError;
					upgrade.name = attrib.Value;
					attrib = attribs.GetNamedItem("Cost");
					if (attrib == null) goto XMLError;
					upgrade.cost = Int32.Parse(attrib.Value);
					attrib = attribs.GetNamedItem("UpgradeTime");
					if (attrib == null) goto XMLError;
					upgrade.buildTime = Int32.Parse(attrib.Value);
					attrib = attribs.GetNamedItem("AdditionalHealth");
					if (attrib != null)
						upgrade.health = Int32.Parse(attrib.Value);
					attrib = attribs.GetNamedItem("AdditionalPowerUsage");
					if (attrib != null)
						upgrade.power = Int32.Parse(attrib.Value);
					attrib = attribs.GetNamedItem("ModelFile");
					if (attrib != null)
						upgrade.modelPath = attrib.Value;
					attrib = attribs.GetNamedItem("TextureFile");
					if (attrib != null)
						upgrade.texturePath = attrib.Value;
					
					prefab.techUpgrades.Add(upgrade);
				}
				
				// Model
				nodes = node.SelectNodes("Model");
				if (nodes.Count != 1) goto XMLError;
				attribs = nodes[0].Attributes;
				if (attribs.Count != 2) goto XMLError;
				attrib = attribs.GetNamedItem("ModelFileName");
				if (attrib == null) goto XMLError;
				prefab.modelPath = attrib.Value;
				attrib = attribs.GetNamedItem("TextureFileName");
				if (attrib != null)
					prefab.texturePath = attrib.Value;
				
				// Cameo
				nodes = node.SelectNodes("Cameo");
				if (nodes.Count != 1) goto XMLError;
				attribs = nodes[0].Attributes;
				if (attribs.Count != 1) goto XMLError;
				attrib = attribs.GetNamedItem("CameoFileName");
				if (attrib == null) goto XMLError;
				prefab.cameoPath = attrib.Value;
				
				m_buildingPrefabs.Add(ID, prefab);
			}
			else
			{
				UnitPrefab prefab = new UnitPrefab();
				string ID = "";
				
				m_unitPrefabs.Add(ID, prefab);
			}
			
			return;
			
			XMLError:
				Debug.LogError("Invalid XML file: " + a_name);
		}
		
		public void Compress()
		{
			string basePath = Application.dataPath;
			
			ZipFile zip = new ZipFile();
		    zip.AddDirectory(basePath + "/BaseData/");
			zip.Save(basePath + "/BaseData.Data.temp");
			
			CryptoProvider crypto = new CryptoProvider();
			crypto.EncryptFile(basePath + "/BaseData.Data.temp", basePath + "/BaseData.Data", "Rens");
			File.Delete(basePath + "/BaseData.Data.temp");
		}
	}
}