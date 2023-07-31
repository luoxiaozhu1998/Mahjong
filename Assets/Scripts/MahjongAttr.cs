using System.Linq;
using Controller;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using Photon.Pun;
using Unity.Mathematics;
using UnityEngine;

public class MahjongAttr : MonoBehaviourPunCallbacks
{
    private PhotonView _photonView;
    public int id;
    public int num;
    private Rigidbody _rigidbody;
    [HideInInspector] public PointableUnityEventWrapper pointableUnityEventWrapper;
    private HandGrabInteractable[] _handGrabInteractable;

    /// <summary>
    /// 是否在自己手中，如果在自己手中，这个bool为true
    /// </summary>
    public bool inMyHand;

    /// <summary>
    /// 是否在他人手中，如果在他人手中，这个bool为true
    /// </summary>
    public bool inOthersHand;

    /// <summary>
    /// 是否在可以打出的区域，如果在可以打出的区域，这个bool为true，此时松手牌视为被打出，这个bool由场景中的碰撞体来确定
    /// </summary>
    public bool isPut;

    /// <summary>
    /// 是否把牌拿入自己的牌堆，用于检测玩家把桌上的牌拿到自己的牌堆的时候，把牌放回原来的位置
    /// </summary>
    public bool isAdd;

    /// <summary>
    /// 是否摆在桌子上，如果没有被任何人拿到手中，且没有被任何人打出，这个bool为true
    /// </summary>
    public bool onDesk;

    /// <summary>
    /// 是否已经被打出，如果被任何人打出，这个bool为true
    /// </summary>
    public bool isThrown;

    private Vector3 _originalPos;
    private GameObject _effectGo;
    private GameObject _eyeInteractGo;
    private Transform _transform;
    private MeshRenderer _meshRenderer;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _meshRenderer = GetComponent<MeshRenderer>();
        _handGrabInteractable = GetComponentsInChildren<HandGrabInteractable>();
        pointableUnityEventWrapper = GetComponent<PointableUnityEventWrapper>();
        pointableUnityEventWrapper.InjectAllPointableUnityEventWrapper(GetComponent<Grabbable>());
        _photonView = photonView;
        pointableUnityEventWrapper.WhenSelect.AddListener(OnGrab);
        pointableUnityEventWrapper.WhenUnselect.AddListener(OnPut);
        _transform = transform;
    }

    public void OnGrab()
    {
        _photonView.RPC(nameof(SetKinematic), RpcTarget.Others, true);
        //如果玩家尝试拿起界外的牌，先记录自己的位置，然后当玩家扔牌的时候，如果扔到自己的手牌，强制归位
        if (isPut && isThrown)
        {
            _originalPos = _transform.position;
        }
    }

    public void OnPut()
    {
        _photonView.RPC(nameof(SetKinematic), RpcTarget.All, false);
        if (inMyHand && isPut)
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
                _photonView.RPC(nameof(RPCSetIsThrown), RpcTarget.All, true);
                _photonView.RPC(nameof(RPCSetInMyHand), RpcTarget.All, false);
                _photonView.RPC(nameof(RPCSetInOthersHand), RpcTarget.All, false);
                _photonView.RPC(nameof(RPCSetOnDesk), RpcTarget.All, false);
                _photonView.RPC(nameof(RPCSetLayer), RpcTarget.All, LayerMask.NameToLayer("Ignore Raycast"));
            }
            //本回合不能出牌，直接整理牌
            else
            {
                GameController.Instance.SortMyMahjong();
            }
        }

        //当玩家尝试把桌子上的牌拿到手牌，强制归位
        if (isAdd && isThrown)
        {
            _transform.position = _originalPos;
        }
    }

    /// <summary>
    /// 设置当前的Rigidbody是否为运动学的
    /// </summary>
    /// <param name="isKinematic">是否是运动学的，当被某人拿在手里，设置为false，否则为true</param>
    [PunRPC]
    private void SetKinematic(bool isKinematic)
    {
        _rigidbody.isKinematic = isKinematic;
    }

    /// <summary>
    /// 设置自己的麻将不能被其他人抓取
    /// </summary>
    [PunRPC]
    public void SetState()
    {
        //对所有其他人，麻将不能抓取
        foreach (var handGrabInteractable in _handGrabInteractable)
        {
            handGrabInteractable.enabled = false;
        }
    }

    /// <summary>
    /// 设置麻将为在自己手中的状态
    /// </summary>
    /// <param name="flag">是否在手中，如果在自己手中，inMyHand为true</param>
    [PunRPC]
    public void RPCSetInMyHand(bool flag)
    {
        inMyHand = flag;
    }

    /// <summary>
    /// 设置麻将为在他人手中的状态
    /// </summary>
    /// <param name="flag">是否在手中，如果在他人手中，inOtherHand为true</param>
    [PunRPC]
    public void RPCSetInOthersHand(bool flag)
    {
        inOthersHand = flag;
    }

    /// <summary>
    /// 设置麻将为已经被扔出的状态
    /// </summary>
    /// <param name="flag">是否已经被打出，如果被任何人打出，这个bool为true</param>
    [PunRPC]
    public void RPCSetIsThrown(bool flag)
    {
        isThrown = flag;
    }

    /// <summary>
    /// 设置麻将为摆在桌上的状态
    /// </summary>
    /// <param name="flag"></param>
    [PunRPC]
    public void RPCSetOnDesk(bool flag)
    {
        onDesk = flag;
    }

    /// <summary>
    /// 设置当前的Layer（用于射线检测）
    /// </summary>
    /// <param name="layer"></param>
    [PunRPC]
    public void RPCSetLayer(int layer)
    {
        gameObject.layer = layer;
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
                _photonView.RPC(nameof(Send), RpcTarget.MasterClient, playerId, 0);
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
                _photonView.RPC(nameof(Send), RpcTarget.MasterClient,
                    GameController.Instance.myPlayerController.playerID, flag);
            }
        }
    }

    [PunRPC]
    public void Send(int playerId, int flag)
    {
        GameController.Instance.ReadyDict.Add(playerId, flag);
    }

    /// <summary>
    /// 其他人接触自己牌的时候，生成护盾特效
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        if (inOthersHand && other.gameObject.CompareTag("Hand") && _eyeInteractGo == null)
        {
            _eyeInteractGo = Instantiate(GameController.Instance.bubbleEffect, transform.position, quaternion.identity);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (_eyeInteractGo != null)
        {
            Destroy(_eyeInteractGo);
        }
    }


    public void OnEyeHoverEnter()
    {
        var materials = _meshRenderer.materials;
        materials[0] = GameController.Instance.transparentMaterials[0];
        materials[1] = GameController.Instance.transparentMaterials[1];
        _meshRenderer.materials = materials;
        _effectGo = Instantiate(GameController.Instance.effectPrefab, _transform.position, _transform.rotation);
    }

    public void OnEyeHoverExit()
    {
        var materials = _meshRenderer.materials;
        materials[0] = GameController.Instance.normalMaterials[0];
        materials[1] = GameController.Instance.normalMaterials[1];
        _meshRenderer.materials = materials;
        Destroy(_effectGo);
    }
}