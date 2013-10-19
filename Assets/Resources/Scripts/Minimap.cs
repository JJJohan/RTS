using UnityEngine;
using System.Collections.Generic;

namespace RTS
{	
	public class Minimap : MonoBehaviour 
	{
		private GameObject m_left;
		private GameObject m_right;
		private float m_scale;
		private Material m_minimap;
		private Camera m_camera;
		private List<MinimapIcon> m_icons;
		private Rect m_bounds;

		void Start()
		{
			// Init minimap
			m_icons = new List<MinimapIcon>();
			m_bounds = Main.ResizeGUI(new Rect(1920 - 224, 16, 209, 184));
				
			// Create the minimap render texture
			m_minimap = (Material)UnityEngine.Resources.Load("textures/minimapRT");
			
			// Create Camera
			m_camera = gameObject.AddComponent<Camera>();
			m_camera.isOrthoGraphic = true;
			m_camera.orthographicSize = 500f;
			m_camera.nearClipPlane = 10f;
			m_camera.farClipPlane = 1000f;
			m_camera.clearFlags = CameraClearFlags.Color;
			m_camera.backgroundColor = Color.black;
			m_camera.targetTexture = (RenderTexture)m_minimap.mainTexture;
			m_camera.rect = new Rect(0f, 0f, 1f, 1f);
			gameObject.transform.position = new Vector3(0f, 100f, 0f);
			gameObject.transform.eulerAngles = new Vector3(90f, 0f, 0f);
			gameObject.layer = LayerMask.NameToLayer("Minimap");
			gameObject.name = "Minimap";
		}
		
		public void AddIcon(ref MinimapIcon a_icon)
		{
			a_icon.SetBounds(m_bounds);
			m_icons.Add(a_icon);
		}
		
		public Vector2 WorldToMap(Vector2 a_pos)
		{
			Vector2 pos = new Vector2(a_pos.x / 1000f * m_bounds.width, -a_pos.y / 1000f * m_bounds.height);
			return new Vector2(m_bounds.x + m_bounds.width/2 + pos.x, m_bounds.y + m_bounds.height/2 + pos.y);
		}
		
		public Vector2 MapToWorld(Vector2 a_pos)
		{
			Vector2 pos = new Vector2(a_pos.x - m_bounds.width/2 - m_bounds.x, a_pos.y - m_bounds.height/2 - m_bounds.y);
			return new Vector2((pos.x / m_bounds.width) * 1000f, (-pos.y / m_bounds.height) * 1000f);
		}
		
		public void Draw()
		{			
			// Draw minimap
			Graphics.DrawTexture(m_bounds, m_minimap.mainTexture, m_minimap);
			
			// Draw icons
			foreach (MinimapIcon icon in m_icons)
				icon.Draw();
			
			// Calculate camera line direction
			Vector2 camPos = new Vector2(Camera.main.transform.position.x, Camera.main.transform.position.z);
			Vector2 left2, left = new Vector2(-10, -10);
			Vector2 right2, right = new Vector2(10, -10);
			float rad = Camera.main.transform.eulerAngles.y * Mathf.Deg2Rad;
			float cs = Mathf.Cos(rad);
			float sn = Mathf.Sin(rad);
			left2.x = left.x * cs - left.y * sn;
			left2.y = left.x * sn + left.y * cs;
			right2.x = right.x * cs - right.y * sn;
			right2.y = right.x * sn + right.y * cs;
			
			// Draw camera lines
			camPos = WorldToMap(camPos);
			Line.Draw(camPos, camPos + left2, Color.white);
			Line.Draw(camPos, camPos + right2, Color.white);
		}
	}
}