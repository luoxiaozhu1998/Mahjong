using System;
using System.Linq;
using Controller;
using Oculus.Interaction;
using Oculus.Interaction.Grab;
using Oculus.Interaction.HandGrab;
using Photon.Pun;
using UnityEngine;

public class MahjongAttr : MonoBehaviourPunCallbacks
{
    private PhotonView _photonView;
    public int id;
    public int num;
    public bool canPlay;
    private Rigidbody _rigidbody;
    [HideInInspector] public PointableUnityEventWrapper pointableUnityEventWrapper;
    private HandGrabInteractable[] _handGrabInteractable;
    public bool inHand = false;
    public bool isPut = true;
    public bool isAdd = false;
    private Vector3 _originPosition;
    private GameObject _effectGo;
    private GameObject _eyeInteractGo;
    [HideInInspector] public int ownerID;
    private Transform _transform;

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
        isPut = true;
        _transform = transform;
    }

    public void OnGrab()
    {
        _photonView.RPC(nameof(SetKinematic), RpcTarget.Others, true);
    }

    public void OnPut()
    {
        _photonView.RPC(nameof(SetKinematic), RpcTarget.All, false);
        if (isPut)
        {
            var playerController = GameController.Instance.myPlayerController;
            var playerId = playerController.playerID;
            //可以出牌，先把牌移除，再整理牌
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

                _photonView.RPC(nameof(PlayTile), RpcTarget.All, playerId, id, gameObject.GetPhotonView().ViewID);
            }
            //本回合不能出牌，直接整理牌
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
    public void SetState()
    {
        //对所有其他人，麻将不能抓取
        foreach (var handGrabInteractable in _handGrabInteractable)
        {
            handGrabInteractable.enabled = false;
        }
    }


    [PunRPC]
    private void PlayTile(int playerId, int tileId, int viewID)
    {
        GameController.Instance.lastTurn = playerId;
        //每个客户端先把把当前轮次的ID设置好（下面代码可能会更改）
        GameController.Instance.nowTurn = playerId == PhotonNetwork.CurrentRoom.PlayerCount
            ? 1
            : playerId + 1;
        //每个客户端先把把当前轮次的牌ID设置好（下面代码可能会更改）
        GameController.Instance.nowTile = tileId;
        GameController.Instance.tileViewID = viewID;
        var thisID = GameController.Instance.myPlayerController.playerID;
        //牌打出之后，ownerID为0
        _photonView.RPC(nameof(SetOwnerID), RpcTarget.All, 0);
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
    public void Send(int playerId, int flag)
    {
        GameController.Instance.ReadyDict.Add(playerId, flag);
    }

    [PunRPC]
    public void StoreTile(int viewID)
    {
        GameController.Instance.tileViewID = viewID;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_photonView.IsMine && other.gameObject.CompareTag("Hand") && _effectGo == null)
        {
            _effectGo = PhotonNetwork.Instantiate(GameController.Instance.bubbleEffect.name, _transform.position,
                _transform.rotation);
        }

        if (!_photonView.IsMine && other.gameObject.CompareTag("EyeInteractor") && _eyeInteractGo == null)
        {
            var materials = GetComponent<MeshRenderer>().materials;
            materials[0] = GameController.Instance.transparentMaterials[0];
            materials[1] = GameController.Instance.transparentMaterials[1];
            GetComponent<MeshRenderer>().materials = materials;
            _eyeInteractGo = PhotonNetwork.Instantiate(GameController.Instance.effectPrefab.name, _transform.position,
                _transform.rotation);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (ownerID != GameController.Instance.myPlayerController.playerID && ownerID != 0 &&
            other.gameObject.CompareTag("Hand") &&
            _effectGo != null)
        {
            PhotonNetwork.Destroy(_effectGo);
        }

        if (ownerID != GameController.Instance.myPlayerController.playerID && ownerID != 0 &&
            other.gameObject.CompareTag("EyeInteractor") && _eyeInteractGo != null)
        {
            PhotonNetwork.Destroy(_eyeInteractGo);
            var mats = GetComponent<MeshRenderer>().materials;
            mats[0] = GameController.Instance.normalMaterials[0];
            mats[1] = GameController.Instance.normalMaterials[1];
            GetComponent<MeshRenderer>().materials = mats;
        }
    }

    [PunRPC]
    public void SetOwnerID(int id)
    {
        ownerID = id;
    }

    // private void OnCollisionEnter(Collision other)
    // {
    //     if (other.gameObject.CompareTag("Hand"))
    //     {
    //         var o = gameObject;
    //         _effectGo = PhotonNetwork.Instantiate(GameController.Instance.BubbleEffect.name, o.transform.position,
    //             o.transform.rotation);
    //     }
    // }
    //
    // private void OnCollisionExit(Collision other)
    // {
    //     if (other.gameObject.CompareTag("Hand"))
    //     {
    //         pointableUnityEventWrapper.WhenUnhover.AddListener(() => { PhotonNetwork.Destroy(_effectGo); });
    //     }
    // }
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