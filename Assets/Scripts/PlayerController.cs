using System.Collections.Generic;
using Photon.Pun;
using PlayerAttr;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private float _verticalRotation;
    private bool _grounded;
    private Vector3 _smoothMoveVelocity;
    private Vector3 _moveAmount;
    private PhotonView _pv;
    public SortedDictionary<int, List<GameObject>> MyMahjong;
    public bool isMyTurn = true;
    public int playerID;
    public Vector3 putPos;
    public Vector3 putRotate;
    private Transform _mainCamera;
    private Transform _xrOriginTransform;
    public PlayTileStrategy PlayTileStrategy;

    private void Awake()
    {
        _pv = GetComponent<PhotonView>();
        MyMahjong = new SortedDictionary<int, List<GameObject>>();
        //_xrOriginTransform = GameObject.Find("XR Rig").transform;
    }

    // private void Update()
    // {
    //     if (!_pv.IsMine) return;
    //     var transform1 = transform;
    //     _xrOriginTransform.position = transform1.position;
    //     _xrOriginTransform.rotation = transform1.rotation;
    // }

    public void SetPlayerStrategy()
    {
        PlayTileStrategy = playerID switch
        {
            1 => new PlayerAPlayTileStrategy(),
            2 => new PlayerBPlayTileStrategy(),
            3 => new PlayerCPlayTileStrategy(),
            4 => new PlayerDPlayTileStrategy(),
            _ => PlayTileStrategy
        };
    }

    public void BackTrace()
    {
        PlayTileStrategy.BackTrace();
    }
}