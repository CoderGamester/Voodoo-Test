﻿using Cysharp.Threading.Tasks;
using GameLovers;
using GameLovers.ConfigsProvider;
using GameLovers.Services;
//using GameLovers.UiService;
using Game.Logic;
using Game.Services;
using Game.StateMachines;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Core;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem.UI;
using System.Collections;
using Game.Messages;

// ReSharper disable once CheckNamespace

namespace Game
{
	/// <summary>
	/// The Main entry point of the game
	/// </summary>
	public class Main : MonoBehaviour
	{
		[SerializeField] private InputSystemUIInputModule _inputSystem;
		//[SerializeField] private Camera _mainCamera;

		private GameStateMachine _stateMachine;
		private GameServices _services;
		private IGameLogic _gameLogic;
		private IDataService _dataService;
		private Coroutine _pauseCoroutine;
		private bool _onApplicationPauseFlag;
		private bool _onApplicationAlreadyQuitFlag;

		private void Awake()
		{
			var installer = new Installer();
			var dataService = new DataService();

			installer.Bind<IMessageBrokerService>(new MessageBrokerService());
			installer.Bind<ITimeService>(new TimeService());
			//installer.Bind<IGameUiServiceInit, IGameUiService>(new GameUiService(new UiAssetLoader()));
			installer.Bind<IPoolService>(new PoolService());
			installer.Bind<ITickService>(new TickService());
			installer.Bind<IAnalyticsService>(new AnalyticsService());
			installer.Bind<ICoroutineService>(new CoroutineService());
			installer.Bind<IWorldObjectReferenceService>(new WorldObjectReferenceService(_inputSystem, null));// _mainCamera));
			installer.Bind<IAuthenticationService>(new BackendAuthenticationService(dataService));
			//installer.Bind<IAssetResolverService>(new AssetResolverService());
			installer.Bind<IConfigsAdder, IConfigsProvider>(new ConfigsProvider());
			installer.Bind<IDataService, IDataProvider>(dataService);

			var gameLogic = new GameLogic(installer);

			installer.Bind<ICommandService<IGameLogic>>(new CommandService<IGameLogic>(gameLogic));

			var gameServices = new GameServices(installer);

			MainInstaller.Bind<IGameDataProvider>(gameLogic);
			MainInstaller.Bind<IGameServices>(gameServices);

			_gameLogic = gameLogic;
			_services = gameServices;
			_dataService = dataService;
			_stateMachine = new GameStateMachine(gameLogic, gameServices, installer);
			Screen.sleepTimeout = SleepTimeout.NeverSleep;

			System.Threading.Tasks.TaskScheduler.UnobservedTaskException += TaskExceptionLogging;

			DontDestroyOnLoad(this);
		}

		private void OnDestroy()
		{
			System.Threading.Tasks.TaskScheduler.UnobservedTaskException -= TaskExceptionLogging;

			MainInstaller.Clean();
		}

		private void Start()
		{
			_services.Init();

			_ = OnStart();
		}

		private async UniTask OnStart()
		{
			//TouchSimulation.Enable();
			EnhancedTouchSupport.Enable();
			InitAtt();

			await Task.WhenAll(VersionServices.LoadVersionDataAsync(), UnityServices.InitializeAsync());

			InitAnalytics();
			_stateMachine.Run();
		}

		private void OnApplicationFocus(bool hasFocus)
		{
			if (!hasFocus)
			{
				_dataService.SaveAllData();
			}
		}

		private void OnApplicationPause(bool isPaused)
		{
			// This is mandatory because OnApplicationPause is always called when the GameObject starts after Awake() call -> https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnApplicationPause.html
			if (!_onApplicationPauseFlag)
			{
				_onApplicationPauseFlag = true;
				return;
			}

			if (isPaused)
			{
				_pauseCoroutine = StartCoroutine(EndAppCoroutine());
			}
			else if (_pauseCoroutine != null)
			{
				StopCoroutine(_pauseCoroutine);

				_pauseCoroutine = null;
			}

			_services.MessageBrokerService.Publish(new ApplicationPausedMessage { IsPaused = isPaused });
		}

		private void OnApplicationQuit()
		{
			if (_onApplicationAlreadyQuitFlag) return;

			_onApplicationAlreadyQuitFlag = true;

			_services.MessageBrokerService.Publish(new ApplicationQuitMessage());
			_services.AnalyticsService.SessionCalls.SessionEnd(_gameLogic.AppLogic.QuitReason);
		}

		private IEnumerator EndAppCoroutine()
		{
			// The app is closed after 30 sec of being unused
			yield return new WaitForSeconds(30);

			OnApplicationQuit();
			_gameLogic.AppLogic.QuitGame("App closed after 30 sec of being unused");
		}

		private void TaskExceptionLogging(object sender, UnobservedTaskExceptionEventArgs e)
		{
			if (sender.GetType().GetGenericTypeDefinition() == typeof(Task<>))
			{
				var task = sender as Task<object>;
				var objName = task.Result is UnityEngine.Object ? ((UnityEngine.Object)task.Result).name : task.Result.ToString();

				Debug.LogError($"Task exception sent by the object {objName}");
			}

			_services.AnalyticsService.ErrorsCalls.CrashLog(e.Exception);
		}

		private void InitAnalytics()
		{
			// TODO: request data collection permition (use ask age screen for example)
			Unity.Services.Analytics.AnalyticsService.Instance.StartDataCollection();
			_services.AnalyticsService.SessionCalls.PlayerLogin(SystemInfo.deviceUniqueIdentifier);
		}

		private void InitAtt()
		{
#if UNITY_IOS
			Unity.Advertisement.IosSupport.SkAdNetworkBinding.SkAdNetworkRegisterAppForNetworkAttribution();
			
			if (Unity.Advertisement.IosSupport.ATTrackingStatusBinding.GetAuthorizationTrackingStatus() == 
			    Unity.Advertisement.IosSupport.ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
			{
				Unity.Advertisement.IosSupport.ATTrackingStatusBinding.RequestAuthorizationTracking();
			}
#endif
		}
	}
}
