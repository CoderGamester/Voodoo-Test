using GameLovers.Services;
using GameLovers.StatechartMachine;
using Game.Logic;
using Game.Services;
using System;
using UnityEngine;

namespace Game.StateMachines
{
	/// <summary>
	/// The State Machine that controls the entire flow of the game
	/// </summary>
	public class GameStateMachine : IDisposable
	{
		private readonly IStatechart _stateMachine;
		private readonly IGameServices _services;
		//private readonly IGameUiServiceInit _uiService;
		private readonly InitialLoadingState _initialLoadingState;
		private readonly GameplayState _gameplayState;

		/// <inheritdoc cref="IStateMachine.LogsEnabled"/>
		public bool LogsEnabled
		{
			get => _stateMachine.LogsEnabled;
			set => _stateMachine.LogsEnabled = value;
		}

		public GameStateMachine(IGameLogicInit gameLogic, IGameServices services, IInstaller installer)
		{
			_services = services;
			//_uiService = installer.Resolve<IGameUiServiceInit>();
			
			_initialLoadingState = new InitialLoadingState(gameLogic, _services, installer, Trigger);
			_gameplayState = new GameplayState(_services, installer, Trigger);
			_stateMachine = new Statechart(Setup);
		}

		/// <inheritdoc cref="IStatechart.LogsEnabled"/>
		public void Run()
		{
			_stateMachine.Run();
		}

		/// <inheritdoc />
		public void Dispose()
		{
		}

		private void Trigger(IStatechartEvent eventTrigger)
		{
			_stateMachine.Trigger(eventTrigger);
		}

		private void Setup(IStateFactory stateFactory)
		{
			var initial = stateFactory.Initial("Initial");
			var final = stateFactory.Final("Final");
			var initialLoading = stateFactory.Nest("Initial Loading");
			var game = stateFactory.Nest("Game");
			
			initial.Transition().Target(initialLoading);
			initial.OnExit(SubscribeEvents);
			
			initialLoading.OnEnter(InitPlugins);
			initialLoading.Nest(_initialLoadingState.Setup).Target(game);
			
			game.Nest(_gameplayState.Setup).Target(final);
			
			final.OnEnter(UnsubscribeEvents);
		}

		private void UnsubscribeEvents()
		{
			_services.MessageBrokerService.UnsubscribeAll(this);
		}

		private void SubscribeEvents()
		{
			// Add any events to subscribe
		}

		private void InitPlugins()
		{
			if (Debug.isDebugBuild)
			{
				//SRDebug.Init();
			}
		}
	}
}