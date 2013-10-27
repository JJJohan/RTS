using UnityEngine;

namespace RTS
{
	public class UnitTemplate
	{		
		public Unit Load(UnitPrefab a_prefab, Vector3 a_pos, Vector3 a_rot)
		{
			// Properties
			Unit.Properties properties = new Unit.Properties();
			properties.pos = a_pos;
			properties.rot = a_rot;
			properties.health = a_prefab.health;
			properties.buildTime = a_prefab.buildTime;
			properties.cost = a_prefab.cost;		
			properties.miniSize = a_prefab.miniMapSize;
			properties.ID = a_prefab.ID;
			properties.turnSpeed = a_prefab.turnSpeed;
			properties.speed = a_prefab.speed;
			properties.accel = a_prefab.accel;

			// Type
			Unit unit = null;
			switch (a_prefab.type)
			{
				case Unit.Type.DOZER:
					unit = new Dozer(properties, a_prefab.model, a_prefab.texture);
					break;

				//case Unit.Type.INFANTRY:
					//unit = new Infantry(properties, a_prefab.model, a_prefab.texture);
					//break;

				default:
					unit = new Unit(properties, a_prefab.model, a_prefab.texture);
					break;
			}

			return unit;
		}
	}
}
