using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using DG.Tweening;
using UnityEngine.UI;

public class LoginAnimations : MonoBehaviour
{
	public static LoginAnimations Instance;

	[BoxGroup("UI components")] [SerializeField] private GameObject _startButton;
	[BoxGroup("UI components")] [SerializeField] private GameObject _logo;
	[BoxGroup("UI components")] [SerializeField] private GameObject _loading;
	[BoxGroup("UI components")] [SerializeField] private GameObject _background;
	[Space]
	[BoxGroup("Login")] [SerializeField] private CanvasGroup _loginButtons;
	[BoxGroup("Login")] [SerializeField] private CanvasGroup _loginScreen;
	[BoxGroup("Login")] [SerializeField] private CanvasGroup _loginTab;
	[BoxGroup("Login")] [SerializeField] private TMPro.TextMeshProUGUI _loginTabTitle;



	private void Awake()
	{
		if (Instance)
			Destroy(Instance);
		Instance = this;

	}

	private void Start()
	{
		ConnectionManager.Instance.joinedLobby += JoinedLobby;

		_loading.GetComponent<CanvasGroup>().DOFade(0, 0);

		_logo.GetComponent<CanvasGroup>().DOFade(0, 0);

		_loginScreen.GetComponent<CanvasGroup>().DOFade(0, 0);
		_loginScreen.GetComponent<CanvasGroup>().interactable = false;
		_loginScreen.GetComponent<CanvasGroup>().blocksRaycasts = false;

		_loginTab.GetComponent<CanvasGroup>().DOFade(0, 0);
		_loginTab.GetComponent<CanvasGroup>().interactable = false;
		_loginTab.GetComponent<CanvasGroup>().blocksRaycasts = false;

		_startButton.GetComponent<CanvasGroup>().DOFade(0, 0);
		_startButton.GetComponent<CanvasGroup>().interactable = false;
		_startButton.GetComponent<CanvasGroup>().blocksRaycasts = false;

		StartCoroutine(StartRoutine());
	}

	private void Update()
	{

	}

	public void StartClicked()
	{
		_startButton.GetComponent<CanvasGroup>().interactable = false;
		_startButton.GetComponent<CanvasGroup>().blocksRaycasts = false;

		_loginButtons.interactable = true;
		_loginButtons.blocksRaycasts = true;

		_startButton.GetComponent<CanvasGroup>().DOFade(0, .5f).SetEase(Ease.InOutSine)
			.OnComplete(() => _loginScreen.DOFade(1, 1));
		_loginScreen.interactable = true;
		_loginScreen.blocksRaycasts = true;
	}

	public void ShowLoginScreen(string loginType)
	{
		_loginTab.interactable = true;
		_loginTab.blocksRaycasts = true;
		_loginTabTitle.text = "Login with " + loginType;
		_loginButtons.DOFade(0, .5f).SetEase(Ease.InOutSine).OnComplete(() => _loginTab.GetComponent<CanvasGroup>().DOFade(1, .5f));
	}

	private void JoinedLobby()
	{
		ShowLoading(false);
	}

	public void ShowLoading(bool show)
	{
		_loading.GetComponent<CanvasGroup>().DOFade(show ? 1 : 0, .5f);
	}

	private IEnumerator StartRoutine()
	{
		_logo.GetComponent<CanvasGroup>().DOFade(1, 2).SetEase(Ease.InOutSine);
		yield return new WaitForSeconds(2);
		_logo.GetComponent<CanvasGroup>().DOFade(0, 1.5f).SetEase(Ease.InOutSine);
		yield return new WaitForSeconds(2f);
		_startButton.GetComponent<CanvasGroup>().DOFade(1, .5f).SetEase(Ease.InOutSine);
		_startButton.GetComponent<CanvasGroup>().interactable = true;
		_startButton.GetComponent<CanvasGroup>().blocksRaycasts = true;
	}
}
