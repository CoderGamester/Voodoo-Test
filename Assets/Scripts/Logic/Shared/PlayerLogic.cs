using Game.Data;
using GameLovers;
using GameLovers.ConfigsProvider;
using GameLovers.Services;
using System;
using UnityEngine;

namespace Game.Logic.Shared
{
	/// <summary>
	/// This logic provides the necessary behaviour to manage the player's currency
	/// </summary>
	public interface IPlayerDataProvider
	{
		/// <summary>
		/// Requests the player's level
		/// </summary>
		IObservableFieldReader<int> PlayerLevel { get; }

		/// <summary>
		/// Requests the player's xp
		/// </summary>
		IObservableFieldReader<int> PlayerXP{ get; }

		/// <summary>
		/// Requests the player's xp
		/// </summary>
		IObservableFieldReader<int> BestScore { get; }

		/// <summary>
		/// Requests the player's favorite skin in the game to be automatically selected
		/// </summary>
		IObservableField<int> FavoriteSkin { get; }

		/// <summary>
		/// Requests the player's game name (trimmed of whitespaces).
		/// </summary>
		IObservableField<string> Nickname { get; }

		int GetGameResult(int index);
	}

	/// <inheritdoc />
	public interface IPlayerLogic : IPlayerDataProvider
	{
		/// <summary>
		/// Adds the given <paramref name="amount"/> to the player's xp
		/// </summary>
		void AddXP(int amount);

		/// <summary>
		/// Process the player's <paramref name="playerScore"/> and <paramref name="rankingScore"/> match result
		/// and saves it for future match dificulty and score adjustments
		/// </summary>
		void EndGameScores(int playerScore, int rankingScore);
	}

	/// <inheritdoc cref="ICurrencyLogic"/>
	public class PlayerLogic : AbstractBaseLogic<PlayerData>, IPlayerLogic, IGameLogicInitializer
	{
		private IObservableField<int> _playerLevel;
		private IObservableField<int> _playerXP;
		private IObservableField<int> _playerBest;

		/// <inheritdoc />
		public IObservableFieldReader<int> PlayerLevel => _playerLevel;
		/// <inheritdoc />
		public IObservableFieldReader<int> PlayerXP => _playerXP;
		/// <inheritdoc />
		public IObservableFieldReader<int> BestScore => _playerBest;
		/// <inheritdoc />
		public IObservableField<string> Nickname { get; private set; }
		/// <inheritdoc />
		public IObservableField<int> FavoriteSkin { get; private set; }

		public PlayerLogic(IConfigsProvider configsProvider, IDataService dataService, ITimeService timeService) :
			base(configsProvider, dataService, timeService)
		{
		}

		/// <inheritdoc />
		public void Init()
		{
			Nickname = new ObservableResolverField<string>(() => Data.Nickname, SetNickname);
			FavoriteSkin = new ObservableResolverField<int>(() => Data.FavoriteSkin, SetFavoriteSkin);

			_playerLevel = new ObservableResolverField<int>(() => Data.Level, level => Data.Level = level);
			_playerXP = new ObservableResolverField<int>(() => Data.XP, xp => Data.XP = xp);
			_playerBest = new ObservableResolverField<int>(() => Data.BestScore, bestScore => Data.BestScore = bestScore);
		}

		/// <inheritdoc />
		public int GetGameResult(int index)
		{
			UnityEngine.Debug.Log(index + " " + Data.GameResult.Length);
			return Data.GameResult[index];
		}

		/// <inheritdoc />
		public void AddXP(int amount)
		{
			var xp = amount + PlayerXP.Value;
			// TODO:This logic should not live in the StatsManager but 30 stats is a lot to copy. Documented in Tech Debt
			var statsManager = StatsManager.Instance;

			while (xp >= statsManager.XPToNextLevel())
			{
				xp -= statsManager.XPToNextLevel();
				_playerLevel.Value += 1;
			}

			_playerXP.Value = xp;
		}

		/// <inheritdoc />
		public void EndGameScores(int playerScore, int rankingScore)
		{
			if(playerScore > Data.BestScore)
			{
				Data.BestScore = playerScore;
			}

			// Move results
			for (int i = Constants.c_SavedGameCount - 1; i > 0; --i)
			{
				Data.GameResult[i] = GetGameResult(i - 1);
			}

			Data.GameResult[0] = rankingScore;
		}

		private void SetNickname(string name)
		{
			Data.Nickname = name.Trim();

			SaveData();
		}

		private void SetFavoriteSkin(int skin)
		{
			// TODO: Clamp the skin number and rest to 0 if overflow it
			Data.FavoriteSkin = skin;

			SaveData();
		}
	}
}
