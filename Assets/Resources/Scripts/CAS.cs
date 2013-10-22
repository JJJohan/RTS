using UnityEngine;

namespace RTS
{
	// Collision avoidance system
	public static class CAS
	{
		private static float m_fps;
		private static float m_timeScale;

		public static void Init()
		{
			m_fps = 1f/10f;
			m_timeScale = 0f;
		}

		public static void SetUpdateRate(float a_fps)
		{
			m_fps = 1f/Mathf.Clamp(a_fps, 1f, 30f);
		}

		public static void Update()
		{
			m_timeScale += Time.deltaTime;
			if (m_timeScale > m_fps)
			{
				foreach(Unit unit in Main.m_unitList)
				{
					if (unit.Moving())
					{
						RaycastHit hit;

						Ray ray = new Ray(unit.Position(), unit.GetDestinationDir());
						if (Physics.Raycast(ray, out hit, unit.Radius() * 6, 1 << 10))
						{
							Vector3 between = hit.point - unit.Position();
							Vector3 rightAngle = hit.point + Vector3.Cross(Vector3.up, between);
							float minRadius = ((Selectable)hit.collider.GetComponent<UserData>().data).Radius();
							//if (Vector2.Distance(hit.point, rightAngle) < minRadius)
							//	rightAngle *= minRadius;

							GameObject test = GameObject.CreatePrimitive(PrimitiveType.Cube);
							test.transform.position = rightAngle;

							unit.AddDestination(rightAngle);
						}
					}
				}

				m_timeScale -= m_fps;
			}
		}
	}
}