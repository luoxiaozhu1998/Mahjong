using UnityEngine;

public class AddTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Mahjong"))
        {
            var attr = other.GetComponent<MahjongAttr>();
            attr.isAdd = true;
            attr.isPut = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Mahjong"))
        {
            var attr = other.GetComponent<MahjongAttr>();
            attr.isAdd = false;
            attr.isPut = true;
        }
    }
}