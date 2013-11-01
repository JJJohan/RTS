using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

namespace RTS
{
	public class BuildingArgs : EventArgs
	{
		public BuildingPrefab prefab;
	}

	public class UnitArgs : EventArgs
	{
		public UnitPrefab prefab;
		public bool create;
	}

	public class SidePanel
	{
		public struct PrefabButton
		{
			public UnitPrefab unit;
			public BuildingPrefab building;
			public int ID;
			public int cost;
			public int type;
			public int count;
			public Texture2D cameo;
		}

		private struct Type
		{
			public const int None = 0;
			public const int Building = 1;
			public const int Unit = 2;
		}

		public static Material m_guiMat;

		public delegate void BuildingHandler(SidePanel a_sidePanel, BuildingArgs a_events);
		public delegate void UnitHandler(SidePanel a_sidePanel, UnitArgs a_events);

		public event BuildingHandler BuildingEvent;
		public event UnitHandler UnitEvent;

		public Minimap m_minimap;
		private GUIStyle m_textStyle;
		private Texture2D m_guiPanel;
		private Button m_buttonLeft;
		private Button m_buttonRight;
		//private Texture2D m_guiUpgrade;
		private int m_buildingCount;
		private int m_index;
		private List<Button> m_buttons;
		private List<int> m_unitList;
		private List<PrefabButton> m_prefabButtons;
		private static Rect[] m_origPositions;
		private static Rect[] m_positions;
		private static int m_display;
		private static Texture2D m_progressTex;
		private static Material m_progressMat;

		public SidePanel()
		{
			// Init
			m_index = 0;
			m_prefabButtons = new List<PrefabButton>();
			m_buildingCount = -1;
			m_display = Type.Building;
			m_progressTex = (Texture2D)UnityEngine.Resources.Load("Textures/angular");
			m_progressMat = new Material(Shader.Find("ButtonProgress"));
			m_progressMat.color = new Color(1f, 1f, 0f, 0.5f);
			
			m_positions = new Rect[8];
			m_origPositions = new Rect[8];
			m_origPositions[0] = new Rect(1920 - 224, 452, 100, 100);
			m_origPositions[1] = new Rect(1920 - 116, 452, 100, 100);
			m_origPositions[2] = new Rect(1920 - 224, 561, 100, 100);
			m_origPositions[3] = new Rect(1920 - 116, 561, 100, 100);
			m_origPositions[4] = new Rect(1920 - 224, 670, 100, 100);
			m_origPositions[5] = new Rect(1920 - 116, 670, 100, 100);
			m_origPositions[6] = new Rect(1920 - 224, 779, 100, 100);
			m_origPositions[7] = new Rect(1920 - 116, 779, 100, 100);
			
			// Text GUI Style
			m_textStyle = new GUIStyle();
			m_textStyle.font = (Font)UnityEngine.Resources.Load("Fonts/gil");
			m_textStyle.fontSize = 24;
			m_textStyle.alignment = TextAnchor.MiddleCenter;
			m_textStyle.normal.textColor = Color.white;
			
			// Panel Textures
			m_guiMat = new Material(Shader.Find("Billboard"));
			m_guiPanel = (Texture2D)UnityEngine.Resources.Load("Textures/panel");
			
			// Left Button
			Texture2D leftUp = (Texture2D)UnityEngine.Resources.Load("Textures/panel_left");
			Texture2D leftDown = (Texture2D)UnityEngine.Resources.Load("Textures/panel_left_down");
			m_buttonLeft = new Button(leftUp, leftDown, UserInterface.ResizeGUI(new Rect(1920 - 234, 898, 26, 63)));
			
			// Right Button
			Texture2D rightUp = (Texture2D)UnityEngine.Resources.Load("Textures/panel_right");
			Texture2D rightDown = (Texture2D)UnityEngine.Resources.Load("Textures/panel_right_down");
			m_buttonRight = new Button(rightUp, rightDown, UserInterface.ResizeGUI(new Rect(1920 - 34, 898, 26, 63)));
				
			// Main Buttons
			Texture2D buttonUp = (Texture2D)UnityEngine.Resources.Load("Textures/panel_button");
			Texture2D buttonDown = (Texture2D)UnityEngine.Resources.Load("Textures/panel_button_down");
			m_buttons = new List<Button>();
			for (int i = 0; i < 8; ++i)
			{
				Rect rect = new Rect(m_origPositions[i].x - 4, m_origPositions[i].y - 4, m_origPositions[i].width + 4, m_origPositions[i].height + 4);
				Button button = new Button(buttonUp, buttonDown, rect);
				m_buttons.Add(button);
			}
			
			// Minimap
			m_minimap = new Minimap();

			// Initial bounds update
			UpdateBounds();
		}

		public void UpdateBounds()
		{
			// Update cameo positions
			for (int i = 0; i < 8; ++i)
				m_positions[i] = UserInterface.ResizeGUI(m_origPositions[i]);

			// Update minimap
			m_minimap.UpdateBounds();

			// Update buttons
			for (int i = 0; i < 8; ++i)
				m_buttons[i].UpdateBounds();

			// Update other buttons
			m_buttonLeft.UpdateBounds();
			m_buttonRight.UpdateBounds();
		}

		public void Update()
		{
			if (Main.m_res.buildings.Count != m_buildingCount)
			{
				m_buildingCount = Main.m_res.buildings.Count;
				if (m_display == Type.Building)
					ListBuildings();
				else if (m_display == Type.Unit)
					ListUnits();
			}
			
			// Update buttons
			if (Main.m_event != null)
			{
				// Left & Right Arrows
				if (m_index > 0)
				{
					if (m_buttonLeft.Process(Main.m_event) == Button.MOUSE1UP)
					{
						--m_index;
					}
				}
				if (m_index < m_prefabButtons.Count / 8)
				{
					if (m_buttonRight.Process(Main.m_event) == Button.MOUSE1UP)
					{
						++m_index;
					}
				}
				
				// Update main buttons
				for (int i = 0; i < 8; ++i)
				{
					int index = i + (m_index * 8);
					if (index >= m_prefabButtons.Count)
						break;
					
					// Check if we can afford the building or unit.
					m_buttons[index].Lock(Main.m_res.funds < m_prefabButtons[index].cost && m_prefabButtons[index].count < 20);
					
					// Process buttons
					int eventType = m_buttons[index].Process(Main.m_event);
					if (eventType == Button.MOUSE1UP)
					{
						if (m_prefabButtons[index].type == Type.Building)
						{
							if (BuildingEvent != null)
							{
								BuildingArgs args = new BuildingArgs();
								args.prefab = m_prefabButtons[index].building;
								BuildingEvent(this, args);
								break;
							}
						}
						else
						{
							if (UnitEvent != null)
							{
								UnitArgs args = new UnitArgs();
								args.prefab = m_prefabButtons[index].unit;
								args.create = true;
								UnitEvent(this, args);

								PrefabButton button = m_prefabButtons[index];
								++button.count;
								m_prefabButtons[index] = button;

								break;
							}
						}
					}
					else if (eventType == Button.MOUSE2UP)
					{
						if (m_prefabButtons[index].type == Type.Unit && m_prefabButtons[index].count > 0)
						{
							UnitArgs args = new UnitArgs();
							args.prefab = m_prefabButtons[index].unit;
							args.create = false;
							UnitEvent(this, args);

							PrefabButton button = m_prefabButtons[index];
							--button.count;
							m_prefabButtons[index] = button;

							break;
						}
					}
				}
			}
		}

		private void ListUnits()
		{
			ListUnits(m_unitList);
		}

		public void ListUnits(List<int> a_units)
		{
			// Clear button list
			m_prefabButtons.Clear();
			m_display = Type.Unit;
			m_unitList = a_units;

			// Check against building tech requirements.
			foreach (int key in a_units)
			{
				UnitPrefab prefab = Main.m_res.prefabs.unitPrefabs[key];
				
				int reqs = prefab.techReqs.Count;
				for (int j = 0; j < prefab.techReqs.Count; ++j)
				{
					for (int k = 0; k < Main.m_res.buildings.Count; ++k)
					{
						if (prefab.techReqs[j] == Main.m_res.buildings[k])
							--reqs;
						
						if (reqs == 0)
							goto Buttons;
					}
					
				}
				
				Buttons:	
				// Add buttons if requirements are met.
				if (reqs == 0)
				{
					PrefabButton button = new PrefabButton();
					button.unit = prefab;
					button.cameo = prefab.cameo;
					button.cost = prefab.cost;
					button.type = Type.Unit;
					button.ID = prefab.ID;
					button.count = 0;
					m_prefabButtons.Add(button);
				}
			}
			
			// Order buttons
			m_prefabButtons.Sort((x, y) => x.unit.menuID.CompareTo(y.unit.menuID));
		}

		public void Clear()
		{
			// Clear button list
			m_prefabButtons.Clear();
			m_display = Type.None;
		}

		public void ListBuildings()
		{
			// Clear button list
			m_prefabButtons.Clear();
			m_display = Type.Building;
			
			// Check against building tech requirements.
			foreach (int key in Main.m_res.prefabs.buildingPrefabs.Keys)
			{
				BuildingPrefab prefab = Main.m_res.prefabs.buildingPrefabs[key];
				
				int reqs = prefab.techReqs.Count;
				for (int j = 0; j < prefab.techReqs.Count; ++j)
				{
					for (int k = 0; k < Main.m_res.buildings.Count; ++k)
					{
						if (prefab.techReqs[j] == Main.m_res.buildings[k])
							--reqs;
						
						if (reqs == 0)
							goto Buttons;
					}
					
				}
				
				Buttons:	
				// Add buttons if requirements are met.
				if (reqs == 0)
				{
					PrefabButton button = new PrefabButton();
					button.building = prefab;
					button.cameo = prefab.cameo;
					button.cost = prefab.cost;
					button.type = Type.Building;
					button.ID = prefab.ID;
					button.count = 0;
					m_prefabButtons.Add(button);
				}
			}
			
			// Order buttons
			m_prefabButtons.Sort((x, y) => x.building.menuID.CompareTo(y.building.menuID));
		}

		public void Draw()
		{
			// Draw scaled GUI.
			GUI.DrawTexture(UserInterface.ResizeGUI(new Rect(1920 - 240, 0, 240, 1080)), m_guiPanel);
			m_minimap.Draw();

			// Draw power available / required.
			if (Main.m_res.powerUsed > Main.m_res.power)
				m_textStyle.normal.textColor = Color.red;
			else if (Main.m_res.power - Main.m_res.powerUsed < 10)
				m_textStyle.normal.textColor = Color.yellow;
			else
				m_textStyle.normal.textColor = Color.white;
			GUI.Label(UserInterface.ResizeGUI(new Rect(1920 - 225, 265, 209, 27)), (Main.m_res.powerUsed).ToString() + " / " + (Main.m_res.power).ToString(), m_textStyle);
			m_textStyle.normal.textColor = Color.white;
			
			// Draw available funds.
			GUI.Label(UserInterface.ResizeGUI(new Rect(1920 - 225, 365, 209, 27)), (Main.m_res.funds).ToString(), m_textStyle);
			
			// Draw buttons.
			for (int i = 0; i < 8; ++i)
			{
				int index = i + (m_index * 8);
				if (index >= m_prefabButtons.Count)
					break;

				m_buttons[i].Draw();
				Graphics.DrawTexture(m_positions[i], m_prefabButtons[index].cameo, m_buttons[i].GetMaterial());
			}

			// Draw page count and buttons
			if (m_prefabButtons.Count > 8)
			{
				if (m_index > 0)
				{
					m_buttonLeft.Draw();
				}
				if (m_index < m_prefabButtons.Count / 8)
				{
					m_buttonRight.Draw();
				}
					
				GUI.Label(UserInterface.ResizeGUI(new Rect(1920 - 172, 914, 100, 27)), m_index + 1 + " / " + (m_prefabButtons.Count / 8 + 1), m_textStyle); 
			}
		}

		public void UpdateQueue(int m_ID, bool a_add)
		{
			for (int i = 0; i < 8; ++i)
			{
				int index = i + (m_index * 8);
				if (index >= m_prefabButtons.Count)
					break;

				if (m_prefabButtons[index].ID == m_ID)
				{
					if (a_add)
					{
						PrefabButton button = m_prefabButtons[index];
						++button.count;
						m_prefabButtons[index] = button;
					}
					else
					{
						PrefabButton button = m_prefabButtons[index];
						--button.count;
						m_prefabButtons[index] = button;
					}
				}
			}
		}

		public void DrawQueue(int m_current, float a_percent)
		{
			if (m_display != Type.Unit)
				return;

			for (int i = 0; i < 8; ++i)
			{
				int index = i + (m_index * 8);
				if (index >= m_prefabButtons.Count)
					break;

				if (m_prefabButtons[index].count > 0)
				{
					// Check if front queue item
					if (m_prefabButtons[index].ID == m_current)
						m_progressMat.SetFloat("_Cutoff", a_percent/2);
					else
						m_progressMat.SetFloat("_Cutoff", 0f);

					// Draw progress circle
					Graphics.DrawTexture(m_positions[index], m_progressTex, m_progressMat);

					// Draw number
					if (m_prefabButtons[index].count > 1)
					{
						GUI.Label(m_positions[i], m_prefabButtons[index].count.ToString(), m_textStyle);
					}
				}
			}
		}
	}
}