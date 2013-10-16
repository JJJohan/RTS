using UnityEngine;

namespace RTS
{
	// Ghost building when placing a new builing.	
	public class GhostBuilding : MonoBehaviour
	{
		private bool m_placeable;
		private bool m_air;
		private Vector3 m_offset;
		private Vector2 m_dims;
		private Vector3[] m_points;
		private bool m_copy = false;
		
		public bool Placeable() { return (m_placeable && !m_air); }
		public Vector3 Offset() { return m_offset; }
		
		// Copy building mesh.
		public void Copy(string a_name)
		{
			MeshFilter filter = gameObject.AddComponent<MeshFilter>();
			MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();
			Rigidbody rigid = gameObject.AddComponent<Rigidbody>();
			BoxCollider collider = gameObject.AddComponent<BoxCollider>();
			
			// Copy details.
			GameObject tempBuilding = (GameObject)Instantiate(UnityEngine.Resources.Load(a_name));
			m_offset = tempBuilding.GetComponent<Building>().Offset();
			transform.localScale = tempBuilding.transform.localScale;
			filter.mesh = tempBuilding.GetComponent<MeshFilter>().mesh;
			renderer.material = tempBuilding.GetComponent<MeshRenderer>().material;
			renderer.material.color = new Color(1f, 1f, 1f, .5f);
			renderer.material.shader = Shader.Find("Transparent/Diffuse");
			collider.size = tempBuilding.GetComponent<Building>().PlacementBounds();
			Destroy(tempBuilding);
			
			// Set up boundary check.
			rigid.isKinematic = true;
			rigid.useGravity = false;
			collider.isTrigger = true;
			m_dims = new Vector2(collider.bounds.size.x/2, collider.bounds.size.z/2);
			m_placeable = true;
			
			// Initialise bounding box.
			m_points = new Vector3[9];
			m_points[0] = new Vector3(m_dims.x, -collider.bounds.size.y/2, m_dims.y);
			m_points[1] = new Vector3(-m_dims.x, -collider.bounds.size.y/2, m_dims.y);
			m_points[2] = new Vector3(-m_dims.x, -collider.bounds.size.y/2, -m_dims.y);
			m_points[3] = new Vector3(m_dims.x, -collider.bounds.size.y/2, -m_dims.y);
			m_points[4] = new Vector3(m_dims.x, -collider.bounds.size.y/2, 0);
			m_points[5] = new Vector3(-m_dims.x, -collider.bounds.size.y/2, 0);
			m_points[6] = new Vector3(0, -collider.bounds.size.y/2, m_dims.y);
			m_points[7] = new Vector3(0, -collider.bounds.size.y/2, -m_dims.y);
			m_points[8] = new Vector3(0, -collider.bounds.size.y/2, 0);
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
				//Debug.DrawLine(transform.position + m_points[i], transform.position + m_points[i] + new Vector3(0f, -2f, 0f), Color.red);
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
