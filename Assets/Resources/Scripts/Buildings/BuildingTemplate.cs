using UnityEngine;

namespace RTS
{
	public class BuildingTemplate
	{		
		public Building Load(BuildingPrefab a_prefab)
		{						
			// Properties
			Building.Properties properties = new Building.Properties();
			properties.health = a_prefab.health;
			properties.power = a_prefab.powerUsage;
			properties.buildTime = a_prefab.buildTime;
			properties.cost = a_prefab.cost;		
			properties.miniSize = new Vector2(6f, 6f);
			properties.ID = a_prefab.ID;
			
			// Type
			Building building = null;
			switch (a_prefab.type)
			{
				case Building.Type.HEADQUARTERS:
					building = new Headquarters(properties, a_prefab.model, a_prefab.texture);  
					break;
				
				case Building.Type.POWERFACTORY:
					building = new PowerFactory(properties, a_prefab.model, a_prefab.texture);  
					break;
					
				case Building.Type.UNITFACTORY:
					building = new UnitFactory(properties, a_prefab.model, a_prefab.texture);  
					break;
					
				default:
					building = new Building(properties, a_prefab.model, a_prefab.texture);  
					break;
			}

			return building;
		}
	}
}
