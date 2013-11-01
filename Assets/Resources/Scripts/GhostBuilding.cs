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
		private bool m_inRange;
		private bool m_visible;
		private Vector2 m_dims;
		private Vector3[] m_points;
		private bool m_copy = false;
		public BuildingPrefab m_prefab;
		List<Headquarters> m_headquarters;
		private int m_collisions;
		private float m_largest;

		public bool Placeable()
		{
			return (m_placeable && !m_air && m_inRange && m_visible);
		}

		// Create ghost building.
		public void Create(BuildingPrefab a_prefab)
		{
			MeshFilter filter = gameObject.AddComponent<MeshFilter>();
			MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();
			Rigidbody rigid = gameObject.AddComponent<Rigidbody>();
			BoxCollider collider = gameObject.AddComponent<BoxCollider>();
			m_prefab = a_prefab;
			gameObject.name = "Cursor Building";
			m_air = false;
			m_inRange = true;
			m_placeable = false;
			m_visible = true;
			m_collisions = 0;
			
			// Get list of headquarters buildings
			m_headquarters = new List<Headquarters>();
			foreach (Building building in Main.m_buildingList)
			{
				if (building.BuildingType() == Building.Type.HEADQUARTERS)
					m_headquarters.Add((Headquarters)building);
			}
					   
			// Load resources.
			filter.mesh = m_prefab.model;
			renderer.material.mainTexture = m_prefab.texture;

			// Configure entity.
			renderer.material.color = new Color(1f, 1f, 1f, .5f);
			renderer.material.shader = Shader.Find("Transparent/Diffuse");
			collider.size = filter.mesh.bounds.size;
			collider.size = new Vector3(collider.size.x, collider.size.y * 0.9f, collider.size.z);
			collider.center = new Vector3(0f, filter.mesh.bounds.size.y * 0.5f, 0f);
			
			// Set up boundary check.
			rigid.isKinematic = true;
			rigid.useGravity = false;
			collider.isTrigger = true;
			m_dims = new Vector2(collider.bounds.size.x / 2, collider.bounds.size.z / 2);
			m_largest = m_dims.x;
			if (m_dims.y > m_largest) m_largest = m_dims.y;
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
					
			// Check if a part of the building is in the air.
			m_air = false;
			Vector3[] points = new Vector3[9];
			for (int i = 0; i < 9; ++i)
			{				
				points[i] = transform.rotation * m_points[i];
				//Debug.DrawLine(transform.position + m_points[i], transform.position + m_points[i] + new Vector3(0f, -2f, 0f), Color.green);
				Ray ray = new Ray(transform.position + points[i], new Vector3(0f, -1f, 0f));
				if (!Physics.Raycast(ray, 0.8f, 1 << 8))
					m_air = true;
			}
			
			// Check if we're close enough to HQ.
			if (m_prefab.type != Building.Type.HEADQUARTERS)
			{
				m_inRange = false;
				foreach (Headquarters building in m_headquarters)
				{
					if (!building.Built()) continue;

					if (Vector3.Distance(transform.position, building.Position()) < 100)
					{
						m_inRange = true;
						break;
					}
				 }
			}

			// Check if building is in a visible area.
			if (Main.m_fog != null)
			{
				if (Main.m_fog.Visible(transform.position, m_largest) != Fog.Visibility.VISIBLE)
					m_visible = false;
				else
					m_visible = true;
			}
			
			// Update building colour.
			if (m_placeable && !m_air && m_inRange && m_visible)
				renderer.material.color = new Color(1f, 1f, 1f, .5f);
			else
				renderer.material.color = new Color(1f, 0f, 0f, .5f);
		}

		public void OnTriggerEnter(Collider a_collision)
		{
			++m_collisions;
			m_placeable = false;
		}

		public void OnTriggerExit(Collider a_collision)
		{
			--m_collisions;
			if (m_collisions == 0)
				m_placeable = true;
		}
	}
}
