using UnityEngine;
using System.Collections.Generic;

namespace RTS
{
    public class MinimapIcon
    {
        private Vector2 m_size;
        private Vector2 m_pos;
        private Material m_icon;
        private Rect m_bounds;
        private Rect m_iconRect;
        bool m_static;

        public MinimapIcon(Vector2 a_size, bool a_static)
        {
            // Create the icon material
            m_icon = new Material(Shader.Find("Self-Illumin/Diffuse"));
            m_icon.mainTexture = (Texture2D)UnityEngine.Resources.Load("Textures/blank");
            m_icon.color = Color.blue;
            m_size = a_size;
            m_static = a_static;
            m_iconRect = new Rect(-1, -1, -1, -1);
        }

        public void SetBounds(Rect a_bounds)
        {
            m_bounds = a_bounds;
			
            // If static, precalculate minimap position.
            if (m_static)
            {				
                Vector2 pos = new Vector2(m_pos.x / 1000f * m_bounds.width, m_pos.y / 1000f * m_bounds.height);
	
                m_iconRect = new Rect(m_bounds.x - m_size.x / 2 + m_bounds.width / 2 + pos.x,
                          m_bounds.y - m_size.y / 2 + m_bounds.height / 2 + pos.y, 
                          m_size.x, m_size.y);
            }
        }

        public void Process(Vector2 a_pos)
        {
            m_pos = a_pos;
			
            if (m_static && m_iconRect.x > 0f)
                Debug.LogWarning("Static minimap icon is being updated.");
        }

        public void SetColour(Color a_color)
        {
            m_icon.color = a_color;
        }

        public void Draw()
        {
            // Calculate icon position if dynamic
            if (!m_static)
            {
                Vector2 pos = new Vector2(m_pos.x / 1000f * m_bounds.width, m_pos.y / 1000f * m_bounds.height);
	
                m_iconRect = new Rect(m_bounds.x - m_size.x / 2 + m_bounds.width / 2 + pos.x,
                          m_bounds.y - m_size.y / 2 + m_bounds.height / 2 + pos.y, 
                          m_size.x, m_size.y);
            }
			
            Graphics.DrawTexture(m_iconRect, m_icon.mainTexture, m_icon);
        }
    }
}