using UnityEngine;

namespace RTS
{
	// Building responsible for producing power.   
	public class PowerFactory : Building
	{
		private int m_powerProduced;

		public PowerFactory(Properties a_properties, Mesh a_mesh, Texture2D a_texture)
			: base(a_properties, a_mesh, a_texture)
		{
			m_type = Type.POWERFACTORY;
			m_powerProduced = 0;
		}
		
		public override int Power()
		{
			return m_powerProduced;
		}

		public override void Process()
		{
			// Update power produced
			if (m_built)
				m_powerProduced = -(int)(m_power * ((m_health - m_damage) / m_totalHealth));
			
			base.Process();
		}
	}
}