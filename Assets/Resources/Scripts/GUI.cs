using UnityEngine;

namespace RTS
{
    public static class UserInterface
    {
        private static Texture2D m_guiSelect;
        public static SidePanel m_sidePanel;
        public static Vector2 m_resolution;

        public static void InitGUI()
        {           
            // Init
            m_resolution = new Vector2(Screen.width, Screen.height);

            // Area Selection Texture
            m_guiSelect = (Texture2D)UnityEngine.Resources.Load("Textures/gray");
            
            // Side Panel
            m_sidePanel = new SidePanel();
            m_sidePanel.SelectableEvent += new SidePanel.SelectableHandler(Main.CreateBuildingGhost);
        }

        public static void Update()
        {
            // Update the command interface.
            if (m_sidePanel != null)
                m_sidePanel.Update();

            // Check for screen resolution changes.
            if (m_resolution.x != Screen.width || m_resolution.y != Screen.height)
            {
                m_resolution = new Vector2(Screen.width, Screen.height);
                m_sidePanel.UpdateBounds();
            }
        }

        public static void Draw()
        {                       
            // Draw selection rectangle.
            if (InputHandler.m_clickPos.x > -0.5f)
            {
                Vector3 mousePos = new Vector3(Input.mousePosition.x, Screen.height, 0.0f) - new Vector3(0.0f, Input.mousePosition.y, 0.0f);
                GUI.DrawTexture(new Rect(InputHandler.m_clickPos.x, InputHandler.m_clickPos.y, mousePos.x - InputHandler.m_clickPos.x, mousePos.y - InputHandler.m_clickPos.y), m_guiSelect);
                Line.Draw(InputHandler.m_clickPos, new Vector2(mousePos.x, InputHandler.m_clickPos.y));
                Line.Draw(new Vector2(mousePos.x, InputHandler.m_clickPos.y), mousePos);
                Line.Draw(mousePos, new Vector2(InputHandler.m_clickPos.x, mousePos.y));
                Line.Draw(new Vector2(InputHandler.m_clickPos.x, mousePos.y), InputHandler.m_clickPos);
            }
            
            // Draw the command interface.
            if (m_sidePanel != null)
                m_sidePanel.Draw();
        }

        static public Rect ResizeGUI(Rect a_rect)
        {
            float FilScreenWidth = a_rect.width / 1920;
            float rectWidth = FilScreenWidth * Screen.width;
            float FilScreenHeight = a_rect.height / 1080;
            float rectHeight = FilScreenHeight * Screen.height;
            float rectX = (a_rect.x / 1920) * Screen.width;
            float rectY = (a_rect.y / 1080) * Screen.height;
         
            return new Rect(rectX, rectY, rectWidth, rectHeight);
        }

        static public Vector2 ResizeGUI(Vector2 a_pos)
        {
            return new Vector2((a_pos.x / 1920) * Screen.width, (a_pos.y / 1080) * Screen.height);
        }
    }
}