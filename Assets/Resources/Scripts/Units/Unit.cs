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
		private List<Vector3> m_destinations;

		public struct Properties
		{
			public string ID;
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

		public bool Moving() { return m_destinations.Count > 0; }
		public bool Rerouting() { return m_destinations.Count > 1; }
		public float DestDistance() { return m_agent.remainingDistance; }

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

			base.Init(a_mesh, a_texture);

			// Init navmesh agent
			m_agent = m_gameObject.AddComponent<NavMeshAgent>();
			m_agent.acceleration = m_accel;
			m_agent.speed = m_speed;
			m_agent.angularSpeed = m_turnSpeed;
			m_agent.radius = m_radius;
			m_agent.height = m_mesh.mesh.bounds.size.y/3;
			m_gameObject.transform.position = m_agent.transform.position;
			m_destinations = new List<Vector3>();
		}

		public override void Process()
		{
			UpdateGuiPosition();

			if (Moving())
			{
				if (m_type != Type.INFANTRY && !m_moved)
				{
					float angle = Quaternion.Angle(m_gameObject.transform.rotation, m_moveDir);
					if (Mathf.Abs(angle) > 15f)
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
					}
				}
			}

			base.Process();
		}

		public Vector3 GetDestinationDir()
		{
			if (m_destinations.Count > 0)
				return (m_destinations[m_destinations.Count - 1] - Position()).normalized;
			else
				return Vector3.zero;
		}

		public void SetDestination(Vector3 a_pos)
		{
			m_destinations.Clear();
			AddDestination(a_pos);
		}

		public void AddDestination(Vector3 a_pos)
		{
			m_destinations.Add(a_pos);
			m_agent.SetDestination(a_pos);
			m_moveDir = Quaternion.LookRotation(a_pos - m_gameObject.transform.position);
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
	}
}