using UnityEngine;

namespace RTS
{	
	// Base class of a selectable unit in the RTS project.	
	public abstract class Selectable : MonoBehaviour
	{
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
		
		public void GetIcon(out MinimapIcon a_icon)
		{
			a_icon = m_icon;
		}
		
		public virtual void Init()
		{
			// Create minimap icon				
			m_icon = new MinimapIcon(m_miniSize, true);
		}
		
		public virtual void Process(ref Resources a_ref)
		{	
			// Update health bar if selected.
			if (m_selectionBox)
			{
				float percent = (m_health - m_damage) / m_totalHealth;
				if (percent < 0.01f) percent = 0.01f;
				
				m_healthFront.transform.localScale = new Vector3(percent, 1f, 1f);
				m_healthBack.transform.localScale = new Vector3(1f - percent, 1f, 1f);
			}
		}
		
		public virtual void Select()
		{
			// Set minimap icon colour.
			m_icon.SetColour(Color.white);
			
			// Find largest scale vector.
			Vector3 bounds = gameObject.GetComponent<MeshFilter>().mesh.bounds.size;
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
			healthBack.Create(largest/2, largest/20, new Vector2(largest/2, -bounds.y * 1.5f), Billboard.Anchor.Right);
			m_healthBack.transform.position = m_position + new Vector3(0f, bounds.y/2, 0f);
			m_healthBack.GetComponent<MeshRenderer>().material.color = Color.red;
			m_healthBack.transform.localScale = new Vector3(0f, 1f, 1f);
			m_healthBack.name = "Health Bar Back";
			
			// Health Front			
			m_healthFront = new GameObject();
			Billboard healthFront = m_healthFront.AddComponent<Billboard>();
			healthFront.Create(largest/2, largest/20, new Vector2(-largest/2, -bounds.y * 1.5f), Billboard.Anchor.Left);
			m_healthFront.transform.position = m_position + new Vector3(0f, bounds.y/2, 0f);
			m_healthFront.GetComponent<MeshRenderer>().material.color = Color.green;
			m_healthFront.transform.localScale = new Vector3(1f, 1f, 1f);
			m_healthFront.name = "Health Bar Front";
		}
		
		public virtual void Deselect()
		{
			// Set minimap icon colour.
			m_icon.SetColour(Color.blue);
			
			// Clear selection box.
			Destroy (m_selectionBox);
			Destroy (m_healthFront);
			Destroy (m_healthBack);
		}
		
		public virtual void OnDestroy()
		{
			if (m_selectionBox)
				Destroy (m_selectionBox);
			
			if (m_healthFront)
				Destroy (m_healthFront);
			
			if (m_healthBack)
				Destroy (m_healthBack);
		}
	}
}