using GameLovers.ConfigsProvider;
using GameLovers.Services;
using Game.Logic.Shared;

namespace Game.Logic
{
	/// <summary>
	/// This interface marks the Game Logic as one that needs to initialize it's internal state
	/// </summary>
	public interface IGameLogicInitializer
	{
		/// <summary>
		/// Initializes the Game Logic state to it's default initial values
		/// </summary>
		void Init();
	}
	
	/// <summary>
	/// Provides access to all game's data.
	/// This interface provides the data with view only permissions
	/// </summary>
	public interface IGameDataProvider
	{
		/// <inheritdoc cref="IAppDataProvider"/>
		IAppDataProvider AppDataProvider { get; }
		/// <inheritdoc cref="IPlayerDataProvider"/>
		IPlayerDataProvider PlayerDataProvider { get; }
	}

	/// <summary>
	/// Provides access to all game's logic
	/// This interface shouldn't be exposed to the views or controllers
	/// To interact with the logic, execute a <see cref="Commands.IGameCommand"/> via the <see cref="ICommandService"/>
	/// </summary>
	public interface IGameLogic : IGameDataProvider
	{
		/// <inheritdoc cref="IAppLogic"/>
		IAppLogic AppLogic { get; }
		/// <inheritdoc cref="IPlayerLogic"/>
		IPlayerLogic PlayerLogic { get; }

		/// <summary>
		/// Force to save all game data
		/// </summary>
		void SaveData();
	}

	/// <inheritdoc cref="IGameLogic"/>
	public interface IGameLogicInit : IGameLogic, IGameLogicInitializer
	{
	}

	/// <inheritdoc cref="IGameLogic"/>
	public class GameLogic : IGameLogicInit
	{
		/// <inheritdoc />
		public IAppDataProvider AppDataProvider => AppLogic;
		/// <inheritdoc />
		public IPlayerDataProvider PlayerDataProvider => PlayerLogic;

		/// <inheritdoc />
		public IAppLogic AppLogic { get; }

		/// <inheritdoc />
		public IPlayerLogic PlayerLogic { get; }

		private IDataService _dataService;

		public GameLogic(IInstaller installer)
		{
			var configsProvider = installer.Resolve<IConfigsProvider>();
			var dataService = installer.Resolve<IDataService>();
			var timeService = installer.Resolve<ITimeService>();

			AppLogic = new AppLogic(configsProvider, dataService, timeService);
			PlayerLogic = new PlayerLogic(configsProvider, dataService, timeService);

			_dataService = dataService;
		}

		/// <inheritdoc />
		public void Init()
		{
			((IGameLogicInitializer)PlayerLogic).Init();
			((IGameLogicInitializer)AppLogic).Init();
		}

		/// <inheritdoc />
		public void SaveData()
		{
			_dataService.SaveAllData();
		}
	}
}