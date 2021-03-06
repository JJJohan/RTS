using UnityEngine;

namespace RTS
{	
	// Base class of a selectable unit in the RTS project.	
	public abstract class Selectable
	{
		protected GameObject m_gameObject;
		protected Vector3 m_position;
		protected int m_cost;
		protected Texture2D m_cameo;
		protected float m_totalHealth;
		protected float m_health;
		protected float m_damage;
		protected Vector2 m_miniSize;
		protected MinimapIcon m_icon;
		protected int m_ID;
		protected string m_name;
		protected int m_type;
		protected MeshFilter m_mesh;
		protected MeshRenderer m_renderer;
		protected bool m_destroyed;
		protected float m_radius;
		protected bool m_selected;

		private GameObject m_selectionBox;
		private GameObject m_healthFront;
		private GameObject m_healthBack;
		
		public GameObject GetObject() { return m_gameObject; }
		public string Tag() { return m_gameObject.tag; }
		public int Cost() { return m_cost; }
		public Vector3 Position() { return m_gameObject.transform.position + new Vector3(0f, m_mesh.mesh.bounds.size.y/2, 0f); }
		public bool Destroyed() { return m_destroyed; }
		public float Radius() { return m_radius; }
		public int ID() { return m_ID; }

		public Selectable(int a_ID, string a_name)
		{
			m_ID = a_ID;
			m_gameObject = new GameObject();
			m_gameObject.name = a_name;
			m_destroyed = false;
			UserData data = m_gameObject.AddComponent<UserData>();
			data.data = this;
			m_selected = false;

			m_mesh = m_gameObject.AddComponent<MeshFilter>();
			m_renderer = m_gameObject.AddComponent<MeshRenderer>();
			m_renderer.material.shader = Shader.Find("Diffuse");
		}

		protected void Init(Mesh a_mesh, Texture2D a_texture)
		{
			// Init model and texture
			m_renderer.material.mainTexture = a_texture;
			m_mesh.mesh = a_mesh;
			m_position = m_gameObject.transform.position;

			// Calculate radius
			m_radius = m_mesh.mesh.bounds.size.x;
			if (m_mesh.mesh.bounds.size.z > m_radius) m_radius = m_mesh.mesh.bounds.size.z;
			m_radius /= 2;

			BoxCollider collider = m_gameObject.AddComponent<BoxCollider>();
			collider.size = m_mesh.mesh.bounds.size;
			collider.size = new Vector3(collider.size.x, collider.size.y * 2f, collider.size.z);
			collider.center = new Vector3(0f, m_mesh.mesh.bounds.size.y * 0.5f, 0f);
		}

		public void GetIcon(out MinimapIcon a_icon)
		{
			a_icon = m_icon;
		}
		
		public virtual void Process()
		{	
			UpdateSelectionBox();
		}

		protected void UpdateGuiPosition()
		{
			if (m_selectionBox)
			{
				// Find largest scale vector.
				Vector3 bounds = m_gameObject.GetComponent<MeshFilter>().mesh.bounds.size;

				m_selectionBox.transform.position = m_gameObject.transform.position + new Vector3(0f, bounds.y/2, 0f);
				m_healthFront.transform.position = m_gameObject.transform.position + new Vector3(0f, bounds.y/2, 0f);
				m_healthBack.transform.position = m_gameObject.transform.position + new Vector3(0f, bounds.y/2, 0f);
			}
		}
		
		private void UpdateSelectionBox()
		{
			// Update health bar if selected.
			if (m_selectionBox)
			{
				float percent = (m_health - m_damage) / m_totalHealth;
				float blocks = Mathf.Floor(m_totalHealth / 200);
				blocks = Mathf.Clamp(blocks, 4, 40);
				percent = Mathf.Floor(percent * blocks) / blocks;
				if (percent < 0.01f) percent = 0.01f;
				
				// Update health bar texture to have 20 blocks.
				Vector2 scale = new Vector2(percent * blocks, 1f);
				m_healthFront.GetComponent<MeshRenderer>().material.mainTextureScale = scale;
				
				m_healthFront.transform.localScale = new Vector3(percent, 1f, 1f);
				m_healthBack.transform.localScale = new Vector3(1f - percent, 1f, 1f);
			}
		}
		
		public virtual void Select()
		{
			m_selected = true;

			// Set minimap icon colour.
			m_icon.SetColour(Color.white);
			
			// Find largest scale vector.
			Vector3 bounds = m_gameObject.GetComponent<MeshFilter>().mesh.bounds.size;
			float largest = bounds.x;
			if (bounds.y > largest) largest = bounds.y;
			if (bounds.z > largest) largest = bounds.z;

			// Selection Box
			m_selectionBox = new GameObject();
			Billboard selectionBox = m_selectionBox.AddComponent<Billboard>();
			selectionBox.Create(largest, largest, "Textures/selection");
			m_selectionBox.transform.position = m_position + new Vector3(0f, bounds.y/2, 0f);
			m_selectionBox.name = "Selection Box";
			
			// Health Back
			m_healthBack = new GameObject();
			Billboard healthBack = m_healthBack.AddComponent<Billboard>();
			healthBack.Create(largest/2, largest/20, new Vector2(largest/2, -largest * 0.6f), Billboard.Anchor.Right, "Textures/HealthBack");
			m_healthBack.transform.position = m_position + new Vector3(0f, bounds.y/2, 0f);
			//m_healthBack.GetComponent<MeshRenderer>().material.color = Color.red;
			m_healthBack.transform.localScale = new Vector3(0f, 1f, 1f);
			m_healthBack.name = "Health Bar Back";
			
			// Health Front			
			m_healthFront = new GameObject();
			Billboard healthFront = m_healthFront.AddComponent<Billboard>();
			healthFront.Create(largest/2, largest/20, new Vector2(-largest/2, -largest * 0.6f), Billboard.Anchor.Left, "Textures/HealthFront");
			m_healthFront.transform.position = m_position + new Vector3(0f, bounds.y/2, 0f);
			//m_healthFront.GetComponent<MeshRenderer>().material.color = Color.green;
			m_healthFront.transform.localScale = new Vector3(1f, 1f, 1f);
			m_healthFront.name = "Health Bar Front";
			
			// Update health bar
			UpdateSelectionBox();
		}
		
		public virtual void Deselect()
		{
			m_selected = false;

			// Set minimap icon colour.
			m_icon.SetColour(Color.blue);
			
			// Clear selection box.
			Object.Destroy (m_selectionBox);
			Object.Destroy (m_healthFront);
			Object.Destroy (m_healthBack);
		}
		
		public virtual void Destroy()
		{
			UserInterface.m_sidePanel.m_minimap.RemoveIcon(m_icon);

			if (m_selectionBox)
				Object.Destroy (m_selectionBox);
			
			if (m_healthFront)
				Object.Destroy (m_healthFront);
			
			if (m_healthBack)
				Object.Destroy (m_healthBack);
		}

		public virtual void Draw()
		{
			
		}
	}
}