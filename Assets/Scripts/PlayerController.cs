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
    private ulong _userId;
    private SampleAvatarEntity _sampleAvatarEntity;

    private void Awake()
    {
        _pv = GetComponent<PhotonView>();
        _sampleAvatarEntity = GetComponent<SampleAvatarEntity>();
        MyMahjong = new SortedDictionary<int, List<GameObject>>();
    }
}