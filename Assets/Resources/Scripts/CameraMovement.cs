using UnityEngine;

namespace RTS
{
	public class CameraMovement : MonoBehaviour
	{
		public float m_moveSpeed;
		private float m_height;
		private float m_scroll;
		private float m_scale;
		private bool m_zoomed;

		public float Scale()
		{
			return m_scale;
		}

		public bool Zoomed()
		{
			if (m_zoomed)
			{
				m_zoomed = false;
				return true;
			}
			else
			{
				return false;
			}
		}

		public void SetPos(Vector3 a_pos)
		{
			transform.position = a_pos;
			m_height = a_pos.y;
		}

		void Start()
		{
			Main.Init();
		}
		// Update is called once per frame
		void Update()
		{
			Main.Update();
			
			bool shift = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
			float speed = m_moveSpeed * Time.deltaTime * (m_height * 3);
			if (shift)
				speed *= 2.0f;
			
			// Camera movement
			if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
				camera.transform.Translate(-speed * .5f, 0.0f, 0.0f, Space.Self);
			if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
				camera.transform.Translate(speed * .5f, 0.0f, 0.0f, Space.Self);
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
			if (m_scroll > 0.0f)
				m_scroll -= m_scroll / 10;
			else if (m_scroll < 0.0f)
				m_scroll -= m_scroll / 10;
			float scroll = -Input.GetAxis("Mouse ScrollWheel");
			if (Mathf.Abs(scroll) > 0)
			{
				m_scroll = scroll;
				m_zoomed = true;
			}
			m_height += m_scroll * 20f;
			m_height = Mathf.Clamp(m_height, 40, 95);
			float tiltDiff = (m_height - 50) * 0.25f;
			m_scale = 15.0f + (m_height * 0.15f);
			
			camera.transform.localEulerAngles = new Vector3(55 + tiltDiff, camera.transform.localEulerAngles.y, camera.transform.localEulerAngles.z);
			camera.transform.position = new Vector3(camera.transform.position.x, m_height, camera.transform.position.z);
		}

		void OnGUI()
		{
			// Draw interface
			if (Event.current.type == EventType.Repaint)
				Main.Draw();
			
			// Get event
			Main.m_event = Event.current;
		}
	}
}