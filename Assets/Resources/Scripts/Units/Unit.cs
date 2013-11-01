using UnityEngine;
using System.Collections.Generic;

namespace RTS
{
	// Base class of a unit in the RTS project.	
	public class Unit : Selectable
	{
		private NavMeshAgent m_agent;
		private bool m_moved;
		private Quaternion m_moveDir;
		private float m_speed;
		private float m_accel;
		private float m_turnSpeed;
		private float m_rotateScale;
		private float m_stuckTimer;
		public List<Vector3> m_destinations;

		public struct Properties
		{
			public int ID;
			public string name;
			public Vector3 pos;
			public Vector3 rot;
			public int health;
			public int buildTime;
			public int cost;
			public float speed;
			public float accel;
			public float turnSpeed;
			public Vector2 miniSize;
		}

		public struct Type
		{
			public const int DOZER = 0;
			public const int INFANTRY = 1;
			public const int TANK = 2;
		}

		public bool Moving() { return m_destinations.Count > 0 || m_agent.velocity.sqrMagnitude > 10f; }
		public bool Rerouting() { return m_destinations.Count > 1; }
		public float DestDistance() { return m_agent.remainingDistance; }
		public int UnitType() { return m_type; }

		public Unit(Properties a_properties, Mesh a_mesh, Texture2D a_texture)
			: base(a_properties.ID, a_properties.name)
		{
			// Init properties
			m_gameObject.name = a_properties.name;
			m_totalHealth = m_health = a_properties.health;
			m_cost = a_properties.cost;
			m_miniSize = a_properties.miniSize;
			m_type = Type.TANK;
			m_gameObject.tag = "Unit";
			m_gameObject.transform.position = a_properties.pos;
			m_gameObject.transform.eulerAngles = a_properties.rot;
			m_accel = a_properties.accel;
			m_speed = a_properties.speed;
			m_turnSpeed = a_properties.turnSpeed;
			m_gameObject.layer = 11;
			m_stuckTimer = 0f;
			Main.m_res.units.Add(m_ID);

			base.Init(a_mesh, a_texture);
			UpdateGuiPosition();

			// Init navmesh agent
			m_agent = m_gameObject.AddComponent<NavMeshAgent>();
			m_agent.acceleration = m_accel;
			m_agent.speed = m_speed;
			m_agent.angularSpeed = m_turnSpeed;
			m_agent.radius = m_radius;
			m_agent.height = m_mesh.mesh.bounds.size.y/3;
			m_agent.baseOffset = 0f;
			m_destinations = new List<Vector3>();

			// Create icon
			Vector2 pos = new Vector2(Position().x, Position().z);
			UserInterface.m_sidePanel.m_minimap.AddIcon(m_miniSize, pos, false, out m_icon);

			CAS.AddDestination(this.GetHashCode(), Position());
		}

		public override void Process()
		{
			if (Moving())
			{
				m_position = m_gameObject.transform.position;
				UpdateGuiPosition();
				m_icon.Update(new Vector2(Position().x, Position().z));

				// Update angle based on terrain
				RaycastHit hit;
				Ray ray = new Ray(Position(), Vector3.down);
				if (Physics.Raycast(ray, out hit, m_mesh.mesh.bounds.size.y, 1 << 8))
				{
					float yaw = m_gameObject.transform.eulerAngles.y;
					m_gameObject.transform.rotation = Quaternion.FromToRotation(m_gameObject.transform.up, hit.normal) *  Quaternion.Euler(0f, yaw, 0f);
				}

				if (m_agent.velocity.sqrMagnitude < 5f)
				{
					m_stuckTimer += Time.deltaTime;
					if (m_moved)
					{
						float dist = Vector3.Distance(Position(), GetDestination());
						float time = Mathf.Clamp(dist * dist * 0.01f, 0.1f, 5f);

						if (m_stuckTimer > time)
						{
							m_stuckTimer = 0f;
							Stop();
						}
					}
					else
					{
						float angle = Quaternion.Angle(m_gameObject.transform.rotation, m_moveDir);
						if (Mathf.Abs(angle) < 30f && m_stuckTimer > 0.5f)
						{
							m_stuckTimer = 0f;
							Stop();
						}
						else
						{
							m_stuckTimer = 0f;
						}
					}


				}
				else
				{
					m_stuckTimer = 0f;
				}

				if (m_type != Type.INFANTRY && !m_moved)
				{

					float angle = Quaternion.Angle(m_gameObject.transform.rotation, m_moveDir);
					if (Mathf.Abs(angle) > 20f)
					{
						m_agent.speed = 0f;
						m_agent.acceleration = 15f;
						m_agent.angularSpeed = 0f;
						m_rotateScale += Time.deltaTime;

						if (m_agent.velocity.sqrMagnitude < 5f)
							m_gameObject.transform.rotation = Quaternion.Lerp(m_gameObject.transform.rotation, m_moveDir, m_turnSpeed/angle * Time.deltaTime);
					}
					else
					{
						m_agent.speed = m_speed;
						m_agent.angularSpeed = m_turnSpeed;
						m_agent.acceleration = m_accel;
						m_moved = true;
					}
				}

				if (Rerouting())
				{
					if (m_agent.remainingDistance < 5f)
					{
						m_agent.stoppingDistance = 0f;
						m_destinations.Remove(m_destinations[m_destinations.Count - 1]);
						m_agent.SetDestination(m_destinations[m_destinations.Count - 1]);
					}
				}
				else
				{
					if (m_agent.remainingDistance < 1f)
					{
						m_agent.stoppingDistance = 1f;
						m_destinations.Clear();
						CAS.RemoveDestination(this.GetHashCode());
					}
				}
			}

			base.Process();
		}

		public Vector3 GetDestinationDir(int a_dest = 0)
		{
			if (m_destinations.Count == 0)
				return Vector3.zero;

			int index = 0;
			if (a_dest == 1)
				index = m_destinations.Count - 1;
			else if (a_dest == 2)
			{
				if (m_destinations.Count > 2)
					index = m_destinations.Count - 2;
				else
					index = m_destinations.Count - 1;
			}

			return (m_destinations[index] - Position()).normalized;
		}

		public Vector3 GetDestination(int a_dest = 0)
		{
			if (m_destinations.Count == 0)
				return Position();

			int index = 0;
			if (a_dest == 1)
				index = m_destinations.Count - 1;
			else if (a_dest == 2 && m_destinations.Count > 1)
				index = m_destinations.Count - 2;

			return m_destinations[index];
		}

		public void ClearDetour(bool a_once = false)
		{
			if (a_once && m_destinations.Count > 0)
			{
				if (CAS.m_debug)
				{
					GameObject test = GameObject.CreatePrimitive(PrimitiveType.Cube);
					test.transform.position = m_destinations[m_destinations.Count - 1];
					test.transform.localScale = new Vector3(2f, 2f, 2f);
					test.renderer.material.color = Color.blue;
				}
				m_destinations.RemoveAt(m_destinations.Count - 1);
			}
			else if (!a_once)
			{
				while (m_destinations.Count > 2)
					m_destinations.RemoveAt(m_destinations.Count - 1);
			}

			m_agent.SetDestination(m_destinations[m_destinations.Count - 1]);
		}

		public virtual void SetDestination(Vector3 a_pos)
		{
			m_destinations.Clear();
			AddDestination(a_pos);
			CAS.AddDestination(this.GetHashCode(), a_pos + new Vector3(0f, m_mesh.mesh.bounds.size.y/2, 0f));
		}

		public void OverrideDestination(Vector3 a_pos)
		{
			if (m_destinations.Count > 1)
			{
				m_destinations[0] = a_pos;
				CAS.RemoveDestination(this.GetHashCode());
				CAS.AddDestination(this.GetHashCode(), a_pos);
			}
			else if (m_destinations.Count == 1)
			{
				m_destinations[0] = a_pos;
				m_agent.SetDestination(a_pos);
				CAS.RemoveDestination(this.GetHashCode());
				CAS.AddDestination(this.GetHashCode(), a_pos);
			}
			else
			{
				m_agent.SetDestination(a_pos);
				CAS.AddDestination(this.GetHashCode(), a_pos);
			}
		}

		public void Stop()
		{
			if (Moving())
			{
				CAS.RemoveDestination(this.GetHashCode());
				m_destinations.Clear();
				m_agent.stoppingDistance = 6;
				m_agent.Stop();
				m_moved = false;
			}
		}

		public void AddDestination(Vector3 a_pos)
		{
			a_pos = a_pos + new Vector3(0f, m_mesh.mesh.bounds.size.y/2, 0f);
			m_destinations.Add(a_pos);
			if (m_agent.SetDestination(a_pos) == false)
				m_agent.SetDestination(new Vector3(a_pos.x, m_gameObject.transform.position.y, a_pos.z));
			m_moveDir = Quaternion.LookRotation(a_pos - m_gameObject.transform.position);
			//if (m_destinations.Count > 0) m_moved = true;
			m_moved = false;
			m_rotateScale = 0f;
		}
		
		public override void Select()
		{
			base.Select();

			m_gameObject.GetComponent<MeshRenderer>().material.color = Color.green;
			
			// TODO: Play selection sound.
		}
		
		public override void Deselect()
		{
			base.Deselect();

			m_gameObject.GetComponent<MeshRenderer>().material.color = Color.white;
		}

		public override void Destroy()
		{
			for (int i = 0; i < Main.m_res.units.Count; ++i)
			{
				if (Main.m_res.units[i] == m_ID)
				{
					Main.m_res.units.Remove(Main.m_res.units[i]);
					break;
				}
			}

			if (m_gameObject)
				Object.Destroy(m_gameObject);

			base.Destroy();
		}
	}
}