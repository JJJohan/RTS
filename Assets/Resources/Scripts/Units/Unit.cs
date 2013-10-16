using UnityEngine;

namespace RTS
{
	// Base class of a unit in the RTS project.	
	public abstract class Unit : Selectable
	{	
		public override void Select()
		{
			gameObject.GetComponent<MeshRenderer>().material.color = Color.green;
			
			// TODO: Play selection sound.
		}
		
		public override void Deselect()
		{
			gameObject.GetComponent<MeshRenderer>().material.color = Color.white;
		}
	}
}