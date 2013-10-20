using UnityEngine;
using System.Xml;
using System.IO;
using System.Collections.Generic;
using Ionic.Zip;
using System.Linq;

namespace RTS
{
	public class BuildingTemplate
	{		
		public Building Load(BuildingPrefab a_prefab)
		{						
			// Properties
			Building.Properties properties = new Building.Properties();
			properties.health = a_prefab.health;
			properties.power = a_prefab.powerUsage;
			properties.buildTime = a_prefab.buildTime;
			properties.cost = a_prefab.cost;		
			properties.miniSize = new Vector2(6f, 6f);		
			
			// Texture
			Texture2D texture = FileParser.LoadImage(a_prefab.texturePath, a_prefab.dataItem);
			Mesh mesh = null;
			
			if (a_prefab.dataItem)
			{
				// Model
				List<ZipEntry> entries = FileParser.m_dataFile.SelectEntries(a_prefab.modelPath).ToList();
				if (entries.Count == 1)
				{
					ObjImporter importer = new ObjImporter();
					mesh = importer.ImportStream(entries[0].OpenReader());
				}
			}
			else
			{
				// Model
				if (File.Exists(Application.dataPath + "/mods/" + a_prefab.modelPath))
				{
					ObjImporter importer = new ObjImporter();
					mesh = importer.ImportFile(Application.dataPath + "/mods/" + a_prefab.modelPath);
				}
			}
			
			// Type
			Building building = null;
			if (a_prefab.type == Building.Type.DEFAULT)
			{
				building = new Building(properties, mesh, texture);	
			}
			
			return building;
		}
	}
}
