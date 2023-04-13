using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Manager;
using Newtonsoft.Json;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Controller
{
    /// <summary>
    /// 负责管理整个游戏的逻辑,单例
    /// </summary>
    public class GameController : MonoBehaviourPunCallbacks
    {
        public static GameController Instance { get; private set; }

        // public Button pongButton;
        // public Button kongButton;
        // public Button addKongButton;
        // public Button skipButton;
        // public Button winButton;
        public int playerCount;
        public PlayerController myPlayerController;
        private PhotonView _gameManagerPhotonView;
        public Dictionary<int, int> ReadyDict;
        public bool canNext;
        public int nowTurn;
        public int lastTurn;
        public int nowTile;
        [HideInInspector] public GameObject tile;
        public Image bg;
        public TMP_Text text;
        public Button button;
        public Transform canvas;
        private ulong m_userId;
        private List<MahjongAttr> _mahjong;
        private List<Transform> _playerButtons;
        private bool _canPong;
        private bool _canKong;
        private bool _canWin;
        private Button _confirmButton;

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
            // canvas = GameObject.Find("Canvas").transform;
            //
            // pongButton = canvas.GetChild(0).GetChild(0).GetComponent<Button>();
            // kongButton = canvas.GetChild(0).GetChild(1).GetComponent<Button>();
            // winButton = canvas.GetChild(0).GetChild(2).GetComponent<Button>();
            // skipButton = canvas.GetChild(0).GetChild(3).GetComponent<Button>();
            // addKongButton = canvas.GetChild(0).GetChild(5).GetComponent<Button>();
            // pongButton.gameObject.SetActive(false);
            // skipButton.gameObject.SetActive(false);
            // kongButton.gameObject.SetActive(false);
            // addKongButton.gameObject.SetActive(false);
            // winButton.gameObject.SetActive(false);
            //bg.gameObject.SetActive(false);
            //bg.GetComponent<Image>().raycastTarget = false;
            playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
            _gameManagerPhotonView = GameManager.Instance.GetComponent<PhotonView>();
            canNext = true;
            ReadyDict = new Dictionary<int, int>();
            _mahjong = new List<MahjongAttr>();
            _playerButtons = new List<Transform>();
        }

        /// <summary>
        /// 给button注册事件
        /// </summary>
        // private void Start()
        // {
        //     pongButton.onClick.AddListener(() =>
        //     {
        //         //得到出牌权（但是不发牌）
        //         _gameManagerPhotonView.RPC(nameof(GameManager.Instance.NextTurn), RpcTarget.All,
        //             myPlayerController.playerID, false);
        //         _gameManagerPhotonView.RPC(nameof(GameManager.Instance.HideButton), RpcTarget.All);
        //         SolvePong();
        //     });
        //     kongButton.onClick.AddListener(() =>
        //     {
        //         //得到出牌权（同时发牌）
        //         _gameManagerPhotonView.RPC(nameof(GameManager.Instance.NextTurn), RpcTarget.All,
        //             myPlayerController.playerID, true);
        //         _gameManagerPhotonView.RPC(nameof(GameManager.Instance.HideButton), RpcTarget.All);
        //         SolveKong();
        //     });
        //     addKongButton.onClick.AddListener(() =>
        //     {
        //         if (!myPlayerController.MyMahjong[nowTile][0].GetComponent<MahjongAttr>().canPlay)
        //         {
        //             myPlayerController.MyMahjong[nowTile][3].transform.DOMove(
        //                 myPlayerController.MyMahjong[nowTile][1].transform.position -
        //                 myPlayerController.MyMahjong[nowTile][1].transform.forward * 1.5f, 1f);
        //
        //             myPlayerController.MyMahjong[nowTile][3].GetComponent<MahjongAttr>().canPlay =
        //                 false;
        //             myPlayerController.MyMahjong[nowTile][3].transform
        //                 .DORotate(new Vector3(0.0f, 180.0f, 0.0f), 1f);
        //         }
        //         else
        //         {
        //             var idx = 0;
        //             TweenerCore<Vector3, Vector3, VectorOptions> a = null;
        //             foreach (var go in myPlayerController.MyMahjong[nowTile])
        //             {
        //                 if (idx < 3)
        //                 {
        //                     var script = go.GetComponent<MahjongAttr>();
        //                     script.canPlay = false;
        //                     if (idx == 1)
        //                     {
        //                         a = go.transform.DOMove(
        //                             myPlayerController.putPos - new Vector3(0.0f, 1.0f, 0.0f), 1f);
        //                     }
        //                     else
        //                     {
        //                         go.transform.DOMove(
        //                             myPlayerController.putPos - new Vector3(0.0f, 1.0f, 0.0f), 1f);
        //                     }
        //
        //                     go.transform.DORotate(
        //                         GameManager.Instance.GetPlayerPutRotations()[
        //                             myPlayerController.playerID - 1], 1f);
        //                     myPlayerController.putPos -=
        //                         GameManager.Instance.GetBias()[myPlayerController.playerID - 1];
        //                     script.num = 0;
        //                     idx++;
        //                 }
        //                 else
        //                 {
        //                     var script = go.GetComponent<MahjongAttr>();
        //                     script.canPlay = false;
        //                     script.num = 0;
        //                     go.transform.DOMove(
        //                         a.endValue -
        //                         myPlayerController.MyMahjong[nowTile][0].transform.forward * 1.5f,
        //                         1f);
        //                     go.transform.DORotate(new Vector3(0f, 180.0f, 0.0f), 1f);
        //                 }
        //             }
        //
        //             SortMyMahjong();
        //         }
        //
        //         _gameManagerPhotonView.RPC(nameof(GameManager.Instance.HideButton), RpcTarget.All);
        //         _gameManagerPhotonView.RPC(nameof(GameManager.Instance.NextTurn), RpcTarget.All,
        //             myPlayerController.playerID, true);
        //     });
        //     skipButton.onClick.AddListener(() =>
        //     {
        //         if (nowTurn != myPlayerController.playerID)
        //         {
        //             _gameManagerPhotonView.RPC(nameof(GameManager.Instance.NextTurn), RpcTarget.All,
        //                 nowTurn, true);
        //         }
        //
        //         _gameManagerPhotonView.RPC(nameof(GameManager.Instance.HideButton),
        //             RpcTarget.All);
        //     });
        //     winButton.onClick.AddListener(() =>
        //     {
        //         _gameManagerPhotonView.RPC(nameof(GameManager.Instance.ShowResult),
        //             RpcTarget.Others);
        //         text.text = "You Win!";
        //         bg.gameObject.SetActive(true);
        //         _gameManagerPhotonView.RPC(nameof(GameManager.Instance.HideButton),
        //             RpcTarget.All);
        //     });
        //     button.onClick.AddListener(() =>
        //     {
        //         Destroy(GameManager.Instance.gameObject);
        //         PhotonNetwork.LeaveRoom();
        //     });
        //     var simulator = FindObjectOfType<XRDeviceSimulator>().gameObject;
        //     simulator.SetActive(false);
        //     simulator.SetActive(true);
        // }
        private void SolveKong()
        {
            if (!_canKong) return;
            _canKong = false;
            photonView.RPC(nameof(NextTurn), RpcTarget.All,
                myPlayerController.playerID, false);
            photonView.RPC(nameof(ResetButton), RpcTarget.All);
            var idx = 0;
            var flag = false;
            foreach (var go in myPlayerController.MyMahjong[nowTile])
            {
                var script = go.GetComponent<MahjongAttr>();
                script.canPlay = false;
                if (idx == 1)
                {
                    flag = true;
                }

                go.transform.DOMove(myPlayerController.putPos, 1f);
                go.transform.DORotate(
                    GameManager.Instance.GetPlayerPutRotations()[
                        myPlayerController.playerID - 1], 1f);
                myPlayerController.putPos -=
                    GameManager.Instance.GetBias()[myPlayerController.playerID - 1];
                script.num = 0;
                idx++;
            }

            if (!flag) return;
            var newGo = PhotonNetwork.Instantiate("mahjong_tile_" + nowTile,
                myPlayerController.putPos,
                Quaternion.Euler(GameManager.Instance.GetPlayerPutRotations()[
                    myPlayerController.playerID - 1]));
            newGo.GetComponent<MahjongAttr>().num = 0;
            newGo.GetComponent<MahjongAttr>().canPlay = false;
            myPlayerController.MyMahjong[nowTile].Add(newGo);
            SortMyMahjong();
            photonView.RPC(nameof(DestroyItem), RpcTarget.All, lastTurn);
        }

        public override void OnLeftRoom()
        {
            PhotonNetwork.LoadLevel(0);
        }

        /// <summary>
        /// 开始游戏
        /// </summary>
        public void StartGame()
        {
            // if (PhotonNetwork.IsMasterClient)
            // {
            //     GetComponent<PhotonView>().RPC(nameof(NotMyTurn), RpcTarget.Others);
            // }

            GeneratePlayers();
            if (CheckWin())
            {
                photonView.RPC(nameof(CanH), RpcTarget.All, myPlayerController.playerID);
            }
            //CheckWin();
            //StartCoroutine(Leave());
        }

        private IEnumerator Leave()
        {
            yield return new WaitForSeconds(2f);
            PhotonNetwork.LeaveRoom();
        }

        [PunRPC]
        private void SetList(string a, string b)
        {
            GameManager.Instance.SetMahjongList(JsonConvert.DeserializeObject<List<Mahjong>>(a));
            GameManager.Instance.SetUserMahjongLists(
                JsonConvert.DeserializeObject<List<List<Mahjong>>>(b));
            var count = 14 + (PhotonNetwork.CurrentRoom.PlayerCount - 1) * 13;
            _mahjong = FindObjectsOfType<MahjongAttr>().ToList();
            _mahjong.Sort((a, b) =>
                int.Parse(a.gameObject.name.Split('_')[2]).CompareTo(int.Parse(b.gameObject.name.Split('_')[2])));
            for (var i = 0; i < count; i++)
            {
                Destroy(_mahjong[i].gameObject);
            }

            _mahjong.RemoveRange(0, count);
            var mahjongList = GameManager.Instance.GetMahjongList();
            var length = _mahjong.Count;
            for (var i = 0; i < length; i++)
            {
                _mahjong[i].GetComponent<MeshFilter>().mesh = GameManager.Instance.GetMahjongMesh(mahjongList[i].ID);
                var rb = _mahjong[i].GetComponent<Rigidbody>();
                var attr = _mahjong[i].GetComponent<MahjongAttr>();
                attr.id = mahjongList[i].ID;
                rb.constraints = RigidbodyConstraints.FreezeAll;
                // attr.pointableUnityEventWrapper.WhenSelect.AddListener(() =>
                // {
                //     rb.constraints = RigidbodyConstraints.None;
                // });
                // attr.pointableUnityEventWrapper.WhenUnselect.AddListener(() => AddMahjong(attr, rb));
                var grabInteractables = _mahjong[i].GetComponentsInChildren<HandGrabInteractable>();
                foreach (var grabInteractable in grabInteractables)
                {
                    grabInteractable.enabled = false;
                }
            }

            myPlayerController.MyMahjong =
                GameManager.Instance.GenerateMahjongAtStart(myPlayerController.playerID - 1);
            for (var i = 1; i <= 4; i++)
            {
                _playerButtons.Add(GameObject.Find("Player" + i + "Button").transform);
            }

            var pongButton = _playerButtons[myPlayerController.playerID - 1].GetChild(0).GetChild(2).GetChild(1)
                .GetChild(0).GetChild(0);
            pongButton.GetComponentInParent<InteractableUnityEventWrapper>().WhenSelect.AddListener(SolvePong);
            var kongButton = _playerButtons[myPlayerController.playerID - 1].GetChild(1).GetChild(2).GetChild(1)
                .GetChild(0).GetChild(0);
            kongButton.GetComponentInParent<InteractableUnityEventWrapper>().WhenSelect.AddListener(SolveKong);
            var winButton = _playerButtons[myPlayerController.playerID - 1].GetChild(2).GetChild(2).GetChild(1)
                .GetChild(0).GetChild(0);
            winButton.GetComponentInParent<InteractableUnityEventWrapper>().WhenSelect.AddListener(SolveWin);
            var skipButton = _playerButtons[myPlayerController.playerID - 1].GetChild(3).GetChild(2).GetChild(1)
                .GetChild(0).GetChild(0);
            skipButton.GetComponentInParent<InteractableUnityEventWrapper>().WhenSelect.AddListener(SolveSkip);
            _confirmButton = _playerButtons[myPlayerController.playerID - 1].GetChild(4)
                .GetComponentInChildren<Button>();
            _confirmButton.onClick.AddListener(() => PhotonNetwork.LeaveRoom());
            foreach (var playerButton in _playerButtons)
            {
                for (var i = 0; i < 5; i++)
                {
                    playerButton.GetChild(i).gameObject.SetActive(false);
                }
            }

            nowTurn = 1;
            // if (!PhotonNetwork.IsMasterClient) return;
            // foreach (var item in _mahjong[0].GetComponentsInChildren<HandGrabInteractable>())
            // {
            //     item.enabled = true;
            // }
        }

        private void SolveWin()
        {
            _playerButtons[myPlayerController.playerID - 1].GetChild(4).gameObject.SetActive(true);
        }

        private void SolveSkip()
        {
            if (!_canKong && !_canPong && !_canWin) return;
            photonView.RPC(nameof(NextTurn), RpcTarget.All,
                nowTurn, true);
            _canKong = _canPong = _canWin = false;
            photonView.RPC(nameof(ResetButton), RpcTarget.All);
        }

        [PunRPC]
        private void ResetButton()
        {
            foreach (var playerButton in _playerButtons)
            {
                for (var i = 0; i < 4; i++)
                {
                    playerButton.GetChild(i).gameObject.SetActive(false);
                }
            }
        }

        private void AddMahjong(MahjongAttr attr, Rigidbody rb)
        {
            if (!attr.isAdd && !attr.isPut) return;
            var sum = myPlayerController.MyMahjong.Sum(item => item.Value.Count);
            //牌数大于14张，此时不能拿牌
            if (sum >= 14)
            {
                rb.GetComponent<BoxCollider>().isTrigger = true;
                DOTween.Sequence().Insert(0f, rb.DOMove(attr.originPosition, 1f))
                    .Insert(0f, rb.DORotate(attr.originalRotation.eulerAngles, 1f)).onComplete += () =>
                {
                    rb.Sleep();
                    rb.GetComponent<BoxCollider>().isTrigger = false;
                };
                rb.constraints = RigidbodyConstraints.FreezeAll;
            }
            else
            {
                AddMahjongToHand(attr);
            }
        }

        private void AddMahjongToHand(MahjongAttr attr)
        {
            attr.inHand = true;
            if (!myPlayerController.MyMahjong.ContainsKey(attr.id))
            {
                myPlayerController.MyMahjong[attr.id] = new List<GameObject>();
            }

            attr.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
            attr.num = 1;
            myPlayerController.MyMahjong[attr.id].Add(attr.gameObject);
            //新牌，把所有监听事件移除，然后添加监听事件
            attr.pointableUnityEventWrapper.WhenUnselect.RemoveAllListeners();
            attr.pointableUnityEventWrapper.WhenSelect.RemoveAllListeners();
            attr.pointableUnityEventWrapper.WhenUnselect.AddListener(attr.OnPut);
            attr.pointableUnityEventWrapper.WhenSelect.AddListener(attr.OnGrab);

            foreach (var item in attr.GetComponentsInChildren<HandGrabInteractable>())
            {
                item.enabled = true;
            }

            photonView.RPC(nameof(SendEffect), RpcTarget.Others, attr.gameObject);
            photonView.RPC(nameof(RemoveMahjong), RpcTarget.All);
        }

        [PunRPC]
        private void SendEffect(GameObject go)
        {
            go.GetComponent<MahjongAttr>().pointableUnityEventWrapper.WhenSelect.AddListener(() =>
                Debug.LogError(1));
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
                    _mahjong[0].GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.LocalPlayer);
                    DOTween.Sequence().Insert(0f, _mahjong[0].transform.DOMove(myPlayerController.putPos, 1f)).Insert(
                                0f,
                                _mahjong[0].transform.DORotate(GameManager.Instance.GetRotateList()[id - 1], 1f))
                            .SetEase(Ease.Linear)
                            .onComplete +=
                        _mahjong[0].GetComponent<Rigidbody>().Sleep;
                    AddMahjongToHand(_mahjong[0].GetComponent<MahjongAttr>());
                    var idx = 1;
                    foreach (var item in myPlayerController.MyMahjong)
                    {
                        foreach (var iGameObject in item.Value)
                        {
                            var script = iGameObject.GetComponent<MahjongAttr>();
                            if (script.num == 0 || !script.canPlay)
                            {
                                continue;
                            }

                            script.num = idx++;
                        }
                    }
                }
            }
        }

        private void ActivateHandGrab()
        {
            photonView.RPC(nameof(RPCActivateHandGrab), RpcTarget.Others);
        }

        [PunRPC]
        private void RPCActivateHandGrab()
        {
            foreach (var item in _mahjong[0].GetComponentsInChildren<HandGrabInteractable>())
            {
                item.enabled = true;
            }
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
                myPlayerController.SetPlayerStrategy();
                myPlayerController.putPos =
                    GameManager.Instance.GetNewPositions()[myPlayerController.playerID - 1];
                if (!PhotonNetwork.IsMasterClient) continue;
                GameManager.Instance.MahjongSplit(players.Count);
                var a = JsonConvert.SerializeObject(GameManager.Instance.GetMahjongList());
                var b = JsonConvert.SerializeObject(GameManager.Instance.GetUserMahjongLists());
                photonView.RPC(nameof(SetList), RpcTarget.All, a, b);
            }
        }

        /// <summary>
        /// 让主客户端每回合处理牌
        /// </summary>
        private void Update()
        {
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
                    //To Do:该客户端可以处理牌
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

        [PunRPC]
        private void CanP(int id)
        {
            _playerButtons[id - 1].GetChild(0).gameObject.SetActive(true);
            _playerButtons[id - 1].GetChild(3).gameObject.SetActive(true);
            if (myPlayerController.playerID != id) return;
            _canPong = true;
        }

        [PunRPC]
        private void CanK(int id)
        {
            _playerButtons[id - 1].GetChild(1).gameObject.SetActive(true);
            _playerButtons[id - 1].GetChild(3).gameObject.SetActive(true);
            if (myPlayerController.playerID != id) return;
            _canKong = true;
        }

        [PunRPC]
        private void CanH(int id)
        {
            // _playerButtons[id - 1].GetChild(2).GetChild(2).GetChild(1).GetChild(0).GetChild(0).GetComponent<Renderer>()
            //     .material.color = new Color(0.5f, 0.5f, 0.5f);
            // _playerButtons[id - 1].GetChild(3).GetChild(2).GetChild(1).GetChild(0).GetChild(0).GetComponent<Renderer>()
            //     .material.color = new Color(0.5f, 0.5f, 0.5f);
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
        }

        [PunRPC]
        public void NoOneWin()
        {
            text.text = "No one Win!";
            bg.gameObject.SetActive(true);
        }

        [PunRPC]
        public void NotMyTurn()
        {
            myPlayerController.isMyTurn = false;
        }

        public void SortMyMahjong()
        {
            var num = 1;
            foreach (var item in myPlayerController.MyMahjong)
            {
                foreach (var go in item.Value)
                {
                    var script = go.GetComponent<MahjongAttr>();
                    if (!script.canPlay || script.num == 0)
                    {
                        continue;
                    }

                    script.num = num++;

                    DOTween.Sequence().Insert(0f, go.transform.DOMove(
                                GameManager.Instance.GetPickPoses()[myPlayerController.playerID - 1].position +
                                GameManager.Instance.GetBias()[myPlayerController.playerID - 1] *
                                (script.num - 1), 1f)).Insert(0f, go.transform.DORotate(
                                GameManager.Instance.GetRotateList()[myPlayerController.playerID - 1], 1f))
                            .SetEase(Ease.Linear).onComplete +=
                        go.GetComponent<Rigidbody>().Sleep;
                }
            }
        }

        private void SolvePong()
        {
            if (!_canPong) return;
            _canPong = false;
            photonView.RPC(nameof(NextTurn), RpcTarget.All,
                myPlayerController.playerID, false);
            photonView.RPC(nameof(ResetButton), RpcTarget.All);
            foreach (var go in myPlayerController.MyMahjong[nowTile])
            {
                var script = go.GetComponent<MahjongAttr>();
                script.canPlay = false;
                go.transform.DOMove(myPlayerController.putPos, 1f);
                go.transform.DORotate(
                    GameManager.Instance.GetPlayerPutRotations()[
                        myPlayerController.playerID - 1], 1f);
                myPlayerController.putPos -=
                    GameManager.Instance.GetBias()[myPlayerController.playerID - 1];
                script.num = 0;
            }

            var newGo = PhotonNetwork.Instantiate("mahjong_tile_" + nowTile,
                myPlayerController.putPos,
                Quaternion.Euler(GameManager.Instance.GetPlayerPutRotations()[
                    myPlayerController.playerID - 1]));
            newGo.GetComponent<MahjongAttr>().num = 0;
            newGo.GetComponent<MahjongAttr>().canPlay = false;
            myPlayerController.MyMahjong[nowTile].Add(newGo);
            SortMyMahjong();
            photonView.RPC(nameof(DestroyItem), RpcTarget.All, lastTurn);
            myPlayerController.putPos -=
                GameManager.Instance.GetBias()[myPlayerController.playerID - 1];
            _playerButtons[myPlayerController.playerID - 1].GetChild(3).GetChild(2).GetChild(1).GetChild(0).GetChild(0)
                .GetComponent<Renderer>()
                .material.color = Color.white;
        }

        [PunRPC]
        public void DestroyItem(int playerId)
        {
            if (myPlayerController.playerID != playerId) return;
            PhotonNetwork.Destroy(tile);
        }

        // private enum OperationCode
        // {
        //     None = 0,
        //     Pong = 1,
        //     Kong = 2,
        //     Win = 3,
        //     PongAndKong = 4,
        //     PongAndWin = 5,
        //     KongAndWin = 6,
        //     PongAndKongAndWin = 7
        // }
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
                    myPlayerController.MyMahjong[id][0].GetComponent<MahjongAttr>().canPlay)
                {
                    if (ans == 1)
                    {
                        ans = 4;
                    }
                    else
                    {
                        ans = 2;
                    }
                }

                if (CheckWin(id))
                {
                    if (ans == 1)
                    {
                        ans = 5;
                    }
                    else if (ans == 2)
                    {
                        ans = 6;
                    }
                    else if (ans == 4)
                    {
                        ans = 7;
                    }
                    else
                    {
                        ans = 3;
                    }
                }
            }

            return ans;
        }

        public bool CheckWin(int id = 0)
        {
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

            return cnt2 + cnt3 + cnt4 == 5;
        }

        public void SetCamera(Camera canvasCamera)
        {
            canvas.GetComponent<Canvas>().worldCamera = canvasCamera;
        }
    }
}