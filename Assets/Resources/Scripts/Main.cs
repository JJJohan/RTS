using UnityEngine;
using System.Collections.Generic;

namespace RTS
{	
	public partial class Main : MonoBehaviour 
	{		
		public List<Unit> m_unitList;
		public List<Building> m_buildingList;
		public List<Selectable> m_selected;
		private RaycastHit m_hit1, m_hit2;
		private Ray m_ray1, m_ray2;
		private Vector2 m_clickPos;
		private Resources m_res;
		private Texture2D m_guiPanel;
		private Texture2D m_guiSelect;
		private int m_cursorMode;
		private int m_selectionType;
		private GameObject m_cursorBuilding;
		private Vector3 m_cursorOffset;
		
		void Start()
		{	
			m_unitList = new List<Unit>(256);
			m_buildingList = new List<Building>(64);
			m_selected = new List<Selectable>(32);
			m_cursorMode = Cursor.SELECTION;
			m_selectionType = Selection.NONE;
			m_res = new Resources();
			m_clickPos = new Vector2(-1f,-1f);	
			
			// RTS Panel
			m_guiPanel = (Texture2D)UnityEngine.Resources.Load("Textures/panel");
			m_guiSelect = (Texture2D)UnityEngine.Resources.Load("Textures/gray");
			
			m_res.funds = 3100f;
			m_res.power = 50f;	
			
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
			// Update buildings
			foreach (Building building in m_buildingList)
			{
				building.Process(ref m_res);
			}
			
			// Update units
			//foreach (Unit unit in m_unitList)
			{
				//unit.Process(ref m_res);
			}
			
			ProcessInput();
		}
		
		// Place a building.
		void CreateBuilding(string a_name, Vector3 a_pos)
		{			
			GameObject obj = (GameObject)Instantiate(UnityEngine.Resources.Load(a_name));
			if (obj)
			{
				Building building = (Building)obj.GetComponent(a_name);
				building.Position = a_pos;	
				building.Construct();
				m_buildingList.Add(building);
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
			MeshFilter filter = m_cursorBuilding.AddComponent<MeshFilter>();
			MeshRenderer renderer = m_cursorBuilding.AddComponent<MeshRenderer>();
			
			// Copy details.
			GameObject tempBuilding = (GameObject)Instantiate(UnityEngine.Resources.Load(a_name));
			m_cursorOffset = tempBuilding.GetComponent<Building>().Offset();
			m_cursorBuilding.transform.localScale = tempBuilding.transform.localScale;
			filter.mesh = tempBuilding.GetComponent<MeshFilter>().mesh;
			renderer.material = tempBuilding.GetComponent<MeshRenderer>().material;
			renderer.material.color = new Color(1f, 1f, 1f, .5f);
			renderer.material.shader = Shader.Find("Transparent/Diffuse");
			Destroy(tempBuilding);
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