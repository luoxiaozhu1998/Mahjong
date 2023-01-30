using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

public class PlayerListItem : MonoBehaviourPunCallbacks
{
    [SerializeField] private TMP_Text text;
    private Player _player;
    public void Setup(Player player)
    {
        _player = player;
        text.text = player.NickName;
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        
        if (Equals(_player, otherPlayer))
        {
            Destroy(gameObject);
        }
    }

    public override void OnLeftRoom()
    {
        Destroy(gameObject);
    }
}
