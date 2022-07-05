using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
	public static LobbyManager Instance;

	[SerializeField] private TMPro.TMP_InputField _roomName;

	public event Action<string> joinOrCreateRoomFailed; 

	private void Awake()
	{
		if (Instance)
			Destroy(Instance);
		Instance = this;

		if(ConnectionManager.Instance)
		{
			joinOrCreateRoomFailed += ConnectionManager.Instance.JoinOrCreateRoom;
			ConnectionManager.Instance.joinedRoom += JoinedRoom;
		}
	}

	public void JoinRoomClicked()
	{
		joinOrCreateRoomFailed?.Invoke(_roomName.text);
	}

	private void JoinedRoom()
	{
		ConnectionManager.Instance.PhotonLoadScene("Online");
	}
}
