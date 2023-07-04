using System.Collections.Generic;
using Oculus.Avatar2;
using UnityEngine;
using UnityEngine.SceneManagement;
using Oculus.Platform;

namespace Manager
{
    /// <summary>
    /// 游戏总管理类,外观模式+中介者模式
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        private ulong _userId;
        public static GameManager Instance { get; private set; }
        private ResourceManager _resourceManager;
        private MenuManager _menuManager;
        private string _playerName;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                _resourceManager = new ResourceManager();
                _menuManager = new MenuManager();
                DontDestroyOnLoad(gameObject);
            }

            // 初始化oculus环境
            try
            {
                Core.AsyncInitialize();
                Entitlements.IsUserEntitledToApplication().OnComplete(EntitlementCallback);
            }
            catch (UnityException e)
            {
                Debug.LogError("Platform failed to initialize due to exception.");
                Debug.LogException(e);
                // Immediately quit the application.
                UnityEngine.Application.Quit();
            }
        }

        // Called when the Meta Quest Platform completes the async entitlement check request and a result is available.
        void EntitlementCallback(Message msg)
        {
            if (msg.IsError) // User failed entitlement check
            {
                // Implements a default behavior for an entitlement check failure -- log the failure and exit the app.
                Debug.LogError("You are NOT entitled to use this app.");
                UnityEngine.Application.Quit();
            }
            else // User passed entitlement check
            {
                // Log the succeeded entitlement check for debugging.
                Debug.Log("You are entitled to use this app.");
                GetTokens();
            }
        }

        private void GetTokens()
        {
            Users.GetAccessToken().OnComplete(message =>
            {
                if (!message.IsError)
                {
                    OvrAvatarEntitlement.SetAccessToken(message.Data);
                    Users.GetLoggedInUser().OnComplete(message =>
                    {
                        if (!message.IsError)
                        {
                            _userId = message.Data.ID;
                            SceneManager.LoadSceneAsync(1);
                        }
                        else
                        {
                            var e = message.GetError();
                            Debug.LogError(e);
                        }
                    });
                }
                else
                {
                    var e = message.GetError();
                    Debug.LogError(e);
                }
            });
        }

        #region ResourceManager

        public List<Vector3> GetPutMoveList()
        {
            return _resourceManager.GetPutMoveList();
        }

        public List<Vector3> GetPutRotateList()
        {
            return _resourceManager.GetPutRotateList();
        }

        public List<Mahjong> GetMahjongList()
        {
            return _resourceManager.GetMahjongList();
        }

        public Mesh GetMahjongMesh(int id)
        {
            return _resourceManager.GetMahjongMesh(id);
        }

        public void SetMahjongList(List<Mahjong> mahjongList)
        {
            _resourceManager.SetMahjongList(mahjongList);
        }

        public List<List<Mahjong>> GetUserMahjongLists()
        {
            return _resourceManager.GetUserMahjongLists();
        }

        public void SetUserMahjongLists(List<List<Mahjong>> userMahjongList)
        {
            _resourceManager.SetUserMahjongLists(userMahjongList);
        }

        public void MahjongSplit(int count)
        {
            _resourceManager.MahjongSplit(count);
        }

        public void InitWhenStart()
        {
            _resourceManager.InitWhenStart();
        }

        public List<Transform> GetPickPoses()
        {
            return _resourceManager.GetPickPoses();
        }

        public SortedDictionary<int, List<GameObject>> GenerateMahjongAtStart(int id)
        {
            return _resourceManager.GenerateMahjongAtStart(id);
        }

        public GameObject GeneratePlayer(int id)
        {
            return _resourceManager.GeneratePlayer(id);
        }

        public List<Vector3> GetRotateList()
        {
            return _resourceManager.GetRotateList();
        }

        public List<Vector3> GetBias()
        {
            return _resourceManager.GetBias();
        }

        public List<Vector3> GetPlayerPutPositions()
        {
            return _resourceManager.GetPlayerPutPositions();
        }

        public List<Vector3> GetNewPositions()
        {
            return _resourceManager.GetNewList();
        }

        public List<Vector3> GetPlayerPutRotations()
        {
            return _resourceManager.GetPlayerPutRotations();
        }

        public void LoadMahjong()
        {
            _resourceManager.LoadMahjong();
        }

        #endregion

        #region MenuManager

        public void AddMenu(string name, GameObject go)
        {
            _menuManager.AddMenu(name, go);
        }

        public void OpenMenu(string menuName)
        {
            _menuManager.OpenMenu(menuName);
        }

        public void CloseMenu(string menuName)
        {
            _menuManager.CloseMenu(menuName);
        }

        #endregion

        public ulong GetUserId()
        {
            return _userId;
        }

        public void SetPlayerName(string str)
        {
            _playerName = str;
        }

        public string GetPlayerName()
        {
            return _playerName;
        }
        // /// <summary>
        // /// 给出牌权
        // /// </summary>
        // /// <param name="nextUserId">下一个出牌用户编号</param>
        // /// <param name="drawTile">给不给他发牌</param>
        // [PunRPC]
        // public void NextTurn(int nextUserId, bool drawTile)
        // {
        //     if (GameController.Instance.myPlayerController.playerID == nextUserId)
        //     {
        //         GameController.Instance.myPlayerController.isMyTurn = true;
        //         if (drawTile)
        //         {
        //             var go = PhotonNetwork.Instantiate(GetMahjongList()[0].Name,
        //                 GameController.Instance.myPlayerController.putPos,
        //                 Quaternion.Euler(GetRotateList()[nextUserId - 1]));
        //             var newScript = go.GetComponent<MahjongAttr>();
        //             newScript.canPlay = true;
        //             var myMahjong = GameController.Instance.myPlayerController.MyMahjong;
        //             newScript.id = GetMahjongList()[0].ID;
        //             newScript.num = 10;
        //             if (!myMahjong.ContainsKey(newScript.id))
        //             {
        //                 myMahjong[newScript.id] = new List<GameObject>();
        //             }
        //
        //             if (GameController.Instance.CheckWin(newScript.id))
        //             {
        //                 // photonView.RPC(nameof(CanH), RpcTarget.All,
        //                 //     GameController.Instance.myPlayerController.playerID);
        //             }
        //
        //             myMahjong[newScript.id].Add(go);
        //             var idx = 1;
        //             foreach (var item in myMahjong)
        //             {
        //                 foreach (var iGameObject in item.Value)
        //                 {
        //                     var script = iGameObject.GetComponent<MahjongAttr>();
        //                     if (script.num == 0 || !script.canPlay)
        //                     {
        //                         continue;
        //                     }
        //
        //                     script.num = idx++;
        //                 }
        //             }
        //
        //             if (myMahjong[newScript.id].Count == 4)
        //             {
        //                 // photonView.RPC(nameof(AddKong), RpcTarget.All,
        //                 //     GameController.Instance.myPlayerController.playerID, newScript.id);
        //             }
        //         }
        //     }
        //     else
        //     {
        //         GameController.Instance.myPlayerController.isMyTurn = false;
        //     }
        //
        //     if (drawTile)
        //     {
        //         GetMahjongList().RemoveAt(0);
        //     }
        // }

        // [PunRPC]
        // public void PlayTile(int id, int playerID)
        // {
        //     GameController.Instance.lastTurn = playerID;
        //     //每个客户端先把把当前轮次的ID设置好（下面代码可能会更改）
        //     GameController.Instance.nowTurn = playerID == PhotonNetwork.CurrentRoom.PlayerCount
        //         ? 1
        //         : playerID + 1;
        //     //每个客户端先把把当前轮次的牌ID设置好（下面代码可能会更改）
        //     GameController.Instance.nowTile = id;
        //     var thisID = GameController.Instance.myPlayerController.playerID;
        //     //打出牌的一定准备好了
        //     if (playerID == thisID)
        //     {
        //         //是主客户端，直接加入
        //         if (PhotonNetwork.IsMasterClient)
        //         {
        //             GameController.Instance.ReadyDict.Add(playerID, 0);
        //         }
        //         //向主客户端发送自己的状态
        //         else
        //         {
        //             photonView.RPC(nameof(Send), RpcTarget.MasterClient, playerID, 0);
        //         }
        //     }
        //     else
        //     {
        //         //check自己的状态
        //         var flag = GameController.Instance.CheckMyState(id);
        //         //是主客户端，直接加入
        //         if (PhotonNetwork.IsMasterClient)
        //         {
        //             GameController.Instance.ReadyDict.Add(
        //                 GameController.Instance.myPlayerController.playerID, flag);
        //         }
        //         //向主客户端发送自己的状态
        //         else
        //         {
        //             photonView.RPC(nameof(Send), RpcTarget.MasterClient,
        //                 GameController.Instance.myPlayerController.playerID, flag);
        //         }
        //     }
        // }
        //
        // [PunRPC]
        // public void Send(int id, int flag)
        // {
        //     GameController.Instance.ReadyDict.Add(id, flag);
        // }
        //
        //
        // [PunRPC]
        // public void CanNext(bool flag)
        // {
        //     GameController.Instance.canNext = flag;
        // }

        // /// <summary>
        // /// 可以碰牌的客户端
        // /// </summary>
        // /// <param name="id">客户端id</param>
        // [PunRPC]
        // public void CanP(int id)
        // {
        //     if (id != GameController.Instance.myPlayerController.playerID) return;
        //     GameController.Instance.pongButton.gameObject.SetActive(true);
        //     GameController.Instance.skipButton.gameObject.SetActive(true);
        // }
        //
        // /// <summary>
        // /// 可以杠牌的客户端
        // /// </summary>
        // /// <param name="id">客户端id</param>
        // [PunRPC]
        // public void CanK(int id)
        // {
        //     if (id != GameController.Instance.myPlayerController.playerID) return;
        //     GameController.Instance.pongButton.gameObject.SetActive(true);
        //     GameController.Instance.kongButton.gameObject.SetActive(true);
        //     GameController.Instance.skipButton.gameObject.SetActive(true);
        // }
        //
        // [PunRPC]
        // public void AddKong(int playerId, int tileId)
        // {
        //     if (playerId != GameController.Instance.myPlayerController.playerID) return;
        //     GameController.Instance.addKongButton.gameObject.SetActive(true);
        //     GameController.Instance.skipButton.gameObject.SetActive(true);
        //     GameController.Instance.nowTile = tileId;
        // }

        // /// <summary>
        // /// 可以胡牌的客户端
        // /// </summary>
        // /// <param name="id">客户端id</param>
        // [PunRPC]
        // public void CanH(int id)
        // {
        //     if (id != GameController.Instance.myPlayerController.playerID) return;
        //     GameController.Instance.winButton.gameObject.SetActive(true);
        //     GameController.Instance.skipButton.gameObject.SetActive(true);
        // }
        //
        // [PunRPC]
        // public void CanPAndK(int id)
        // {
        //     if (id != GameController.Instance.myPlayerController.playerID) return;
        //     GameController.Instance.pongButton.gameObject.SetActive(true);
        //     GameController.Instance.kongButton.gameObject.SetActive(true);
        //     GameController.Instance.skipButton.gameObject.SetActive(true);
        // }
        //
        // [PunRPC]
        // public void CanPAndH(int id)
        // {
        //     if (id != GameController.Instance.myPlayerController.playerID) return;
        //     GameController.Instance.pongButton.gameObject.SetActive(true);
        //     GameController.Instance.winButton.gameObject.SetActive(true);
        //     GameController.Instance.skipButton.gameObject.SetActive(true);
        // }
        //
        // [PunRPC]
        // public void CanKAndH(int id)
        // {
        //     if (id != GameController.Instance.myPlayerController.playerID) return;
        //     GameController.Instance.kongButton.gameObject.SetActive(true);
        //     GameController.Instance.winButton.gameObject.SetActive(true);
        //     GameController.Instance.skipButton.gameObject.SetActive(true);
        // }
        //
        // [PunRPC]
        // public void CanPAndKAndH(int id)
        // {
        //     if (id != GameController.Instance.myPlayerController.playerID) return;
        //     GameController.Instance.pongButton.gameObject.SetActive(true);
        //     GameController.Instance.kongButton.gameObject.SetActive(true);
        //     GameController.Instance.winButton.gameObject.SetActive(true);
        //     GameController.Instance.skipButton.gameObject.SetActive(true);
        // }
        //
        // [PunRPC]
        // public void HideButton()
        // {
        //     GameController.Instance.pongButton.gameObject.SetActive(false);
        //     GameController.Instance.kongButton.gameObject.SetActive(false);
        //     GameController.Instance.skipButton.gameObject.SetActive(false);
        //     GameController.Instance.addKongButton.gameObject.SetActive(false);
        //     GameController.Instance.winButton.gameObject.SetActive(false);
        // }
        // [PunRPC]
        // public void DestroyItem(int playerId)
        // {
        //     if (GameController.Instance.myPlayerController.playerID != playerId) return;
        //     PhotonNetwork.Destroy(GameController.Instance.tile);
        //     GameController.Instance.myPlayerController.BackTrace();
        // }
        //
        // [PunRPC]
        // public void ShowResult()
        // {
        //     GameController.Instance.bg.gameObject.SetActive(true);
        //     GameController.Instance.text.text = "You Lose!";
        // }

        // [PunRPC]
        // private void SendMaxId(int id)
        // {
        //     Constants.MaxId = id;
        // }
    }
}