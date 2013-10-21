using UnityEngine;
using System.Collections.Generic;
using System;

namespace RTS
{
	public class UserData : MonoBehaviour
	{
		public object data;
	}

	public class Resources
	{
		public Resources()
		{
			buildings = new List<string>();
			prefabs.buildingPrefabs = new Dictionary<string, BuildingPrefab>();
			prefabs.unitPrefabs = new Dictionary<string, UnitPrefab>();
		}

		public int powerUsed;
		public int power;
		public int funds;
		public List<string> buildings;
		public Prefabs prefabs;
	}

	public static class Main
	{
		public static List<Unit> m_unitList;
		public static List<Building> m_buildingList;
		public static Resources m_res;
		public static Event m_event;

		public static void Init()
		{	
			m_unitList = new List<Unit>(256);
			m_buildingList = new List<Building>(64);
			m_res = new Resources();
			m_res.funds = 3100;
			m_res.power = 0;
			m_res.powerUsed = 0;
			
			InputHandler.InitInput();
			UserInterface.InitGUI();
			FileParser.ParseFiles();
		}
		// Update is called once per frame
		public static void Update()
		{
			List<Selectable> m_destroyList = new List<Selectable>();
			
			// Update buildings
			m_res.power = 0;
			m_res.powerUsed = 0;
			foreach (Building building in m_buildingList)
			{
				building.Process();
				if (building.Destroyed())
				{
					building.Destroy();
					m_destroyList.Add(building);
				}
				else
				{
					if (building.BuildingType() != Building.Type.POWERFACTORY)
					{
						m_res.powerUsed += building.Power();
					}
					else
					{
						m_res.power += building.Power();
					}
				}
			}
			
			// Update units
			//foreach (Unit unit in m_unitList)
			{
				//unit.Process(ref m_res);
			}
			
			// Destroy units queued for removal.
			for (int i = 0; i < m_destroyList.Count; ++i)
			{
				if (m_destroyList[i].Tag() == "Building")
					m_buildingList.Remove((Building)m_destroyList[i]);
				else if (m_destroyList[i].Tag() == "Unit")
					m_unitList.Remove((Unit)m_destroyList[i]);
			}
			
			// Update GUI
			UserInterface.Update();
			
			// Update Input
			InputHandler.ProcessInput();
		}
		// Draw interface.
		public static void Draw()
		{
			UserInterface.Draw();
		}
		// Place a building.
		public static Building CreateBuilding(BuildingPrefab a_prefab, Vector3 a_pos, Vector3 a_rot)
		{				
			// Initialise the building.;
			BuildingTemplate template = new BuildingTemplate();
			Building building = template.Load(a_prefab);
			building.Construct(a_pos, a_rot);
			m_buildingList.Add(building);
			MinimapIcon icon;
			building.GetIcon(out icon);
			UserInterface.m_sidePanel.m_minimap.AddIcon(ref icon);
			
			return building;
		}
		// Create a ghost building to show where it is being placed.
		public static void CreateBuildingGhost(SidePanel a_sidePanel, BuildingArgs a_events)
		{
			int cost = a_events.prefab.cost;
			if (InputHandler.m_cursorBuilding)
				cost += InputHandler.m_cursorBuilding.GetComponent<GhostBuilding>().m_prefab.cost;
			if (cost > m_res.funds)
				return;
		
			// Discard any previous ghost object.
			if (InputHandler.m_cursorBuilding != null)
			{
				m_res.funds += InputHandler.m_cursorBuilding.GetComponent<GhostBuilding>().m_prefab.cost;
				UnityEngine.Object.Destroy(InputHandler.m_cursorBuilding);
			}
			
			// Instantiate the ghost.
			m_res.funds -= cost;
			InputHandler.m_cursorMode = Cursor.BUILD;
			InputHandler.m_selectionType = Selection.NONE;
			InputHandler.m_cursorBuilding = new GameObject();
			GhostBuilding script = InputHandler.m_cursorBuilding.AddComponent<GhostBuilding>();
			script.Create(a_events.prefab);
		}
		// Place a unit.
		public static void CreateUnit(string a_name, Vector3 a_pos)
		{			
			/*GameObject obj = (GameObject)Instantiate(UnityEngine.Resources.Load(a_name));
			if (obj)
			{
				if (obj == null) throw new UnityException();
				Unit unit = (Unit)obj.GetComponent(a_name);
				// TODO: Init unit?
				m_unitList.Add(unit);
			}
			else
			{
				throw new UnityException();
			}*/
		}
	}
}