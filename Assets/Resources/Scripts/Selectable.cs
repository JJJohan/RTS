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
		
		private GameObject m_selectionBox;
		private GameObject m_healthFront;
		private GameObject m_healthBack;
		
		public GameObject GetObject() { return m_gameObject; }
		public string Tag() { return m_gameObject.tag; }
		
		public void GetIcon(out MinimapIcon a_icon)
		{
			a_icon = m_icon;
		}
		
		public Selectable()
		{
			m_gameObject = new GameObject();
			UserData data = m_gameObject.AddComponent<UserData>();
			data.data = this;
		}
		
		public virtual void Process(ref Resources a_ref)
		{	
			UpdateSelectionBox();
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
			healthBack.Create(largest/2, largest/20, new Vector2(largest/2, -bounds.y * 1.5f), Billboard.Anchor.Right, "Textures/HealthBack");
			m_healthBack.transform.position = m_position + new Vector3(0f, bounds.y/2, 0f);
			//m_healthBack.GetComponent<MeshRenderer>().material.color = Color.red;
			m_healthBack.transform.localScale = new Vector3(0f, 1f, 1f);
			m_healthBack.name = "Health Bar Back";
			
			// Health Front			
			m_healthFront = new GameObject();
			Billboard healthFront = m_healthFront.AddComponent<Billboard>();
			healthFront.Create(largest/2, largest/20, new Vector2(-largest/2, -bounds.y * 1.5f), Billboard.Anchor.Left, "Textures/HealthFront");
			m_healthFront.transform.position = m_position + new Vector3(0f, bounds.y/2, 0f);
			//m_healthFront.GetComponent<MeshRenderer>().material.color = Color.green;
			m_healthFront.transform.localScale = new Vector3(1f, 1f, 1f);
			m_healthFront.name = "Health Bar Front";
			
			// Update health bar
			UpdateSelectionBox();
		}
		
		public virtual void Deselect()
		{
			// Set minimap icon colour.
			m_icon.SetColour(Color.blue);
			
			// Clear selection box.
			Object.Destroy (m_selectionBox);
			Object.Destroy (m_healthFront);
			Object.Destroy (m_healthBack);
		}
		
		public virtual void Destroy()
		{
			if (m_selectionBox)
				Object.Destroy (m_selectionBox);
			
			if (m_healthFront)
				Object.Destroy (m_healthFront);
			
			if (m_healthBack)
				Object.Destroy (m_healthBack);
		}
	}
}