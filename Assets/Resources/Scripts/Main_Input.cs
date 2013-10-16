using UnityEngine;

namespace RTS
{	
	public partial class Main : MonoBehaviour 
	{	
		public void ProcessInput()
		{
			switch(m_cursorMode)
			{
			case Cursor.ORDER:			
				if (Input.GetMouseButtonUp(1))
				{	
					// TODO: Execute order.
					if (m_selectionType == Selection.UNIT)
					{	
					}
					else if (m_selectionType == Selection.BUILDING)
					{
					}
					else
					{
						throw new UnityException();
					}
				}
				goto case Cursor.SELECTION;
				
			case Cursor.SELECTION:
				if (Input.GetMouseButtonDown(0))
				{
					// Raycast first half of rectangle.
					m_clickPos = new Vector3(Input.mousePosition.x, Screen.height, 0.0f) - new Vector3(0.0f, Input.mousePosition.y, 0.0f);
					m_ray1 = camera.ScreenPointToRay(Input.mousePosition);
					Physics.Raycast(m_ray1, out m_hit1, 10000.0f);
				}
				else if (Input.GetMouseButtonUp(0))
				{	
					m_clickPos = new Vector2(-1f, -1f);
					
					// Raycast second half of rectangle.
					m_ray2 = camera.ScreenPointToRay(Input.mousePosition);
					if (Physics.Raycast(m_ray2, out m_hit2, 10000.0f))
					{
						// Determine if it's a rectangular selection.
						if (Mathf.Abs(Vector2.Distance(m_hit1.point, m_hit2.point)) > 1.0f)
						{					
							Rect selection = new Rect(m_hit1.point.x, m_hit1.point.z, m_hit2.point.x - m_hit1.point.x, m_hit2.point.z - m_hit1.point.z);
							if (m_hit1.point.x > m_hit2.point.x)
							{
								selection.x = m_hit2.point.x;
								selection.width = m_hit1.point.x - m_hit2.point.x;
							}
							if (m_hit1.point.z > m_hit2.point.z)
							{
								selection.y = m_hit2.point.z;
								selection.height = m_hit1.point.z - m_hit2.point.z;
							}
							
							// Select all units within the rectangular zone - no buildings.
							int selectIndex = 0;
							foreach (Unit unit in m_unitList)
							{					
								if (selection.Contains(unit.gameObject.transform.position))
								{							
									if (selectIndex == 0)
										ClearSelection();
									++selectIndex;
									
									m_selected.Add((Selectable)unit);
									unit.Select();
								}
							}
							
							if (selectIndex == 0)
							{
								ClearSelection();
							}
							else
							{
								m_selectionType = Selection.UNIT;
								m_cursorMode = Cursor.ORDER;
							}
						}
						else
						{
							// Single selection.
							ClearSelection();
							if (m_hit2.collider.gameObject.tag == "building" || m_hit2.collider.gameObject.tag == "unit")
							{
								m_selected.Add((Selectable)m_hit2.collider.gameObject.GetComponent<Selectable>());
								m_selected[m_selected.Count-1].Select();
								m_cursorMode = Cursor.ORDER;
								
								if (m_hit2.collider.gameObject.tag == "building")
									m_selectionType = Selection.BUILDING;
								else
									m_selectionType = Selection.UNIT;
							}
						}
					}
				}
				break;
				
			case Cursor.BUILD:
				if (m_cursorBuilding)
				{
					Ray ray = camera.ScreenPointToRay(Input.mousePosition);
					RaycastHit hit;
					if (Physics.Raycast(ray, out hit, 10000.0f))
					{
						m_cursorBuilding.transform.position = hit.point + m_cursorOffset;
					}
				}
				break;
				
			case Cursor.REPAIR:
				// TODO: Select buildings that require repair.
				break;
				
			case Cursor.SELL:
				// TODO: Sell selected structure.
				break;				
			
			default:
				throw new UnityException();
			}
		}
		
		void ClearSelection()
		{
			// Clear selection list starting from a given index.
			m_selected.Clear();
			
			// Clear selection type
			m_selectionType = Selection.NONE;
			m_cursorMode = Cursor.SELECTION;
		}
	}
}