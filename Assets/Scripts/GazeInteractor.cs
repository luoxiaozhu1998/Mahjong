using System.Collections;
using System.Collections.Generic;
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

	public Transform _nowHoveredTransform;

	[SerializeField] private GameObject GazeIcon;

	private bool _isCameraNotNull;

	private RaycastHit _hit;
	public CopyMesh CopyMesh;
	private Transform cache;
	private Coroutine _coroutine;
	public TMP_Text message;

	private void Start()
	{
		_isCameraNotNull = _camera != null;
		_transform = transform;
	}

	private void Update()
	{
		if (cache)
		{
			message.text = cache.name;
		}
		else
		{
			message.text = "null";
		}

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
					GazeIcon.transform.position = _hit.point;
					//当前没有hover的麻将，把hover的麻将设置为hit的transform
					if (_hit.transform.CompareTag("Button") && _nowHoveredTransform == null)
					{
						if (cache == null)
						{
							_nowHoveredTransform = _hit.transform;
							_nowHoveredTransform.GetComponent<ButtonAttr>().OnEyeEnter();
						}
						else
						{
							return;
						}
					}
					//当前有hover的麻将，并且hover的麻将与hit的transform不同，把hover的麻将设置为hit的transform，并执行原始的Exit和新的Hover
					else if (_hit.transform.CompareTag("Button") && _nowHoveredTransform != null &&
					         _hit.transform != _nowHoveredTransform)
					{
						if (cache == null)
						{
							_nowHoveredTransform.GetComponent<ButtonAttr>().OnEyeExit();
							_nowHoveredTransform = _hit.transform;
							_nowHoveredTransform.GetComponent<ButtonAttr>().OnEyeEnter();
						}
					}
					//没有新的Hover直接返回
					else if (_hit.transform.CompareTag("Button") && _nowHoveredTransform != null &&
					         _hit.transform == _nowHoveredTransform)

					{
						if (_coroutine != null)
						{
							StopCoroutine(_coroutine);
							cache = null;
							_coroutine = null;
						}
					}
					else if (!_hit.transform.CompareTag("Button") && _nowHoveredTransform != null)
					{
						_nowHoveredTransform.GetComponent<ButtonAttr>().OnEyeExit();
						//_nowHoveredTransform = null;
						_coroutine = StartCoroutine(CacheButton());
					}
				}
				else
				{
					if (_nowHoveredTransform == null) return;
					_nowHoveredTransform.GetComponent<ButtonAttr>().OnEyeExit();
					_nowHoveredTransform = null;
				}

				break;
			//如果是麻将，layerMask为64
			case 64:
				if (Physics.Raycast(_transform.position, _transform.forward * maxDistance, out _hit, maxDistance,
					    _layerMask))
				{
					//当前没有hover的麻将，把hover的麻将设置为hit的transform
					if (_nowHoveredTransform == null)
					{
						_nowHoveredTransform = _hit.transform;
						_nowHoveredTransform.GetComponent<MahjongAttr>().OnEyeHoverEnter();
					}
					//当前有hover的麻将，并且hover的麻将与hit的transform不同，把hover的麻将设置为hit的transform，并执行原始的Exit和新的Hover
					else if (_nowHoveredTransform != null && _hit.transform != _nowHoveredTransform)
					{
						_nowHoveredTransform.GetComponent<MahjongAttr>().OnEyeHoverExit();
						_nowHoveredTransform = _hit.transform;
						_nowHoveredTransform.GetComponent<MahjongAttr>().OnEyeHoverEnter();
					}
					//没有新的Hover直接返回
				}
				else
				{
					if (_nowHoveredTransform == null) return;
					_nowHoveredTransform.GetComponent<MahjongAttr>().OnEyeHoverExit();
					_nowHoveredTransform = null;
				}

				break;
		}
	}

	public IEnumerator CacheButton()
	{
		cache = _nowHoveredTransform;
		yield return new WaitForSeconds(2f);
		cache = null;
		_nowHoveredTransform = null;
	}

	public void PinchSelect()
	{
		CopyMesh.AddPinch();
		if (_nowHoveredTransform == null && cache == null) return;
		if (_nowHoveredTransform.TryGetComponent<Button>(out var button))
		{
			button.onClick.Invoke();
		}
		else if (cache.TryGetComponent<Button>(out var cacheButton))
		{
			cacheButton.onClick.Invoke();
		}
		else if (_nowHoveredTransform.TryGetComponent<TMP_InputField>(out var inputField))
		{
			inputField.ActivateInputField();
		}
		//CopyMesh.AddCount();
	}
}