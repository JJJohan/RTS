using UnityEngine;

namespace RTS
{
	// Main base HQ
	public class Headquarters : UnitFactory
	{
		public Headquarters(Properties a_properties, Mesh a_mesh, Texture2D a_texture)
			: base(a_properties, a_mesh, a_texture)
		{
			m_type = Type.HEADQUARTERS;
		}

		public void Finish()
		{
			if (m_text)
				Object.Destroy(m_text.gameObject);
			if (m_ghost)
				Object.Destroy(m_ghost);
			m_health = m_totalHealth;
			m_built = true;
			m_buildPercent = m_buildTime;
			Main.m_res.buildings.Add(m_ID);
			m_gameObject.transform.position = m_position;

			if (Performance.Effects == Performance.LOW)
				m_gameObject.renderer.enabled = true;
		}
	}
}