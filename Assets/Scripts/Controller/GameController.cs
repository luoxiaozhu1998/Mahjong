using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Manager;
using Newtonsoft.Json;

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
        public Button pongButton;
        public Button kongButton;
        public Button addKongButton;
        public Button skipButton;
        public Button winButton;
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
            var idx = 0;
            TweenerCore<Vector3, Vector3, VectorOptions> a = null;
            foreach (var go in myPlayerController.MyMahjong[nowTile])
            {
                var script = go.GetComponent<MahjongAttr>();
                script.canPlay = false;
                if (idx == 1)
                {
                    a = go.transform.DOMove(
                        myPlayerController.putPos - new Vector3(0.0f, 1.0f, 0.0f), 1f);
                }
                else
                {
                    go.transform.DOMove(myPlayerController.putPos - new Vector3(0.0f, 1.0f, 0.0f),
                        1f);
                }

                go.transform.DORotate(
                    GameManager.Instance.GetPlayerPutRotations()[
                        myPlayerController.playerID - 1], 1f);
                myPlayerController.putPos -=
                    GameManager.Instance.GetBias()[myPlayerController.playerID - 1];
                script.num = 0;
                idx++;
            }

            if (a != null)
            {
                var newGo = PhotonNetwork.Instantiate("mahjong_tile_" + nowTile,
                    a.endValue -
                    myPlayerController.MyMahjong[nowTile][0].transform.forward * 1.5f,
                    Quaternion.Euler(GameManager.Instance.GetPlayerPutRotations()[
                        myPlayerController.playerID - 1]));
                myPlayerController.PlayTileStrategy.KongStrategy(newGo.transform);
                newGo.GetComponent<MahjongAttr>().num = 0;
                newGo.GetComponent<MahjongAttr>().canPlay = false;
                myPlayerController.MyMahjong[nowTile].Add(newGo);
            }

            SortMyMahjong();
            _gameManagerPhotonView.RPC(nameof(GameManager.Instance.DestroyItem),
                RpcTarget.All, lastTurn);
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
            if (PhotonNetwork.IsMasterClient)
            {
                GetComponent<PhotonView>().RPC(nameof(NotMyTurn), RpcTarget.Others);
            }

            GeneratePlayers();
            CheckWin();
        }

        [PunRPC]
        private void SetList(string a, string b)
        {
            GameManager.Instance.SetMahjongList(JsonConvert.DeserializeObject<List<Mahjong>>(a));
            GameManager.Instance.SetUserMahjongLists(
                JsonConvert.DeserializeObject<List<List<Mahjong>>>(b));
            myPlayerController.MyMahjong =
                GameManager.Instance.GenerateMahjongAtStart(myPlayerController.playerID - 1);
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
                // canvas.GetComponent<Canvas>().planeDistance = 50f;
                // canvas.GetComponent<Canvas>().worldCamera = myPlayerController
                //     .GetComponent<VRIK_PUN_Player>().vrRig.GetComponentInChildren<Camera>();
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
                        _gameManagerPhotonView.RPC(nameof(GameManager.Instance.CanP), RpcTarget.All,
                            item.Key);
                        break;
                    //可以杠牌
                    case 2:
                        _gameManagerPhotonView.RPC(nameof(GameManager.Instance.CanK), RpcTarget.All,
                            item.Key);
                        break;
                    //可以胡牌
                    case 3:
                        _gameManagerPhotonView.RPC(nameof(GameManager.Instance.CanH), RpcTarget.All,
                            item.Key);
                        break;
                    case 4:
                        _gameManagerPhotonView.RPC(nameof(GameManager.Instance.CanPAndK),
                            RpcTarget.All,
                            item.Key);
                        break;
                    //碰且赢
                    case 5:
                        _gameManagerPhotonView.RPC(nameof(GameManager.Instance.CanPAndH),
                            RpcTarget.All,
                            item.Key);
                        break;
                    case 6:
                        _gameManagerPhotonView.RPC(nameof(GameManager.Instance.CanKAndH),
                            RpcTarget.All,
                            item.Key);
                        break;
                    case 7:
                        _gameManagerPhotonView.RPC(nameof(GameManager.Instance.CanPAndKAndH),
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
                _gameManagerPhotonView.RPC(nameof(GameManager.Instance.NextTurn), RpcTarget.All,
                    nowTurn, true);
            }
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
                    go.transform.DOMove(
                        GameManager.Instance.GetPickPoses()[
                                myPlayerController.playerID - 1]
                            .position +
                        GameManager.Instance.GetBias()[myPlayerController.playerID - 1] *
                        (script.num - 1), 1f);
                }
            }
        }

        private void SolvePong()
        {
            foreach (var go in myPlayerController.MyMahjong[nowTile])
            {
                var script = go.GetComponent<MahjongAttr>();
                script.canPlay = false;
                go.transform.DOMove(myPlayerController.putPos - new Vector3(0.0f, 1.0f, 0.0f), 1f);
                go.transform.DORotate(
                    GameManager.Instance.GetPlayerPutRotations()[
                        myPlayerController.playerID - 1], 1f);
                myPlayerController.putPos -=
                    GameManager.Instance.GetBias()[myPlayerController.playerID - 1];
                script.num = 0;
            }

            var newGo = PhotonNetwork.Instantiate("mahjong_tile_" + nowTile,
                myPlayerController.putPos - new Vector3(0.0f, 1.0f, 0.0f),
                Quaternion.Euler(GameManager.Instance.GetPlayerPutRotations()[
                    myPlayerController.playerID - 1]));
            newGo.GetComponent<MahjongAttr>().num = 0;
            newGo.GetComponent<MahjongAttr>().canPlay = false;
            myPlayerController.MyMahjong[nowTile].Add(newGo);
            SortMyMahjong();
            _gameManagerPhotonView.RPC(nameof(GameManager.Instance.DestroyItem),
                RpcTarget.All, lastTurn);
            myPlayerController.putPos -=
                GameManager.Instance.GetBias()[myPlayerController.playerID - 1];
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

            return cnt2 + cnt3 + cnt4 == 5 && cnt2 == 1;
        }

        public void SetCamera(Camera canvasCamera)
        {
            canvas.GetComponent<Canvas>().worldCamera = canvasCamera;
        }
    }
}