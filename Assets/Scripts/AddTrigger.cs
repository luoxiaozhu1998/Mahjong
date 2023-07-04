using UnityEngine;

public class AddTrigger : MonoBehaviour
{
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Mahjong"))
        {
            var attr = other.GetComponent<MahjongAttr>();
            attr.isPut = false;
            attr.isAdd = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Mahjong"))
        {
            var attr = other.GetComponent<MahjongAttr>();
            attr.isPut = true;
            attr.isAdd = false;
        }
    }
}