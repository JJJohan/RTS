using UnityEngine;

namespace RTS
{
	// Building responsible for producing units.   
	public class UnitFactory : Building
	{
		public UnitFactory(Properties a_properties, Mesh a_mesh, Texture2D a_texture)
			: base(a_properties, a_mesh, a_texture)
		{
			m_type = Type.UNITFACTORY;
		}
	}
}