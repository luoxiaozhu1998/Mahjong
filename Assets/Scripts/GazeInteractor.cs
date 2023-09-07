using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GazeInteractor : MonoBehaviour
{
    [SerializeField] private float maxDistance = 10f;

    [SerializeField] private LayerMask _layerMask;

    [SerializeField] private Transform _camera;

    private Transform _transform;

    private Transform _nowHoveredTransform;

    [SerializeField] private GameObject GazeIcon;

    private bool _isCameraNotNull;

    private RaycastHit _hit;

    private void Start()
    {
        _isCameraNotNull = _camera != null;
        _transform = transform;
    }

    private void Update()
    {
        if (_isCameraNotNull)
        {
            _transform.position = _camera.position;
        }

        Debug.DrawRay(_transform.position, _transform.forward * maxDistance, Color.red);

        switch (_layerMask.value)
        {
            //如果是UI，layerMask为32
            case 32:
                if (Physics.Raycast(_transform.position, _transform.forward * maxDistance, out _hit, maxDistance,
                        _layerMask))
                {
                    GazeIcon.transform.position = _hit.point + _hit.normal * 0.005f;
                    _nowHoveredTransform = _hit.transform;

                }
                else
                {
                    _nowHoveredTransform = null;
                }

                break;
            //如果是麻将，layerMask为64
            case 64:
                if (Physics.Raycast(_transform.position, _transform.forward * maxDistance, out _hit, maxDistance,
                        _layerMask))
                {
                    //当前没有hover的麻将，把hover的麻将设置为hit的transform
                    if (ReferenceEquals(_nowHoveredTransform, null))
                    {
                        _nowHoveredTransform = _hit.transform;
                        _nowHoveredTransform.GetComponent<MahjongAttr>().OnEyeHoverEnter();
                    }
                    //当前有hover的麻将，并且hover的麻将与hit的transform不同，把hover的麻将设置为hit的transform，并执行原始的Exit和新的Hover
                    else if (!ReferenceEquals(_nowHoveredTransform, null) && _hit.transform != _nowHoveredTransform)
                    {
                        _nowHoveredTransform.GetComponent<MahjongAttr>().OnEyeHoverExit();
                        _nowHoveredTransform = _hit.transform;
                        _nowHoveredTransform.GetComponent<MahjongAttr>().OnEyeHoverEnter();
                    }
                    //没有新的Hover直接返回
                }
                else
                {
                    if (ReferenceEquals(_nowHoveredTransform, null)) return;
                    _nowHoveredTransform.GetComponent<MahjongAttr>().OnEyeHoverExit();
                    _nowHoveredTransform = null;
                }

                break;
        }
    }

    public void PinchSelect()
    {
        if (_nowHoveredTransform != null)
        {
            if (_nowHoveredTransform.TryGetComponent<Button>(out var button))
            {
                button.onClick.Invoke();
            }
            else if (_nowHoveredTransform.TryGetComponent<TMP_InputField>(out var inputField))
            {
                inputField.ActivateInputField();
            }
        }
    }
}