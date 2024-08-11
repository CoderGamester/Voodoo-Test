using System;

namespace Game.Data
{
	/// <summary>
	/// Contains all the data in the scope of the Game's App
	/// </summary>
	[Serializable]
	public class AppData
	{
		public enum QualityLevel
		{
			High,
			Medium,
			Low
		}

		public DateTime FirstLoginTime;
		public DateTime LastLoginTime;
		public DateTime LoginTime;
		public bool IsFirstSession;
		public string Environment;
		public string DeviceAuthId;
		public DateTime GameReviewDate;

		public bool HapticEnabled = true;
		public int FpsTarget = 30;
		public QualityLevel GraphicQuality = QualityLevel.Medium;
	}
}