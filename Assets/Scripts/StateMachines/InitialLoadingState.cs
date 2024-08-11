using System.Threading.Tasks;
using Game.Data;
using GameLovers.ConfigsProvider;
using GameLovers.Services;
using GameLovers.StatechartMachine;
//using GameLovers.UiService;
//using Game.Ids;
//using Game.Configs;
using Game.Logic;
using Newtonsoft.Json;
using Game.Services;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

namespace Game.StateMachines
{
	/// <summary>
	/// This class represents the Loading state in the <seealso cref="GameStateMachine"/>
	/// </summary>
	internal class InitialLoadingState
	{
		private readonly IStatechartEvent _authSuccessEvent = new StatechartEvent("Authentication Success Event");
		private readonly IStatechartEvent _authFailEvent = new StatechartEvent("Authentication Fail Generic Event");

		private readonly IGameServices _services;
		private readonly IGameLogicInit _gameLogic;
		//private readonly IGameUiServiceInit _uiService;
		private readonly IConfigsAdder _configsAdder;
		private readonly IDataService _dataService;
		private readonly Action<IStatechartEvent> _statechartTrigger;

		public InitialLoadingState(IGameLogicInit gameLogic, IGameServices services, IInstaller installer, 
			Action<IStatechartEvent> statechartTrigger)
		{
			_gameLogic = gameLogic;
			_services = services;
			//_uiService = installer.Resolve<IGameUiServiceInit>();
			_configsAdder = installer.Resolve<IConfigsAdder>();
			_dataService = installer.Resolve<IDataService>();
		}

		private void UnsubscribeEvents()
		{
			_services.MessageBrokerService.UnsubscribeAll(this);
		}

		private void SubscribeEvents()
		{
			// Add any events to subscribe
		}

		/// <summary>
		/// Setups the Initial Loading state
		/// </summary>
		public void Setup(IStateFactory stateFactory)
		{
			var initial = stateFactory.Initial("Initial");
			var final = stateFactory.Final("Final");
			var autoAuthCheck = stateFactory.Choice("Auto Auth Check");
			var authLoginDevice = stateFactory.State("Login Device Authentication");
			var authFail = stateFactory.State("Authentication Fail Dialog");

			initial.Transition().Target(autoAuthCheck);
			initial.OnExit(SubscribeEvents);

			autoAuthCheck.OnEnter(LoadGameData);
			// Requirement 2) c) -> Check if the device is already configured with the backend to authenticate the user to retrieve the data online
			autoAuthCheck.Transition().Condition(HasLinkedDevice).Target(authLoginDevice);
			autoAuthCheck.Transition().Target(final);

			authLoginDevice.OnEnter(LoginWithDevice);
			authLoginDevice.Event(_authSuccessEvent).Target(final);
			authLoginDevice.Event(_authFailEvent).Target(authFail);

			final.OnEnter(_gameLogic.Init);
			final.OnEnter(UnsubscribeEvents);
		}

		private bool HasLinkedDevice()
		{
			return !string.IsNullOrWhiteSpace(_dataService.GetData<AppData>().DeviceAuthId);
		}

		private void LoginWithDevice()
		{
			_services.AuthenticationService.LoginWithDevice(
				data => _statechartTrigger(_authSuccessEvent),
				error => _statechartTrigger(_authFailEvent));
		}

		/// <summary>
		/// Requirement 2) a)
		/// This method loads the player data and the app data from the local saving files
		/// When no saving file exists, create a new object type serialized in JSON
		/// </summary>
		private void LoadGameData()
		{
			var time = _services.TimeService.DateTimeUtcNow;
			var appDataJson = PlayerPrefs.GetString(nameof(AppData), "");
			var playerDataJson = PlayerPrefs.GetString(nameof(PlayerData), "");
			var appData = string.IsNullOrEmpty(appDataJson) ? new AppData() : JsonConvert.DeserializeObject<AppData>(appDataJson);
			var playerData = string.IsNullOrEmpty(playerDataJson) ? new PlayerData() : JsonConvert.DeserializeObject<PlayerData>(playerDataJson);

			if (string.IsNullOrEmpty(appDataJson))
			{
				appData.FirstLoginTime = time;
				appData.LoginTime = time;
			}
			
			appData.LastLoginTime = appData.LoginTime;
			appData.LoginTime = time;
			
			_dataService.AddOrReplaceData(appData);
			_dataService.AddOrReplaceData(playerData);
		}

		private async Task LoadInitialUi()
		{
			await UniTask.CompletedTask;
			//await Task.WhenAll(_uiService.LoadUiSetAsync((int) UiSetId.InitialLoadUi));
		}

		private async Task LoadConfigs()
		{
			await UniTask.CompletedTask;
			//var uiConfigs = await _services.AssetResolverService.LoadAssetAsync<UiConfigs>(AddressableId.Addressables_Configs_UiConfigs.GetConfig().Address);
			//var gameConfigs = await _services.AssetResolverService.LoadAssetAsync<GameConfigs>(AddressableId.Addressables_Configs_GameConfigs.GetConfig().Address);
			//var dataConfigs = await _services.AssetResolverService.LoadAssetAsync<DataConfigs>(AddressableId.Addressables_Configs_DataConfigs.GetConfig().Address);

			//_uiService.Init(uiConfigs);
			//_configsAdder.AddSingletonConfig(gameConfigs.Config);
			//_configsAdder.AddConfigs(data => (int) data.Id, dataConfigs.Configs);

			//_services.AssetResolverService.UnloadAsset(uiConfigs);
			//_services.AssetResolverService.UnloadAsset(gameConfigs);
			//_services.AssetResolverService.UnloadAsset(dataConfigs);
		}
	}
}