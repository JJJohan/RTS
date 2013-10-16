using UnityEngine;

namespace RTS
{
	// Ghost building when placing a new builing.	
	public class GhostBuilding : MonoBehaviour
	{
		private bool m_placeable;
		private bool m_air;
		private Vector3 m_offset;
		
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
			Destroy(tempBuilding);
			
			// Set up boundary check.
			rigid.isKinematic = true;
			rigid.useGravity = false;
			collider.isTrigger = true;
			collider.size = new Vector3(1.2f, 0.9f, 1.2f);
			m_placeable = true;
		}
		
		public void Update()
		{
			Vector2 dim = new Vector2(collider.bounds.size.x/2, collider.bounds.size.z/2);
			Vector3[] points = new Vector3[9];
			m_air = false;
			points[0] = transform.position + new Vector3(dim.x, -collider.bounds.size.y/2, dim.y);
			points[1] = transform.position + new Vector3(-dim.x, -collider.bounds.size.y/2, dim.y);
			points[2] = transform.position + new Vector3(-dim.x, -collider.bounds.size.y/2, -dim.y);
			points[3] = transform.position + new Vector3(dim.x, -collider.bounds.size.y/2, -dim.y);
			points[4] = transform.position + new Vector3(dim.x, -collider.bounds.size.y/2, 0);
			points[5] = transform.position + new Vector3(-dim.x, -collider.bounds.size.y/2, 0);
			points[6] = transform.position + new Vector3(0, -collider.bounds.size.y/2, dim.y);
			points[7] = transform.position + new Vector3(0, -collider.bounds.size.y/2, -dim.y);
			points[8] = transform.position + new Vector3(0, -collider.bounds.size.y/2, 0);
			for (int i = 0; i < 9; ++i)
			{
				Ray ray = new Ray(points[i], new Vector3(0f, -1f, 0f));
				if (!Physics.Raycast(ray, 2.0f, 1 << 8)) m_air = true;
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
