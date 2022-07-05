using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using ExitGames.Client.Photon;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviourPunCallbacks
{
	public static UIManager Instance;

	public Joystick movementJoystick;
	public Joystick aimJoytstick;

	[BoxGroup("Inventory")] [SerializeField] private InvetorySlot _inventorySlot1;
	[BoxGroup("Inventory")] [SerializeField] private InvetorySlot _inventorySlot2;

	[BoxGroup("UI Components")] [SerializeField] private CanvasGroup _attackButton;
	[BoxGroup("UI Components")] [SerializeField] private Slider _lifeBar;
	[BoxGroup("UI Components")] [SerializeField] private TextMeshProUGUI _lifeText;
	[BoxGroup("UI Components")] [SerializeField] private TextMeshProUGUI _timer;
	[BoxGroup("UI Components")] [SerializeField] private TextMeshProUGUI _score;
	[BoxGroup("UI Components")] [SerializeField] private CanvasGroup _retryScreen;
	[BoxGroup("UI Components")] [SerializeField] private TextMeshProUGUI _retryMaxScore;
	[BoxGroup("UI Components")] [SerializeField] private TextMeshProUGUI _retryCurrentScore;

	[BoxGroup("Setting")] [SerializeField] private float _lifeBarSineLenght;
	[BoxGroup("Setting")] [SerializeField] private float _lifeBarSineSpeed;
	[BoxGroup("Setting")] [SerializeField] private float _lifeBarAnimDuration;

	[HideInInspector] public float countDownStartTime;
	[HideInInspector] public float countDownDuration;
	[HideInInspector] public bool countDownActive;

	private int _zombiesKilled;
	private int _currentHorde;

	public event Action attackButtonClicked;
	public event Action<bool> shootButtonClicked;
	public event Action<bool> shootButtoReleased;
	public event Action<Item> swapWeapomButtonClicked;
	public event Action<Item> collectWeapomButtonClicked;

	private InvetorySlot _lastSlot;

	private void Awake()
	{
		if (Instance)
			Destroy(Instance);
		Instance = this;

	}

	void Start()
	{
	}

	void Update()
	{
		UpdateCountDown();
		_score.text = "X" + _zombiesKilled;
	}

	public void AttackButtonClicked()
	{
		attackButtonClicked?.Invoke();
	}

	public void ShootButtonClicked(bool clicked)
	{
		if (clicked)
			shootButtonClicked?.Invoke(clicked);
		else
			shootButtoReleased?.Invoke(clicked);
	}

	public void CollectableAvaiable(Item item, bool avaiable)
	{
		_inventorySlot2.itemToSwap = item;
		_inventorySlot1.itemToSwap = item;

		if (avaiable)
		{
			if (_inventorySlot1.currentItem == null && _inventorySlot2.currentItem != item)
			{
				CollectWeapon(_inventorySlot1);
				return;
			}

			if (_inventorySlot2.currentItem == null && _inventorySlot1.currentItem != item)
			{
				CollectWeapon(_inventorySlot2);
				return;
			}

			if (_inventorySlot1.currentItem != item && _inventorySlot2.currentItem != item)
				_inventorySlot1.ShowSwapButton(avaiable);

			if (_inventorySlot2.currentItem != item && _inventorySlot1.currentItem != item)
				_inventorySlot2.ShowSwapButton(avaiable);

			return;
		}

		_inventorySlot1.itemToSwap = item;
		_inventorySlot1.ShowSwapButton(avaiable);

		_inventorySlot2.itemToSwap = item;
		_inventorySlot2.ShowSwapButton(avaiable);
	}

	public void SwapWeapon(InvetorySlot slot)
	{
		if (slot.currentItem == null)
			return;

		if (slot.selected)
			return;

		swapWeapomButtonClicked?.Invoke(slot.currentItem);

		_inventorySlot1.ShowSwapButton(false);
		_inventorySlot2.ShowSwapButton(false);
		slot.Selected(true);
		slot.UpdateIcon();

		if (_lastSlot != null)
			_lastSlot.Selected(false);
		_lastSlot = slot;

	}

	public void CollectWeapon(InvetorySlot slot)
	{
		if (slot.itemToSwap == null)
			return;
		slot.currentItem = slot.itemToSwap;

		_inventorySlot1.ShowSwapButton(false);
		_inventorySlot2.ShowSwapButton(false);
		slot.Selected(true);
		slot.UpdateIcon();

		collectWeapomButtonClicked?.Invoke(slot.itemToSwap);

		slot.itemToSwap = null;
		if (_lastSlot != null)
			_lastSlot.Selected(false);
		_lastSlot = slot;
	}

	public void ChangeweaponUI(bool gun)
	{
		CanvasGroup aimJoystickCanvasGroup = aimJoytstick.GetComponent<CanvasGroup>();
		aimJoystickCanvasGroup.DOFade(gun ? 1 : 0, .2f).SetEase(Ease.InOutSine);
		aimJoystickCanvasGroup.interactable = gun;
		aimJoystickCanvasGroup.blocksRaycasts = gun;

		_attackButton.DOFade(gun ? 0 : 1, .2f).SetEase(Ease.InOutSine);
		_attackButton.interactable = !gun;
		_attackButton.blocksRaycasts = !gun;

	}

	public void UpdateLifeBar(float life)
	{
		StartCoroutine(LifeBarUpdateAnimation(life));
	}

	public void ShowCountdown(bool show)
	{
		_timer.transform.DOScale(show ? 1 : 0, .5f).SetEase(Ease.OutSine);
		countDownActive = show;
	}

	private void UpdateCountDown()
	{
		if (!countDownActive)
			return;

		if (countDownStartTime != 0 && countDownDuration != 0)
		{
			double countDownDuration = (PhotonNetwork.Time - countDownStartTime);
			double currentTimer = this.countDownDuration - countDownDuration;

			if (currentTimer <= 0)
			{
				ShowCountdown(false);
				countDownActive = false;
			}

			string minutes = Mathf.Floor((float)currentTimer / 60).ToString("00");
			string seconds = Mathf.Floor((float)currentTimer % 60).ToString("00");

			_timer.text = string.Format("{0}:{1}", minutes, seconds);
		}
	}

	private IEnumerator LifeBarUpdateAnimation(float life)
	{
		float currentLifeBarValue = _lifeBar.value;
		float desiredLifeBarValue = currentLifeBarValue;

		Vector3 startPosition = _lifeBar.transform.position;
		Vector3 desiredPosition = startPosition;

		Quaternion startRotation = _lifeBar.transform.rotation;
		Quaternion desiredRotation = startRotation;

		float elapsedTime = 0;
		while (elapsedTime < _lifeBarAnimDuration)
		{
			desiredLifeBarValue = Mathf.Lerp(currentLifeBarValue, life, elapsedTime / _lifeBarAnimDuration);
			desiredPosition.y += Mathf.Sin(Time.time * _lifeBarSineSpeed) * _lifeBarSineLenght * Time.deltaTime;
			desiredPosition.x += Mathf.Sin(Time.time * _lifeBarSineSpeed) * _lifeBarSineLenght * Time.deltaTime;

			desiredRotation.z += Mathf.Sin(Time.time * _lifeBarSineSpeed / 2) * .2f * Time.deltaTime;


			elapsedTime += Time.deltaTime;

			_lifeBar.transform.rotation = desiredRotation;
			_lifeBar.transform.position = desiredPosition;
			_lifeBar.value = (int)desiredLifeBarValue;
			_lifeText.text = ((int)desiredLifeBarValue).ToString();
			yield return null;
		}
		_lifeBar.transform.rotation = startRotation;
		_lifeBar.transform.position = startPosition;
		_lifeBar.value = life;
		_lifeText.text = life.ToString();
	}

	public void ShowRetryScreen()
	{
		if (PlayerPrefs.HasKey("MAX"))
			_retryMaxScore.text = "MAX SCORE: "  + PlayerPrefs.GetInt("MAX").ToString();
		else
		{
			PlayerPrefs.SetInt("MAX", _zombiesKilled);
			_retryMaxScore.text = "MAX SCORE: " + _zombiesKilled;
		}

		_retryCurrentScore.text = "SCORE: " + _zombiesKilled;
			
		_retryScreen.DOFade(1, .4f);
		_retryScreen.blocksRaycasts = true;
		_retryScreen.interactable = true;
	}

	public void RetryClicked()
	{
		PhotonNetwork.LeaveRoom();
		SceneManager.LoadScene("Lobby");
	}

	public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
	{
		if(propertiesThatChanged.ContainsKey("startTime"))
		{
			countDownStartTime = (float)propertiesThatChanged["startTime"];
			countDownDuration = (float)propertiesThatChanged["duration"];
		}

		if (propertiesThatChanged.ContainsKey("zombiesKilled"))
			_zombiesKilled = (int)propertiesThatChanged["zombiesKilled"];

		if (propertiesThatChanged.ContainsKey("currentHorde"))
			_currentHorde = (int)propertiesThatChanged["currentHorde"];
	}
}
