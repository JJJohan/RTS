namespace RTS
{
	public static class Cursor
	{
		public const int SELECTION = 0;
		public const int BUILD = 1;
		public const int ORDER = 2;
		public const int REPAIR = 3;
		public const int SELL = 4;
	}
	
	public static class Selection
	{
		public const int NONE = 0;
		public const int UNIT = 1;
		public const int BUILDING = 2;
	}
	
	public struct Resources
	{
		public float power;
		public float funds;
	}
}
