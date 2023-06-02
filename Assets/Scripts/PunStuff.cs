using System;
using System.Collections.Generic;
using System.Linq;
using Manager;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PunStuff : MonoBehaviourPunCallbacks
{
    [Header("Buttons")] public Button startGameButton;
    [Header("UIs")] public TMP_InputField roomNameInputField;
    public TMP_Text roomNameText;
    public TMP_InputField ipAddressInputField;
    public TMP_InputField userNameInputField;
    public Transform roomListContent;
    public Transform playerListContent;
    [Header("Prefabs")] [SerializeField] private GameObject roomLIstItemPrefab;
    [SerializeField] private GameObject playerLIstItemPrefab;
    [Header("Menus")] public GameObject loadingMenu;
    public GameObject titleMenu;
    public GameObject createRoomMenu;
    public GameObject roomMenu;
    public GameObject findRoomMenu;
    public GameObject startMenu;
    private const string LoadingMenuName = "LoadingMenu";
    private const string TitleMenuName = "TitleMenu";
    private const string CreateRoomMenuName = "CreateRoomMenu";
    private const string RoomMenuName = "RoomMenu";
    private const string FindRoomMenuName = "FindRoomMenu";
    private const string StartMenuName = "StartMenu";
    public ServerSettings serverSettings;
    private const string UserNameKey = "UserName";
    private const string IPAddressKey = "IPAddress";

    private void Awake()
    {
        ipAddressInputField.text = PlayerPrefs.GetString(IPAddressKey, "192.168.137.1");
        userNameInputField.text = PlayerPrefs.GetString(UserNameKey, "FuDan-TA-01");
        GameManager.Instance.AddMenu(LoadingMenuName, loadingMenu);
        GameManager.Instance.AddMenu(TitleMenuName, titleMenu);
        GameManager.Instance.AddMenu(CreateRoomMenuName, createRoomMenu);
        GameManager.Instance.AddMenu(RoomMenuName, roomMenu);
        GameManager.Instance.AddMenu(FindRoomMenuName, findRoomMenu);
        GameManager.Instance.AddMenu(StartMenuName, startMenu);
        GameManager.Instance.OpenMenu(!PhotonNetwork.IsConnected ? "StartMenu" : "TitleMenu");
    }

    public void JoinLobby()
    {
        serverSettings.AppSettings.Server = ipAddressInputField.text;
        GameManager.Instance.SetPlayerName(userNameInputField.text);
        PlayerPrefs.SetString(IPAddressKey, ipAddressInputField.text);
        PlayerPrefs.SetString(UserNameKey, userNameInputField.text);
        GameManager.Instance.OpenMenu("LoadingMenu");
        PhotonNetwork.ConnectUsingSettings();
    }

    // public override void OnEnable()
    // {
    //     base.OnEnable();
    //     SceneManager.sceneLoaded += OnSceneLoaded;
    // }
    //
    // public override void OnDisable()
    // {
    //     base.OnDisable();
    //     SceneManager.sceneLoaded -= OnSceneLoaded;
    // }
    //
    // private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    // {
    //     if (scene.buildIndex != 2) return; //we are in the game scene
    //     GameManager.Instance.InitWhenStart();
    //     GameController.Instance.StartGame();
    // }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public override void OnJoinedLobby()
    {
        GameManager.Instance.OpenMenu("TitleMenu");
        Debug.Log("OnJoinedLobby()");
        PhotonNetwork.LocalPlayer.NickName = GameManager.Instance.GetPlayerName();
        //PhotonNetwork.NickName = "Player" + Random.Range(0, 1000).ToString("0000");
    }

    public void CreateRoom()
    {
        // if (string.IsNullOrEmpty(roomNameInputField.text))
        // {
        //     return;
        // }

        PhotonNetwork.CreateRoom(roomNameInputField.text, new RoomOptions {MaxPlayers = 4});
        GameManager.Instance.OpenMenu("LoadingMenu");
        GameManager.Instance.LoadMahjong();
    }

    public override void OnJoinedRoom()
    {
        roomNameText.text = PhotonNetwork.CurrentRoom.Name;
        GameManager.Instance.OpenMenu("RoomMenu");
        foreach (Transform item in playerListContent)
        {
            Destroy(item.gameObject);
        }

        var players = PhotonNetwork.PlayerList;

        foreach (var t in players)
        {
            Instantiate(playerLIstItemPrefab, playerListContent).GetComponent<PlayerListItem>()
                .Setup(t);
        }

        // for (var i = 0; i < players.Length; i++)
        // {
        //     if (PhotonNetwork.IsMasterClient)
        //     {
        //         players[i].NickName = "Fudan-VR-TA" + 1;
        //     }
        //     else
        //     {
        //         players[i].NickName = "Fudan-VR-TA" + 2;
        //     }
        //
        //     Instantiate(playerLIstItemPrefab, _playerListContent).GetComponent<PlayerListItem>()
        //         .Setup(players[i]);
        // }

        startGameButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        GameManager.Instance.OpenMenu("LoadingMenu");
    }

    public override void OnLeftRoom()
    {
        GameManager.Instance.OpenMenu("TitleMenu");
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        if (roomList.Count <= 0 || roomListContent == null) return;
        foreach (Transform t in roomListContent)
        {
            Destroy(t.gameObject);
        }

        foreach (var info in roomList.Where(info => !info.RemovedFromList))
        {
            Instantiate(roomLIstItemPrefab, roomListContent).GetComponent<RoomListItem>()
                .SetUp(info);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Instantiate(playerLIstItemPrefab, playerListContent)
            .GetComponent<PlayerListItem>()
            .Setup(newPlayer);
    }

    public void StartGame()
    {
        //只有房主能点击
        GameManager.Instance.OpenMenu("LoadingMenu");
        PhotonNetwork.LoadLevel(2);
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (startGameButton == null) return;
        if (PhotonNetwork.IsMasterClient)
        {
            GameManager.Instance.LoadMahjong();
        }

        startGameButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
    }

    public void BackToTitleMenu()
    {
        GameManager.Instance.OpenMenu("TitleMenu");
    }

    public void OpenCreateRoomMenu()
    {
        GameManager.Instance.OpenMenu(CreateRoomMenuName);
    }

    public void OpenFindRoomMenu()
    {
        GameManager.Instance.OpenMenu(FindRoomMenuName);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
                UnityEngine.Application.Quit();
#endif
    }
}