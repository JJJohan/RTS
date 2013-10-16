using UnityEngine;
using System.Collections.Generic;

namespace RTS
{	
	public partial class Minimap : MonoBehaviour 
	{
		private GameObject m_left;
		private GameObject m_right;
		private float m_scale;

		void Start()
		{
			m_scale = Camera.main.GetComponent<CameraMovement>().Scale();
			m_left = GameObject.CreatePrimitive(PrimitiveType.Plane);
			m_right = GameObject.CreatePrimitive(PrimitiveType.Plane);
			m_left.layer = m_right.layer = LayerMask.NameToLayer("Minimap");
			m_left.renderer.material = m_right.renderer.material = new Material(Shader.Find("Unlit/Texture"));
			m_left.transform.localScale = m_right.transform.localScale = new Vector3(1,1,m_scale);
		}
		
		void Update()
		{
			// Update scale
			if (Camera.main.GetComponent<CameraMovement>().Zoomed() || m_scale < 0.1f)
			{
				m_scale = Camera.main.GetComponent<CameraMovement>().Scale();
				m_left.transform.localScale = m_right.transform.localScale = new Vector3(1,1,m_scale);
			}
			
			Vector3 camForward = Camera.main.transform.forward;
			camForward = new Vector3(camForward.x, 0, camForward.z);
			
			m_left.transform.position = Camera.main.transform.position;
			m_right.transform.position = Camera.main.transform.position;
			
			Vector3 camDir = Camera.main.transform.eulerAngles;
			camDir = new Vector3(0, camDir.y, 0);
			m_left.transform.eulerAngles = m_right.transform.eulerAngles = camDir;
			m_left.transform.Rotate(new Vector3(0,1,0), -45.0f);
			m_right.transform.Rotate(new Vector3(0,1,0), 45.0f);
			//m_right.transform.eulerAngles = new Quaternion(0f, 0.5f, 0f, -0.5f) * camDir;
			
			m_left.transform.Translate(new Vector3(0,0,30 + m_scale * 4.0f), Space.Self);
			m_right.transform.Translate(new Vector3(0,0,30 + m_scale * 4.0f), Space.Self);
		}
	}
}