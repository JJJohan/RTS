using UnityEngine;

namespace RTS
{
    public class Button
    {
        public const int UNPRESSED = 0;
        public const int DOWN = 1;
        public const int UP = 2;
        private Rect m_bounds;
        private Rect m_origBounds;
        private Texture2D m_up;
        private Texture2D m_down;
        private Texture2D m_current;
        private Material m_mat;
        private bool m_locked;

        public Material GetMaterial()
        {
            return m_mat;
        }

        public bool Locked()
        {
            return m_locked;
        }

        public Button(Texture2D a_up, Texture2D a_down, Rect a_bounds)
        {
            m_locked = false;
            m_up = a_up;
            m_down = a_down;
            m_origBounds = a_bounds;
            m_current = m_up;
            m_mat = new Material(Shader.Find("Billboard"));
            UpdateBounds();
        }

        public void Lock(bool a_locked)
        {
            if (m_locked == a_locked)
                return;

            if (m_locked = a_locked)
                m_mat.color = new Color(0.5f, 0.5f, 0.5f);
            else
                m_mat.color = Color.white;
        }

        public void UpdateBounds()
        {
            m_bounds = UserInterface.ResizeGUI(m_origBounds);
        }

        public int Process(Event a_event)
        {
            if (m_locked)
                return UNPRESSED;
            
            if (a_event.type == EventType.MouseDown)
            {
                return Down(a_event.mousePosition);
            }
            else if (a_event.type == EventType.MouseUp)
            {
                return Up(a_event.mousePosition);
            }
            else if (m_current == m_down && a_event.mousePosition.x > 0 && !m_bounds.Contains(a_event.mousePosition))
            {
                Debug.Log(a_event.mousePosition);
                m_current = m_up;
            }
            
            return UNPRESSED;
        }

        public int Up(Vector2 a_pos)
        {
            if (m_locked)
                return UNPRESSED;
            
            if (m_bounds.Contains(a_pos))
            {
                m_current = m_up;
                return UP;
            }
            
            return UNPRESSED;
        }

        public int Down(Vector2 a_pos)
        {
            if (m_locked)
                return UNPRESSED;
            
            if (m_bounds.Contains(a_pos))
            {
                m_current = m_down;
                return DOWN;
            }
            
            return UNPRESSED;
        }

        public void Draw()
        {
            Graphics.DrawTexture(m_bounds, m_current, m_mat);
        }
    }
}