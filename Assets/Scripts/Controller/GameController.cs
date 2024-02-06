using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Manager;
using Newtonsoft.Json;
using Oculus.Avatar2;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using Oculus.Interaction.Input;
using Photon.Pun;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = System.Random;

namespace Controller
{
    /// <summary>
    /// 负责管理整个游戏的逻辑,单例
    /// </summary>
    public class GameController : MonoBehaviourPunCallbacks
    {
        public static GameController Instance { get; private set; }
        private int _playerCount;
        [HideInInspector] public PlayerController myPlayerController;
        public Dictionary<int, int> ReadyDict;
        [HideInInspector] public int nowTurn;
        [HideInInspector] public int nowTile;
        [HideInInspector] public int tileViewID;
        private List<MahjongAttr> _mahjong;
        [SerializeField] private Transform[] playerCanvases;
        [SerializeField] private Transform[] playerCardContainers;
        [SerializeField] private Transform[] playerButtonContainers;
        [SerializeField] private Transform[] playerResultPanels;

        [Header("-------------麻将前面显示玩家积分的Text----------------"), SerializeField]
        private TMP_Text[] playerScoreTexts;

        private bool _canPong;
        private bool _canKong;
        private bool _canWin;
        private Button _confirmButton;
        public Material[] transparentMaterials;
        public Material[] normalMaterials;
        [SerializeField] private GameObject WinnerHat;

        [SerializeField] private GameObject LoserNose;
        public GameObject effectPrefab;
        public GameObject bubbleEffect;
        public Transform[] playerPanelContainers;
        public GameObject playerPanelPrefab;
        [SerializeField] private GazeInteractor GazeInteractor;
        private static Random rng = new();
        public GameObject nowMahjong;
        public AudioTrigger changeMahjongAudio;

        [SerializeField, Interface(typeof(IHand))]
        public MonoBehaviour _leftHand;

        [SerializeField, Interface(typeof(IHand))]
        public MonoBehaviour _rightHand;

        [FormerlySerializedAs("_transformer")]
        [SerializeField, Interface(typeof(ITrackingToWorldTransformer))]
        [Tooltip("Transformer is required so calculations can be done in Tracking space")]
        public UnityEngine.Object _transformer;

        public OVRCameraRig OvrCameraRig;
        private int _playerDictCount;
        [HideInInspector] public List<MahjongAttr> effectGoList = new();
        // [SerializeField] private RecorderControllerSettingsPreset _recorderControllerSettingsPreset;
        // private RecorderController _recorderController;

        /// <summary>
        /// 初始化
        /// </summary>
        private void Awake()
        {
            if (Instance)
            {
                Destroy(gameObject);
            }

            Instance = this;
            GameManager.Instance.InitWhenStart();
            _playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
            ReadyDict = new Dictionary<int, int>();
            _mahjong = new List<MahjongAttr>();
            //playerButtons = new List<Transform>();
            StartGame();
            FindObjectOfType<OvrAvatarManager>().GetComponent<AvatarInputManager>().Init();
        }

        // public override void OnEnable()
        // {
        //     base.OnEnable();
        //     var controllerSettings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
        //     _recorderControllerSettingsPreset.ApplyTo(controllerSettings);
        //     _recorderController = new RecorderController(controllerSettings);
        //     RecorderOptions.VerboseMode = false;
        //     _recorderController.PrepareRecording();
        //     _recorderController.StartRecording();
        //     StartCoroutine(nameof(StopRecording));
        // }
        //
        // private void StopRecording()
        // {
        //     _recorderController.StopRecording();
        // }

        public override void OnLeftRoom()
        {
            PhotonNetwork.LoadLevel(1);
        }

        /// <summary>
        /// 开始游戏
        /// </summary>
        public void StartGame()
        {
            GeneratePlayers();
            //房主检测是否天胡
            var isWin = PhotonNetwork.IsMasterClient && CheckWin();
            //所有玩家检测是否能杠
            foreach (var pair in myPlayerController.MyMahjong)
            {
                if (pair.Value.Count == 4)
                {
                    if (isWin)
                    {
                        photonView.RPC(nameof(CanKAndH), RpcTarget.All, myPlayerController.playerID);
                        break;
                    }

                    photonView.RPC(nameof(CanK), RpcTarget.All, myPlayerController.playerID);
                    break;
                }
            }

            GetPlayerScore();
        }

        [PunRPC]
        private void SetList(string a, string b)
        {
            GameManager.Instance.SetMahjongList(JsonConvert.DeserializeObject<List<Mahjong>>(a));
            GameManager.Instance.SetUserMahjongLists(
                JsonConvert.DeserializeObject<List<List<Mahjong>>>(b));
            var count = 14 + (PhotonNetwork.CurrentRoom.PlayerCount - 1) * 13;
            _mahjong = FindObjectsOfType<MahjongAttr>().ToList();
            _mahjong.Sort((nameA, nameB) =>
                int.Parse(nameA.gameObject.name.Split('_')[2])
                    .CompareTo(int.Parse(nameB.gameObject.name.Split('_')[2])));
            for (var i = 0; i < count; i++)
            {
                Destroy(_mahjong[i].gameObject);
            }

            _mahjong.RemoveRange(0, count);
            var mahjongList = GameManager.Instance.GetMahjongList();
            var length = _mahjong.Count;
            for (var i = 0; i < length; i++)
            {
                _mahjong[i].GetComponent<MeshFilter>().mesh =
                    GameManager.Instance.GetMahjongMesh(mahjongList[i].ID);
                var rb = _mahjong[i].GetComponent<Rigidbody>();
                var attr = _mahjong[i].GetComponent<MahjongAttr>();
                attr.ID = mahjongList[i].ID;
                rb.constraints = RigidbodyConstraints.FreezeAll;
                _mahjong[i].GetComponent<HandGrabInteractable>().enabled = false;
                _mahjong[i].GetComponent<TouchHandGrabInteractable>().enabled = false;
            }

            //生成当前玩家的麻将有序字典
            myPlayerController.MyMahjong =
                GameManager.Instance.GenerateMahjongAtStart(myPlayerController.playerID - 1);
            SortMyMahjong(true, false);
            // for (var i = 1; i <= 4; i++)
            // {
            //     playerButtons.Add(GameObject.Find("Player" + i + "Button").transform);
            // }
            var pongButton = playerButtonContainers[myPlayerController.playerID - 1].GetChild(0).GetChild(0);
            pongButton.GetComponent<InteractableUnityEventWrapper>().WhenUnselect.AddListener(SolvePong);
            var kongButton = playerButtonContainers[myPlayerController.playerID - 1].GetChild(1).GetChild(0);
            kongButton.GetComponent<InteractableUnityEventWrapper>().WhenUnselect.AddListener(SolveKong);
            var winButton = playerButtonContainers[myPlayerController.playerID - 1].GetChild(2).GetChild(0);
            winButton.GetComponent<InteractableUnityEventWrapper>().WhenUnselect.AddListener(SolveWin);
            var skipButton = playerButtonContainers[myPlayerController.playerID - 1].GetChild(3).GetChild(0);
            skipButton.GetComponent<InteractableUnityEventWrapper>().WhenUnselect.AddListener(SolveSkip);
            var leaveButton = playerResultPanels[myPlayerController.playerID - 1].GetChild(2).GetChild(0);
            leaveButton.GetComponent<InteractableUnityEventWrapper>().WhenUnselect
                .AddListener(() => { PhotonNetwork.LeaveRoom(); });
            // foreach (var playerButton in playerButtonContainers)
            // {
            //     for (var i = 0; i < 5; i++)
            //     {
            //         playerButton.GetChild(i).gameObject.SetActive(false);
            //     }
            //
            //     playerButton.GetChild(6).gameObject.SetActive(false);
            // }
            nowTurn = 1;
        }

        private void GetPlayerScore()
        {
            PlayFabClientAPI.GetUserData(new GetUserDataRequest(),
                data =>
                {
                    photonView.RPC(nameof(UpdatePoint), RpcTarget.All, myPlayerController.playerID,
                        int.Parse(data.Data["Score"].Value));
                },
                error => { Debug.Log(error.ErrorMessage); });
        }

        [PunRPC]
        private void UpdatePoint(int id, int point)
        {
            playerScoreTexts[id - 1].text = $"分数：{point.ToString()}";
        }

        [PunRPC]
        private void SetPoint(int id, string userName)
        {
            if (PhotonNetwork.LocalPlayer.NickName == userName)
            {
                var go = PhotonNetwork.Instantiate("m_CrownHat02", Vector3.zero, quaternion.identity);
                go.transform.SetParent(myPlayerController.transform.GetChild(0));
                go.transform.SetLocalPositionAndRotation(new Vector3(0.15f, 0.02f, 0f), Quaternion.Euler(0f, 0f, -90f));
            }
            else
            {
                var go = PhotonNetwork.Instantiate("NasoClown", Vector3.zero, quaternion.identity);
                go.transform.SetParent(myPlayerController.transform.GetChild(0));
                go.transform.SetLocalPositionAndRotation(new Vector3(0.04f, 0.16f, 0f), Quaternion.Euler(-90f, 0f, 0f));
            }


            OpenPlayerResultPanel();
            // var scoreCanvas = playerCanvases[myPlayerController.playerID - 1].GetChild(4).gameObject;
            // scoreCanvas.SetActive(true);
            // scoreCanvas.transform.GetChild(1).GetComponentInChildren<TMP_Text>().text =
            //     id == myPlayerController.playerID ? "您赢了！" : "您输了！";
            playerResultPanels[myPlayerController.playerID - 1].GetChild(0).GetChild(0).GetComponent<TMP_Text>().text =
                id == myPlayerController.playerID ? "您赢了！" : "您输了！";
            for (var i = 0; i < 4; i++)
            {
                if (i < PhotonNetwork.CurrentRoom.PlayerCount)
                {
                    playerResultPanels[myPlayerController.playerID - 1].GetChild(1).GetChild(i).gameObject
                        .SetActive(true);
                    playerResultPanels[myPlayerController.playerID - 1].GetChild(1).GetChild(i).GetChild(0)
                        .GetComponent<TMP_Text>()
                        .text = PhotonNetwork.CurrentRoom.Players[i + 1].NickName;
                    playerResultPanels[myPlayerController.playerID - 1].GetChild(1).GetChild(i).GetChild(1)
                        .GetComponent<TMP_Text>()
                        .text = PhotonNetwork.CurrentRoom.Players[i + 1].NickName == userName ? "积分 + 50" : "积分 - 50";
                }
                else
                {
                    playerResultPanels[myPlayerController.playerID - 1].GetChild(1).GetChild(i).gameObject
                        .SetActive(false);
                }
            }

            // foreach (var item in PhotonNetwork.CurrentRoom.Players)
            // {
            //     var go = Instantiate(playerPanelPrefab, playerPanelContainers[myPlayerController.playerID - 1]);
            //     go.transform.GetChild(0).GetComponent<TMP_Text>().text = item.Value.NickName;
            //     go.transform.GetChild(1).GetComponent<TMP_Text>().text =
            //         item.Value.NickName == userName ? "Score + 50" : "Score - 50";
            // }

            int score;
            PlayFabClientAPI.GetUserData(new GetUserDataRequest { }, data =>
                {
                    score = int.Parse(data.Data["Score"].Value);
                    score = id == myPlayerController.playerID ? score + 50 : score - 50;
                    PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest()
                        {
                            Data = new Dictionary<string, string>
                            {
                                {"Score", score.ToString()},
                            }
                        },
                        _ => { GetPlayerScore(); },
                        _ => { });
                },
                error => { Debug.Log(error.ErrorMessage); });
        }

        private void OpenPlayerResultPanel()
        {
            playerCanvases[myPlayerController.playerID - 1].gameObject.SetActive(true);
            playerResultPanels[myPlayerController.playerID - 1].gameObject.SetActive(true);
            playerButtonContainers[myPlayerController.playerID - 1].gameObject.SetActive(false);
            playerCardContainers[myPlayerController.playerID - 1].gameObject.SetActive(false);
        }

        private void ClosePlayerResultPanel()
        {
            playerCanvases[myPlayerController.playerID - 1].gameObject.SetActive(false);
        }

        private void OpenPlayerButtonContainer()
        {
            playerCanvases[myPlayerController.playerID - 1].gameObject.SetActive(true);
            playerResultPanels[myPlayerController.playerID - 1].gameObject.SetActive(false);
            playerButtonContainers[myPlayerController.playerID - 1].gameObject.SetActive(true);
            playerCardContainers[myPlayerController.playerID - 1].gameObject.SetActive(false);
        }

        private void ClosePlayerButtonContainer()
        {
            playerCanvases[myPlayerController.playerID - 1].gameObject.SetActive(false);
            playerResultPanels[myPlayerController.playerID - 1].gameObject.SetActive(false);
            playerButtonContainers[myPlayerController.playerID - 1].gameObject.SetActive(false);
            playerCardContainers[myPlayerController.playerID - 1].gameObject.SetActive(false);
        }

        private void OpenPlayerCardContainer()
        {
            //通过PlayFab获取道具数量，显示在UI上
            PlayFabClientAPI.GetUserData(new GetUserDataRequest(),
                //在CallBack函数中打开UI，防止提前打开UI
                data =>
                {
                    //找到两张牌的数量
                    var xRayCardCount = int.Parse(data.Data["XRayCard"].Value);
                    var cheatCardCount = int.Parse(data.Data["CheatCard"].Value);
                    //打开所有的UI
                    playerCanvases[myPlayerController.playerID - 1].gameObject.SetActive(true);
                    playerResultPanels[myPlayerController.playerID - 1].gameObject.SetActive(false);
                    playerButtonContainers[myPlayerController.playerID - 1].gameObject.SetActive(false);
                    playerCardContainers[myPlayerController.playerID - 1].gameObject.SetActive(true);
                    //更新数量
                    var xRayCardGo = playerCardContainers[myPlayerController.playerID - 1].GetChild(0);
                    xRayCardGo.GetComponentInChildren<TMP_Text>().text = $"透视道具\n当前拥有：{xRayCardCount}个";
                    var cheatCardGo = playerCardContainers[myPlayerController.playerID - 1].GetChild(1);
                    cheatCardGo.GetComponentInChildren<TMP_Text>().text = $"换牌道具\n当前拥有：{cheatCardCount}个";
                },
                error => { Debug.Log(error.ErrorMessage); });
        }

        private void ClosePlayerCardContainer()
        {
            playerCanvases[myPlayerController.playerID - 1].gameObject.SetActive(false);
            playerResultPanels[myPlayerController.playerID - 1].gameObject.SetActive(false);
            playerButtonContainers[myPlayerController.playerID - 1].gameObject.SetActive(false);
            playerCardContainers[myPlayerController.playerID - 1].gameObject.SetActive(false);
        }

        private void SolveWin()
        {
            photonView.RPC(nameof(SetPoint), RpcTarget.All, myPlayerController.playerID,
                PhotonNetwork.LocalPlayer.NickName);
        }

        /// <summary>
        /// 因为可能有多个玩家同时可以处理牌，当一个玩家点击跳过，可操作玩家数--，当可操作玩家数等于0，才发牌
        /// </summary>
        private void SolveSkip()
        {
            if (!_canKong && !_canPong && !_canWin) return;
            photonView.RPC(nameof(DecreasePlayerDictCount), RpcTarget.MasterClient);
            _canKong = _canPong = _canWin = false;
            //点击跳过，只是我跳过
            ResetButton();
            EnableHandGrab();
        }

        [PunRPC]
        private void DecreasePlayerDictCount()
        {
            _playerDictCount--;
            if (_playerDictCount == 0)
            {
                photonView.RPC(nameof(NextTurn), RpcTarget.All, nowTurn, false);
            }
        }

        [PunRPC]
        private void ResetButton()
        {
            //隐藏碰
            playerButtonContainers[myPlayerController.playerID - 1].GetChild(0).gameObject.SetActive(true);
            //隐藏杠
            playerButtonContainers[myPlayerController.playerID - 1].GetChild(1).gameObject.SetActive(true);
            //隐藏胡
            playerButtonContainers[myPlayerController.playerID - 1].GetChild(2).gameObject.SetActive(true);
            //隐藏跳过
            playerButtonContainers[myPlayerController.playerID - 1].GetChild(3).gameObject.SetActive(true);
            ClosePlayerButtonContainer();
        }

        private void AddMahjongToHand(MahjongAttr attr)
        {
            attr.inMyHand = true;
            attr.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
            attr.num = 1;
            attr.inOthersHand = false;
            attr.photonView.RPC(nameof(attr.RPCSetInMyHand), RpcTarget.Others, false);
            attr.photonView.RPC(nameof(attr.RPCSetInOthersHand), RpcTarget.Others, true);
            attr.photonView.RPC(nameof(attr.RPCSetOnDesk), RpcTarget.All, false);
            attr.photonView.RPC(nameof(attr.RPCSetIsThrown), RpcTarget.All, false);
            attr.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
            attr.photonView.RPC(nameof(attr.RPCSetLayer), RpcTarget.Others, LayerMask.NameToLayer("Mahjong"));

            //新牌，把所有监听事件移除，然后添加监听事件
            attr.pointableUnityEventWrapper.WhenUnselect.RemoveAllListeners();
            attr.pointableUnityEventWrapper.WhenSelect.RemoveAllListeners();
            attr.pointableUnityEventWrapper.WhenUnselect.AddListener(attr.OnPut);
            attr.pointableUnityEventWrapper.WhenSelect.AddListener(attr.OnGrab);
            attr.GetComponent<HandGrabInteractable>().enabled = true;
            photonView.RPC(nameof(RemoveMahjong), RpcTarget.All);

            if (!myPlayerController.MyMahjong.ContainsKey(attr.ID))
            {
                myPlayerController.MyMahjong[attr.ID] = new List<GameObject>();
            }

            var canKong = false;
            if (myPlayerController.MyMahjong[attr.ID].Count == 3)
            {
                DisableHandGrab();
                //可以杠
                canKong = true;
            }

            //自摸
            if (CheckWin(attr.ID))
            {
                //自摸的同时可以杠
                //自摸的同时不能杠
                photonView.RPC(canKong ? nameof(CanKAndH) : nameof(CanH), RpcTarget.All, myPlayerController.playerID);
            }

            myPlayerController.MyMahjong[attr.ID].Add(attr.gameObject);
        }

        [PunRPC]
        private void RemoveMahjong()
        {
            _mahjong.RemoveAt(0);
        }

        [PunRPC]
        public void NextTurn(int id, bool needDrawTile)
        {
            nowTurn = id;
            // My Turn
            if (myPlayerController.playerID == id)
            {
                // needDrawTile
                if (needDrawTile)
                {
                    _mahjong[0].GetComponent<PhotonView>()
                        .TransferOwnership(PhotonNetwork.LocalPlayer);

                    DOTween.Sequence().Insert(0f, _mahjong[0].transform.DOMove(myPlayerController.putPos, 1f))
                        .Insert(0f,
                            _mahjong[0].transform.DORotate(GameManager.Instance.GetRotateList()[id - 1], 1f))
                        .SetEase(Ease.Linear)
                        .onComplete += SolveMahjong;
                }
            }
        }

        private void SolveMahjong()
        {
            var id = _mahjong[0].ID;
            _mahjong[0].GetComponent<Rigidbody>().Sleep();
            AddMahjongToHand(_mahjong[0]);
            var idx = 1;
            foreach (var item in myPlayerController.MyMahjong)
            {
                foreach (var iGameObject in item.Value)
                {
                    var script = iGameObject.GetComponent<MahjongAttr>();
                    if (script.num == 0)
                    {
                        continue;
                    }

                    script.num = idx++;
                }
            }

            myPlayerController.mahjongMap[myPlayerController.MyMahjong[id].Count - 1].Remove(id);
            myPlayerController.mahjongMap[myPlayerController.MyMahjong[id].Count].Add(id);
        }

        /// <summary>
        /// 生成n个玩家
        /// </summary>
        private void GeneratePlayers()
        {
            var players = PhotonNetwork.CurrentRoom.Players;
            var index = 1 +
                        players.Count(player => player.Key < PhotonNetwork.LocalPlayer.ActorNumber);
            foreach (var playerController in from player in players
                     where player.Value.IsLocal
                     select GameManager.Instance.GeneratePlayer(index - 1)
                         .GetComponent<PlayerController>())
            {
                myPlayerController = playerController;
                myPlayerController.playerID = index;
                myPlayerController.putPos =
                    GameManager.Instance.GetNewPositions()[myPlayerController.playerID - 1];
                if (!PhotonNetwork.IsMasterClient) continue;
                GameManager.Instance.MahjongSplit(players.Count);
                var a = JsonConvert.SerializeObject(GameManager.Instance.GetMahjongList());
                var b = JsonConvert.SerializeObject(GameManager.Instance.GetUserMahjongLists());
                for (var i = 1; i <= 4; i++)
                {
                    if (i == myPlayerController.playerID)
                    {
                        continue;
                    }

                    GameObject.Find("PickPos" + i).SetActive(false);
                }

                photonView.RPC(nameof(SetList), RpcTarget.All, a, b);
            }
        }

        /// <summary>
        /// 让主客户端每回合处理牌
        /// 0：无操作
        /// 1：碰
        /// 2：杠
        /// 3：胡
        /// 4：碰/杠
        /// 5：碰/赢
        /// 6：杠/赢
        /// 7：碰/杠/赢
        /// </summary>
        private void Update()
        {
            //按下有手柄的A等于做出旋转手势
            if (OVRInput.GetActiveController() == OVRInput.Controller.Touch && OVRInput.GetDown(OVRInput.Button.One))
            {
                SortMyMahjong(true, false);
            }

            //按下右手柄的B等于做出打开菜单的手势
            if (OVRInput.GetActiveController() == OVRInput.Controller.Touch && OVRInput.GetDown(OVRInput.Button.Two))
            {
                OpenOrClosePlayerCanvas();
            }

            if (!PhotonNetwork.IsMasterClient) return;
            //所有玩家在某人打出牌之后向主客户端汇报自己的状态（能否碰/杠/胡牌）
            //当字典的count等于玩家count，主客户端开始处理，否则锁死所有客户端
            if (ReadyDict.Count != _playerCount) return;
            //记录有多少玩家可以处理牌

            foreach (var item in ReadyDict)
            {
                switch (item.Value)
                {
                    case 0:
                        continue;
                    //该客户端可以处理牌
                    //给他处理
                    //可以碰牌
                    case 1:
                        photonView.RPC(nameof(CanP), RpcTarget.All, item.Key);
                        break;
                    //可以杠牌
                    case 2:
                        photonView.RPC(nameof(CanK), RpcTarget.All, item.Key);
                        break;
                    //可以胡牌
                    case 3:
                        photonView.RPC(nameof(CanH), RpcTarget.All, item.Key);
                        break;
                    case 4:
                        photonView.RPC(nameof(CanPAndK), RpcTarget.All, item.Key);
                        break;
                    //碰且赢
                    case 5:
                        photonView.RPC(nameof(CanPAndH), RpcTarget.All, item.Key);
                        break;
                    case 6:
                        photonView.RPC(nameof(CanKAndH), RpcTarget.All, item.Key);
                        break;
                    case 7:
                        photonView.RPC(nameof(CanPAndKAndH), RpcTarget.All, item.Key);
                        break;
                }

                //只要有一个人可以处理牌，就不应该继续发牌
                _playerDictCount++;
            }

            // 清空字典，准备下一回合
            ReadyDict.Clear();
            // 只要有一个人可以处理牌，就不应该继续发牌
            if (_playerDictCount > 0) return;
            // 牌打完了，荒庄
            if (GameManager.Instance.GetMahjongList().Count == 0)
            {
                photonView.RPC(nameof(NoOneWin), RpcTarget.All);
            }
            else
            {
                //下一回合，给下一位发牌
                photonView.RPC(nameof(NextTurn), RpcTarget.All,
                    nowTurn, true);
            }
        }

        private void DisableHandGrab()
        {
            foreach (var pair in myPlayerController.MyMahjong)
            {
                foreach (var mahjong in pair.Value)
                {
                    mahjong.GetComponent<HandGrabInteractable>().enabled = false;
                }
            }
        }

        private void EnableHandGrab()
        {
            foreach (var pair in myPlayerController.MyMahjong)
            {
                foreach (var mahjong in pair.Value)
                {
                    mahjong.GetComponent<HandGrabInteractable>().enabled = true;
                }
            }
        }

        [PunRPC]
        private void CanP(int id)
        {
            if (myPlayerController.playerID != id) return;
            OpenPlayerButtonContainer();
            playerButtonContainers[myPlayerController.playerID - 1].GetChild(0).gameObject.SetActive(true);
            playerButtonContainers[myPlayerController.playerID - 1].GetChild(3).gameObject.SetActive(true);
            // playerButtonContainers[id - 1].GetChild(0).gameObject.SetActive(true);
            // playerButtonContainers[id - 1].GetChild(3).gameObject.SetActive(true);
            //if (myPlayerController.playerID != id) return;
            _canPong = true;
            DisableHandGrab();
        }

        [PunRPC]
        private void CanK(int id)
        {
            if (myPlayerController.playerID != id) return;
            OpenPlayerButtonContainer();
            playerButtonContainers[myPlayerController.playerID - 1].GetChild(1).gameObject.SetActive(true);
            playerButtonContainers[myPlayerController.playerID - 1].GetChild(3).gameObject.SetActive(true);
            // playerButtonContainers[id - 1].GetChild(1).gameObject.SetActive(true);
            // playerButtonContainers[id - 1].GetChild(3).gameObject.SetActive(true);
            //if (myPlayerController.playerID != id) return;
            _canKong = true;
            DisableHandGrab();
        }

        [PunRPC]
        private void CanH(int id)
        {
            if (myPlayerController.playerID != id) return;
            OpenPlayerButtonContainer();
            playerButtonContainers[myPlayerController.playerID - 1].GetChild(2).gameObject.SetActive(true);
            playerButtonContainers[myPlayerController.playerID - 1].GetChild(3).gameObject.SetActive(true);
            // playerButtonContainers[id - 1].GetChild(2).gameObject.SetActive(true);
            // playerButtonContainers[id - 1].GetChild(3).gameObject.SetActive(true);
            // if (myPlayerController.playerID != id) return;
            _canWin = true;
        }

        [PunRPC]
        private void CanPAndK(int id)
        {
            if (myPlayerController.playerID != id) return;
            OpenPlayerButtonContainer();
            playerButtonContainers[myPlayerController.playerID - 1].GetChild(0).gameObject.SetActive(true);
            playerButtonContainers[myPlayerController.playerID - 1].GetChild(1).gameObject.SetActive(true);
            playerButtonContainers[myPlayerController.playerID - 1].GetChild(3).gameObject.SetActive(true);
            // playerButtonContainers[id - 1].GetChild(0).gameObject.SetActive(true);
            // playerButtonContainers[id - 1].GetChild(1).gameObject.SetActive(true);
            // playerButtonContainers[id - 1].GetChild(3).gameObject.SetActive(true);
            // if (myPlayerController.playerID != id) return;
            _canPong = true;
            _canKong = true;
            DisableHandGrab();
        }

        [PunRPC]
        private void CanPAndH(int id)
        {
            if (myPlayerController.playerID != id) return;
            OpenPlayerButtonContainer();
            playerButtonContainers[myPlayerController.playerID - 1].GetChild(0).gameObject.SetActive(true);
            playerButtonContainers[myPlayerController.playerID - 1].GetChild(2).gameObject.SetActive(true);
            playerButtonContainers[myPlayerController.playerID - 1].GetChild(3).gameObject.SetActive(true);
            // playerButtonContainers[id - 1].GetChild(0).gameObject.SetActive(true);
            // playerButtonContainers[id - 1].GetChild(2).gameObject.SetActive(true);
            // playerButtonContainers[id - 1].GetChild(3).gameObject.SetActive(true);
            // if (myPlayerController.playerID != id) return;
            _canPong = true;
            _canWin = true;
            DisableHandGrab();
        }

        [PunRPC]
        private void CanKAndH(int id)
        {
            if (myPlayerController.playerID != id) return;
            OpenPlayerButtonContainer();
            playerButtonContainers[myPlayerController.playerID - 1].GetChild(1).gameObject.SetActive(true);
            playerButtonContainers[myPlayerController.playerID - 1].GetChild(2).gameObject.SetActive(true);
            playerButtonContainers[myPlayerController.playerID - 1].GetChild(3).gameObject.SetActive(true);
            // playerButtonContainers[id - 1].GetChild(1).gameObject.SetActive(true);
            // playerButtonContainers[id - 1].GetChild(2).gameObject.SetActive(true);
            // playerButtonContainers[id - 1].GetChild(3).gameObject.SetActive(true);
            // if (myPlayerController.playerID != id) return;
            _canKong = true;
            _canWin = true;
            DisableHandGrab();
        }

        /// <summary>
        /// 可以碰，杠，胡
        /// </summary>
        /// <param name="id">可以碰杠胡的玩家的id</param>
        [PunRPC]
        private void CanPAndKAndH(int id)
        {
            if (myPlayerController.playerID != id) return;
            OpenPlayerButtonContainer();
            playerButtonContainers[myPlayerController.playerID - 1].GetChild(0).gameObject.SetActive(true);
            playerButtonContainers[myPlayerController.playerID - 1].GetChild(1).gameObject.SetActive(true);
            playerButtonContainers[myPlayerController.playerID - 1].GetChild(2).gameObject.SetActive(true);
            playerButtonContainers[myPlayerController.playerID - 1].GetChild(3).gameObject.SetActive(true);
            // playerButtonContainers[id - 1].GetChild(0).gameObject.SetActive(true);
            // playerButtonContainers[id - 1].GetChild(1).gameObject.SetActive(true);
            // playerButtonContainers[id - 1].GetChild(2).gameObject.SetActive(true);
            // playerButtonContainers[id - 1].GetChild(3).gameObject.SetActive(true);
            // if (myPlayerController.playerID != id) return;
            _canKong = true;
            _canKong = true;
            _canWin = true;
            DisableHandGrab();
        }

        /// <summary>
        /// 没有人胡牌，所有人显示流局
        /// </summary>
        [PunRPC]
        public void NoOneWin()
        {
            // var scoreCanvas = playerButtonContainers[myPlayerController.playerID - 1].GetChild(4).gameObject;
            // scoreCanvas.SetActive(true);
            OpenPlayerResultPanel();
            //scoreCanvas.transform.GetChild(1).GetComponentInChildren<TMP_Text>().text = "流局";
            playerResultPanels[myPlayerController.playerID - 1].GetChild(0).GetChild(0).GetComponent<TMP_Text>().text =
                "流局";
        }

        /// <summary>
        /// 排列手中麻将
        /// </summary>
        /// <param name="random">是否随机排列</param>
        /// <param name="disableCollider">排列的时候是否把所有牌的碰撞器禁用</param>
        public void SortMyMahjong(bool random, bool disableCollider)
        {
            var num = 1;
            //做出手势，随机摆放
            if (random)
            {
                foreach (var pair in myPlayerController.mahjongMap)
                {
                    pair.Value.Clear();
                }

                foreach (var pair in myPlayerController.MyMahjong)
                {
                    myPlayerController.mahjongMap[pair.Value.Count].Add(pair.Key);
                }

                foreach (var pair in myPlayerController.mahjongMap)
                {
                    var list = pair.Value.OrderBy(_ => rng.Next()).ToList();
                    for (var i = 0; i < pair.Value.Count; i++)
                    {
                        pair.Value[i] = list[i];
                    }
                }
            }

            for (var i = 4; i >= 1; i--)
            {
                myPlayerController.mahjongMap.TryGetValue(i, out var list);
                if (list == null) continue;
                foreach (var index in list)
                {
                    foreach (var go in myPlayerController.MyMahjong[index])
                    {
                        var script = go.GetComponent<MahjongAttr>();
                        if (script.num == 0)
                        {
                            continue;
                        }

                        BoxCollider boxCollider = null;
                        if (disableCollider)
                        {
                            boxCollider = go.GetComponent<BoxCollider>();
                            boxCollider.enabled = false;
                        }

                        script.num = num++;
                        var t = DOTween.Sequence()
                            .Insert(0f,
                                go.transform.DOMove(
                                    GameManager.Instance.GetPickPoses()[myPlayerController.playerID - 1].position +
                                    GameManager.Instance.GetBias()[myPlayerController.playerID - 1] *
                                    (script.num - 1),
                                    1f)).Insert(0f,
                                go.transform.DORotate(
                                    GameManager.Instance.GetRotateList()[myPlayerController.playerID - 1], 1f))
                            .SetEase(Ease.Linear);
                        t.onComplete += go.GetComponent<Rigidbody>().Sleep;
                        if (disableCollider)
                        {
                            t.onComplete += () => { boxCollider.enabled = true; };
                        }
                    }
                }
            }


            // foreach (var item in myPlayerController.MyMahjong)
            // {
            //     foreach (var go in item.Value)
            //     {
            //         var script = go.GetComponent<MahjongAttr>();
            //         if (script.num == 0)
            //         {
            //             continue;
            //         }
            //
            //         if (flag)
            //         {
            //             go.GetComponent<BoxCollider>().enabled = false;
            //         }
            //
            //         script.num = num++;
            //         var t = DOTween.Sequence()
            //             .Insert(0f,
            //                 go.transform.DOMove(
            //                     GameManager.Instance.GetPickPoses()[myPlayerController.playerID - 1].position +
            //                     GameManager.Instance.GetBias()[myPlayerController.playerID - 1] * (script.num - 1),
            //                     1f)).Insert(0f,
            //                 go.transform.DORotate(
            //                     GameManager.Instance.GetRotateList()[myPlayerController.playerID - 1], 1f))
            //             .SetEase(Ease.Linear);
            //         t.onComplete += go.GetComponent<Rigidbody>().Sleep;
            //         if (flag)
            //         {
            //             t.onComplete += () => { go.GetComponent<BoxCollider>().enabled = true; };
            //         }
            //     }
            // }
        }

        /// <summary>
        /// 做出旋转手势，触发随机排列麻将
        /// </summary>
        public void SortPose()
        {
            SortMyMahjong(true, false);
        }

        // 有人碰/杠的时候，直接清零
        [PunRPC]
        private void ResetPlayerDictCount()
        {
            _playerDictCount = 0;
        }

        /// <summary>
        /// 处理碰牌
        /// </summary>
        private void SolvePong()
        {
            //自己能碰的时候，点击按钮才生效
            if (!_canPong) return;
            //点击之后立马不能碰牌
            _canPong = false;
            //向所有人RPC，当前轮次轮到我了，并且不需要发牌
            photonView.RPC(nameof(ResetPlayerDictCount), RpcTarget.MasterClient);
            photonView.RPC(nameof(NextTurn), RpcTarget.All,
                myPlayerController.playerID, false);
            //向所有人RPC，隐藏所有按钮
            photonView.RPC(nameof(ResetButton), RpcTarget.All);
            //遍历自己的所有与被碰的牌相同的牌
            foreach (var go in myPlayerController.MyMahjong[nowTile])
            {
                var script = go.GetComponent<MahjongAttr>();
                //这些牌不能再被拿起来
                script.photonView.RPC(nameof(script.SetState), RpcTarget.All);
                go.GetComponent<HandGrabInteractable>().enabled = false;
                //把牌移动到指定的位置
                go.transform.DOMove(myPlayerController.putPos, 1f);
                //旋转牌
                go.transform.DORotate(GameManager.Instance.GetPlayerPutRotations()[myPlayerController.playerID - 1],
                    1f);
                //自己的位置减去一个牌的距离
                myPlayerController.putPos -= GameManager.Instance.GetBias()[myPlayerController.playerID - 1];
                script.num = 0;
                script.isPonged = true;
                script.inMyHand = false;
            }

            //再生成一个相同的牌，放到指定位置
            var newGo = PhotonNetwork.Instantiate("mahjong_tile_" + nowTile, myPlayerController.putPos,
                Quaternion.Euler(GameManager.Instance.GetPlayerPutRotations()[myPlayerController.playerID - 1]));
            var attr = newGo.GetComponent<MahjongAttr>();
            newGo.GetComponent<HandGrabInteractable>().enabled = false;
            attr.inMyHand = false;
            attr.num = 0;
            attr.isPonged = true;
            //这个牌也不能在被抓取
            attr.photonView.RPC(nameof(attr.SetState), RpcTarget.All);
            myPlayerController.MyMahjong[nowTile].Add(newGo);
            //整理牌
            SortMyMahjong(false, false);
            //销毁场上的那个牌
            photonView.RPC(nameof(DestroyItem), RpcTarget.All);
            myPlayerController.putPos -= GameManager.Instance.GetBias()[myPlayerController.playerID - 1];
            EnableHandGrab();
        }

        /// <summary>
        /// 杠分为杠别人和加杠
        /// </summary>
        private void SolveKong()
        {
            if (!_canKong) return;
            _canKong = false;

            //隐藏button
            photonView.RPC(nameof(ResetButton), RpcTarget.All);
            foreach (var pair in myPlayerController.MyMahjong)
            {
                if (pair.Value.Count == 4)
                {
                    var temp = pair.Value[0].GetComponent<MahjongAttr>();
                    //这种情况是加杠
                    if (temp.num == 0 && temp.isPonged)
                    {
                        pair.Value[3].transform
                            .DOMove(pair.Value[1].transform.position + pair.Value[1].transform.up * 0.5f, 1f);
                        pair.Value[3].transform.DORotate(
                            GameManager.Instance.GetPlayerPutRotations()[myPlayerController.playerID - 1], 1f);
                        var attr = pair.Value[3].GetComponent<MahjongAttr>();
                        attr.num = 0;
                        attr.inMyHand = false;
                        attr.GetComponent<HandGrabInteractable>().enabled = false;
                        foreach (var go in pair.Value)
                        {
                            go.GetComponent<MahjongAttr>().isPonged = false;
                            go.GetComponent<MahjongAttr>().isKonged = true;
                        }
                    }
                    //跳过已经杠过的
                    else if (temp.num == 0 && temp.isKonged)
                    {
                        continue;
                    }
                    //手上有4张牌，按顺序摆好
                    else
                    {
                        foreach (var go in pair.Value)
                        {
                            go.transform.DOMove(myPlayerController.putPos, 1f);
                            go.transform.DORotate(
                                GameManager.Instance.GetPlayerPutRotations()[myPlayerController.playerID - 1],
                                1f);
                            var attr = go.GetComponent<MahjongAttr>();
                            myPlayerController.putPos -=
                                GameManager.Instance.GetBias()[myPlayerController.playerID - 1];
                            attr.num = 0;
                            attr.inMyHand = false;
                            attr.isKonged = true;
                            go.GetComponent<HandGrabInteractable>().enabled = false;
                        }
                    }

                    SortMyMahjong(false, false);
                    photonView.RPC(nameof(ResetPlayerDictCount), RpcTarget.MasterClient);
                    //拿到出牌权，看情况发牌
                    photonView.RPC(nameof(NextTurn), RpcTarget.All,
                        myPlayerController.playerID, true);
                    EnableHandGrab();
                    return;
                }
            }

            //杠牌
            foreach (var go in myPlayerController.MyMahjong[nowTile])
            {
                var script = go.GetComponent<MahjongAttr>();
                go.transform.DOMove(myPlayerController.putPos, 1f);
                go.transform.DORotate(GameManager.Instance.GetPlayerPutRotations()[myPlayerController.playerID - 1],
                    1f);
                myPlayerController.putPos -= GameManager.Instance.GetBias()[myPlayerController.playerID - 1];
                script.num = 0;
                script.inMyHand = false;
                script.isKonged = true;
                go.GetComponent<HandGrabInteractable>().enabled = false;
            }

            var newGo = PhotonNetwork.Instantiate("mahjong_tile_" + nowTile, myPlayerController.putPos,
                Quaternion.Euler(GameManager.Instance.GetPlayerPutRotations()[myPlayerController.playerID - 1]));
            myPlayerController.putPos -= GameManager.Instance.GetBias()[myPlayerController.playerID - 1];
            newGo.GetComponent<MahjongAttr>().num = 0;
            newGo.GetComponent<MahjongAttr>().inMyHand = false;
            newGo.GetComponent<MahjongAttr>().isKonged = true;
            newGo.GetComponent<HandGrabInteractable>().enabled = false;
            myPlayerController.MyMahjong[nowTile].Add(newGo);
            photonView.RPC(nameof(DestroyItem), RpcTarget.All);
            SortMyMahjong(false, false);
            //拿到出牌权，看情况发牌
            photonView.RPC(nameof(NextTurn), RpcTarget.All,
                myPlayerController.playerID, true);
            EnableHandGrab();
        }

        [PunRPC]
        public void DestroyItem()
        {
            var destroyGo =
                (from go in FindObjectsOfType<PhotonView>() where go.ViewID == tileViewID select go.gameObject)
                .FirstOrDefault();
            Destroy(destroyGo);
        }
        //1：碰
        //2：杠
        //3：胡
        //4：碰/杠
        //5：碰/赢
        //6：杠/赢
        //7：碰/杠/赢

        public int CheckMyState(int id)
        {
            var ans = 0;
            if (myPlayerController.MyMahjong.ContainsKey(id))
            {
                //可以碰
                if (myPlayerController.MyMahjong[id].Count == 2)
                {
                    ans = 1;
                }

                //可以杠
                if (myPlayerController.MyMahjong[id].Count == 3 &&
                    myPlayerController.MyMahjong[id][0].GetComponent<MahjongAttr>().num != 0)
                {
                    //可以碰，也可以杠
                    if (ans == 1)
                    {
                        ans = 4;
                    }
                    //可以杠
                    else
                    {
                        ans = 2;
                    }
                }

                //是否赢了
                if (CheckWin(id))
                {
                    //可以赢，也可以碰
                    if (ans == 1)
                    {
                        ans = 5;
                    }
                    //可以杠，也可以赢
                    else if (ans == 2)
                    {
                        ans = 6;
                    }
                    //可以同时碰，杠，赢
                    else if (ans == 4)
                    {
                        ans = 7;
                    }
                    //只能赢
                    else
                    {
                        ans = 3;
                    }
                }
            }

            return ans;
        }

        /// <summary>
        /// 检测能否胡牌，id不传递的时候表示当前有14张，一般是检测自摸或者庄家开局检测能否胡牌
        /// </summary>
        /// <param name="id">待检测的牌id</param>
        /// <returns></returns>
        private bool CheckWin(int id = 0)
        {
            // List<Card> cards = new List<Card>();
            // //var cards = new Card[14];
            // if (id == 0)
            // {
            //     var i = 0;
            //     foreach (var pair in myPlayerController.MyMahjong)
            //     {
            //         for (var j = 0; j < pair.Value.Count; j++)
            //         {
            //             cards.Add(new Card(pair.Key - pair.Key / 9 * 9, pair.Key / 9));
            //             //cards[i++] =
            //         }
            //     }
            // }
            // else
            // {
            //     if (!myPlayerController.MyMahjong.ContainsKey(id))
            //     {
            //         myPlayerController.MyMahjong[id] = new List<GameObject>();
            //     }
            //
            //     var go = new GameObject();
            //     myPlayerController.MyMahjong[id].Add(go);
            //     var i = 0;
            //     foreach (var pair in myPlayerController.MyMahjong)
            //     {
            //         for (var j = 0; j < pair.Value.Count; j++)
            //         {
            //             cards.Add(new Card(pair.Key - pair.Key / 9 * 9, pair.Key / 9));
            //         }
            //     }
            //
            //     myPlayerController.MyMahjong[id].Remove(go);
            // }
            //
            // return isHu(cards);
            var cnt2 = 0;
            var cnt3 = 0;
            var cnt4 = 0;
            foreach (var item in myPlayerController.MyMahjong)
            {
                if (item.Key == id)
                {
                    if (item.Value.Count == 1)
                    {
                        cnt2++;
                    }
                    else if (item.Value.Count == 2)
                    {
                        cnt3++;
                    }
                    else if (item.Value.Count == 3)
                    {
                        cnt4++;
                    }
                }
                else
                {
                    if (item.Value.Count == 3)
                    {
                        cnt3++;
                    }
                    else if (item.Value.Count == 4)
                    {
                        cnt4++;
                    }
                    else if (item.Value.Count == 2)
                    {
                        cnt2++;
                    }
                }
            }

            return (cnt2 + cnt3 + cnt4 == 5 && cnt2 == 1) || cnt2 == 7;
        }

        /// <summary>
        /// 开启眼动追踪，实现透视效果
        /// </summary>
        public void ShowEyeGaze()
        {
            if (!GazeInteractor.enabled)
            {
                PlayFabClientAPI.GetUserData(new GetUserDataRequest(),
                    //在CallBack函数中打开UI，防止提前打开UI
                    data =>
                    {
                        //找到X射线卡牌的数量
                        var xRayCardCount = int.Parse(data.Data["XRayCard"].Value);
                        if (xRayCardCount > 0)
                        {
                            //更新数量
                            xRayCardCount--;
                            //更新显示
                            var xRayCardGo = playerCardContainers[myPlayerController.playerID - 1].GetChild(0);
                            xRayCardGo.GetComponentInChildren<TMP_Text>().text = $"透视道具\n当前拥有：{xRayCardCount}个";
                            //开启射线
                            GazeInteractor.enabled = true;
                            //20秒后关闭射线
                            StartCoroutine(nameof(HideEyeGaze));
                            PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest
                            {
                                Data = new Dictionary<string, string>
                                {
                                    {"XRayCard", xRayCardCount.ToString()}
                                }
                            }, _ => { }, _ => { });
                        }
                    },
                    error => { Debug.Log(error.ErrorMessage); });
            }
        }

        private IEnumerator HideEyeGaze()
        {
            yield return new WaitForSeconds(20f);
            GazeInteractor.enabled = false;
            foreach (var mahjong in effectGoList)
            {
                mahjong.OnEyeHoverExit();
            }

            effectGoList.Clear();
        }


        public int count;

        /// <summary>
        /// 搓牌之后寻找能胡的牌，并换牌
        /// </summary>
        public void ChangeMahjong()
        {
            count++;
            if (count >= 3 && nowMahjong != null)
            {
                count = 0;
                var id = nowMahjong.GetComponent<MahjongAttr>().ID;
                myPlayerController.mahjongMap[myPlayerController.MyMahjong[id].Count].Remove(id);
                myPlayerController.MyMahjong[id].Remove(nowMahjong);
                // if (myPlayerController.MyMahjong[id].Count == 0)
                // {
                //     myPlayerController.MyMahjong.Remove(id);
                // }

                var changeID = 0;
                for (var i = 1; i <= 34; i++)
                {
                    if (CheckWin(i))
                    {
                        changeID = i;
                        break;
                    }
                }

                if (changeID != 0)
                {
                    changeMahjongAudio.PlayAudio();
                    nowMahjong.GetComponent<MahjongAttr>().ID = changeID;
                    nowMahjong.GetComponent<MeshFilter>().mesh = GameManager.Instance.GetMahjongMesh(changeID);
                    myPlayerController.mahjongMap[myPlayerController.MyMahjong[changeID].Count].Remove(changeID);
                    myPlayerController.MyMahjong[changeID].Add(nowMahjong);
                    myPlayerController.mahjongMap[myPlayerController.MyMahjong[changeID].Count].Add(changeID);
                    photonView.RPC(nameof(CanH), RpcTarget.All, myPlayerController.playerID);
                }
                else
                {
                    myPlayerController.mahjongMap[myPlayerController.MyMahjong[id].Count].Add(id);
                    if (!myPlayerController.MyMahjong.ContainsKey(id))
                    {
                        myPlayerController.MyMahjong[id] = new List<GameObject>();
                    }

                    myPlayerController.MyMahjong[id].Add(nowMahjong);
                }
            }
        }

        /// <summary>
        /// 做出剪刀手势，打开或者菜单
        /// </summary>
        public void OpenOrClosePlayerCanvas()
        {
            if (!playerCanvases[myPlayerController.playerID - 1].gameObject.activeSelf)
            {
                OpenPlayerCardContainer();
            }
            else
            {
                ClosePlayerCardContainer();
            }
        }
    }
}