using UnityEngine;
using System.IO;

namespace RTS
{
	static class Logger
	{
		public static void LogWarning(object a_text)
		{
			if (Application.isEditor)
			{
				Debug.LogWarning(a_text);
			}
			else
			{
				string text = a_text.ToString();
				Log("WARNING: " + text);
			}
		}

		public static void LogError(object a_text)
		{
			if (Application.isEditor)
			{
				Debug.LogError(a_text);
			}
			else
			{
				string text = a_text.ToString();
				Log("ERROR: " + text);
			}
		}

		public static void Log(object a_text)
		{
			if (Application.isEditor)
			{
				Debug.Log(a_text);
			}
			else
			{
				TextWriter writer = new StreamWriter(Application.dataPath + "/error.log", true);
				writer.Write("[" + System.DateTime.Now.ToShortDateString() + " - " + System.DateTime.Now.ToShortTimeString() + "] ");
				writer.WriteLine(a_text.ToString());
				writer.Flush();
				writer.Close();
			}
		}
	}
}