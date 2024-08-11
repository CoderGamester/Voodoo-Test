using System;
using Game.Data;
using GameLovers;
using GameLovers.ConfigsProvider;
using GameLovers.Services;
using Game.Logic.Shared;

namespace Game.Logic
{
	/// <summary>
	/// This logic provides the necessary behaviour to manage the game's app
	/// </summary>
	public interface IAppDataProvider
	{
		/// <summary>
		/// Requests the information if the current game session is the first time the player is playing the game or not
		/// </summary>
		bool IsFirstSession { get; }

		/// <summary>
		/// Requests the information if the game was or not yet reviewed
		/// </summary>
		bool IsGameReviewed { get; }

		/// <summary>
		/// Requests if this device is Linked
		/// </summary>
		bool IsDeviceLinked { get; }

		/// <summary>
		/// Is Haptic feedback on device enabled?
		/// </summary>
		bool IsHapticOn { get; set; }

		/// <summary>
		/// Requests the current detail level of the game
		/// </summary>
		AppData.QualityLevel CurrentGraphicQuality { get; set; }

		/// <summary>
		/// Requests the current FPS target
		/// </summary>
		int FpsTarget { get; set; }

		/// <summary>
		/// Requests current device Id
		/// </summary>
		IObservableField<string> DeviceID { get; }

		/// <summary>
		/// Reason why the player quit the app
		/// </summary>
		string QuitReason { get; }
	}

	/// <inheritdoc cref="IAppLogic"/>
	public interface IAppLogic : IAppDataProvider, IGameLogicInitializer
	{
		/// <summary>
		/// Marks the date when the game was last time reviewed
		/// </summary>
		void MarkGameAsReviewed();

		/// <summary>
		/// Method used when we want to leave the app, so we can record the reason
		/// </summary>
		/// <param name="reason">Reason why we quit the app</param>
		void QuitGame(string reason);
	}

	/// <inheritdoc cref="IAppLogic"/>
	public class AppLogic : AbstractBaseLogic<AppData>, IAppLogic
	{
		private readonly DateTime _defaultZeroTime = new(2020, 1, 1);

		/// <inheritdoc />
		public bool IsFirstSession => Data.IsFirstSession;

		/// <inheritdoc />
		public bool IsGameReviewed => Data.GameReviewDate > _defaultZeroTime;

		/// <inheritdoc />
		public bool IsDeviceLinked => !string.IsNullOrWhiteSpace(DeviceID.Value);

		/// <inheritdoc />
		public bool IsHapticOn
		{
			get => Data.HapticEnabled;
			set
			{
				Data.HapticEnabled = value;
				MobileHapticManager.s_Vibrate = value;

				SaveData();
			}
		}

		/// <inheritdoc />
		public AppData.QualityLevel CurrentGraphicQuality
		{
			get => Data.GraphicQuality;
			set
			{
				Data.GraphicQuality = value;

				UnityEngine.QualitySettings.SetQualityLevel((int)value);
			}
		}

		/// <inheritdoc />
		public int FpsTarget
		{
			get => Data.FpsTarget;
			set
			{
				Data.FpsTarget = value;
				UnityEngine.Application.targetFrameRate = value;
			}
		}

		/// <inheritdoc />
		public IObservableField<string> DeviceID { get; private set; }

		/// <inheritdoc />
		public string QuitReason { get; private set; }

		public AppLogic(IConfigsProvider configsProvider, IDataService dataService, ITimeService timeService) :
			base(configsProvider, dataService, timeService)
		{
		}

		/// <inheritdoc />
		public void Init()
		{
			DeviceID = new ObservableResolverField<string>(() => Data.DeviceAuthId, linked => Data.DeviceAuthId = linked);

			// Init saved values
			FpsTarget = FpsTarget;
			CurrentGraphicQuality = CurrentGraphicQuality;
			IsHapticOn = IsHapticOn;
		}

		/// <inheritdoc />
		public void MarkGameAsReviewed()
		{
			if (IsGameReviewed)
			{
				throw new LogicException("The game was already reviewed and cannot be reviewed again");
			}

			Data.GameReviewDate = TimeService.DateTimeUtcNow;
		}

		/// <inheritdoc />
		public void QuitGame(string reason)
		{
			QuitReason = reason;
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#else
			UnityEngine.Application.Quit(); // Apple does not allow to close the app
#endif
		}
	}
}