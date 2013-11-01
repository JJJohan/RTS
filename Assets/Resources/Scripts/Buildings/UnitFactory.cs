using UnityEngine;
using System.Collections.Generic;

namespace RTS
{
	// Building responsible for producing units.   
	public class UnitFactory : Building
	{
		class QueuedUnit
		{
			public UnitPrefab prefab;
			public int buildTime;
			public float percentage;
		}

		private List<int> m_unitList;
		private List<QueuedUnit> m_queue;

		public UnitFactory(Properties a_properties, Mesh a_mesh, Texture2D a_texture)
			: base(a_properties, a_mesh, a_texture)
		{
			m_type = Type.UNITFACTORY;
			m_unitList = new List<int>();
			m_queue = new List<QueuedUnit>();

			// Generate list of producable units.
			foreach (UnitPrefab prefab in Main.m_res.prefabs.unitPrefabs.Values)
			{
				if (prefab.factory == m_ID)
					m_unitList.Add(prefab.ID);
			}
		}

		public override void Process()
		{
			// Process unit queue
			QueuedUnit delete = null;
			if (m_queue.Count > 0)
			{
				QueuedUnit unit = m_queue[0];
				unit.percentage += Time.deltaTime;
				if (unit.percentage >= unit.buildTime)
				{
					// Initialise the unit.
					UnitTemplate template = new UnitTemplate();
					Unit newUnit = template.Load(unit.prefab, m_position - new Vector3(15f, 0f, 15f), new Vector3(0f, 90f, 0f));
					Main.m_unitList.Add(newUnit);

					// Cleanup
					UserInterface.m_sidePanel.UpdateQueue(unit.prefab.ID, false);
					delete = unit;
				}
			}

			if (delete != null)
				m_queue.Remove(delete);

			base.Process();
		}

		public void QueueUnit(SidePanel a_sidePanel, UnitArgs a_events)
		{
			if (a_events.create)
			{
				QueuedUnit unit = new QueuedUnit();
				unit.prefab = a_events.prefab;
				unit.buildTime = a_events.prefab.buildTime;
				unit.percentage = 0f;
				m_queue.Add(unit);
			}
			else
			{
				for (int i = m_queue.Count - 1; i >= 0; --i)
				{
					if (m_queue[i].prefab.ID == a_events.prefab.ID)
					{
						m_queue.Remove(m_queue[i]);
						break;
					}
				}
			}
		}

		public override void Select()
		{
			base.Select();

			SidePanel panel = UserInterface.m_sidePanel;
			panel.ListUnits(m_unitList);
			panel.UnitEvent += new SidePanel.UnitHandler(QueueUnit);
		}

		public override void Deselect()
		{
			base.Deselect();

			SidePanel panel = UserInterface.m_sidePanel;
			panel.Clear();
			panel.UnitEvent -= new SidePanel.UnitHandler(QueueUnit);
		}

		public override void Draw()
		{
			if (m_queue.Count > 0)
				UserInterface.m_sidePanel.DrawQueue(m_queue[0].prefab.ID, m_queue[0].percentage);

			base.Draw();
		}
	}
}