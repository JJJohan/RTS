using UnityEngine;

namespace RTS
{
	// Base class of a structure in the RTS project.	
	public class Building : Selectable
	{
		public struct Properties
		{
			public int ID;
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
		protected TextMesh m_text;
		protected GameObject m_ghost;
		private NavMeshObstacle m_obstacle;

		// Functions
		public float Health { get; set; }

		public float HealthPercent() { return m_health / m_totalHealth; }
		public float BuildPercent() { return m_buildPercent; }
		public bool Repairing { get; set; }

		public virtual int Power() { return m_power; }
		public int BuildTime() { return m_buildTime; }
		public bool Built() { return m_built; }
		public int BuildingType() { return m_type; }

		public struct Type
		{
			public const int DEFAULT = 0;
			public const int HEADQUARTERS = 1;
			public const int POWERFACTORY = 2;
			public const int UNITFACTORY = 3;
		}

		public Building(Properties a_properties, Mesh a_mesh, Texture2D a_texture)
			: base(a_properties.ID, a_properties.name)
		{
			// Init properties
			m_totalHealth = a_properties.health;
			m_power = a_properties.power;
			m_cost = a_properties.cost;
			m_buildTime = a_properties.buildTime;
			m_miniSize = a_properties.miniSize;
			m_type = Type.DEFAULT;
			m_gameObject.tag = "Building";
			m_gameObject.layer = 10;

			base.Init(a_mesh, a_texture);

			// Nav mesh obstacle
			m_obstacle = m_gameObject.AddComponent<NavMeshObstacle>();
			m_obstacle.radius = m_radius;
			m_obstacle.height = m_mesh.mesh.bounds.size.y;
		}

		public void Construct(Vector3 a_pos, Vector3 a_rot)
		{
			// Set position and rotation.
			m_position = a_pos;
			m_gameObject.transform.eulerAngles = a_rot;
			m_gameObject.transform.position = m_position - new Vector3(0f, m_mesh.mesh.bounds.size.y, 0f);

			// Create icon
			Vector2 pos = new Vector2(Position().x, Position().z);
			UserInterface.m_sidePanel.m_minimap.AddIcon(m_miniSize, pos, true, out m_icon);

			if (Performance.Effects == Performance.LOW)
				m_gameObject.renderer.enabled = false;
			
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

		public void Build()
		{
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
					Main.m_res.buildings.Add(m_ID);

					if (Performance.Effects == Performance.LOW)
						m_gameObject.renderer.enabled = true;
				}
				
				// Update building position based on completion percentage.
				float offset = ((m_buildTime - m_buildPercent) / m_buildTime) * m_mesh.mesh.bounds.size.y;
				m_gameObject.transform.position = m_position - new Vector3(0f, offset, 0f);
				
				// Update percentage text.
				if (m_text)
				{
					m_text.transform.rotation = Camera.main.transform.rotation;// * new Quaternion(0.707f, 0f, 0f, 0.707f);
					m_text.text = ((int)((m_buildPercent / m_buildTime) * 100)).ToString() + "%";
				}
				else
				{
					// Create construction progress text
					GameObject text = new GameObject();
					MeshRenderer textRender = text.AddComponent<MeshRenderer>();
					textRender.material = (Material)UnityEngine.Resources.Load("Fonts/gilMat");
					textRender.material.shader = Shader.Find("GUI/Text Shader");
					textRender.material.color = Color.white;
					m_text = text.AddComponent<TextMesh>();
					m_text.font = (Font)UnityEngine.Resources.Load("Fonts/gil");
					m_text.text = "0%";
					m_text.alignment = TextAlignment.Center;
					m_text.anchor = TextAnchor.MiddleCenter;
					m_text.transform.position = m_position + new Vector3(0f, m_mesh.mesh.bounds.size.y / 2, 0f);
					text.name = "Building Text";
				}
			}
		}

		public override void Process()
		{
			if (m_destroyed)
				return;
		
			base.Process();
			
			// Check if destroyed
			if (m_health - m_damage < 0f)
				m_destroyed = true;
			
			// Repair building.
			if (m_repairing && m_health < m_totalHealth)
			{
				// Ensure sufficient funds.
				int cost = (int)(m_cost * Time.deltaTime * .25f);
				if (Main.m_res.funds > cost)
				{
					m_damage -= Time.deltaTime / m_buildTime;
					Main.m_res.funds -= cost;
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
			for (int i = 0; i < Main.m_res.buildings.Count; ++i)
			{
				if (Main.m_res.buildings[i] == m_ID)
				{
					Main.m_res.buildings.Remove(Main.m_res.buildings[i]);
					break;
				}
			}
			
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