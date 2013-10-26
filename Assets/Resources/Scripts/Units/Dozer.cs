using UnityEngine;
using System.Collections.Generic;

namespace RTS
{
	// Construction unit.
	public class Dozer : Unit
	{
		private Building m_building;

		public Dozer(Properties a_properties, Mesh a_mesh, Texture2D a_texture)
			: base(a_properties, a_mesh, a_texture)
		{
			m_building = null;
			m_type = Type.DOZER;
		}

		public override void Process()
		{
			if (m_building != null)
			{
				if (!m_building.Built())
				{
					if (!Moving() && Vector2.Distance(Main.Vec3to2(m_building.Position()), Main.Vec3to2(Position())) < Radius() * 8)
					{
						m_building.Build();
					}
				}
				else
				{
					m_building = null;
				}
			}

			base.Process();
		}

		public void Build(ref Building a_building)
		{
			m_building = a_building;
		}
	}
}