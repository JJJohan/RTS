using UnityEngine;
using System.Collections.Generic;

namespace RTS
{
	public class Fog
	{
		private GameObject m_object;
		private Projector m_projector;
		private Texture2D m_tex;
		private Color[] m_pixels;
		private const int m_size = 64;
		private float m_time;
		private List<int> m_open;
		private List<int> m_closed;
		private int m_unitCount;
		private int m_buildingCount;

		public struct Visibility
		{
			public const int VISIBLE = 0;
			public const int PARTIAL = 1;
			public const int HIDDEN = 2;
		}

		public Fog()
		{
			// Init class
			m_open = new List<int>(m_size * m_size);
			m_closed = new List<int>(m_size * m_size);
			for (int i = 0; i < (m_size * m_size); ++i)
				m_closed.Add(i);
			m_time = 0f;
			m_unitCount = 0;
			m_buildingCount = 0;

			// Init object
			m_object = new GameObject();
			m_object.transform.position = new Vector3(0f, 200f, 0f);
			m_object.transform.eulerAngles = new Vector3(90f, 0f, 0f);
			m_object.name = "Fog";
			m_object.layer = 12;

			// Init collider
			BoxCollider collider = m_object.AddComponent<BoxCollider>();
			collider.size = new Vector3(1000f, 1000f, 1f);

			// Init texture
			m_tex = new Texture2D(m_size, m_size);
			m_pixels = new Color[m_size * m_size];
			for (int i = 0; i < m_size * m_size; ++i)
				m_pixels[i] = Color.black;
			m_tex.SetPixels(m_pixels);
			m_tex.Apply();

			// Init material
			Material mat = new Material(Shader.Find("Transparent/Diffuse"));
			mat.mainTexture = m_tex;

			// Init projector
			m_projector = m_object.AddComponent<Projector>();
			m_projector.orthographic = true;
			m_projector.orthoGraphicSize = 1000f;
			m_projector.farClipPlane = 300f;
			m_projector.material = mat;
			m_projector.ignoreLayers = 1 << 1 | 1 << 2 | 1 << 3 | 1 << 4 | 1 << 5 | 1 << 6 | 1 << 7 | 1 << 9 | 1 << 10 | 1 << 11;
		}

		public void Update()
		{
			m_time += Time.deltaTime;
			if (m_time < 1f/10f) return;
			m_time -= 1f/10f;

			// Calculate partial update rate
			int unitIterations = Mathf.CeilToInt(((float)Main.m_unitList.Count)/10f);
			int buildingIterations = Mathf.CeilToInt(((float)Main.m_buildingList.Count)/10f);

			// Reset all previously found pixels to a semi-transparent state
			foreach(int i in m_open)
				m_pixels[i] = new Color(0f, 0f, 0f, 0.75f);

			// Check unit visibility
			RaycastHit hit;
			for (int i = m_unitCount; i < m_unitCount + unitIterations; ++i)
			{
				++m_unitCount;
				if (i == Main.m_unitList.Count)
				{
					m_unitCount = 0;
					break;
				}

				Ray ray = new Ray(Main.m_unitList[i].Position(), Vector3.up);
				if (Physics.Raycast(ray, out hit, 1 << 12))
				{
					int pos = (((int)hit.point.x + 500)/(1000/m_size)) + ((((int)hit.point.z + 500)/(1000/m_size) - 2) * m_size);
					List<int> list = Neighbours(pos, 3);
					for (int j = 0; j < list.Count; ++j)
						m_pixels[list[j]] = new Color(0f, 0f, 0f, 0f);
				}
			}

			// Check building visibility
			for (int i = m_buildingCount; i < m_buildingCount + buildingIterations; ++i)
			{
				++m_buildingCount;
				if (i == Main.m_buildingList.Count)
				{
					m_buildingCount = 0;
					break;
				}

				Ray ray = new Ray(Main.m_buildingList[i].Position(), Vector3.up);
				if (Physics.Raycast(ray, out hit, 1 << 12))
				{
					int pos = (((int)hit.point.x + 500)/(1000/m_size)) + ((((int)hit.point.z + 500)/(1000/m_size) - 2) * m_size);
					List<int> list = Neighbours(pos, 3);
					for (int j = 0; j < list.Count; ++j)
						m_pixels[list[j]] = new Color(0f, 0f, 0f, 0f);
				}
			}

			// Update texture
			m_tex.SetPixels(m_pixels);
			m_tex.Apply();

			// Remove discovered pixels from undiscovered list
			List<int> removal = new List<int>();
			for (int i = 0; i < m_closed.Count; ++i)
			{
				if (m_pixels[m_closed[i]].a < 0.9f)
				{
					m_open.Add(m_closed[i]);
					removal.Add(i);
				}
			}

			while (removal.Count != 0)
			{
				m_closed.RemoveAt(removal[removal.Count - 1]);
				removal.RemoveAt(removal.Count - 1);
			}
		}

		public void Draw()
		{
			Graphics.DrawTexture(UserInterface.m_sidePanel.m_minimap.GetBounds(), m_tex);
		}

		public List<int> Neighbours(int a_index, int a_radius)
		{
			List<int> list = new List<int>();

			for (int i = -a_radius; i <= a_radius; ++i)
			{
				for (int j = a_index - a_radius; j <= a_index + a_radius; ++j)
				{
					// Remove corners from neighbours for circular result.
					if (i == -a_radius && j == a_index - a_radius) continue;
					if (i == -a_radius && j == a_index + a_radius) continue;
					if (i == a_radius && j == a_index - a_radius) continue;
					if (i == a_radius && j == a_index + a_radius) continue;

					list.Add(j + (i * m_size));
				}
			}

			return list;
		}

		public int Visible(Vector3 a_point, float a_radius)
		{
			// Set up points based on radius
			List<Vector3> points = new List<Vector3>(4);
			int[] check = new int[4];
			if (a_radius < 1f)
			{
				points.Add(a_point);
			}
			else
			{
				a_radius /= 4f;
				points.Add(a_point - new Vector3(-a_radius, 0f, -a_radius));
				points.Add(a_point - new Vector3(-a_radius, 0f, a_radius));
				points.Add(a_point - new Vector3(a_radius, 0f, -a_radius));
				points.Add(a_point - new Vector3(a_radius, 0f, a_radius));
			}

			// Calculate visibility
			RaycastHit hit;
			for (int i = 0; i < points.Count; ++i)
			{
				Ray ray = new Ray(points[i], Vector3.up);
				if (Physics.Raycast(ray, out hit, 200f, 1 << 12))
				{
					int pos = (((int)hit.point.x + 500)/(1000/m_size)) + ((((int)hit.point.z + 500)/(1000/m_size) - 2) * m_size);
					if (m_pixels[pos].a < 0.1f)
						check[i] = Visibility.VISIBLE;
					else if (m_pixels[pos].a < 0.9f)
						check[i] = Visibility.PARTIAL;
					else
						check[i] = Visibility.HIDDEN;
				}
			}

			// Determine result
			int iter = 0;
			foreach (int i in check)
			{
				if (i == Visibility.VISIBLE)
					++iter;

				if (iter == points.Count)
					return Visibility.VISIBLE;
			}

			iter = 0;
			foreach (int i in check)
			{
				if (i >= Visibility.PARTIAL)
					++iter;

				if (iter == points.Count)
					return Visibility.PARTIAL;
			}

			return Visibility.HIDDEN;
		}
	}
}