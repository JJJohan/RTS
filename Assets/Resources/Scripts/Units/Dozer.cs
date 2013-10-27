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
					if (Vector2.Distance(Main.Vec3to2(m_building.Position()), Main.Vec3to2(Position())) < Radius() * 6)
					{
						Stop();
						if (!Moving())
						{
							m_building.Build();
						}
					}
				}
				else
				{
					m_building = null;
				}
			}

			base.Process();
		}

		public override void SetDestination(Vector3 a_pos)
		{
			m_building = null;

			base.SetDestination(a_pos);
		}

		public void Build(ref Building a_building)
		{
			m_building = a_building;
		}

		public override void Select()
		{
			base.Select();

			UserInterface.m_sidePanel.ListBuildings();
		}

		public override void Deselect()
		{
			base.Deselect();

			UserInterface.m_sidePanel.Clear();
		}
	}
}