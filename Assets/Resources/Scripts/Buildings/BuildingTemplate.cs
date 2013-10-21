using UnityEngine;
using System.Xml;
using System.IO;
using System.Collections.Generic;
using Ionic.Zip;
using System.Linq;

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
			
			// Type
			Building building = null;
			if (a_prefab.type == Building.Type.DEFAULT)
			{
				building = new Building(properties, a_prefab.model, a_prefab.texture);	
			}
			
			return building;
		}
	}
}
