using UnityEngine;
using System.IO;
using Ionic.Zip;
using System.Collections.Generic;
using System.Linq;

namespace RTS
{
	// Ghost building when placing a new builing.	
	public class GhostBuilding : MonoBehaviour
	{
		private bool m_placeable;
		private bool m_air;
		private Vector2 m_dims;
		private Vector3[] m_points;
		private bool m_copy = false;
		public BuildingPrefab m_prefab;
		
		public bool Placeable() { return (m_placeable && !m_air); }
		
		// Create ghost building.
		public void Create(BuildingPrefab a_prefab)
		{
			MeshFilter filter = gameObject.AddComponent<MeshFilter>();
			MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();
			Rigidbody rigid = gameObject.AddComponent<Rigidbody>();
			BoxCollider collider = gameObject.AddComponent<BoxCollider>();
			m_prefab = a_prefab;
			gameObject.name = "Cursor Building";
			
			// Load resources.
			ObjImporter importer = new ObjImporter();
			if (a_prefab.dataItem)
			{
				List<ZipEntry> entries = FileParser.m_dataFile.SelectEntries(a_prefab.modelPath).ToList();
				if (entries.Count == 1)
				{
					filter.mesh = importer.ImportStream(entries[0].OpenReader());
				}
				
				entries = FileParser.m_dataFile.SelectEntries(a_prefab.texturePath).ToList();
				if (entries.Count == 1)
				{
					Ionic.Crc.CrcCalculatorStream stream = entries[0].OpenReader();
					Texture2D texture = new Texture2D(0, 0);
					byte[] image = new byte[stream.Length];
					stream.Read(image, 0, (int)stream.Length);
					texture.LoadImage(image);
					renderer.material.mainTexture = texture;
				}
				//filter.mesh = tempBuilding.GetComponent<MeshFilter>().mesh;
				//renderer.material = tempBuilding.GetComponent<MeshRenderer>().material;
			}
			else
			{
				filter.mesh = importer.ImportFile(Application.dataPath + "/mods/" + a_prefab.modelPath);
				Texture2D texture = new Texture2D(0,0);
				FileStream file = File.OpenRead(Application.dataPath + "/mods/" + a_prefab.texturePath);
				byte[] image = new byte[file.Length];
				file.Read(image, 0, (int)file.Length);
				texture.LoadImage(image);
				renderer.material.mainTexture = texture;
			}
			
			renderer.material.color = new Color(1f, 1f, 1f, .5f);
			renderer.material.shader = Shader.Find("Transparent/Diffuse");
			collider.size = filter.mesh.bounds.size;
			collider.size = new Vector3(collider.size.x, collider.size.y * 0.9f, collider.size.z);
			collider.center = new Vector3(0f, filter.mesh.bounds.size.y * 0.5f, 0f);
			
			// Set up boundary check.
			rigid.isKinematic = true;
			rigid.useGravity = false;
			collider.isTrigger = true;
			m_dims = new Vector2(collider.bounds.size.x/2, collider.bounds.size.z/2);
			m_placeable = true;
			
			// Initialise bounding box.
			m_points = new Vector3[9];
			m_points[0] = new Vector3(m_dims.x, 0.5f, m_dims.y);
			m_points[1] = new Vector3(-m_dims.x, 0.5f, m_dims.y);
			m_points[2] = new Vector3(-m_dims.x, 0.5f, -m_dims.y);
			m_points[3] = new Vector3(m_dims.x, 0.5f, -m_dims.y);
			m_points[4] = new Vector3(m_dims.x, 0.5f, 0f);
			m_points[5] = new Vector3(-m_dims.x, 0.5f, 0f);
			m_points[6] = new Vector3(0f, 0.5f, m_dims.y);
			m_points[7] = new Vector3(0f, 0.5f, -m_dims.y);
			m_points[8] = new Vector3(0f, 0.5f, 0f);
			m_copy = true;
		}
		
		public void Update()
		{
			if (!m_copy)
				return;
			
			m_air = false;
			Vector3[] points = new Vector3[9];
			for (int i = 0; i < 9; ++i)
			{				
				points[i] = transform.rotation * m_points[i];
				//Debug.DrawLine(transform.position + m_points[i], transform.position + m_points[i] + new Vector3(0f, -2f, 0f), Color.green);
				Ray ray = new Ray(transform.position + points[i], new Vector3(0f, -1f, 0f));
				if (!Physics.Raycast(ray, 0.8f, 1 << 8)) m_air = true;
			}
			
			if (m_placeable && !m_air)
				renderer.material.color = new Color(1f, 1f, 1f, .5f);
			else
				renderer.material.color = new Color(1f, 0f, 0f, .5f);
		}
		
		public void OnTriggerEnter(Collider a_collision)
		{
			m_placeable = false;
		}
		
		public void OnTriggerExit(Collider a_collision)
		{
			m_placeable = true;
		}
	}
}
