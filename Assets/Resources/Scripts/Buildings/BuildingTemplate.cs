using UnityEngine;
using System.Xml;
using System.IO;
using System.Collections.Generic;
using Ionic.Zip;
using System.Linq;

namespace RTS
{
	public class BuildingTemplate : Building
	{		
		public void Load(BuildingPrefab a_prefab, ref ZipFile a_dataFile)
		{			
			m_mesh = gameObject.AddComponent<MeshFilter>();
			m_renderer = gameObject.AddComponent<MeshRenderer>();
			gameObject.name = a_prefab.name;
				
			// Properties
			m_totalHealth = a_prefab.health;
			m_power = a_prefab.powerUsage;
			m_buildTime = a_prefab.buildTime;
			m_cost = a_prefab.cost;		
			m_miniSize = new Vector2(6f, 6f);
			
			// Texture
			renderer.material.mainTexture = Main.LoadImage(a_prefab.texturePath, a_prefab.dataItem);
			m_renderer.material.shader = Shader.Find("Diffuse");
			
			if (a_prefab.dataItem)
			{
				// Model
				List<ZipEntry> entries = a_dataFile.SelectEntries(a_prefab.modelPath).ToList();
				if (entries.Count == 1)
				{
					ObjImporter importer = new ObjImporter();
					m_mesh.mesh = importer.ImportStream(entries[0].OpenReader());
				}
			}
			else
			{
				// Model
				if (File.Exists(Application.dataPath + "/mods/" + a_prefab.modelPath))
				{
					ObjImporter importer = new ObjImporter();
					m_mesh.mesh = importer.ImportFile(Application.dataPath + "/mods/" + a_prefab.modelPath);
				}
			}
			
			base.Init();
		}
	}
}
