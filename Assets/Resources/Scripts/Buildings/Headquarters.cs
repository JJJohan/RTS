using UnityEngine;

namespace RTS
{
	// Main base HQ
	public class Headquarters : Building
	{
		public Headquarters(Properties a_properties, Mesh a_mesh, Texture2D a_texture)
			: base(a_properties, a_mesh, a_texture)
		{
			m_type = Type.HEADQUARTERS;
		}
	}
}