using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using NaughtyAttributes;
using DG.Tweening;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class LoginManager : MonoBehaviour
{
	public static LoginManager Instance;

	[SerializeField] private LoginAnimations _loginAnimations;
	[Space]

	[BoxGroup("Login")] [SerializeField] private TMPro.TMP_InputField _emailInput;
	[BoxGroup("Login")] [SerializeField] private TMPro.TMP_InputField _passwordInput;
	[BoxGroup("Login")] [SerializeField] private TMPro.TMP_InputField _usernameInput;
	[Space]
	[BoxGroup("Login")] [SerializeField] private Button _loginButton;
	[BoxGroup("Login")] [SerializeField] private Button _registerButton;
	[BoxGroup("Login")] [SerializeField] private TMPro.TextMeshProUGUI _inputErrorMessage;


	public event Action<string, string, string> _register;
	public event Action<string, string> _login;

	private void Awake()
	{
		if (Instance)
			Destroy(Instance);
		Instance = this;
	}

	private void Start()
	{
		_loginAnimations = GetComponent<LoginAnimations>();
		_login += ConnectionManager.Instance.PlayFabLogin;
		ConnectionManager.Instance.joinedLobby += JoinedLobby;
	}

	private void Update()
	{

	}

	public void SelectLogin(string loginType)
	{
		switch (loginType)
		{
			case "PlayFab":
				_loginButton.onClick.AddListener(PlayFabLogin);
				_registerButton.onClick.AddListener(PlayFabRegister);
				_loginAnimations.ShowLoginScreen(loginType);
				break;
			case "Guest":
				PhotonNetwork.NickName = "Noob" + Random.Range(1000, 1999);
				PhotonNetwork.ConnectUsingSettings();
				_loginAnimations.ShowLoading(true);
				break;
		}
	}

	private void PlayFabLogin()
	{
		_login?.Invoke(_emailInput.text, _passwordInput.text);
		_loginButton.interactable = false;
		_loginAnimations.ShowLoading(true);
	}

	private void PlayFabRegister()
	{
		if (!_inRegister)
			return;

		_register += ConnectionManager.Instance.PlayFabRegister;
		_inputErrorMessage.text = CheckRegisterInput();
		_register?.Invoke(_usernameInput.text, _emailInput.text, _passwordInput.text);
		_inRegister = false;
	}

	private bool _inRegister;
	public void RegisterClicked()
	{
		if (!_inRegister)
		{
			_usernameInput.GetComponent<RectTransform>().DOSizeDelta(new Vector2(510, 70), .5f).SetEase(Ease.InOutSine).OnComplete(() => _inRegister = true);
			_loginButton.GetComponent<RectTransform>().DOSizeDelta(new Vector2(0, 0), .5f).SetEase(Ease.InOutSine);
			_registerButton.GetComponent<Image>().DOColor(_loginButton.GetComponent<Image>().color, .5f).SetEase(Ease.InOutSine);
			return;
		}

	}

	private string CheckRegisterInput()
	{
		string registerErrorMessage = "";
		if (!_emailInput.text.Contains("@"))
			registerErrorMessage += "\n Email is not valid.";

		if ((_usernameInput.text.Length < 3))
			registerErrorMessage += "\n Username is too short.";

		if ((_passwordInput.text.Length < 6))
			registerErrorMessage += "\n Password is too short.";

		return registerErrorMessage;
	}

	private bool CheckLoginInput()
	{
		bool isFine = true;

		//VALIDATE INPUTS

		return isFine;
	}

	private void JoinedLobby()
	{
		SceneManager.LoadScene("Lobby");
	}

}
