using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;
using Photon.Realtime;
using PlayFab;
using PlayFab.ClientModels;

public class ConnectionManager : MonoBehaviourPunCallbacks
{
	public static ConnectionManager Instance;

	public event Action connectedToPhoton;
	public event Action joinedLobby;
	public event Action joinedRoom;

	private string _playFabId;

	private void Awake()
	{
		DontDestroyOnLoad(gameObject);
		if (Instance)
			Destroy(Instance);
		Instance = this;
	}

	private void Start()
	{
	}

	#region Facebook
	public void InitializeFacebookLogin()
	{
		Debug.Log("InitializeFacebookLogin");
		//if (!FB.IsInitialized)
		//{
		//	Debug.Log("InitializeFacebookLogin FB.INIT CALL");
		//	FB.Init(FacebookInitCallback);
		//	Debug.Log("InitializeFacebookLogin FB.INIT AFTER CALL");

		//}
		//else
		//{
		//	Debug.Log("InitializeFacebookLogin FACEBOOKLOGIN CALL");
		//	FacebookLogin();
		//	Debug.Log("InitializeFacebookLogin FACEBOOKLOGIN AFTER CALL");
		//}
	}

	private void FacebookLogin()
	{
		//if (FB.IsLoggedIn)
		//{
		//	OnFacebookLogin();
		//}
		//else
		//{
		//	Debug.Log("FacebookLogin LOGINWITHPERMISSIONS CALL");
		//	var perms = new List<string>() { "public_profile", "email", "user_friends" };
		//	FB.LogInWithReadPermissions(perms, FacebookAuthCallback);
		//	Debug.Log("FacebookLogin LOGINWITHPERMISSIONS AFTER CALL");

		//}
	}

	private void FacebookInitCallback()
	{
		Debug.Log("FacebookInitCallback");

		//if (FB.IsInitialized)
		//{
		//	Debug.Log("FacebookInitCallback FB.ISINITIALIZED");
		//	FacebookLogin();
		//}
		//else
		//	Debug.Log("Failed to initialize facebook");
	}

	private void OnFacebookLogin()
	{
		//string aToken = AccessToken.CurrentAccessToken.TokenString;
		//string facebookId = AccessToken.CurrentAccessToken.UserId;
		//PhotonNetwork.AuthValues = new AuthenticationValues();
		//PhotonNetwork.AuthValues.AuthType = CustomAuthenticationType.Facebook;
		//PhotonNetwork.AuthValues.UserId = facebookId;
		//PhotonNetwork.AuthValues.AddAuthParameter("token", aToken);
		//PhotonNetwork.ConnectUsingSettings();
	}

	//private void FacebookAuthCallback(ILoginResult result)
	//{
	//	if (FB.IsLoggedIn)
	//		OnFacebookLogin();
	//	else
	//	{
	//		Debug.LogErrorFormat("Error in Facebook login {0}", result.Error);
	//	}
	//}

	#endregion

	#region Playfab

	public void PlayFabLogin(string email, string password)
	{
		var request = new LoginWithEmailAddressRequest
		{
			Email = email,
			Password = password,
		};
		PlayFabClientAPI.LoginWithEmailAddress(request, RequestTokenPlayFab, OnPlayFabError);
	}

	public void PlayFabRegister(string username, string email, string password)
	{
		var request = new RegisterPlayFabUserRequest
		{
			Username = username,
			Email = email,
			Password = password,
			RequireBothUsernameAndEmail = false
		};
		PlayFabClientAPI.RegisterPlayFabUser(request, OnPlayFabRegisterSuccess, OnPlayFabError);
	}

	private void OnPlayFabRegisterSuccess(RegisterPlayFabUserResult result)
	{
		_playFabId = result.PlayFabId;
		ConnectToPhoton();
	}

	private void RequestTokenPlayFab(LoginResult result)
	{

		_playFabId = result.PlayFabId;

		PlayFabClientAPI.GetAccountInfo(new GetAccountInfoRequest { PlayFabId = _playFabId },
			result =>
			{
				//Handle AccountInfo
				Debug.Log(result.AccountInfo.Username);
			},
			error => { Debug.LogError(error.GenerateErrorReport()); });

		GetPhotonAuthenticationTokenRequest request = new GetPhotonAuthenticationTokenRequest();
		request.PhotonApplicationId = "80149c32-9956-4fb7-af69-59f5387d15a8";
		PlayFabClientAPI.GetPhotonAuthenticationToken(request, AuthenticateWithPhoton, OnPlayFabError);
	}

	private void OnPlayFabError(PlayFabError obj)
	{
		Debug.Log(obj);
	}

	#endregion

	private void RequestPhotonToken()
	{
		PlayFabClientAPI.GetPhotonAuthenticationToken(new GetPhotonAuthenticationTokenRequest()
		{
			PhotonApplicationId = PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime,
		}, AuthenticateWithPhoton, OnPlayFabError); ;
	}

	private void AuthenticateWithPhoton(GetPhotonAuthenticationTokenResult obj)
	{
		var customAuth = new AuthenticationValues { AuthType = CustomAuthenticationType.Custom };
		customAuth.AddAuthParameter("username", _playFabId);    // expected by PlayFab custom auth service
		customAuth.AddAuthParameter("token", obj.PhotonCustomAuthenticationToken);

		PhotonNetwork.AuthValues = customAuth;
		ConnectToPhoton();
	}

	private void ConnectToPhoton()
	{
		Debug.Log("Connect To photon");

		PlayFabClientAPI.GetAccountInfo(new GetAccountInfoRequest { PlayFabId = _playFabId },
			result =>
				{
					//Handle AccountInfo
					PhotonNetwork.NickName = result.AccountInfo.Username;
				},
			error => { Debug.LogError(error.GenerateErrorReport()); });

		PhotonNetwork.ConnectUsingSettings();
	}

	public void JoinOrCreateRoom(string roomName)
	{
		if (!PhotonNetwork.IsConnected)
			return;
		//JOIN OR CREATE ROOM
		//HERE WE CAN SET THE ROOM OPTIONS IF WE NEED 
		RoomOptions options = new RoomOptions();
		PhotonNetwork.JoinOrCreateRoom(roomName, options, default);
	}

	public void PhotonLoadScene(string scene)
	{
		PhotonNetwork.LoadLevel(scene);
	}

	//PHOTON CALLBACK FUNCS
	public override void OnConnectedToMaster()
	{
		base.OnConnectedToMaster();

		PhotonNetwork.JoinLobby();
		connectedToPhoton?.Invoke();
	}

	public override void OnJoinedRoom()
	{
		base.OnJoinedRoom();
		joinedRoom?.Invoke();
	}

	public override void OnCreateRoomFailed(short returnCode, string message)
	{
		base.OnCreateRoomFailed(returnCode, message);
		Debug.Log("Created Room Failed");
	}

	public override void OnJoinedLobby()
	{
		base.OnJoinedLobby();
		joinedLobby?.Invoke();
	}


}
