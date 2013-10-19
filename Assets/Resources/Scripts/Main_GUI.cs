using UnityEngine;

namespace RTS
{	
	public partial class Main : MonoBehaviour 
	{	
		private Texture2D m_guiPanel;
		private Texture2D m_guiSelect;
		private GUIStyle m_gui;
		private Material m_minimap;
		
		void InitGUI()
		{
			// RTS Panel
			m_guiPanel = (Texture2D)UnityEngine.Resources.Load("Textures/panel");
			m_guiSelect = (Texture2D)UnityEngine.Resources.Load("Textures/gray");
			m_minimap = (Material)UnityEngine.Resources.Load("textures/minimapRT");
			
			// GUI Style
			m_gui = new GUIStyle();
			m_gui.font = (Font)UnityEngine.Resources.Load("Fonts/coop");
			m_gui.fontSize = 24;
			m_gui.alignment = TextAnchor.MiddleCenter;
			m_gui.normal.textColor = Color.white;
		}
		
		void OnGUI()
		{		
			// Draw selection rectangle.
			if (m_clickPos.x > -0.5f)
			{
				Vector3 mousePos = new Vector3(Input.mousePosition.x, Screen.height, 0.0f) - new Vector3(0.0f, Input.mousePosition.y, 0.0f);
				GUI.DrawTexture(new Rect(m_clickPos.x, m_clickPos.y, mousePos.x - m_clickPos.x, mousePos.y - m_clickPos.y), m_guiSelect);
			}
				
			// Draw scaled GUI.
		    GUI.DrawTexture(ResizeGUI(new Rect(1920 - 240, 0, 240, 1080)), m_guiPanel);
			Graphics.DrawTexture(ResizeGUI(new Rect(1920 - 224, 16, 209, 184)), m_minimap.mainTexture, m_minimap);
			
			// Draw power available / required.
			GUI.Label(ResizeGUI(new Rect(1920 - 225, 265, 209, 27)), ((int)m_res.powerUsed).ToString() + "/" + ((int)m_res.power).ToString(), m_gui);
			
			// Draw available funds.
			GUI.Label(ResizeGUI(new Rect(1920 - 225, 365, 209, 27)), ((int)m_res.funds).ToString(), m_gui);
		}
		 
		Rect ResizeGUI(Rect _rect)
		{
		    float FilScreenWidth = _rect.width / 1920;
		    float rectWidth = FilScreenWidth * Screen.width;
		    float FilScreenHeight = _rect.height / 1080;
		    float rectHeight = FilScreenHeight * Screen.height;
		    float rectX = (_rect.x / 1920) * Screen.width;
		    float rectY = (_rect.y / 1080) * Screen.height;
		 
		    return new Rect(rectX,rectY,rectWidth,rectHeight);
		}
	}
}