using UnityEngine;
using System.Collections.Generic;

namespace RTS
{	
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
			m_res.power = 50;
			m_res.powerUsed = 0;
			
			InitInput();
			InitGUI();

			//////////
			// TEMP //
			//////////
			
			//CreateBuilding("ConstructionYard", new Vector3(20,5,3));
			
			m_cursorMode = Cursor.BUILD;
			CreateBuildingGhost("ConstructionYard");
			
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
			
			ProcessInput();
		}
		
		// Place a building.
		Building CreateBuilding(string a_name, Vector3 a_pos, Vector3 a_rot)
		{			
			GameObject obj = (GameObject)Instantiate(UnityEngine.Resources.Load(a_name));
			if (obj)
			{
				Building building = (Building)obj.GetComponent(a_name);
				if (building.Cost() <= m_res.funds)
				{
					// Update available resources.
					m_res.funds -= building.Cost();
					m_res.powerUsed += building.Power();
					
					// Create the building.
					building.Construct(a_pos, a_rot);
					m_buildingList.Add(building);
					return building;
				}
				else
				{
					Destroy(obj);
					return null;
				}
			}
			else
			{
				throw new UnityException();
			}
		}
		
		// Create a ghost building to show where it is being placed.
		void CreateBuildingGhost(string a_name)
		{
			// Instantiate the ghost.
			m_cursorBuilding = new GameObject();
			GhostBuilding script = m_cursorBuilding.AddComponent<GhostBuilding>();
			script.Copy(a_name);
			m_cursorOffset = script.Offset();
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