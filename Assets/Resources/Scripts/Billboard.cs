using UnityEngine;

public class Billboard : MonoBehaviour
{	
	private int m_anchor;
	private Mesh m_mesh;
	private Vector3[] m_vertices;
	private Vector2 m_offset;
	private float m_width;
	private float m_height;
	private bool m_directionSet;
	
	public struct Anchor
	{
		public const int Left = 0;
		public const int Middle = 1;
		public const int Right = 2;
	}
	
	public void Create(float a_width, float a_height, string a_texture = "")
	{
		Create(a_width, a_height, new Vector2(0f, 0f), Anchor.Middle, a_texture);
	}
	
	public void Create(float a_width, float a_height, Vector2 a_offset, string a_texture = "")
	{
		Create(a_width, a_height, a_offset, Anchor.Middle, a_texture);
	}
	
	public void Create(float a_width, float a_height, int a_anchor, string a_texture = "")
	{
		Create(a_width, a_height, new Vector2(0f, 0f), a_anchor, a_texture);
	}
	
	public void Create(float a_width, float a_height, Vector2 a_offset, int a_anchor, string a_texture = "")
	{
		m_anchor = a_anchor;
		m_offset = a_offset;
		m_width = a_width;
		m_height = a_height;
		m_mesh = new Mesh();
		MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();
		MeshFilter filter = gameObject.AddComponent<MeshFilter>();
		
		if (a_anchor == Anchor.Left)
		{
		    m_vertices = new Vector3[] { new Vector3(a_offset.x + a_width * 2, 0, a_offset.y + a_height),
									   new Vector3(a_offset.x + a_width * 2, 0, a_offset.y - a_height),
									   new Vector3(a_offset.x, 0, a_offset.y + a_height),
									   new Vector3(a_offset.x, 0, a_offset.y - a_height) };
		}
		else if (a_anchor == Anchor.Middle)
		{
		    m_vertices = new Vector3[] { new Vector3(a_offset.x + a_width, 0, a_offset.y + a_height),
						 			   new Vector3(a_offset.x + a_width, 0, a_offset.y - a_height),
									   new Vector3(a_offset.x - a_width, 0, a_offset.y + a_height),
						 			   new Vector3(a_offset.x - a_width, 0, a_offset.y - a_height) };
		}
		else
		{
		    m_vertices = new Vector3[] { new Vector3(a_offset.x - a_width * 2, 0, a_offset.y + a_height),
									   new Vector3(a_offset.x - a_width * 2, 0, a_offset.y - a_height),
									   new Vector3(a_offset.x, 0, a_offset.y + a_height),
									   new Vector3(a_offset.x, 0, a_offset.y - a_height) };
		}
		
	    Vector2[] uv = new Vector2[] { new Vector2(1, 1),
									   new Vector2(1, 0),
									   new Vector2(0, 1),
									   new Vector2(0, 0) };
	    int[] triangles = new int[] { 0, 1, 2,
									  2, 1, 3 };
	
	    m_mesh.vertices = m_vertices;
	    m_mesh.uv = uv;
	
	    m_mesh.triangles = triangles;
	    m_mesh.RecalculateNormals();
	    m_mesh.RecalculateBounds();
		
		filter.mesh = m_mesh;
		if (a_texture != "")
			renderer.material.mainTexture = (Texture2D)Resources.Load(a_texture);
		renderer.material.shader = Shader.Find("GUI/Text Shader");
		
		gameObject.transform.rotation = Camera.main.transform.rotation * new Quaternion(0.707f, 0f, 0f, 0.707f);
	}
	
	public void LookAt(Vector3 a_point)
	{
		m_directionSet = true;
		gameObject.transform.LookAt(a_point);
	}
			
	public void SetDirection(Vector3 a_direction)
	{
		m_directionSet = true;
		gameObject.transform.eulerAngles = new Quaternion(0.707f, 0f, 0f, 0.707f) * a_direction;
	}
	
	public void Update()
	{
		if (m_directionSet)
			return;
		
		if (m_anchor != Anchor.Middle)
		{
			Vector2 offset = new Vector2();
			offset.x = m_offset.x / transform.localScale.x;
			offset.y = m_offset.y / transform.localScale.y;
			
			Vector3[] vertices = m_mesh.vertices;
			if (m_anchor == Anchor.Left)
			{
				vertices[0] = new Vector3(offset.x + m_width * 2, 0, offset.y + m_height);
				vertices[1] = new Vector3(offset.x + m_width * 2, 0, offset.y - m_height);
				vertices[2] = new Vector3(offset.x, 0, offset.y + m_height);
				vertices[3] = new Vector3(offset.x, 0, offset.y - m_height);
			}
			else
			{
				vertices[0] = new Vector3(offset.x - m_width * 2, 0, offset.y + m_height);
				vertices[1] = new Vector3(offset.x - m_width * 2, 0, offset.y - m_height);
				vertices[2] = new Vector3(offset.x, 0, offset.y + m_height);
				vertices[3] = new Vector3(offset.x, 0, offset.y - m_height);
			}
			
			m_mesh.vertices = vertices;
		}
		
		gameObject.transform.rotation = Camera.main.transform.rotation * new Quaternion(0.707f, 0f, 0f, 0.707f);
	}
}