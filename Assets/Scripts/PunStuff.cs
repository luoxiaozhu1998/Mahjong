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
    [SerializeField] private TMP_InputField ipAddressInputField;
    [SerializeField] private TMP_InputField userNameInputField;
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
    private bool _isStartGameButtonNull;
    private bool _isRoomListContentNull;
    private const string UserNameKey = "UserName";
    private const string IPAddressKey = "IPAddress";

    private void Start()
    {
        _isRoomListContentNull = roomListContent == null;
        _isStartGameButtonNull = startGameButton == null;
    }

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

    private void Update()
    {
        Debug.Log(ipAddressInputField.text);
    }

    public void JoinLobby()
    {
        Debug.Log(ipAddressInputField.text);
        serverSettings.AppSettings.Server = ipAddressInputField.text;
        GameManager.Instance.SetPlayerName(userNameInputField.text);
        PlayerPrefs.SetString(IPAddressKey, ipAddressInputField.text);
        PlayerPrefs.SetString(UserNameKey, userNameInputField.text);
        GameManager.Instance.OpenMenu("LoadingMenu");
        PhotonNetwork.ConnectUsingSettings();
    }

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
#if !UNITY_EDITOR
        if (string.IsNullOrEmpty(roomNameInputField.text))
        {
            return;
        }
#endif
        PhotonNetwork.CreateRoom(roomNameInputField.text, new RoomOptions {MaxPlayers = 4});
        GameManager.Instance.OpenMenu("LoadingMenu");
    }

    /// <summary>
    /// 房主创建房间后，加载麻将
    /// </summary>
    public override void OnCreatedRoom()
    {
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

    /// <summary>
    /// 房间列表信息改变，更新房间列表
    /// </summary>
    /// <param name="roomList">房间列表</param>
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        if (roomList.Count <= 0 || _isRoomListContentNull) return;
        foreach (Transform t in roomListContent)
        {
            Destroy(t.gameObject);
        }

        foreach (var info in roomList)
        {
            if (!info.RemovedFromList)
            {
                Instantiate(roomLIstItemPrefab, roomListContent)
                    .GetComponent<RoomListItem>()
                    .SetUp(info);
            }
        }
    }

    /// <summary>
    /// 新玩家加入房间后，给已经在房间的玩家的玩家列表中生成对应的prefab
    /// </summary>
    /// <param name="newPlayer">新玩家的信息</param>
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Instantiate(playerLIstItemPrefab, playerListContent)
            .GetComponent<PlayerListItem>()
            .Setup(newPlayer);
    }

    /// <summary>
    /// 开始游戏的按钮绑定函数
    /// </summary>
    public void StartGame()
    {
        //只有房主能点击
        GameManager.Instance.OpenMenu("LoadingMenu");
        PhotonNetwork.LoadLevel(2);
    }

    /// <summary>
    /// 当房主切换时，让新房主加载麻将，同时切换开始游戏按钮的隐藏和显示
    /// </summary>
    /// <param name="newMasterClient">新房主</param>
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        var flag = Equals(PhotonNetwork.LocalPlayer, newMasterClient);
        if (flag)
        {
            GameManager.Instance.LoadMahjong();
        }

        if (_isStartGameButtonNull) return;
        startGameButton.gameObject.SetActive(flag);
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