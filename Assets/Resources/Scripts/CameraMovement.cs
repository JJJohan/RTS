using UnityEngine;
using System.Collections;

namespace RTS
{
	public class CameraMovement : MonoBehaviour {
		
		public float m_moveSpeed;
		public float m_height;
		public float m_scroll;
		
		void Start()
		{	

		}
		
		// Update is called once per frame
		void Update() 
		{
			bool shift = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey (KeyCode.RightShift));
			float speed = m_moveSpeed * Time.deltaTime;
			if (shift) speed *= 2.0f;
			
			// Camera movement
			if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
				camera.transform.Translate(-speed, 0.0f, 0.0f, Space.Self);
			if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
				camera.transform.Translate(speed, 0.0f, 0.0f, Space.Self);
			if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
				camera.transform.Translate(0.0f, 0.0f, speed, Space.Self);
			if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
				camera.transform.Translate(0.0f, 0.0f, -speed, Space.Self);
			
			// Rotation
			if (Input.GetKey(KeyCode.Q))
				camera.transform.Rotate(new Vector3(0f, -80f * Time.deltaTime, 0f), Space.World);
			if (Input.GetKey(KeyCode.E))
				camera.transform.Rotate(new Vector3(0f, 80f * Time.deltaTime, 0f), Space.World);
			
			// Tilting
			if (m_scroll > 0.0f) m_scroll -= m_scroll / 10;
			else if (m_scroll < 0.0f) m_scroll -= m_scroll / 10;
			float scroll = -Input.GetAxis("Mouse ScrollWheel");
			if (Mathf.Abs(scroll) > 0) m_scroll = scroll;
			m_height += m_scroll * 20f;
			m_height = Mathf.Clamp(m_height, 40, 60);
			float tiltDiff = (m_height - 50) * 0.5f;
			
			camera.transform.localEulerAngles = new Vector3(45 - tiltDiff, camera.transform.localEulerAngles.y, camera.transform.localEulerAngles.z);
			camera.transform.position = new Vector3(camera.transform.position.x, m_height, camera.transform.position.z);
		}
	}
}