using Photon.Pun;
using UnityEngine;

public class PerspectiveEffectCard : MonoBehaviour
{
    private PhotonView _photonView;

    private void Awake()
    {
        _photonView = GetComponent<PhotonView>();
    }

    public void DestroyThis()
    {
        _photonView.RPC(nameof(RPCDestroyThis), RpcTarget.All);
    }

    [PunRPC]
    public void RPCDestroyThis()
    {
        Destroy(gameObject);
    }
}