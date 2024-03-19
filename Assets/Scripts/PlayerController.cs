using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PlayerController : MonoBehaviourPunCallbacks
{
    //存储当前玩家的所有麻将
    public SortedDictionary<int, List<GameObject>> MyMahjong;

    //存储当前玩家所有麻将的个数到麻将的映射,key表示麻将个数，value表示有key个麻将的麻将的集合
    public Dictionary<int, List<int>> mahjongMap;
    public int playerID;
    public Vector3 putPos;

    private void Awake()
    {
        MyMahjong = new SortedDictionary<int, List<GameObject>>();
        mahjongMap = new Dictionary<int, List<int>>
        {
            {0, new List<int>()},
            {1, new List<int>()},
            {2, new List<int>()},
            {3, new List<int>()},
            {4, new List<int>()}
        };
    }

    public void SetName(int id, string playerName)
    {
        photonView.RPC(nameof(ChangeNameRPC), RpcTarget.Others, photonView.ViewID, playerName, id);
    }

    [PunRPC]
    private void ChangeNameRPC(int viewID, string playerName, int id)
    {
        foreach (var playerController in FindObjectsOfType<PlayerController>())
        {
            if (playerController.GetComponent<PhotonView>().ViewID != viewID) continue;
            playerController.gameObject.name = playerName;
            playerID = id;
            break;
        }
    }
}