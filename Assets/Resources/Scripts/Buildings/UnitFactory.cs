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

		private List<string> m_unitList;
		private List<QueuedUnit> m_queue;

		public UnitFactory(Properties a_properties, Mesh a_mesh, Texture2D a_texture)
			: base(a_properties, a_mesh, a_texture)
		{
			m_type = Type.UNITFACTORY;
			m_unitList = new List<string>();
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
			for (int i = 0; i < m_queue.Count; ++i)
			{
				QueuedUnit unit = m_queue[i];
				unit.percentage += Time.deltaTime;
				if (unit.percentage >= unit.buildTime)
				{
					// Initialise the unit.
					UnitTemplate template = new UnitTemplate();
					Unit newUnit = template.Load(unit.prefab, m_position - new Vector3(15f, 0f, 15f), new Vector3(0f, 90f, 0f));
					Main.m_unitList.Add(newUnit);
					MinimapIcon icon;
					newUnit.GetIcon(out icon);
					UserInterface.m_sidePanel.m_minimap.AddIcon(ref icon);

					delete = unit;
					break;
				}
			}

			if (delete != null)
				m_queue.Remove(delete);
		}

		public void QueueUnit(SidePanel a_sidePanel, UnitArgs a_events)
		{
			QueuedUnit unit = new QueuedUnit();
			unit.prefab = a_events.prefab;
			unit.buildTime = a_events.prefab.buildTime;
			unit.percentage = 0f;
			m_queue.Add(unit);
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
			panel.ProcessBuildingList();
			panel.UnitEvent -= new SidePanel.UnitHandler(QueueUnit);
		}
	}
}