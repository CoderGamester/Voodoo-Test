using Game.Logic;
using GameLovers.Services;

namespace Game.Commands
{
	/// <summary>
	/// Updates player progression and end result stats
	/// </summary>
	public struct EndGameResultsCommand : IGameCommand<IGameLogic>
	{
		public int MatchScore;
		public int MatchRank;

		/// <inheritdoc />
		public void Execute(IGameLogic gameLogic)
		{
			gameLogic.PlayerLogic.EndGameScores(MatchScore, MatchRank);
			gameLogic.PlayerLogic.AddXP(Constants.c_PlayerRankToXP[MatchRank]);

			gameLogic.SaveData();
		}
	}
}
