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

    public class SidePanel
    {
        public struct PrefabButton
        {
            public BuildingPrefab prefab;
            public Texture2D cameo;
        }

        public static Material m_guiMat;

        public delegate void SelectableHandler(SidePanel a_sidePanel,BuildingArgs a_events);

        public event SelectableHandler SelectableEvent;

        public Minimap m_minimap;
        private GUIStyle m_textStyle;
        private Texture2D m_guiPanel;
        private Button m_buttonLeft;
        private Button m_buttonRight;
        //private Texture2D m_guiUpgrade;
        private int m_buildingCount;
        private int m_index;
        private List<Button> m_buttons;
        private List<PrefabButton> m_prefabButtons;
        private static Rect[] m_origPositions;
        private static Rect[] m_positions;

        public SidePanel()
        {
            // Init
            m_index = 0;
            m_prefabButtons = new List<PrefabButton>();
            m_buildingCount = -1;
            
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
            m_textStyle.font = (Font)UnityEngine.Resources.Load("Fonts/coop");
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
                ProcessBuildingList();
            }
            
            // Update buttons
            if (Main.m_event != null)
            {
                // Left & Right Arrows
                if (m_index > 0)
                {
                    if (m_buttonLeft.Process(Main.m_event) == Button.UP)
                    {
                        --m_index;
                    }
                }
                if (m_index < m_prefabButtons.Count / 8)
                {
                    if (m_buttonRight.Process(Main.m_event) == Button.UP)
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
                    
                    // Check if we can afford the building or unity.
                    m_buttons[index].Lock(Main.m_res.funds < m_prefabButtons[index].prefab.cost);
                    
                    // Process buttons
                    if (m_buttons[index].Process(Main.m_event) == Button.UP)
                    {
                        if (SelectableEvent != null)
                        {
                            BuildingArgs args = new BuildingArgs();
                            args.prefab = m_prefabButtons[index].prefab;
                            SelectableEvent(this, args);
                            break;
                        }
                    }
                }
            }
        }

        private void ProcessBuildingList()
        {
            // Clear button list
            m_prefabButtons.Clear();
            
            // Check against building tech requirements.
            foreach (string key in Main.m_res.prefabs.buildingPrefabs.Keys)
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
                    button.prefab = prefab;
                    button.cameo = prefab.cameo;
                    m_prefabButtons.Add(button);
                }
            }
            
            // Order buttons
            m_prefabButtons.Sort((x, y) => x.prefab.menuID.CompareTo(y.prefab.menuID));
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
    }
}