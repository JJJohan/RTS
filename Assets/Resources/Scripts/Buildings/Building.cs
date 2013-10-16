using UnityEngine;

namespace RTS
{
	// Base class of a structure in the RTS project.	
	public abstract class Building : Selectable
	{	
		// Variables
		protected int m_cost;
		protected int m_power;
		protected int m_buildTime;
		protected float m_totalHealth;
		protected Texture2D m_cameo;
		protected float m_health;
		protected float m_buildPercent;
		protected bool m_built;
		protected bool m_repairing;
		protected Vector3 m_position;
		protected Vector3 m_textPos;
		protected Vector3 m_offset;
		private float m_spent;
		private TextMesh m_text;
		private GameObject m_ghost;
		
		// Functions
		public float Health { get; set; }
		public float BuildPercent() { return m_buildPercent; }
		public bool Repairing { get; set; }
		public int Cost() { return m_cost; }
		public int Power() { return m_power; }
		public int BuildTime() { return m_buildTime; }
		public Vector3 Offset() { return m_offset; }
		public bool Built() { return m_built; }
		public Vector3 Position { get; set; }
		
		public void Construct()
		{
			// Create construction progress text
			GameObject text = new GameObject();
			text.transform.position = m_position + m_textPos;
			MeshRenderer textRender = text.AddComponent<MeshRenderer>();
			textRender.material = (Material)UnityEngine.Resources.Load("Fonts/coopMat");
			textRender.material.shader = Shader.Find("GUI/Text Shader");
			textRender.material.color = Color.white;
			m_text = text.AddComponent<TextMesh>();
			m_text.font = (Font)UnityEngine.Resources.Load("Fonts/coop");
			m_text.text = "0%";
			m_text.alignment = TextAlignment.Center;
			m_text.anchor = TextAnchor.MiddleCenter;
			m_text.transform.position = m_position + m_textPos;
			
			// Create ghost copy
			m_ghost = new GameObject();
			m_ghost.transform.rotation = gameObject.transform.rotation;
			m_ghost.transform.localScale = gameObject.transform.localScale;
			MeshRenderer ghostRender = m_ghost.AddComponent<MeshRenderer>();
			MeshFilter ghostMesh = m_ghost.AddComponent<MeshFilter>();
			ghostMesh.mesh = gameObject.GetComponent<MeshFilter>().mesh;
			ghostRender.material = gameObject.GetComponent<MeshRenderer>().material;
			ghostRender.material.color = new Color(1f, 1f, 1f, 0.5f);
			ghostRender.material.shader = Shader.Find("Transparent/Diffuse");
			m_ghost.transform.position = m_position;
		}
		
		public virtual void Process(ref Resources a_res)
		{
			if (!m_built && !m_repairing)
			{
				// Increment build percentage.
				m_buildPercent += Time.deltaTime;
				
				// Delete building assets when finished.
				if (m_buildPercent > (float)m_buildTime)
				{
					Destroy(m_text.gameObject);
					Destroy(m_ghost);
					m_built = true;
					m_buildPercent = m_buildTime;
				}
				
				// Update building position based on completion percentage.
				float offset = ((m_buildTime - m_buildPercent) / m_buildTime) * 10;
				transform.position = m_position - new Vector3(0f, offset, 0f);
				
				// Update percentage text.
				if (m_text)
				{
					m_text.transform.LookAt(m_text.transform.position * 2 - Camera.main.transform.position);
					m_text.transform.eulerAngles = new Vector3(0f, m_text.transform.eulerAngles.y, m_text.transform.eulerAngles.z);
					m_text.text = ((int)(((m_buildPercent / m_buildTime) * 100) + .5f)).ToString() + "%";
				}
			}
			
			// Repair building.
			if (m_repairing && m_health < m_totalHealth)
			{
				// Ensure sufficient funds.
				int cost = (int)(m_cost * Time.deltaTime * .25f);
				if (a_res.funds > cost)
				{
					m_health += Time.deltaTime / m_buildTime;
					a_res.funds -= cost;
					if (m_health > m_totalHealth)
					{
						m_repairing = false;
						m_health = m_totalHealth;
					}
				}
				else
				{
					// TODO: Insufficient funds warning.
				}
			}
			
			// Update colour based on health.
			float percent = m_health / m_totalHealth;
			gameObject.renderer.material.color = new Color(percent, percent, percent);
		}
		
		public override void Select()
		{
			if (m_ghost)
				m_ghost.renderer.material.color = new Color(0f, 1f, 0f, 0.5f);
			
			gameObject.renderer.material.color = Color.green;
			
			// TODO: Play selection sound.
		}
		
		public override void Deselect()
		{
			if (m_ghost)
				m_ghost.renderer.material.color = new Color(1f, 1f, 1f, 0.5f);
				
			gameObject.renderer.material.color = Color.white;
		}
	}
}