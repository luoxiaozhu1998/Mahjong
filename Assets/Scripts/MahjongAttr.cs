using System.Collections;
using System.Linq;
using Controller;
using Manager;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using Photon.Pun;
using UnityEngine;

public class MahjongAttr : MonoBehaviourPunCallbacks
{
    private readonly Vector3 _pos = Vector3.zero;
    private PhotonView _photonView;
    public int id;
    public int num;
    private PhotonView _gameManagerPhotonView;
    public bool canPlay;
    private Vector3 _moveto;
    private Vector3 _rotateTo;
    private bool _isGrounded = true;
    public Transform parentTrans;
    public bool isSet = false;
    private Rigidbody _rigidbody;
    public PointableUnityEventWrapper pointableUnityEventWrapper;
    private HandGrabInteractable[] _handGrabInteractable;
    public bool inHand = false;
    public bool isPut = true;
    public bool isAdd = false;
    public Vector3 originPosition;
    public Quaternion originalRotation;
    private Renderer _renderer;

    private void Awake()
    {
        //_gameManagerPhotonView = GameManager.Instance.GetComponent<PhotonView>();
        _rigidbody = GetComponent<Rigidbody>();
        _handGrabInteractable = GetComponentsInChildren<HandGrabInteractable>();
        //id = int.Parse(name[..^7][13..]);
        pointableUnityEventWrapper = GetComponent<PointableUnityEventWrapper>();
        pointableUnityEventWrapper.InjectAllPointableUnityEventWrapper(GetComponent<Grabbable>());
        _photonView = GetComponent<PhotonView>();
        pointableUnityEventWrapper.WhenSelect.AddListener(OnGrab);
        pointableUnityEventWrapper.WhenUnselect.AddListener(OnPut);
        var transform1 = transform;
        originPosition = transform1.position;
        originalRotation = transform1.rotation;
        isPut = true;
        _renderer = GetComponent<Renderer>();
        // GetComponent<XRGrabInteractable>().hoverEntered.AddListener(_ => { OnHover(); });
        // GetComponent<XRGrabInteractable>().hoverExited.AddListener(_ => { OnHoverExit(); });
        // GetComponent<XRGrabInteractable>().activated.AddListener(_ => { OnTrigger(); });
        //GetComponent<XRGrabInteractable>().firstSelectEntered.AddListener(_ => { OnGrab(); });
    }

    public void OnGrab()
    {
        _photonView.RPC(nameof(SetKinematic), RpcTarget.All, true);
        if (_photonView.IsMine)
            return;
        _renderer.material.color = Color.red;
    }

    public void OnPut()
    {
        _photonView.RPC(nameof(SetKinematic), RpcTarget.All, false);
        if (isPut)
        {
            var playerController = GameController.Instance.myPlayerController;
            var playerId = playerController.playerID;
            if (GameController.Instance.nowTurn == playerId)
            {
                GameObject go = null;
                foreach (var item in playerController.MyMahjong[id]
                             .Where(item => item.GetComponent<MahjongAttr>().num == num))
                {
                    go = item;
                }

                if (go != null)
                {
                    playerController.MyMahjong[id].Remove(go);
                }

                GameController.Instance.SortMyMahjong();

                _photonView.RPC(nameof(PlayTile), RpcTarget.All, playerId, id);
                _photonView.RPC(nameof(StoreTile), RpcTarget.MasterClient, gameObject);
            }
            else
            {
                GameController.Instance.SortMyMahjong();
            }
        }
    }

    [PunRPC]
    private void SetKinematic(bool b)
    {
        _rigidbody.isKinematic = b;
    }

    [PunRPC]
    public void SetState(bool b)
    {
        pointableUnityEventWrapper.WhenSelect.AddListener(() =>
        {
            _renderer.material.color = Color.red;
            foreach (var handGrabInteractable in _handGrabInteractable)
            {
                handGrabInteractable.enabled = b;
            }

            if (!b)
            {
                StartCoroutine(ResetState());
            }
        });
    }

    private IEnumerator ResetState()
    {
        yield return new WaitForSeconds(2f);
        foreach (var handGrabInteractable in _handGrabInteractable)
        {
            handGrabInteractable.enabled = true;
        }

        _renderer.material.color = Color.white;
    }


    [PunRPC]
    private void PlayTile(int playerId, int tileId)
    {
        GameController.Instance.lastTurn = playerId;
        //每个客户端先把把当前轮次的ID设置好（下面代码可能会更改）
        GameController.Instance.nowTurn = playerId == PhotonNetwork.CurrentRoom.PlayerCount
            ? 1
            : playerId + 1;
        //每个客户端先把把当前轮次的牌ID设置好（下面代码可能会更改）
        GameController.Instance.nowTile = tileId;
        var thisID = GameController.Instance.myPlayerController.playerID;
        //打出牌的一定准备好了
        if (playerId == thisID)
        {
            //是主客户端，直接加入
            if (PhotonNetwork.IsMasterClient)
            {
                GameController.Instance.ReadyDict.Add(playerId, 0);
            }
            //向主客户端发送自己的状态
            else
            {
                photonView.RPC(nameof(Send), RpcTarget.MasterClient, playerId, 0);
            }
        }
        else
        {
            //check自己的状态
            var flag = GameController.Instance.CheckMyState(tileId);
            //是主客户端，直接加入
            if (PhotonNetwork.IsMasterClient)
            {
                GameController.Instance.ReadyDict.Add(
                    GameController.Instance.myPlayerController.playerID, flag);
            }
            //向主客户端发送自己的状态
            else
            {
                photonView.RPC(nameof(Send), RpcTarget.MasterClient,
                    GameController.Instance.myPlayerController.playerID, flag);
            }
        }
    }

    [PunRPC]
    public void Send(int id, int flag)
    {
        GameController.Instance.ReadyDict.Add(id, flag);
    }

    [PunRPC]
    public void StoreTile(GameObject go)
    {
        GameController.Instance.tile = go;
    }
    // private void OnHover()
    // {
    //     if (!photonView.IsMine) return;
    //     if (!canPlay) return;
    //     //transform1.localScale = new Vector3(3f, 3f, 3f);
    //     if (!_isGrounded) return;
    //     transform.position += new Vector3(0f, 1f, 0f);
    //     _isGrounded = false;
    // }
    //
    // private void OnHoverExit()
    // {
    //     if (!photonView.IsMine) return;
    //     if (!canPlay) return;
    //     if (_isGrounded) return;
    //     //transform1.localScale = new Vector3(2f, 2f, 2f);
    //     transform.position -= new Vector3(0f, 1f, 0f);
    //     _isGrounded = true;
    // }
    //
    // private void OnGrab()
    // {
    //     if(isSet) return;
    //     var go = new GameObject();
    //     go.transform.position = gameObject.transform.position;
    //     go.transform.SetParent(parentTrans);
    //     parentTrans.GetComponent<XRSocketInteractor>().attachTransform = go.transform;
    //     isSet = true;
    // }

    // private void OnTrigger()
    // {
    //     if (!photonView.IsMine) return;
    //     if (!canPlay) return;
    //     if (!GameController.Instance.myPlayerController.isMyTurn) return;
    //     GetComponent<BoxCollider>().isTrigger = true;
    //     var transform1 = transform;
    //     _myPlayerController.PlayTileStrategy.MahjongPut(transform1);
    //     _myPlayerController.PlayTileStrategy.MahjongRotate(transform1);
    //     //_gameManagerPhotonView.RPC(nameof(GameManager.instance.SendID), RpcTarget.Others,id);
    //     if (GameController.Instance.myPlayerController.MyMahjong[id].Count == 1)
    //     {
    //         GameController.Instance.myPlayerController.MyMahjong.Remove(id);
    //     }
    //     else
    //     {
    //         GameObject t = null;
    //         foreach (var iGameObject in GameController.Instance.myPlayerController.MyMahjong[id])
    //         {
    //             if (iGameObject.GetComponent<MahjongAttr>().num == num)
    //             {
    //                 t = iGameObject;
    //             }
    //         }
    //
    //         if (t != null)
    //         {
    //             GameController.Instance.myPlayerController.MyMahjong[id].Remove(t);
    //         }
    //     }
    //
    //     GameController.Instance.SortMyMahjong();
    //     //我打出一张牌
    //     _gameManagerPhotonView.RPC(nameof(GameManager.Instance.PlayTile), RpcTarget.All, id,
    //         GameController.Instance.myPlayerController.playerID);
    //     GameController.Instance.NotMyTurn();
    //     //_gameManagerPhotonView.RPC(nameof(GameManager.instance.StoreTile), RpcTarget.MasterClient, gameObject);
    //     GameController.Instance.tile = gameObject;
    // }
}