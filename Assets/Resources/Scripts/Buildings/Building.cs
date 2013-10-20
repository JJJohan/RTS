using UnityEngine;

namespace RTS
{
	// Base class of a structure in the RTS project.	
	public class Building : Selectable
	{	
		public struct Properties
		{
			public string name;
			public int health;
			public int power;
			public int buildTime;
			public int cost;
			public Vector2 miniSize;
		}
		
		// Variables
		protected int m_power;
		protected int m_buildTime;
		protected float m_buildPercent;
		protected bool m_built;
		protected bool m_repairing;
		protected Vector3 m_rotation;
		protected MeshFilter m_mesh;
		protected MeshRenderer m_renderer;
		private TextMesh m_text;
		private GameObject m_ghost;
		private BoxCollider m_collider;
		private bool m_destroyed;
		
		// Functions
		public float Health { get; set; }
		public float HealthPercent() { return m_health / m_totalHealth; }
		public float BuildPercent() { return m_buildPercent; }
		public bool Repairing { get; set; }
		public int Cost() { return m_cost; }
		public int Power() { return m_power; }
		public int BuildTime() { return m_buildTime; }
		public bool Built() { return m_built; }
		public Vector3 Position { get; set; }
		public Vector3 Rotation { get; set; }
		public bool Destroyed() { return m_destroyed; }
		
		public struct Type
		{
			public const int DEFAULT = 0;
		}
		
		public Building(Properties a_properties, Mesh a_mesh, Texture2D a_texture)
		{		
			m_mesh = m_gameObject.AddComponent<MeshFilter>();
			m_renderer = m_gameObject.AddComponent<MeshRenderer>();
			m_renderer.material.shader = Shader.Find("Diffuse");
			
			// Init properties
			m_gameObject.name = a_properties.name;
			m_totalHealth = a_properties.health;
			m_power = a_properties.power;
			m_cost = a_properties.cost;
			m_buildTime = a_properties.buildTime;
			m_miniSize = a_properties.miniSize;
			
			// Init model and texture
			m_renderer.material.mainTexture = a_texture;
			m_mesh.mesh = a_mesh;
			
			m_collider = m_gameObject.AddComponent<BoxCollider>();
			m_collider.size = m_mesh.mesh.bounds.size;
			m_collider.size = new Vector3(m_collider.size.x, m_collider.size.y * 0.9f, m_collider.size.z);
			m_collider.center = new Vector3(0f, m_mesh.mesh.bounds.size.y * 0.5f, 0f);
			m_destroyed = false;
			m_gameObject.layer = 10;
			m_gameObject.tag = "Building";
			m_icon = new MinimapIcon(m_miniSize, true);
		}
		
		public void Construct(Vector3 a_pos, Vector3 a_rot)
		{	
			// Set position and rotation.
			m_gameObject.transform.position = m_position = a_pos;
			m_gameObject.transform.eulerAngles = m_rotation = a_rot;
			m_icon.Process(new Vector2(a_pos.x, -a_pos.z));
			
			// Create construction progress text
			GameObject text = new GameObject();
			MeshRenderer textRender = text.AddComponent<MeshRenderer>();
			textRender.material = (Material)UnityEngine.Resources.Load("Fonts/coopMat");
			textRender.material.shader = Shader.Find("GUI/Text Shader");
			textRender.material.color = Color.white;
			m_text = text.AddComponent<TextMesh>();
			m_text.font = (Font)UnityEngine.Resources.Load("Fonts/coop");
			m_text.text = "0%";
			m_text.alignment = TextAlignment.Center;
			m_text.anchor = TextAnchor.MiddleCenter;
			m_text.transform.position = m_position + new Vector3(0f, m_mesh.mesh.bounds.size.y / 2, 0f);
			text.name = "Building Text";
			
			// Create ghost copy
			m_ghost = new GameObject();
			m_ghost.transform.rotation = m_gameObject.transform.rotation;
			m_ghost.transform.localScale = m_gameObject.transform.localScale;
			MeshRenderer ghostRender = m_ghost.AddComponent<MeshRenderer>();
			MeshFilter ghostMesh = m_ghost.AddComponent<MeshFilter>();
			ghostMesh.mesh = m_gameObject.GetComponent<MeshFilter>().mesh;
			ghostRender.material = m_gameObject.GetComponent<MeshRenderer>().material;
			ghostRender.material.color = new Color(1f, 1f, 1f, 0.5f);
			ghostRender.material.shader = Shader.Find("Transparent/Diffuse");
			m_ghost.transform.position = m_position;
			m_ghost.name = "Ghost Building";
		}
		
		public override void Process(ref Resources a_res)
		{
			if (m_destroyed)
				return;
		
			base.Process(ref a_res);
			
			// Check if destroyed
			if (m_health - m_damage < 0f)
				m_destroyed = true;
			
			if (!m_built && !m_repairing)
			{
				// Increment build percentage.
				m_buildPercent += Time.deltaTime;
				m_health = (m_buildPercent / m_buildTime) * m_totalHealth;
				
				// Delete building assets when finished.
				if (m_buildPercent > (float)m_buildTime)
				{
					Object.Destroy(m_text.gameObject);
					Object.Destroy(m_ghost);
					m_built = true;
					m_buildPercent = m_buildTime;
				}
				
				// Update building position based on completion percentage.
				float offset = ((m_buildTime - m_buildPercent) / m_buildTime) * 10;
				m_gameObject.transform.position = m_position - new Vector3(0f, offset, 0f);
				
				// Update percentage text.
				if (m_text)
				{
					m_text.transform.rotation = Camera.main.transform.rotation;// * new Quaternion(0.707f, 0f, 0f, 0.707f);
					m_text.text = ((int)((m_buildPercent / m_buildTime) * 100)).ToString() + "%";
				}
			}
			
			// Repair building.
			if (m_repairing && m_health < m_totalHealth)
			{
				// Ensure sufficient funds.
				int cost = (int)(m_cost * Time.deltaTime * .25f);
				if (a_res.funds > cost)
				{
					m_damage -= Time.deltaTime / m_buildTime;
					a_res.funds -= cost;
					if (m_damage < 0)
					{
						m_repairing = false;
						m_damage = 0;
					}
				}
				else
				{
					// TODO: Insufficient funds warning.
				}
			}
			
			// Update colour based on health.
			//float percent = m_health / m_totalHealth;
			//gameObject.renderer.material.color = new Color(percent, percent, percent);
		}
		
		public override void Select()
		{
			base.Select();
			
			if (m_ghost)
				m_ghost.renderer.material.color = new Color(0f, 1f, 0f, 0.5f);
			
			m_gameObject.renderer.material.color = Color.green;

			// TODO: Play selection sound.
		}
		
		public override void Deselect()
		{
			base.Deselect();
			
			if (m_ghost)
				m_ghost.renderer.material.color = new Color(1f, 1f, 1f, 0.5f);
				
			m_gameObject.renderer.material.color = Color.white;
		}
		
		public override void Destroy()
		{
			if (m_gameObject)
				Object.Destroy(m_gameObject);
			
			if (m_ghost)
				Object.Destroy(m_ghost);
			
			if (m_text)
				Object.Destroy(m_text.gameObject);
			
			base.Destroy();
		}
	}
}