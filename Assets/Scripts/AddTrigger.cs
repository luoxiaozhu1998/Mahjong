using UnityEngine;

public class AddTrigger : MonoBehaviour
{
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Mahjong"))
        {
            var attr = other.GetComponent<MahjongAttr>();
            if (attr != null)
            {
                attr.isPut = false;
                attr.isAdd = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Mahjong"))
        {
            var attr = other.GetComponent<MahjongAttr>();
            if (attr != null)
            {
                attr.isPut = true;
                attr.isAdd = false;
            }
        }
    }
}