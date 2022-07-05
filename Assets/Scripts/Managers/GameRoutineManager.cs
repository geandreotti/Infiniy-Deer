using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class GameRoutineManager : MonoBehaviour
{
	public static GameRoutineManager Instance;

	public List<Transform> cratesSpawnsOccupied = new List<Transform>();

	[BoxGroup("Settings")] [SerializeField] private float _lootTimeDuration;
	[BoxGroup("Settings")] [SerializeField] private int _baseHordeZombieQuantity;
	[BoxGroup("Settings")] [SerializeField] private float _hordeSpawnDelayMaxRandom;
	[BoxGroup("Settings")] [SerializeField] private HordeInfo _hordeInfo;

	[BoxGroup("Zombies Spawn")] [SerializeField] private GameObject _zombiePrefab;
	[BoxGroup("Zombies Spawn")] [SerializeField] private Transform[] _zombiesSpawnPoints;

	[BoxGroup("Crates Spawn")] [SerializeField] private GameObject _cratesPrefab;
	[BoxGroup("Crates Spawn")] [SerializeField] private Transform[] _cratesSpawnPoints;

	[BoxGroup("Events")] public static byte STARTGAMEROUTINE_EVENT;

	private PhotonView _photonView;

	private int _currentHorde;
	private int _spawnedZombies;
	private HordeInfo _lastHorde;
	public int hordeDeadZombies;

	private int _killedZombies;

	private Coroutine _hordesRoutine;
	public ExitGames.Client.Photon.Hashtable CustomValue { get; private set; }

	private void Awake()
	{
		if (Instance)
			Destroy(Instance);
		Instance = this;
		TryGetComponent<PhotonView>(out _photonView);
	}

	private void Start()
	{
		
	}

	private void Update()
	{
		
	}

	public void StartGameRoutine()
	{
		if (GameManager.Instance.online)
			if (!PhotonNetwork.IsMasterClient)
				return;

		_hordesRoutine = StartCoroutine(HordeRoutine());
	}

	private IEnumerator HordeRoutine()
	{
		_hordeInfo = GetNextHorde();

		SpawnLootCrates();

		StartCountdown();

		yield return LootTime();

		yield return new WaitForSeconds(2);

		StartCoroutine(ZombieSpawnRoutine());

		while (hordeDeadZombies < _hordeInfo.zombieQuantity)
		{
			yield return null;
		}
		HordeCleared();
	}

	private IEnumerator LootTime()
	{
		float elapsedTime = 0;
		while (elapsedTime < _lootTimeDuration)
		{
			elapsedTime += Time.deltaTime;
			yield return null;
		}
	}

	private IEnumerator ZombieSpawnRoutine()
	{
		Debug.Log("Zombie Spawn routine");
		while (_spawnedZombies < _hordeInfo.zombieQuantity)
		{
			bool multiple = Random.Range(0, 2) == 1 ? true : false;
			Debug.Log("Multiple: " + multiple);
			int quantity;
			if (multiple)
			{
				quantity = Random.Range(2, _zombiesSpawnPoints.Length);
				for (int i = 0; i < quantity; i++)
					SpawnZombie();
			}
			else
				SpawnZombie();
			yield return new WaitForSeconds(_hordeInfo.zombieSpawnDelay + Random.Range(-_hordeSpawnDelayMaxRandom, _hordeSpawnDelayMaxRandom));
		}

	}

	private Transform GetRandomZombieSpawnPoint()
	{
		Transform spawnPoint = _zombiesSpawnPoints[Random.Range(0, _zombiesSpawnPoints.Length)];
		return spawnPoint;
	}



	private Transform GetRandomLootCrateSpawnPoint()
	{
		Transform spawnPoint = _cratesSpawnPoints[Random.Range(0, _cratesSpawnPoints.Length)];
		if (cratesSpawnsOccupied.Contains(spawnPoint))
			spawnPoint = GetRandomLootCrateSpawnPoint();
		else
			cratesSpawnsOccupied.Add(spawnPoint);
		return spawnPoint;
	}

	private void SpawnZombie()
	{
		if (_spawnedZombies >= _hordeInfo.zombieQuantity)
			return;
		Transform spawnPoint = GetRandomZombieSpawnPoint();

		if (GameManager.Instance.online)
		{
			if (!PhotonNetwork.IsMasterClient)
				return;

			GameObject zombie = PhotonNetwork.Instantiate(_zombiePrefab.name, spawnPoint.position, Quaternion.identity);
			_spawnedZombies++;
			return;
		}

		_spawnedZombies++;
		Instantiate(_zombiePrefab, spawnPoint.position, Quaternion.identity);
	}

	private void SpawnLootCrates()
	{
		for (int i = 0; i < _hordeInfo.crateMeleeQuantity; i++)
		{
			Transform spawnPoint = GetRandomLootCrateSpawnPoint();

			Quaternion rotation = Quaternion.Euler(0, Random.Range(-180, 180), 0);
			GameObject crate;
			if (GameManager.Instance.online)
				crate = PhotonNetwork.Instantiate(_cratesPrefab.name, spawnPoint.position, rotation).gameObject;
			else
				crate = Instantiate(_cratesPrefab, spawnPoint.position, rotation);

			crate.GetComponent<LootCrate>().lootType = CrateLoot.melee;
		}
	}

	public void ZombieKilled()
	{
		hordeDeadZombies++;
		_killedZombies++;
		ExitGames.Client.Photon.Hashtable CustomValue = new ExitGames.Client.Photon.Hashtable();
		CustomValue.Add("zombiesKilled", _killedZombies);
		PhotonNetwork.CurrentRoom.SetCustomProperties(CustomValue);
	}

	private HordeInfo GetNextHorde()
	{
		HordeInfo horde = new HordeInfo();

		int zombieQuantity;
		float zombieSpawnDelay;
		if (_lastHorde != null)
		{
			zombieQuantity = _lastHorde.zombieQuantity;
			zombieSpawnDelay = _lastHorde.zombieSpawnDelay;
			horde.crateMeleeQuantity = 3;
			zombieQuantity += 5;

			if (_currentHorde >= 5)
				if (zombieSpawnDelay <= .5f)
					zombieSpawnDelay -= .2f;
		}
		else
		{
			//FIRST HORDE
			_currentHorde = 1;
			zombieQuantity = _baseHordeZombieQuantity;
			zombieSpawnDelay = 2;
			horde.crateMeleeQuantity = 3;
			horde.crateWeaponQuantity = 0;
		}

		horde.zombieQuantity = zombieQuantity;
		horde.zombieSpawnDelay = zombieSpawnDelay;
		return horde;
	}

	private void HordeCleared()
	{
		_spawnedZombies = 0;
		_currentHorde++;
		ExitGames.Client.Photon.Hashtable CustomValue = new ExitGames.Client.Photon.Hashtable();
		CustomValue.Add("currentHorde", _currentHorde);
		PhotonNetwork.CurrentRoom.SetCustomProperties(CustomValue);
		hordeDeadZombies = 0;
		_lastHorde = _hordeInfo;
		_lootTimeDuration = 0;
		StartCoroutine(HordeRoutine());
	}

	private float _startTime;
	private void StartCountdown()
	{
		if (GameManager.Instance.online)
		{
			if (!PhotonNetwork.IsMasterClient)
				return;

			ExitGames.Client.Photon.Hashtable CustomValue = new ExitGames.Client.Photon.Hashtable();
			_startTime = (float)PhotonNetwork.Time;
			CustomValue.Add("startTime", _startTime);
			CustomValue.Add("duration", _lootTimeDuration);
			PhotonNetwork.CurrentRoom.SetCustomProperties(CustomValue);

			if (_photonView)
				_photonView.RPC("ShowTimer", RpcTarget.All, true);

			return;
		}

		UIManager.Instance.countDownStartTime = Time.time;
		UIManager.Instance.countDownDuration = _lootTimeDuration;
		UIManager.Instance.ShowCountdown(true);
	}

	[PunRPC]
	private void ShowTimer(bool show)
	{
		if (UIManager.Instance)
			UIManager.Instance.ShowCountdown(show);
	}
}



[System.Serializable]
public class HordeInfo
{
	public int zombieQuantity;
	public float zombieSpawnDelay;
	public int crateMeleeQuantity;
	public int crateWeaponQuantity;
}
