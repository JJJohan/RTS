using UnityEngine;

namespace RTS
{	
	public partial class Main : MonoBehaviour 
	{		
		void OnGUI()
		{
			// Draw selection rectangle.
			if (m_clickPos.x > -0.5f)
			{
				Vector3 mousePos = new Vector3(Input.mousePosition.x, Screen.height, 0.0f) - new Vector3(0.0f, Input.mousePosition.y, 0.0f);
				GUI.DrawTexture(new Rect(m_clickPos.x, m_clickPos.y, mousePos.x - m_clickPos.x, mousePos.y - m_clickPos.y), m_guiSelect);
			}
			
			// Draw scaled GUI.
		    GUI.DrawTexture(ResizeGUI(new Rect(800 - 171, 0, 171, 600)), m_guiPanel);
			
			// Draw available funds.
			GUI.TextArea(ResizeGUI(new Rect(800 - 160, 0, 150, 15)), ((int)m_res.funds).ToString());
		}
		 
		Rect ResizeGUI(Rect _rect)
		{
		    float FilScreenWidth = _rect.width / 800;
		    float rectWidth = FilScreenWidth * Screen.width;
		    float FilScreenHeight = _rect.height / 600;
		    float rectHeight = FilScreenHeight * Screen.height;
		    float rectX = (_rect.x / 800) * Screen.width;
		    float rectY = (_rect.y / 600) * Screen.height;
		 
		    return new Rect(rectX,rectY,rectWidth,rectHeight);
		}
	}
}