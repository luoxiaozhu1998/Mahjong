using System;
using System.Collections.Generic;
using Oculus.Avatar2;
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
    private ulong _userId;
    private SampleAvatarEntity _sampleAvatarEntity;
    private bool _beginSync = false;

    private void Awake()
    {
        _pv = GetComponent<PhotonView>();
        _sampleAvatarEntity = GetComponent<SampleAvatarEntity>();
        MyMahjong = new SortedDictionary<int, List<GameObject>>();
    }
    

    public void BeginSync()
    {
        _beginSync = true;
    }

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

    public void SetUserId(ulong userId)
    {
        _userId = userId;
    }

    public ulong GetUserId()
    {
        return _userId;
    }
}