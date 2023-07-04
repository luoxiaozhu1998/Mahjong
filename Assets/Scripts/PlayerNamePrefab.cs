using UnityEngine;

/// <summary>
/// 广告牌，让其他玩家ID始终面向当前玩家
/// </summary>
public class PlayerNamePrefab : MonoBehaviour
{
    private Transform _cameraTransform;

    private void Start()
    {
        if (Camera.main != null) _cameraTransform = Camera.main.transform;
    }

    private void LateUpdate()
    {
        transform.LookAt(transform.position + _cameraTransform.forward);
    }
}