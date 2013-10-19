using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace RTS
{			
	public class SidePanel
	{	
		private struct Button
		{
			public BuildingPrefab prefab;
			public Material cameo;
		}
		
		private Minimap m_minimap;
		private GUIStyle m_gui;
		private Texture2D m_guiPanel;
		//private Texture2D m_guiLeft;
		//private Texture2D m_guiRight;
		//private Texture2D m_guiUpgrade;
		private Resources m_res;
		private int m_buildingCount;
		private int m_index;
		private List<Button> m_buttons;
		private static Rect[] m_positions;
		
		public Minimap GetMinimap() { return m_minimap; }
		
		public SidePanel()
		{
			// Init
			m_index = 0;
			m_buttons = new List<Button>();
			m_positions = new Rect[8];
			m_positions[0] = Main.ResizeGUI(new Rect(1920 - 224, 452, 100, 100));
			m_positions[1] = Main.ResizeGUI(new Rect(1920 - 116, 452, 100, 100));
			m_positions[2] = Main.ResizeGUI(new Rect(1920 - 224, 561, 100, 100));
			m_positions[3] = Main.ResizeGUI(new Rect(1920 - 116, 561, 100, 100));
			m_positions[4] = Main.ResizeGUI(new Rect(1920 - 224, 670, 100, 100));
			m_positions[5] = Main.ResizeGUI(new Rect(1920 - 116, 670, 100, 100));
			m_positions[6] = Main.ResizeGUI(new Rect(1920 - 224, 779, 100, 100));
			m_positions[7] = Main.ResizeGUI(new Rect(1920 - 116, 779, 100, 100));
			
			// GUI Style
			m_gui = new GUIStyle();
			m_gui.font = (Font)UnityEngine.Resources.Load("Fonts/coop");
			m_gui.fontSize = 24;
			m_gui.alignment = TextAnchor.MiddleCenter;
			m_gui.normal.textColor = Color.white;
			
			// Panel Textures
			m_guiPanel = (Texture2D)UnityEngine.Resources.Load("Textures/panel");
			//m_guiLeft = (Texture2D)UnityEngine.Resources.Load("Textures/panel_left");
			//m_guiRight = (Texture2D)UnityEngine.Resources.Load("Textures/panel_right");
			//m_guiUpgrade = (Texture2D)UnityEngine.Resources.Load("Textures/panel_upgrade");
			
			// Minimap
			GameObject minimap = new GameObject();
			m_minimap = minimap.AddComponent<Minimap>();
		}
				
		public void Update(Resources a_res)
		{
			m_res = a_res;
			if (m_res.buildings.Count != m_buildingCount)
			{
				m_buildingCount = m_res.buildings.Count;
				ProcessBuildingList();
			}
		}
		
		private void ProcessBuildingList()
		{
			// Clear button list
			m_buttons.Clear();
			
			// Check against building tech requirements.
			foreach (string key in m_res.prefabs.buildingPrefabs.Keys)
			{
				BuildingPrefab prefab = m_res.prefabs.buildingPrefabs[key];
				
				int reqs = prefab.techReqs.Count;
				for (int j = 0; j < prefab.techReqs.Count; ++j)
				{
					for (int k = 0; k < m_res.buildings.Count; ++k)
					{
						if (prefab.techReqs[j] == m_res.buildings[k])
						--reqs;
						
						if (reqs == 0)
							goto Buttons;
					}
					
				}
				
				Buttons:	
				// Add buttons if requirements are met.
				if (reqs == 0)
				{
					Button button = new Button();
					button.prefab = prefab;
					button.cameo = new Material(Shader.Find("Billboard"));
					button.cameo.mainTexture = Main.LoadImage(prefab.cameoPath, prefab.dataItem);
					m_buttons.Add(button);
				}
			}
			
			// Order buttons
			m_buttons.Sort((x, y) => x.prefab.menuID.CompareTo(y.prefab.menuID));
		}
		
		public void Draw()
		{
			// Draw scaled GUI.
		    GUI.DrawTexture(Main.ResizeGUI(new Rect(1920 - 240, 0, 240, 1080)), m_guiPanel);
			m_minimap.Draw();
			
			// Draw power available / required.
			if (m_res.powerUsed > m_res.power)
				m_gui.normal.textColor = Color.red;
			else if (m_res.power - m_res.powerUsed < 10)
				m_gui.normal.textColor = Color.yellow;
			else
				m_gui.normal.textColor = Color.white;
			GUI.Label(Main.ResizeGUI(new Rect(1920 - 225, 265, 209, 27)), ((int)m_res.powerUsed).ToString() + "/" + ((int)m_res.power).ToString(), m_gui);
			m_gui.normal.textColor = Color.white;
			
			// Draw available funds.
			GUI.Label(Main.ResizeGUI(new Rect(1920 - 225, 365, 209, 27)), ((int)m_res.funds).ToString(), m_gui);
			
			// Draw buttons.
			for (int i = 0; i < 8; ++i)
			{
				int index = i + m_index;
				if (index >= m_buttons.Count) break;
				Graphics.DrawTexture(m_positions[i], m_buttons[index].cameo.mainTexture, m_buttons[index].cameo);
			}
		}
	}
}