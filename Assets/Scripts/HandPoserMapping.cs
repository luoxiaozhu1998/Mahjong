using RootMotion.FinalIK;
using UnityEngine;
using UnityEngine.InputSystem;

public class HandPoserMapping : MonoBehaviour
{
    private Poser _poser;

    public InteractionTarget NormalHand;
    public InteractionTarget HoverHand;
    public InteractionTarget GrabHand;

    public InputActionReference GrabReference;

    public InteractionTarget Target;

    private bool _isHovering;

    // Start is called before the first frame update
    void Start()
    {
        _poser = GetComponent<Poser>();
        if (Target != null)
        {
            PoserMapping(Target);
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (GetInputAcion(GrabReference) != null && GetInputAcion(GrabReference).ReadValue<float>()>0)
        {
            PoserMapping(GrabHand);
        }
        else
        {
            if (_isHovering)
            {
                PoserMapping(HoverHand);
            }
            else
            {
                PoserMapping(NormalHand);
            }
        }
    }

    public void HoverEnteredAction()
    {
        _isHovering = true;
    }

    public void HoverExitedAction()
    {
        _isHovering = false;
    }

    private void PoserMapping(InteractionTarget target)
    {
        if (_poser != null)
        {
            //if (_poser.poseRoot == null) _poser.weight = 0f;

            _poser.poseRoot = target != null ? target.transform : null;

            _poser.AutoMapping();
            //poser.UpdateManual();
        }
    }

    InputAction GetInputAcion(InputActionReference actionReference)
    {
        return actionReference != null ? actionReference.action : null;
    }
}
