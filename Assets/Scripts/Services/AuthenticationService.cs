using Game.Logic;
using Game.Data;
using GameLovers.ConfigsProvider;
using GameLovers.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Debug = UnityEngine.Debug;
using UnityEngine;
using System.Threading.Tasks;
using Game.Info;

namespace Game.Services
{
	/// <summary>
	/// This services handles all authentication functionality
	/// </summary>
	public interface IAuthenticationService
	{
		/// <summary>
		/// Requests the information of this account from the server
		/// </summary>
		ServerInfo GetServerInfo();

		/// <summary>
		/// Authenticates to the backend with current device as the credentials based on a previous authenticated 
		/// login form accepted in by the system (sign-in with apple, google, facebook or email)
		/// </summary>
		Task LoginWithDevice(Action<PlayerData> onSuccess, Action<string> onError);

		/// <summary>
		/// Authenticates to the backend with apple token credentials
		/// </summary>
		Task LoginWithApple(Action<PlayerData> onSuccess, Action<string> onError);

		/// <summary>
		/// Authenticates to the backend with google token credentials
		/// </summary>
		Task LoginWithGoogle(Action<PlayerData> onSuccess, Action<string> onError);

		/// <summary>
		/// Authenticates to the backend with facebook token credentials
		/// </summary>
		Task LoginWithFacebook(Action<PlayerData> onSuccess, Action<string> onError);

		/// <summary>
		/// Authenticates the backend with an email address and password
		/// </summary>
		Task LoginWithEmail(string email, string password, Action<PlayerData> onSuccess, Action<string> onError);

		/// <summary>
		/// Logs out of the current account. This includes unlinking the device, and logging out of other services
		/// </summary>
		void Logout(Action onSuccess, Action<string> onError);

		/// <summary>
		/// Registers and logins the user on the backend with the provided credentials.
		/// </summary>
		Task RegisterWithEmail(string email, string password, Action onSuccess, Action<string> onError);
	}

	/// <inheritdoc cref="IAuthenticationService" />
	public class BackendAuthenticationService : IAuthenticationService
	{
		private IDataService _dataService;

		public BackendAuthenticationService(IDataService dataService)
		{
			_dataService = dataService;
		}

		/// <inheritdoc />
		public ServerInfo GetServerInfo()
		{
			// This data should come directly from the server authentication return model.
			// Either saved during login or an RPC request to the backend server
			return new ServerInfo();
		}

		/// <inheritdoc />
		public async Task LoginWithDevice(Action<PlayerData> onSuccess, Action<string> onError)
		{
			if (Application.internetReachability == NetworkReachability.NotReachable)
			{
				onError?.Invoke("No internet connection");
			}
			else
			{
				// Login directly to the backend with the device Id saved internaly to attribute to the player's account
				var data = await BackendAuthenticationMock(_dataService.GetData<AppData>().DeviceAuthId);

				await LinkDeviceToAccount();

				onSuccess?.Invoke(data);
			}
		}

		/// <inheritdoc />
		public async Task LoginWithApple(Action<PlayerData> onSuccess, Action<string> onError)
		{
			if (Application.internetReachability == NetworkReachability.NotReachable)
			{
				onError?.Invoke("No internet connection");
			}
			else
			{
				// Login to Apple with sign-in plugin
				{
					await Task.Yield();
				}

				var data = await BackendAuthenticationMock("appleTokenKey");

				await LinkDeviceToAccount();

				onSuccess?.Invoke(data);
			}
		}

		/// <inheritdoc />
		public async Task LoginWithGoogle(Action<PlayerData> onSuccess, Action<string> onError)
		{
			if (Application.internetReachability == NetworkReachability.NotReachable)
			{
				onError?.Invoke("No internet connection");
			}
			else
			{
				// Login to Google with sign-in plugin
				{
					await Task.Yield();
				}

				var data = await BackendAuthenticationMock("googleTokenKey");

				await LinkDeviceToAccount();

				onSuccess?.Invoke(data);
			}
		}

		/// <inheritdoc />
		public async Task LoginWithFacebook(Action<PlayerData> onSuccess, Action<string> onError)
		{
			if (Application.internetReachability == NetworkReachability.NotReachable)
			{
				onError?.Invoke("No internet connection");
			}
			else
			{
				// Login to Facebook with sign-in plugin
				{
					await Task.Yield();
				}

				var data = await BackendAuthenticationMock("facebookTokenKey");

				await LinkDeviceToAccount();

				onSuccess?.Invoke(data);
			}
		}

		/// <inheritdoc />
		public async Task LoginWithEmail(string email, string password, Action<PlayerData> onSuccess, Action<string> onError)
		{
			if (Application.internetReachability == NetworkReachability.NotReachable)
			{
				onError?.Invoke("No internet connection");
			}
			else
			{
				// Login directly to the backend with the email and email using the backend plugin API (Nakama API)
				{
					await Task.Yield();
				}

				var data = await BackendAuthenticationMock(email + password);

				await LinkDeviceToAccount();

				onSuccess?.Invoke(data);
			}
		}

		/// <inheritdoc />
		public void Logout(Action onSuccess, Action<string> onError)
		{
			if(Application.internetReachability == NetworkReachability.NotReachable)
			{
				onError?.Invoke("No internet connection");
			}
			else
			{
				onSuccess?.Invoke();
			}
		}

		/// <inheritdoc />
		public async Task RegisterWithEmail(string email, string password, Action onSuccess, Action<string> onError)
		{
			if (Application.internetReachability == NetworkReachability.NotReachable)
			{
				onError?.Invoke("No internet connection");
			}
			else
			{
				// Register and login directly to the backend with the email and email using the backend plugin API (Nakama API)
				{
					await Task.Yield();
				}

				var data = await BackendAuthenticationMock(email + password);

				await LinkDeviceToAccount();

				onSuccess?.Invoke();
			}
		}

		/// <inheritdoc />
		private async Task<PlayerData> BackendAuthenticationMock(string authToken)
		{
			// Authenticate with the server
			// Request player data from the server
			{
				await Task.Yield();
			}

			// this line is here for exemplary purposes
			var data = _dataService.GetData<PlayerData>();

			// Save the data requested from the server in the data service
			_dataService.AddOrReplaceData(data);

			return data;
		}

		/// <inheritdoc />
		private async Task LinkDeviceToAccount()
		{
			// Link deviceId to account in the backend to allow game to auto-login the next player opens the app
			// using the backend plugin API (Nakama API)
			await Task.Yield();
		}
	}
}
