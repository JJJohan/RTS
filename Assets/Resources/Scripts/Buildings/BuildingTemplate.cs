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
		public Texture2D m_cameoResource;
		
		public void Load(BuildingPrefab a_prefab, ref ZipFile a_dataFile)
		{			
			m_mesh = gameObject.AddComponent<MeshFilter>();
			m_renderer = gameObject.AddComponent<MeshRenderer>();
			
			// Properties
			m_totalHealth = a_prefab.health;
			m_power = a_prefab.powerUsage;
			m_buildTime = a_prefab.buildTime;
			m_cost = a_prefab.cost;		
			m_miniSize = new Vector2(6f, 6f);
			
			if (a_prefab.dataItem)
			{
				// Model
				List<ZipEntry> entries = a_dataFile.SelectEntries(a_prefab.modelPath).ToList();
				if (entries.Count == 1)
				{
					ObjImporter importer = new ObjImporter();
					m_mesh.mesh = importer.ImportStream(entries[0].OpenReader());
				}
				
				// Texture
				entries = a_dataFile.SelectEntries(a_prefab.texturePath).ToList();
				if (entries.Count == 1)
				{
					Ionic.Crc.CrcCalculatorStream stream = entries[0].OpenReader();
					byte[] texBytes = new byte[stream.Length];
					stream.Read(texBytes, 0, (int)stream.Length);
					Texture2D tex = new Texture2D(0, 0);
					tex.LoadImage(texBytes);
					renderer.material.mainTexture = tex;
					m_renderer.material.shader = Shader.Find("Diffuse");
				}
				
				// Cameo
				entries = a_dataFile.SelectEntries(a_prefab.texturePath).ToList();
				if (entries.Count == 1)
				{
					Ionic.Crc.CrcCalculatorStream stream = entries[0].OpenReader();
					byte[] cameoBytes = new byte[stream.Length];
					stream.Read(cameoBytes, 0, (int)stream.Length);
					m_cameo = new Texture2D(0, 0);	
					m_cameo.LoadImage(cameoBytes);
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
				
				// Texture
				if (a_prefab.texturePath != "")
				{
					FileStream texFile = File.OpenRead(Application.dataPath + "/mods/" + a_prefab.texturePath);
					if (texFile != null)
					{
						byte[] texBytes = new byte[texFile.Length];
						texFile.Read(texBytes, 0, (int)texFile.Length);
						Texture2D tex = new Texture2D(0, 0);
						tex.LoadImage(texBytes);
						m_renderer.material.mainTexture = tex;
						m_renderer.material.shader = Shader.Find("Diffuse");
					}
				}
				
				// Cameo
				if (a_prefab.cameoPath != "")
				{
					FileStream cameoFile = File.OpenRead(Application.dataPath + "/mods/" + a_prefab.cameoPath);
					if (cameoFile != null)
					{
						byte[] cameoBytes = new byte[cameoFile.Length];
						cameoFile.Read(cameoBytes, 0, (int)cameoFile.Length);
						m_cameo = new Texture2D(0, 0);	
						m_cameo.LoadImage(cameoBytes);
					}
				}
			}
		}
	}
}
