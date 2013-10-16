using UnityEngine;

namespace RTS
{
	// Base class of a selectable unit in the RTS project.	
	public abstract class Selectable : MonoBehaviour
	{
		public abstract void Select();
		public abstract void Deselect();
	}
}