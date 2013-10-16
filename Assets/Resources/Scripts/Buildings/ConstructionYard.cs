using UnityEngine;

namespace RTS
{
	public class ConstructionYard : Building
	{
		public Texture2D m_cameoResource;
		
		public ConstructionYard()
		{
			m_health = 1000;
			m_power = 20;
			m_buildTime = 20;
			m_cost = 3000;
			m_cameo = m_cameoResource;
			m_textPos = new Vector3(0f, 10f, 0f);
			m_offset = new Vector3(0f, 5f, 0f);
		}
	}
}
