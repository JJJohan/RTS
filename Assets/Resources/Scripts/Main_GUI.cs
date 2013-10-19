using UnityEngine;

namespace RTS
{		
	public partial class Main : MonoBehaviour 
	{	
		private Texture2D m_guiSelect;
		private SidePanel m_sidePanel;
		
		void InitGUI()
		{			
			// Area Selection Texture
			m_guiSelect = (Texture2D)UnityEngine.Resources.Load("Textures/gray");
			
			// Side Panel
			m_sidePanel = new SidePanel();
		}

		void OnGUI()
		{		
			// Draw selection rectangle.
			if (m_clickPos.x > -0.5f)
			{
				Vector3 mousePos = new Vector3(Input.mousePosition.x, Screen.height, 0.0f) - new Vector3(0.0f, Input.mousePosition.y, 0.0f);
				GUI.DrawTexture(new Rect(m_clickPos.x, m_clickPos.y, mousePos.x - m_clickPos.x, mousePos.y - m_clickPos.y), m_guiSelect);
				Line.Draw(m_clickPos, new Vector2(mousePos.x, m_clickPos.y));
				Line.Draw(new Vector2(mousePos.x, m_clickPos.y), mousePos);
				Line.Draw(mousePos, new Vector2(m_clickPos.x, mousePos.y));
				Line.Draw(new Vector2(m_clickPos.x, mousePos.y), m_clickPos);
			}
			
			// Draw the command interface.
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