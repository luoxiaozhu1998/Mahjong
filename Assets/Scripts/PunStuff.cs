using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Manager;
using Photon.Pun;
using Photon.Realtime;
using PlayFab;
using PlayFab.ClientModels;
using RenderHeads.Media.AVProVideo;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

// using UnityGoogleDrive;
// using UnityGoogleDrive.Data;
//using File = UnityGoogleDrive.Data.File;


public class PunStuff : MonoBehaviourPunCallbacks
{
	[Header("Buttons")] public Button startGameButton;
	[Header("UIs")] public TMP_InputField roomNameInputField;

	public TMP_Text roomNameText;

	//[SerializeField] private TMP_InputField ipAddressInputField;
	[SerializeField] private TMP_InputField userNameInputField;
	[SerializeField] private TMP_InputField emailInputField;
	public Transform roomListContent;
	public Transform playerListContent;
	public Transform recordingsListContent;
	[SerializeField] private TMP_Text userNameText;
	[SerializeField] private TMP_Text scoreText;
	[Header("Prefabs")] [SerializeField] private GameObject roomLIstItemPrefab;
	[SerializeField] private GameObject playerLIstItemPrefab;
	[SerializeField] private GameObject recordingsListItemPrefab;
	[Header("Menus")] public GameObject loadingMenu;
	public GameObject titleMenu;
	public GameObject shopMenu;
	public GameObject bagMenu;
	public GameObject createRoomMenu;
	public GameObject roomMenu;
	public GameObject findRoomMenu;
	public GameObject startMenu;
	public GameObject recordingsMenu;
	public GameObject backgroundMenu;
	public GameObject leaderBoardMenu;

	/// <summary>
	/// 商店中点击卡牌的描述界面
	/// </summary>
	[Header("Panels")] [SerializeField] private GameObject xRayCardDescriptionPanel;

	[SerializeField] private GameObject cheatCardDescriptionPanel;
	[SerializeField] private GameObject resultPanel;
	[SerializeField] private GameObject recordingsDlsplaypanel;

	/// <summary>
	/// 商店中描述价格的文字
	/// </summary>
	[SerializeField] private TMP_Text xRayCardScoreText;

	[SerializeField] private TMP_Text cheatCardScoreText;
	[SerializeField] private TMP_Text resultText;
	private const string LoadingMenuName = "LoadingMenu";
	private const string TitleMenuName = "TitleMenu";
	private const string ShopMenuName = "ShopMenu";
	private const string BagMenuName = "BagMenu";
	private const string CreateRoomMenuName = "CreateRoomMenu";
	private const string RoomMenuName = "RoomMenu";
	private const string FindRoomMenuName = "FindRoomMenu";
	private const string StartMenuName = "StartMenu";
	private const string RecordingsMenuName = "RecordingsMenu";
	private const string LeaderBoardMenuName = "LeaderBoardMenu";
	public ServerSettings serverSettings;
	private bool _isStartGameButtonNull;
	private bool _isRoomListContentNull;
	private const string UserNameKey = "UserName";
	private const string IPAddressKey = "IPAddress";
	[SerializeField] private GazeInteractor gazeInteractor;
	[SerializeField] private GameObject leftRayInteractor;
	[SerializeField] private GameObject rightRayInteractor;
	[SerializeField] private Transform gazeIcon;
	[SerializeField] private Transform bagContentTransform;
	[SerializeField] private GameObject xRayCardPrefab;
	[SerializeField] private GameObject cheatCardPrefab;

	private void Start()
	{
		_isRoomListContentNull = roomListContent == null;
		_isStartGameButtonNull = startGameButton == null;
	}

	private void Awake()
	{
		//ipAddressInputField.text = PlayerPrefs.GetString(IPAddressKey, "192.168.137.1");
		userNameInputField.text = PlayerPrefs.GetString(UserNameKey, "LuoZhu");
		//emailInputField.text=PlayerPrefs.GetString(UserNameKey, "718366079@qq.com");
		GameManager.Instance.AddMenu(LoadingMenuName, loadingMenu);
		GameManager.Instance.AddMenu(TitleMenuName, titleMenu);
		GameManager.Instance.AddMenu(ShopMenuName, shopMenu);
		GameManager.Instance.AddMenu(BagMenuName, bagMenu);
		GameManager.Instance.AddMenu(CreateRoomMenuName, createRoomMenu);
		GameManager.Instance.AddMenu(RoomMenuName, roomMenu);
		GameManager.Instance.AddMenu(FindRoomMenuName, findRoomMenu);
		GameManager.Instance.AddMenu(StartMenuName, startMenu);
		GameManager.Instance.AddMenu(RecordingsMenuName, recordingsMenu);
		GameManager.Instance.AddMenu(LeaderBoardMenuName, leaderBoardMenu);
		GameManager.Instance.OpenMenu(!PhotonNetwork.IsConnected ? StartMenuName : TitleMenuName);
		//如果persistentDataPath/Videos文件夹不存在，则创建
		Directory.CreateDirectory(Application.persistentDataPath + "/Videos");
		// var directoryInfo = new DirectoryInfo(Application.persistentDataPath + "/Videos");
		// var files = directoryInfo.GetFiles("*", SearchOption.AllDirectories);
		// foreach (var file in files)
		// {
		//     file.Delete();
		// }
		// //找到streamingAssetsPath/Recordings下的所有视频
		// var directoryInfo = new DirectoryInfo(Application.streamingAssetsPath + "/Recordings");
		// var files = directoryInfo.GetFiles("*", SearchOption.AllDirectories);
		// //携程拷贝
		// foreach (var file in files)
		// {
		//     if (file.Name.EndsWith(".meta"))
		//     {
		//         continue;
		//     }
		//
		//     StartCoroutine(Copy(file));
		// }

		// foreach (var file in files)
		// {
		//     file.Delete();
		// }
		// StartCoroutine(Copy(Application.streamingAssetsPath + "/Recordings/",
		//     Application.persistentDataPath + "/Videos/",
		//     "movie_005.mp4"));
	}

	private IEnumerator Copy(FileInfo fileInfo)
	{
		//源路径streamingAssetsPath/Recordings/fileInfo.Name
		var sourceFilePath = Application.streamingAssetsPath + $"/Recordings/{fileInfo.Name}";
		//目标路径persistentDataPath/Videos/fileInfo.Name
		var destinationFilePath = Application.persistentDataPath + $"/Videos/{fileInfo.Name}";

		var www = UnityWebRequest.Get(sourceFilePath);
		yield return www.SendWebRequest();

		if (www.error != null)
		{
			Debug.LogError(www.error);
			yield break;
		}

		if (www.isDone)
		{
			var newFile = File.Create(destinationFilePath);

			var data = www.downloadHandler.data;
			newFile.Write(data, 0, data.Length);

			newFile.Flush();
			newFile.Close();
		}

		www.Dispose();

		// if (File.Exists(perPath + fileName)) yield break;
		// Directory.CreateDirectory(perPath);
		//
		// var uwrFile = UnityWebRequest.Get(strPath + fileName);
		// yield return uwrFile.SendWebRequest();
		//
		// if (uwrFile.error != null) yield break;
		// if (uwrFile.isDone)
		// {
		//     var fullName = perPath + fileName;
		//     var newFile = File.Create(fullName);
		//
		//     var data = uwrFile.downloadHandler.data;
		//     newFile.Write(data, 0, data.Length);
		//
		//     newFile.Flush();
		//     newFile.Close();
		// }
		//
		// uwrFile.Dispose();
	}

	public void OpenLeaderBoardMenu()
	{
		GameManager.Instance.OpenMenu(LeaderBoardMenuName);
	}

	public void OpenRecordingsMenu()
	{
		GameManager.Instance.OpenMenu(RecordingsMenuName);
		foreach (Transform item in recordingsListContent)
		{
			Destroy(item.gameObject);
		}

		var path = Application.persistentDataPath + "/Videos";
		if (Directory.Exists(path))
		{
			var directoryInfo = new DirectoryInfo(path);
			var files = directoryInfo.GetFiles("*", SearchOption.AllDirectories);
			var i = 1;
			foreach (var file in files)
			{
				if (file.Name.EndsWith(".meta"))
				{
					continue;
				}

				var nameString = file.Name;
				var year = nameString.Substring(6, 4);
				var month = nameString.Substring(11, 2);
				var day = nameString.Substring(14, 2);
				var hour = nameString.Substring(17, 2);
				var minute = nameString.Substring(20, 2);
				var second = nameString.Substring(23, 2);

				var fileInfo = $"    录像名：录像{i}\n    录制时间：{year}年{month}月{day}日 {hour}:{minute}:{second}";
				var go = Instantiate(recordingsListItemPrefab, recordingsListContent);
				go.GetComponentInChildren<TMP_Text>().text = fileInfo;
				go.GetComponentInChildren<Button>().onClick.AddListener(() =>
				{
					recordingsMenu.transform.GetChild(1).gameObject.SetActive(false);
					recordingsDlsplaypanel.gameObject.SetActive(true);
					var mediaPlayer = recordingsDlsplaypanel.GetComponentInChildren<MediaPlayer>();
					mediaPlayer.OpenMedia(MediaPathType.RelativeToPersistentDataFolder, $"Videos/{file.Name}");
					mediaPlayer.Loop = true;
					backgroundMenu.SetActive(false);
				});
				i++;
			}
		}
		// var request = GoogleDriveFiles.List();
		// request.Fields = new List<string> {"nextPageToken, files(id, name, createdTime)"};
		// request.Q = "name contains 'movie'";
		// request.Send().OnDone += BuildResults;
	}

	// private void BuildResults(FileList fileList)
	// {
	//     foreach (var file in fileList.Files)
	//     {
	//         var fileInfo = $"Name: {file.Name} Created: {file.CreatedTime:dd.MM.yyyy}";
	//         var go = Instantiate(recordingsListItemPrefab, recordingsListContent);
	//         var request = GoogleDriveFiles.Download(file.Id);
	//         request.Send().OnDone += AfterDownload;
	//         go.GetComponentInChildren<TMP_Text>().text = fileInfo;
	//     }
	// }

	public void JoinLobby()
	{
		//serverSettings.AppSettings.Server = ipAddressInputField.text;
		// var content = File.ReadAllBytes("C:/Users/luozhu/Downloads/movie_003.mp4");
		// var file = new UnityGoogleDrive.Data.File
		//     {Name = Path.GetFileName("C:/Users/luozhu/Downloads/movie_003.mp4"), Content = content};
		// var request = GoogleDriveFiles.Create(file);
		// request.Fields = new List<string> {"id", "name", "size", "createdTime"};
		// request.Send();
		// var request = GoogleDriveFiles.Download("1BquCQaUvIR8aSTS2AIbB0-BviZHWL447");
		// request.Fields = new List<string> {"id", "name"};
		//
		// request.Send().OnDone += AfterDownload;
		GameManager.Instance.SetPlayerName(userNameInputField.text);
		//PlayerPrefs.SetString(IPAddressKey, ipAddressInputField.text);
		PlayerPrefs.SetString(UserNameKey, userNameInputField.text);
		GameManager.Instance.OpenMenu(LoadingMenuName);
		// PhotonNetwork.NetworkingClient.LoadBalancingPeer.SerializationProtocolType =
		//     ExitGames.Client.Photon.SerializationProtocol.GpBinaryV16;
		PlayFabClientAPI.LoginWithCustomID(new LoginWithCustomIDRequest
			{
				AuthenticationContext = null,
				CustomId = userNameInputField.text,
				CustomTags = null,
				EncryptedRequest = null,
				InfoRequestParameters = null,
				PlayerSecret = null,
				TitleId = null,
				CreateAccount = true
			},
			obj =>
			{
				if (obj.NewlyCreated)
				{
					PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest
						{
							AuthenticationContext = null,
							CustomTags = null,
							Data = new Dictionary<string, string>
							{
								{"Score", "200"},
								{"XRayCard", "0"},
								{"CheatCard", "0"}
							},
							KeysToRemove = null,
							Permission = null
						}, _ =>
						{
							PlayFabClientAPI.UpdateUserTitleDisplayName(new UpdateUserTitleDisplayNameRequest
								{
									DisplayName = userNameInputField.text
								},
								_ => { PhotonNetwork.ConnectUsingSettings(); },
								OnUpdateUserTitleDisplayNameFailure);
						},
						OnUpdateUserDataRequestFailure);
				}
				else
				{
					PhotonNetwork.ConnectUsingSettings();
				}
			},
			obj => { Debug.LogError(obj.ErrorMessage); });
	}

	// private void AfterDownload(File obj)
	// {
	//     var content = obj.Content;
	//     Debug.Log(obj.Name);
	//     var count = Directory.GetFiles(Application.persistentDataPath).Length + 1;
	//     System.IO.File.WriteAllBytes($"{Application.persistentDataPath}/{count}.mp4", content);
	// }

	private void OnUpdateUserDataRequestFailure(PlayFabError obj)
	{
		Debug.LogError(obj.ErrorMessage);
	}

	private void OnUpdateUserTitleDisplayNameFailure(PlayFabError obj)
	{
		Debug.LogError(obj.ErrorMessage);
	}

	/// <summary>
	/// 处理点击背包按钮的函数
	/// </summary>
	public void OpenBag()
	{
		foreach (Transform item in bagContentTransform)
		{
			Destroy(item.gameObject);
		}

		PlayFabClientAPI.GetUserData(new GetUserDataRequest(),
			data =>
			{
				GameManager.Instance.OpenMenu(BagMenuName);
				var xRayCardCount = int.Parse(data.Data["XRayCard"].Value);
				var cheatCardCount = int.Parse(data.Data["CheatCard"].Value);
				if (xRayCardCount > 0)
				{
					var xRayCardGo = Instantiate(xRayCardPrefab, bagContentTransform);
					var xRayCardGoButton = xRayCardGo.GetComponentInChildren<Button>();
					xRayCardGoButton.onClick.RemoveAllListeners();
					xRayCardGoButton.onClick.AddListener(() => { OpenDescriptionInBag("XRayCard"); });
					xRayCardGo.GetComponentInChildren<TMP_Text>().text = xRayCardCount.ToString();
				}

				if (cheatCardCount > 0)
				{
					var cheatCardGo = Instantiate(cheatCardPrefab, bagContentTransform);
					var cheatCardGoButton = cheatCardGo.GetComponentInChildren<Button>();
					cheatCardGoButton.onClick.RemoveAllListeners();
					cheatCardGoButton.onClick.AddListener(() => { OpenDescriptionInBag("CheatCard"); });
					cheatCardGo.GetComponentInChildren<TMP_Text>().text = cheatCardCount.ToString();
				}
			},
			error => { Debug.Log(error.ErrorMessage); });
	}

	/// <summary>
	/// 处理购买卡牌的请求的函数
	/// </summary>
	/// <param name="cardName">购买的卡牌名称</param>
	public void UpdateCardCount(string cardName)
	{
		PlayFabClientAPI.GetUserData(new GetUserDataRequest(),
			data =>
			{
				var score = int.Parse(data.Data["Score"].Value);
				var cardCount = int.Parse(data.Data[cardName].Value);
				//打开结果界面
				resultPanel.SetActive(true);
				if (cardName == "XRayCard")
				{
					if (score >= 100)
					{
						PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest()
							{
								Data = new Dictionary<string, string>
								{
									{"Score", (score - 100).ToString()},
									{cardName, (cardCount + 1).ToString()}
								}
							}, _ => { resultText.text = $"购买成功\n" + $"当前透视道具数量：{cardCount + 1}"; },
							OnUpdateUserDataRequestFailure);
					}
					//积分不足，购买失败
					else
					{
						resultText.text = $"当前积分不足\n" + $"购买失败";
					}
				}
				else if (cardName == "CheatCard")
				{
					if (score >= 200)
					{
						PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest()
							{
								Data = new Dictionary<string, string>
								{
									{"Score", (score - 200).ToString()},
									{cardName, (cardCount + 1).ToString()}
								}
							}, _ => { resultText.text = $"购买成功\n" + $"当前换牌道具数量：{cardCount + 1}"; },
							OnUpdateUserDataRequestFailure);
					}
					//积分不足，购买失败
					else
					{
						resultText.text = $"当前积分不足\n" + $"购买失败";
					}
				}
			},
			error => { Debug.Log(error.ErrorMessage); });
	}

	/// <summary>
	/// 商城点击透视卡牌弹出的界面
	/// </summary>
	public void OpenCardDescriptionPanel(string cardName)
	{
		if (cardName == "XRayCard")
		{
			PlayFabClientAPI.GetUserData(new GetUserDataRequest(),
				data =>
				{
					xRayCardScoreText.text = $"当前积分余额：{data.Data["Score"].Value}\n" +
					                         $"购买后积分余额：{(int.Parse(data.Data["Score"].Value) - 100).ToString()}";
					xRayCardDescriptionPanel.SetActive(true);
				},
				error => { Debug.Log(error.ErrorMessage); });
		}
		else if (cardName == "CheatCard")
		{
			PlayFabClientAPI.GetUserData(new GetUserDataRequest(),
				data =>
				{
					cheatCardScoreText.text = $"当前积分余额：{data.Data["Score"].Value}\n" +
					                          $"购买后积分余额：{(int.Parse(data.Data["Score"].Value) - 200).ToString()}";
					cheatCardDescriptionPanel.SetActive(true);
				},
				error => { Debug.Log(error.ErrorMessage); });
		}
	}

	/// <summary>
	/// 关闭透视卡牌购买描述界面
	/// </summary>
	public void CloseXRayCardDescriptionPanel()
	{
		xRayCardDescriptionPanel.SetActive(false);
	}

	public void CloseResultPanel()
	{
		PlayFabClientAPI.GetUserData(new GetUserDataRequest(),
			data =>
			{
				resultPanel.SetActive(false);
				xRayCardScoreText.text = $"当前积分余额：{data.Data["Score"].Value}\n" +
				                         $"购买后积分余额：{(int.Parse(data.Data["Score"].Value) - 100).ToString()}";
				cheatCardScoreText.text = $"当前积分余额：{data.Data["Score"].Value}\n" +
				                          $"购买后积分余额：{(int.Parse(data.Data["Score"].Value) - 200).ToString()}";
			},
			error => { Debug.Log(error.ErrorMessage); });
	}

	/// <summary>
	/// 关闭换牌道具购买描述界面
	/// </summary>
	public void CloseCheatCardDescriptionPanel()
	{
		cheatCardDescriptionPanel.SetActive(false);
	}

	public override void OnConnectedToMaster()
	{
		PhotonNetwork.JoinLobby();
		PhotonNetwork.AutomaticallySyncScene = true;
	}

	public override void OnJoinedLobby()
	{
		Debug.Log("OnJoinedLobby()");
		PhotonNetwork.LocalPlayer.NickName = GameManager.Instance.GetPlayerName();
		UpdateUserDataInTitleMenu();
	}

	public void EnableEyeGaze()
	{
		if (!gazeInteractor.enabled)
		{
			gazeInteractor.enabled = true;
			rightRayInteractor.gameObject.SetActive(false);
			leftRayInteractor.gameObject.SetActive(false);
		}
	}

	public void DisableEyeGaze()
	{
		if (gazeInteractor.enabled)
		{
			gazeInteractor.enabled = false;
			rightRayInteractor.gameObject.SetActive(true);
			leftRayInteractor.gameObject.SetActive(true);
			gazeIcon.position = Vector3.zero;
		}
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
		GameManager.Instance.OpenMenu(LoadingMenuName);
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
		GameManager.Instance.OpenMenu(RoomMenuName);
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

	/// <summary>
	/// 打开商店界面
	/// </summary>
	public void OpenShop()
	{
		GameManager.Instance.OpenMenu(ShopMenuName);
	}

	private void OpenDescriptionInBag(string cardName)
	{
		int xRayCardCount;
		int cheatCardCount;
		PlayFabClientAPI.GetUserData(new GetUserDataRequest(),
			data =>
			{
				xRayCardCount = int.Parse(data.Data["XRayCard"].Value);
				cheatCardCount = int.Parse(data.Data["CheatCard"].Value);
				if (cardName == "XRayCard")
				{
					bagMenu.transform.GetChild(3).gameObject.SetActive(true);
					bagMenu.transform.GetChild(3).GetChild(4).GetComponent<TMP_Text>().text = $"当前持有：{xRayCardCount}";
				}
				else if (cardName == "CheatCard")
				{
					bagMenu.transform.GetChild(4).gameObject.SetActive(true);
					bagMenu.transform.GetChild(4).GetChild(4).GetComponent<TMP_Text>().text = $"当前持有：{cheatCardCount}";
				}
			},
			error => { Debug.Log(error.ErrorMessage); });
	}

	public void CloseDescriptionInBag(string cardName)
	{
		switch (cardName)
		{
			case "XRayCard":
				bagMenu.transform.GetChild(3).gameObject.SetActive(false);
				break;
			case "CheatCard":
				bagMenu.transform.GetChild(4).gameObject.SetActive(false);
				break;
		}
	}

	/// <summary>
	/// 离开房间
	/// </summary>
	public void LeaveRoom()
	{
		PhotonNetwork.LeaveRoom();
		GameManager.Instance.OpenMenu(LoadingMenuName);
	}

	/// <summary>
	/// 关闭商店界面
	/// </summary>
	public void LeaveShop()
	{
		CloseResultPanel();
		CloseXRayCardDescriptionPanel();
		CloseCheatCardDescriptionPanel();
		UpdateUserDataInTitleMenu();
	}

	public void CloseRecordingsMenu()
	{
		backgroundMenu.SetActive(true);
		UpdateUserDataInTitleMenu();
	}

	public void CloseBag()
	{
		CloseDescriptionInBag("XRayCard");
		CloseDescriptionInBag("CheatCard");
		UpdateUserDataInTitleMenu();
	}

	public override void OnLeftRoom()
	{
		UpdateUserDataInTitleMenu();
	}

	private void UpdateUserDataInTitleMenu()
	{
		PlayFabClientAPI.GetUserData(new GetUserDataRequest(),
			data =>
			{
				userNameText.text = GameManager.Instance.GetPlayerName();
				scoreText.text = $"积分：{data.Data["Score"].Value}";
				GameManager.Instance.OpenMenu(TitleMenuName);
			},
			error => { Debug.Log(error.ErrorMessage); });
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

		foreach (var info in roomList.Where(info => !info.RemovedFromList))
		{
			Instantiate(roomLIstItemPrefab, roomListContent)
				.GetComponent<RoomListItem>()
				.SetUp(info);
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
		GameManager.Instance.OpenMenu(LoadingMenuName);
		PhotonNetwork.CurrentRoom.IsOpen = false;
		PhotonNetwork.CurrentRoom.IsVisible = false;
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
		UpdateUserDataInTitleMenu();
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