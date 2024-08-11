using System;

namespace Game.Data
{
	/// <summary>
	/// Contains all the data in the scope of the Player 
	/// </summary>
	[Serializable]
	public class PlayerData
	{
		public int Level = 1;
		public int XP = 0;
		public string Nickname;
		public int BestScore = 0;
		public int FavoriteSkin = 0;
		public int[] GameResult = new int[5];
	}
}