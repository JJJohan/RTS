using UnityEngine;
using System.Collections.Generic;

namespace RTS
{	
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
	
	public partial class Main : MonoBehaviour 
	{		
		public List<Unit> m_unitList;
		public List<Building> m_buildingList;
		private Resources m_res;
		
		void Start()
		{	
			m_unitList = new List<Unit>(256);
			m_buildingList = new List<Building>(64);
			m_res = new Resources();
			m_res.funds = 3100;
			m_res.power = 0;
			m_res.powerUsed = 0;
			
			InitInput();
			InitGUI();
			ParseFiles();

			//////////
			// TEMP //
			//////////
			
			CreateBuildingGhost("CONSTRUCTION_YARD");		
			
			//////////
		}
		
		// Update is called once per frame
		void Update() 
		{
			List<Selectable> m_destroyList = new List<Selectable>();
			
			// Update buildings
			foreach (Building building in m_buildingList)
			{
				building.Process(ref m_res);
				if (building.Destroyed())
				{
					m_res.powerUsed -= building.Power();
					m_destroyList.Add(building);
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
				if (m_destroyList[i].tag == "Building")
					m_buildingList.Remove((Building)m_destroyList[i]);
				else if (m_destroyList[i].tag == "Unit")
					m_unitList.Remove((Unit)m_destroyList[i]);
				
				Destroy (m_destroyList[i]);
			}
			
			// Update GUI
			m_sidePanel.Update(m_res);
			
			// Update Input
			ProcessInput();
		}
		
		// Place a building.
		Building CreateBuilding(BuildingPrefab a_prefab, Vector3 a_pos, Vector3 a_rot)
		{			
			if (a_prefab.cost <= m_res.funds)
			{
				// Initialise the building.
				GameObject obj = new GameObject();
				BuildingTemplate template = obj.AddComponent<BuildingTemplate>();
				template.Load(a_prefab, ref m_dataFile);
				template.Construct(a_pos, a_rot);
				m_buildingList.Add(template);
				MinimapIcon icon;
				template.GetIcon(out icon);
				m_sidePanel.GetMinimap().AddIcon(ref icon);
				
				// Update available resources.
				m_res.powerUsed += template.Power();
				m_res.buildings.Add(a_prefab.ID);
				
				return template;
			}
			
			return null;
		}
		
		// Create a ghost building to show where it is being placed.
		void CreateBuildingGhost(string a_name)
		{
			// Fetch prefab
			BuildingPrefab prefab;
			if (m_res.prefabs.buildingPrefabs.TryGetValue("CONSTRUCTION_YARD", out prefab))
			{
				// Instantiate the ghost.
				m_cursorMode = Cursor.BUILD;
				m_selectionType = Selection.NONE;
				m_cursorBuilding = new GameObject();
				GhostBuilding script = m_cursorBuilding.AddComponent<GhostBuilding>();
				script.Create(prefab, ref m_dataFile);
			}
		}
		
		// Place a unit.
		void CreateUnit(string a_name, Vector3 a_pos)
		{			
			GameObject obj = (GameObject)Instantiate(UnityEngine.Resources.Load(a_name));
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
			}
		}
	}
}