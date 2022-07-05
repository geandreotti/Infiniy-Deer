using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using NaughtyAttributes;


public class GameManager : MonoBehaviour
{
	public static GameManager Instance;

	public bool online;
	[BoxGroup("Settings")] [SerializeField] private GameObject _playerPrefab;
	[BoxGroup("Settings")] [SerializeField] private GameObject _zombiePrafab;
	[BoxGroup("Settings")] [SerializeField] private GameObject _lootCratePrefab;

	[Space]
	public List<PlayerBehaviour> players = new List<PlayerBehaviour>();

	public Camera mainCamera;


	public void Awake()
	{
		if (Instance)
			Destroy(Instance);
		Instance = this;
	}

	private void Start()
	{
		if (online)
		{ 
			SpawnPlayer();
			//if (PhotonNetwork.IsMasterClient)
			//{
			//	PhotonNetwork.Instantiate(_zombiePrafab.name, new Vector3(5, 0, 5), Quaternion.identity);
			//	PhotonNetwork.Instantiate(_zombiePrafab.name, new Vector3(7, 0, 5), Quaternion.identity);
			//	PhotonNetwork.Instantiate(_zombiePrafab.name, new Vector3(9, 0, 5), Quaternion.identity);
			//	PhotonNetwork.Instantiate(_zombiePrafab.name, new Vector3(9, 0, 3), Quaternion.identity);
			//	PhotonNetwork.Instantiate(_zombiePrafab.name, new Vector3(7, 0, 3), Quaternion.identity);
			//	PhotonNetwork.Instantiate(_zombiePrafab.name, new Vector3(5, 0, 3), Quaternion.identity);
			//	PhotonNetwork.Instantiate(_lootCratePrefab.name, new Vector3(2, 0, 2), Quaternion.identity);
			//	PhotonNetwork.Instantiate(_lootCratePrefab.name, new Vector3(0, 0, 5), Quaternion.identity);
			//	PhotonNetwork.Instantiate(_lootCratePrefab.name, new Vector3(-2, 0, 4), Quaternion.identity);
			//	PhotonNetwork.Instantiate(_lootCratePrefab.name, new Vector3(-5, 0, 2), Quaternion.identity);
			//}
		}
	}

	private void Update()
	{
		
	}

	private void SpawnPlayer()
	{
		GameObject player = PhotonNetwork.Instantiate(_playerPrefab.name, new Vector3(0, .4f, 0), Quaternion.identity);
		mainCamera = player.transform.Find("Main Camera").GetComponent<Camera>();
		player.transform.Find("Main Camera").gameObject.SetActive(true);
		player.transform.Find("CM vcam1").gameObject.SetActive(true);
	}
}
