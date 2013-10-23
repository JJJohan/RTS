using UnityEngine;
using System.Collections.Generic;

namespace RTS
{
	// Collision avoidance system
	public static class CAS
	{
		private static float m_fps;
		private static float m_timeScale;
		private static List<GameObject> m_debugPoints;
		private static Node[,] m_grid = new Node[50,50];
		private static int m_gridSize;
		private static int m_spacing;

		private class Node
		{
			public Vector2 point = Vector2.zero;
			public float height = 0f;
			public int cost = 0;
			public Node prev = null;
			public bool local = false;
			public GameObject debug;
			public int F = 0;
			public int G = 0;
		}

		public static void Init()
		{
			m_fps = 1f/10f;
			m_timeScale = 0f;

			// Generate a list of A* nodes.
			m_gridSize = 50;
			m_spacing = 1000/m_gridSize;
			for (int i = 0; i < m_gridSize; ++i)
			{
				for (int j = 0; j < m_gridSize; ++j)
				{
					RaycastHit hit;
					Ray ray = new Ray(new Vector3(-500f + i * m_spacing, 100f, -500f + j * m_spacing), Vector3.down);
					if (Physics.Raycast(ray, out hit))
					{
						float height = hit.point.y;

						NavMeshPath navPath = new NavMeshPath();
						if (NavMesh.CalculatePath(Vector3.zero, new Vector3(-500f + i * m_spacing, height, -500f + j * m_spacing), 1, navPath))
						{
							m_grid[i,j] = new Node();
							m_grid[i,j].height = height;
							m_grid[i,j].point = new Vector2(-500f + i * m_spacing, -500f + j * m_spacing);
						}
					}
				}
			}
		}

		private static bool CalculatePath(Vector2 a_point, Vector2 a_dest, int a_size, out List<Vector3> a_path)
		{
			a_path = new List<Vector3>();
			List<Node> nodes = FetchPoints(a_point, a_dest, a_size);
			foreach (Node n in nodes)
				n.prev = null;

			if (nodes.Count == 0)
				return false;

			// Init list
			List<Node> open = new List<Node>();
			List<Node> closed = new List<Node>();

			// Find Start Node
			Node startNode = nodes[0];
			float closest = 999f;
			foreach (Node n in nodes)
			{
				float distance = Vector2.Distance(n.point, a_point);
				if (distance < closest)
				{
					startNode = n;
					closest = distance;
				}
			}
			open.Add(startNode);

			// Find End Node
			Node endNode = nodes[0];
			closest = 999f;
			foreach (Node n in nodes)
			{
				float distance = Vector2.Distance(n.point, a_dest);
				if (distance < closest)
				{
					endNode = n;
					closest = distance;
				}
			}

			// Traverse path
			while (open.Count > 0)
			{
				// Find lowest cost node
				Node current = open[0];
				foreach (Node n in open)
				{
					if (n.cost < current.cost)
						current = n;
				}

				// Check if destination is reached
				if (current.Equals(endNode))
				{
					Debug.Log("Found path.");
					while (current.prev != null)
					{
						a_path.Add(new Vector3(current.point.x, current.height, current.point.y));
						//current.debug.gameObject.renderer.material.color = Color.green;
						current = current.prev;
					}
					a_path.Add(new Vector3(current.point.x, current.height, current.point.y));
					//current.debug.gameObject.renderer.material.color = Color.green;
					return true;
				}

				open.Remove(current);
				closed.Add(current);

				List<Node> neighbours = Neighbours(current.point);
				foreach (Node n in neighbours)
				{
					int G = (int)(current.G + Vector2.Distance(current.point, n.point));
					int F = (int)(G + Vector2.Distance(n.point, a_dest));

					bool cont = false;
					foreach(Node c in closed)
					{
						if (c.Equals(n))
						{
							cont = true;
							break;
						}
					}
					if (cont) continue;

					foreach(Node o in open)
					{
						if (o.Equals(n))
						{
							cont = true;
							break;
						}
					}

					if (!cont || F < n.F)
					{
						n.prev = current;
						n.G = G;
						n.F = F;
						if (!cont)
							open.Add(n);
					}
				}
			}

			return false;
		}

		private static List<Node> Neighbours(Vector2 a_pos)
		{
			List<Node> neighbours = new List<Node>();

			int x = (int)((a_pos.x + 500f) / m_spacing);
			int y = (int)((a_pos.y + 500f) / m_spacing);

			/*// Find list of obstacles in range
			List<Bounds> obstacles = new List<Bounds>();
			foreach (Selectable s in Main.m_buildingList)
			{
				if (a_point.x + a_radius > s.Position().x && a_point.x - a_radius < s.Position().x)
				{
					if (a_point.y + a_radius > s.Position().z && a_point.y - a_radius < s.Position().z)
					{
						obstacles.Add(s.GetObject().collider.bounds);
					}
				}
			}*/

			Vector3 current = new Vector3(m_grid[x, y].point.x, m_grid[x, y].height + 2f, m_grid[x, y].point.y);

			Node node = m_grid[x-1, y];
			if (node != null && node.local)
			{
				Ray ray = new Ray(current, new Vector3(node.point.x, node.height + 2f, node.point.y) - current);
				if (!Physics.Raycast(ray, m_spacing, 1 << 10))
					neighbours.Add(node);
			}

			node = m_grid[x+1, y];
			if (node != null && node.local)
			{
				Ray ray = new Ray(current, new Vector3(node.point.x, node.height + 2f, node.point.y) - current);
				if (!Physics.Raycast(ray, m_spacing, 1 << 10))
					neighbours.Add(node);
			}

			node = m_grid[x, y-1];
			if (node != null && node.local)
			{
				Ray ray = new Ray(current, new Vector3(node.point.x, node.height + 2f, node.point.y) - current);
				if (!Physics.Raycast(ray, m_spacing, 1 << 10))
					neighbours.Add(node);
			}

			node = m_grid[x, y+1];
			if (node != null && node.local)
			{
				Ray ray = new Ray(current, new Vector3(node.point.x, node.height + 2f, node.point.y) - current);
				if (!Physics.Raycast(ray, m_spacing, 1 << 10))
					neighbours.Add(node);
			}

			return neighbours;
		}

		private static List<Node> FetchPoints(Vector2 a_point, Vector2 a_dest, int a_radius)
		{
			// Fetch nodes
			List<Node> nodes = new List<Node>();
			for (int i = 0; i < m_gridSize; ++i)
			{
				for (int j = 0; j < m_gridSize; ++j)
				{
					if (m_grid[i,j] == null) continue;

					if (a_point.x + a_radius > m_grid[i,j].point.x && a_point.x - a_radius < m_grid[i,j].point.x)
					{
						if (a_point.y + a_radius > m_grid[i,j].point.y && a_point.y - a_radius < m_grid[i,j].point.y)
						{
							//foreach (Bounds r in obstacles)
							//{
								//if (!r.Contains(new Vector3(n.point.x, r.center.y, n.point.y)))
								{
									m_grid[i,j].local = true;
									m_grid[i,j].cost = (int)Vector2.Distance(m_grid[i,j].point, a_dest);
									//m_grid[i,j].debug = GameObject.CreatePrimitive(PrimitiveType.Cube);
									//m_grid[i,j].debug.transform.position = new Vector3(m_grid[i,j].point.x, m_grid[i,j].height, m_grid[i,j].point.y);
									nodes.Add(m_grid[i,j]);
								}
							//}
						}
						else
						{
							m_grid[i,j].local = false;
						}
					}
					else
					{
						m_grid[i,j].local = false;
					}
				}
			}

			//Debug.Log("Returned " + nodes.Count + " Nodes around " + obstacles.Count + " Obstacles.");
			return nodes;
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
						// Get distance to destination.
						//float dist = unit.Radius() * ;
						//float dist = Vector3.Distance(unit.Position(), unit.GetDestination());
						//totalDist = Mathf.Clamp(totalDist, 0f, dist);

						// Check if the previously calculated detour is now blocked.
						/*bool pathfind = !unit.Rerouting();
						Ray ray = new Ray(unit.Position(), unit.GetDestinationDir());
						if (Physics.Raycast(ray, dist, 1 << 10))
						{
							pathfind = true;
						}*/

						// Calculate a new detour around the obstacle.
						RaycastHit hit;
						if (!unit.Rerouting())
						{
							int searchArea = 50;
							int iter = 0;
							Ray ray = new Ray(unit.Position(), unit.GetDestinationDir());
							if (Physics.Raycast(ray, out hit, unit.Radius() * 6, 1 << 10))
							{
								List<Vector3> path;
								bool found = false;
								while ((!found || searchArea < 200) && iter < 5)
								{
									++iter;
									if (CalculatePath(new Vector2(unit.Position().x, unit.Position().z), new Vector2(unit.GetDestination().x, unit.GetDestination().z), searchArea, out path))
									{
										found = true;
										foreach (Vector3 p in path)
										{
											unit.AddDestination(p);
										}
									}
									else
									{
										Debug.Log(searchArea);
										searchArea += 50;
									}
								}

								if (!found)
								{
									Debug.Log("No path found.");
								}
							}
						}
						else
						{
							Ray ray = new Ray(unit.Position(), unit.GetDestinationDir(1));
							if (!Physics.Raycast(ray, out hit, unit.Radius() * 6, 1 << 10))
							{
								unit.ClearDetour();
							}
						}
					}
				}

				m_timeScale -= m_fps;
			}
		}
	}
}