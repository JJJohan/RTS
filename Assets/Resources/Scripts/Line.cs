using UnityEngine;

namespace RTS
{
	public static class Line
	{
		static Material lineMaterial;
		
		private static void CreateLineMaterial()
		{
		    if( !lineMaterial ) 
			{
		        lineMaterial = new Material( "Shader \"Lines/Colored Blended\" {" +
            	"SubShader { Pass { " + 
            	"    BindChannels { Bind \"Color\",color } " +
            	"    Blend SrcAlpha OneMinusSrcAlpha " + 
            	"    ZWrite Off Cull Off Fog { Mode Off } " + 
            	"} } }" ); 

        		lineMaterial.hideFlags = HideFlags.HideAndDontSave; 
      			lineMaterial.shader.hideFlags = HideFlags.HideAndDontSave; 
			}
		} 
		
		public static void Draw(Vector2 a_start, Vector2 a_end)
		{
			Draw (a_start, a_end, Color.white);
		}
		
		public static void Draw(Vector2 a_start, Vector2 a_end, Color a_color)
		{
		    CreateLineMaterial();
		    lineMaterial.SetPass( 0 );
			
			Vector2 start = new Vector2(a_start.x / Screen.width, a_start.y / Screen.height);
			Vector2 end = new Vector2(a_end.x / Screen.width, a_end.y / Screen.height);
		
			GL.PushMatrix();
			GL.LoadOrtho();
		    GL.Begin( GL.LINES );
		    GL.Color( a_color );
		    GL.Vertex3( start.x, 1f - start.y, 0 );
 			GL.Vertex3( end.x, 1f - end.y, 0 );
		    GL.End();
			GL.PopMatrix();
		}
	}
}