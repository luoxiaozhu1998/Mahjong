using System;
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
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

namespace Controller
{
    public class Card
    {
        private int number;

        public int Number
        {
            get { return number; }
            set { number = value; }
        }

        private int type;

        public int Type
        {
            get { return type; }
            set { type = value; }
        }

        public Card(int n, int t)
        {
            number = n;
            type = t;
        }
    }

    /// <summary>
    /// 负责管理整个游戏的逻辑,单例
    /// </summary>
    public class GameController : MonoBehaviourPunCallbacks
    {
        public static GameController Instance { get; private set; }
        public int playerCount;
        public PlayerController myPlayerController;
        public Dictionary<int, int> ReadyDict;
        public bool canNext;
        public int nowTurn;
        public int nowTile;
        public int tileViewID;
        private List<MahjongAttr> _mahjong;
        private List<Transform> _playerButtons;
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
        private bool isWin;

        [SerializeField, Interface(typeof(IHand))]
        public MonoBehaviour _leftHand;

        [SerializeField, Interface(typeof(IHand))]
        public MonoBehaviour _rightHand;

        [SerializeField, Interface(typeof(ITrackingToWorldTransformer))]
        [Tooltip("Transformer is required so calculations can be done in Tracking space")]
        public UnityEngine.Object _transformer;

        public OVRCameraRig OvrCameraRig;

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
            playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
            canNext = true;
            ReadyDict = new Dictionary<int, int>();
            _mahjong = new List<MahjongAttr>();
            _playerButtons = new List<Transform>();
            isWin = false;
            StartGame();
            FindObjectOfType<OvrAvatarManager>().GetComponent<AvatarInputManager>().Init();
        }

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
            if (PhotonNetwork.IsMasterClient)
            {
                if (CheckWin())
                {
                    isWin = true;
                    photonView.RPC(nameof(CanH), RpcTarget.All, myPlayerController.playerID);
                }
            }

            foreach (var pair in myPlayerController.MyMahjong)
            {
                if (pair.Value.Count == 4)
                {
                    photonView.RPC(isWin ? nameof(CanKAndH) : nameof(CanH), RpcTarget.All, myPlayerController.playerID);
                    break;
                }
            }
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

            for (var i = 1; i <= 4; i++)
            {
                _playerButtons.Add(GameObject.Find("Player" + i + "Button").transform);
            }

            var pongButton = _playerButtons[myPlayerController.playerID - 1].GetChild(0).GetChild(2)
                .GetChild(1).GetChild(0).GetChild(0);
            pongButton.GetComponentInParent<InteractableUnityEventWrapper>().WhenUnselect
                .AddListener(SolvePong);
            var kongButton = _playerButtons[myPlayerController.playerID - 1].GetChild(1).GetChild(2)
                .GetChild(1).GetChild(0).GetChild(0);
            kongButton.GetComponentInParent<InteractableUnityEventWrapper>().WhenUnselect
                .AddListener(SolveKong);
            var winButton = _playerButtons[myPlayerController.playerID - 1].GetChild(2).GetChild(2)
                .GetChild(1).GetChild(0).GetChild(0);
            winButton.GetComponentInParent<InteractableUnityEventWrapper>().WhenUnselect
                .AddListener(SolveWin);
            var skipButton = _playerButtons[myPlayerController.playerID - 1].GetChild(3).GetChild(2)
                .GetChild(1).GetChild(0).GetChild(0);
            skipButton.GetComponentInParent<InteractableUnityEventWrapper>().WhenUnselect
                .AddListener(SolveSkip);
            var leaveButton = _playerButtons[myPlayerController.playerID - 1].GetChild(6).GetChild(2).GetChild(1)
                .GetChild(0).GetChild(0);
            leaveButton.GetComponentInParent<InteractableUnityEventWrapper>().WhenUnselect
                .AddListener(() => { PhotonNetwork.LeaveRoom(); });
            foreach (var playerButton in _playerButtons)
            {
                for (var i = 0; i < 5; i++)
                {
                    playerButton.GetChild(i).gameObject.SetActive(false);
                }

                playerButton.GetChild(6).gameObject.SetActive(false);
            }

            nowTurn = 1;
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

            _playerButtons[myPlayerController.playerID - 1].GetChild(5).GetComponentInChildren<TMP_Text>().text =
                "Score:" + (id == myPlayerController.playerID ? 20 : 0);
            var scoreCanvas = _playerButtons[myPlayerController.playerID - 1].GetChild(4).gameObject;
            scoreCanvas.SetActive(true);
            scoreCanvas.transform.GetChild(1).GetComponentInChildren<TMP_Text>().text =
                id == myPlayerController.playerID ? "您赢了！" : "您输了！";
            foreach (var item in PhotonNetwork.CurrentRoom.Players)
            {
                var go = Instantiate(playerPanelPrefab, playerPanelContainers[myPlayerController.playerID - 1]);
                go.transform.GetChild(0).GetComponent<TMP_Text>().text = item.Value.NickName;
                go.transform.GetChild(1).GetComponent<TMP_Text>().text =
                    item.Value.NickName == userName ? "Score + 10" : "Score - 10";
            }

            photonView.RPC(nameof(ResetButton), RpcTarget.All, true);
        }

        private void SolveWin()
        {
            isWin = true;
            photonView.RPC(nameof(SetPoint), RpcTarget.All, myPlayerController.playerID,
                PhotonNetwork.LocalPlayer.NickName);
        }

        private void SolveSkip()
        {
            if (!_canKong && !_canPong && !_canWin) return;
            photonView.RPC(nameof(NextTurn), RpcTarget.All, nowTurn, false);
            _canKong = _canPong = _canWin = false;
            photonView.RPC(nameof(ResetButton), RpcTarget.All, false);
            EnableHandGrab();
        }

        [PunRPC]
        private void ResetButton(bool flag = false)
        {
            foreach (var playerButton in _playerButtons)
            {
                for (var i = 0; i < 4; i++)
                {
                    playerButton.GetChild(i).gameObject.SetActive(false);
                }

                playerButton.GetChild(6).gameObject.SetActive(flag);
            }
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

            if (myPlayerController.MyMahjong[attr.ID].Count == 3)
            {
                DisableHandGrab();
                photonView.RPC(CheckWin(attr.ID) ? nameof(CanKAndH) : nameof(CanK), RpcTarget.All,
                    myPlayerController.playerID);
            }

            if (CheckWin(attr.ID))
            {
                photonView.RPC(myPlayerController.MyMahjong[attr.ID].Count == 3 ? nameof(CanPAndH) : nameof(CanH),
                    RpcTarget.All, myPlayerController.playerID);
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
            var ID = _mahjong[0].ID;
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

            myPlayerController.mahjongMap[myPlayerController.MyMahjong[ID].Count - 1].Remove(ID);
            myPlayerController.mahjongMap[myPlayerController.MyMahjong[ID].Count].Add(ID);
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
            if (OVRInput.GetActiveController() == OVRInput.Controller.Touch && OVRInput.Get(OVRInput.Button.One))
            {
                SortMyMahjong(true, false);
            }

            if (!PhotonNetwork.IsMasterClient) return;
            //所有玩家在某人打出牌之后向主客户端汇报自己的状态（能否碰/杠/胡牌）
            //当字典的count等于玩家count，主客户端开始处理，否则锁死所有客户端
            if (ReadyDict.Count != playerCount) return;
            var flag = false;
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
                        photonView.RPC(nameof(CanP), RpcTarget.All,
                            item.Key);
                        break;
                    //可以杠牌
                    case 2:
                        photonView.RPC(nameof(CanK), RpcTarget.All,
                            item.Key);
                        break;
                    //可以胡牌
                    case 3:
                        photonView.RPC(nameof(CanH), RpcTarget.All,
                            item.Key);
                        break;
                    case 4:
                        photonView.RPC(nameof(CanPAndK),
                            RpcTarget.All,
                            item.Key);
                        break;
                    //碰且赢
                    case 5:
                        photonView.RPC(nameof(CanPAndH),
                            RpcTarget.All,
                            item.Key);
                        break;
                    case 6:
                        photonView.RPC(nameof(CanKAndH),
                            RpcTarget.All,
                            item.Key);
                        break;
                    case 7:
                        photonView.RPC(nameof(CanPAndKAndH),
                            RpcTarget.All,
                            item.Key);
                        break;
                }

                //只要有一个人可以处理牌，就不应该继续发牌
                flag = true;
            }

            // 清空字典，准备下一回合
            ReadyDict.Clear();
            // 只要有一个人可以处理牌，就不应该继续发牌
            if (flag) return;
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
            _playerButtons[id - 1].GetChild(0).gameObject.SetActive(true);
            _playerButtons[id - 1].GetChild(3).gameObject.SetActive(true);
            if (myPlayerController.playerID != id) return;
            _canPong = true;
            DisableHandGrab();
        }

        [PunRPC]
        private void CanK(int id)
        {
            _playerButtons[id - 1].GetChild(1).gameObject.SetActive(true);
            _playerButtons[id - 1].GetChild(3).gameObject.SetActive(true);
            if (myPlayerController.playerID != id) return;
            _canKong = true;
            DisableHandGrab();
        }

        [PunRPC]
        private void CanH(int id)
        {
            _playerButtons[id - 1].GetChild(2).gameObject.SetActive(true);
            _playerButtons[id - 1].GetChild(3).gameObject.SetActive(true);
            if (myPlayerController.playerID != id) return;
            _canWin = true;
        }

        [PunRPC]
        private void CanPAndK(int id)
        {
            _playerButtons[id - 1].GetChild(0).gameObject.SetActive(true);
            _playerButtons[id - 1].GetChild(1).gameObject.SetActive(true);
            _playerButtons[id - 1].GetChild(3).gameObject.SetActive(true);
            if (myPlayerController.playerID != id) return;
            _canPong = true;
            _canKong = true;
            DisableHandGrab();
        }

        [PunRPC]
        private void CanPAndH(int id)
        {
            _playerButtons[id - 1].GetChild(0).gameObject.SetActive(true);
            _playerButtons[id - 1].GetChild(2).gameObject.SetActive(true);
            _playerButtons[id - 1].GetChild(3).gameObject.SetActive(true);
            if (myPlayerController.playerID != id) return;
            _canPong = true;
            _canWin = true;
            DisableHandGrab();
        }

        [PunRPC]
        private void CanKAndH(int id)
        {
            _playerButtons[id - 1].GetChild(1).gameObject.SetActive(true);
            _playerButtons[id - 1].GetChild(2).gameObject.SetActive(true);
            _playerButtons[id - 1].GetChild(3).gameObject.SetActive(true);
            if (myPlayerController.playerID != id) return;
            _canKong = true;
            _canWin = true;
            DisableHandGrab();
        }

        [PunRPC]
        private void CanPAndKAndH(int id)
        {
            _playerButtons[id - 1].GetChild(0).gameObject.SetActive(true);
            _playerButtons[id - 1].GetChild(1).gameObject.SetActive(true);
            _playerButtons[id - 1].GetChild(2).gameObject.SetActive(true);
            _playerButtons[id - 1].GetChild(3).gameObject.SetActive(true);
            if (myPlayerController.playerID != id) return;
            _canKong = true;
            _canKong = true;
            _canWin = true;
            DisableHandGrab();
        }

        [PunRPC]
        public void NoOneWin()
        {
            var scoreCanvas = _playerButtons[myPlayerController.playerID - 1].GetChild(4).gameObject;
            scoreCanvas.SetActive(true);
            scoreCanvas.transform.GetChild(1).GetComponentInChildren<TMP_Text>().text = "流局";
            photonView.RPC(nameof(ResetButton), RpcTarget.All, true);
        }

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

        public void SortPose()
        {
            SortMyMahjong(true, false);
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
            photonView.RPC(nameof(NextTurn), RpcTarget.All,
                myPlayerController.playerID, false);
            //向所有人RPC，隐藏所有按钮
            photonView.RPC(nameof(ResetButton), RpcTarget.All, false);
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
            photonView.RPC(nameof(ResetButton), RpcTarget.All, false);
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


        public static bool isHu(List<Card> cards)
        {
            int countCards = cards.Count;
            //判斷是否是七小對
            if (countCards == 14)
            {
                int count = 0;
                for (int i = 0; i < countCards; i += 2)
                {
                    if (cards[i].Number == cards[i + 1].Number)
                    {
                        count++;
                    }
                    else
                    {
                        break;
                    }
                }

                if (count == 7)
                {
                    return true;
                }
            }

            int[][] handcards = new int[4][]
            {
                new int[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, new int[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                new int[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, new int[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}
            };
            for (int i = 0; i < countCards; i++)
            {
                //type爲0,1,2,3則爲萬條筒字。   [type = 0,1,2,3 ,  0]代表每種牌型的總數量
                switch (cards[i].Type)
                {
                    case 0:
                        handcards[0][cards[i].Number]++;
                        handcards[0][0]++;
                        break;
                    case 1:
                        handcards[1][cards[i].Number]++;
                        handcards[1][0]++;
                        break;
                    case 2:
                        handcards[2][cards[i].Number]++;
                        handcards[2][0]++;
                        break;
                    case 3:
                        handcards[3][cards[i].Number]++;
                        handcards[3][0]++;
                        break;
                }
            }

            bool isJiang = false; //判斷是否有對子
            int jiangNumber = -1;
            for (int i = 0; i < handcards.GetLength(0); i++)
            {
                if (handcards[i][0] % 3 == 2)
                {
                    if (isJiang)
                    {
                        return false;
                    }

                    isJiang = true;
                    jiangNumber = i;
                }
                //因爲對應四種牌型只能有一種且僅包含一個對子
            }

            //先求沒有將牌的情況判斷其是不是都是由刻子或者砍組成
            for (int i = 0; i < handcards.GetLength(0); i++)
            {
                if (i != jiangNumber)
                {
                    if (!(IsKanOrShun(handcards[i], i == 3)))
                    {
                        return false;
                    }
                }
            }

            bool success = false;
            //有將牌的情況下
            for (int i = 1; i <= 9; i++)
            {
                if (handcards[jiangNumber][i] >= 2)
                {
                    handcards[jiangNumber][i] -= 2;
                    handcards[jiangNumber][0] -= 2;
                    if (IsKanOrShun(handcards[jiangNumber], jiangNumber == 3))
                    {
                        success = true;
                        break;
                    }

                    handcards[jiangNumber][i] += 2;
                    handcards[jiangNumber][0] += 2;
                }
            }

            return success;
        }

        //判斷是否滿足牌組爲順子或砍組成
        public static bool IsKanOrShun(int[] arr, bool isZi)
        {
            if (arr[0] == 0)
            {
                return true;
            }

            int index = -1;
            for (int i = 1; i < arr.Length; i++)
            {
                if (arr[i] > 0)
                {
                    index = i;
                    break;
                }
            }

            bool result;
            //是否滿足全是砍
            if (arr[index] >= 3)
            {
                arr[index] -= 3;
                arr[0] -= 3;
                result = IsKanOrShun(arr, isZi);
                arr[index] += 3;
                arr[0] += 3;
                return result;
            }

            //是否滿足爲順子
            if (!isZi)
            {
                if (index < 8 && arr[index + 1] >= 1 && arr[index + 2] >= 1)
                {
                    arr[index] -= 1;
                    arr[index + 1] -= 1;
                    arr[index + 2] -= 1;
                    arr[0] -= 3;
                    result = IsKanOrShun(arr, isZi);
                    arr[index] += 1;
                    arr[index + 1] += 1;
                    arr[index + 2] += 1;
                    arr[0] += 3;
                    return result;
                }
            }

            return false;
        }


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

        public void ShowEyeGaze()
        {
            GazeInteractor.enabled = true;
            StartCoroutine(nameof(HideEyeGaze));
        }

        private IEnumerator HideEyeGaze()
        {
            yield return new WaitForSeconds(20f);
            GazeInteractor.enabled = false;
        }

        public int count;


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
    }
}