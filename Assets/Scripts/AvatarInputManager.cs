using System.Collections;
using System.Collections.Generic;
using Controller;
using Oculus.Avatar2;
using Oculus.Interaction;
using Oculus.Interaction.Input;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR;

public class AvatarInputManager : OvrAvatarInputManager
{
    private const string logScope = "AvatarInput";

    private OVRCameraRig _ovrCameraRig;


    private IHand LeftHand;


    private IHand RightHand;


    /// <summary>
    /// Transformer is required so calculations can be done in Tracking space
    /// </summary>
    public ITrackingToWorldTransformer Transformer;
    // Only used in editor, produces warnings when packaging
#pragma warning disable CS0414 // is assigned but its value is never used
    [SerializeField] private bool _debugDrawTrackingLocations = false;
#pragma warning restore CS0414 // is assigned but its value is never used

//     public void Init()
//     {
//         LeftHand = GameController.Instance._leftHand as IHand;
//         RightHand = GameController.Instance._rightHand as IHand;
//         Transformer = GameController.Instance._transformer as ITrackingToWorldTransformer;
//         // Debug Drawing
// #if UNITY_EDITOR
// #if UNITY_2019_3_OR_NEWER
//         SceneView.duringSceneGui += OnSceneGUI;
// #else
//         SceneView.onSceneGUIDelegate += OnSceneGUI;
// #endif
// #endif
//     }

    public void Init()
    {
        LeftHand = GameController.Instance._leftHand as IHand;
        RightHand = GameController.Instance._rightHand as IHand;
        Transformer = GameController.Instance._transformer as ITrackingToWorldTransformer;
        _ovrCameraRig = GameController.Instance.OvrCameraRig;
        // If OVRCameraRig doesn't exist, we should set tracking origin ourselves
        if (_ovrCameraRig != null)
        {
            if (OVRManager.instance == null)
            {
                OvrAvatarLog.LogDebug("Creating OVRManager, as one doesn't exist yet.", logScope, this);
                var go = new GameObject("OVRManager");
                var manager = go.AddComponent<OVRManager>();
                manager.trackingOriginType = OVRManager.TrackingOrigin.FloorLevel;
            }
            else
            {
                OVRManager.instance.trackingOriginType = OVRManager.TrackingOrigin.FloorLevel;
            }

            OvrAvatarLog.LogInfo("Setting Tracking Origin to FloorLevel", logScope, this);

            var instances = new List<XRInputSubsystem>();
            SubsystemManager.GetInstances(instances);
            foreach (var instance in instances)
            {
                instance.TrySetTrackingOriginMode(TrackingOriginModeFlags.Floor);
            }
        }

        if (BodyTracking != null)
        {
            if (LeftHand == null || RightHand == null)
            {
                Debug.LogWarning("Use default hand tracking input.");
            }
            else
            {
                Debug.Log("HandTrackingDelegate");
                BodyTracking.HandTrackingDelegate = new PlayerHandTrackingDelegate(LeftHand, RightHand, _ovrCameraRig);
            }

            BodyTracking.InputTrackingDelegate = new SampleInputTrackingDelegate(_ovrCameraRig);
            BodyTracking.InputControlDelegate = new SampleInputControlDelegate();
        }
    }

    protected override void OnDestroyCalled()
    {
#if UNITY_EDITOR
#if UNITY_2019_3_OR_NEWER
        SceneView.duringSceneGui -= OnSceneGUI;
#else
        SceneView.onSceneGUIDelegate -= OnSceneGUI;
#endif
#endif

        base.OnDestroyCalled();
    }

#if UNITY_EDITOR
    #region Debug Drawing

    private void OnSceneGUI(SceneView sceneView)
    {
        if (_debugDrawTrackingLocations)
        {
            DrawTrackingLocations();
        }
    }

    private void DrawTrackingLocations()
    {
        var inputTrackingState = BodyTracking.InputTrackingState;

        float radius = 0.2f;
        Quaternion orientation;
        float outerRadius() => radius + 0.25f;
        Vector3 forward() => orientation * Vector3.forward;

        Handles.color = Color.blue;
        Handles.RadiusHandle(Quaternion.identity, inputTrackingState.headset.position, radius);

        orientation = inputTrackingState.headset.orientation;
        Handles.DrawLine((Vector3) inputTrackingState.headset.position + forward() * radius,
            (Vector3) inputTrackingState.headset.position + forward() * outerRadius());

        radius = 0.1f;
        Handles.color = Color.yellow;
        Handles.RadiusHandle(Quaternion.identity, inputTrackingState.leftController.position, radius);

        orientation = inputTrackingState.leftController.orientation;
        Handles.DrawLine((Vector3) inputTrackingState.leftController.position + forward() * radius,
            (Vector3) inputTrackingState.leftController.position + forward() * outerRadius());

        Handles.color = Color.yellow;
        Handles.RadiusHandle(Quaternion.identity, inputTrackingState.rightController.position, radius);

        orientation = inputTrackingState.rightController.orientation;
        Handles.DrawLine((Vector3) inputTrackingState.rightController.position + forward() * radius,
            (Vector3) inputTrackingState.rightController.position + forward() * outerRadius());
    }

    #endregion

#endif // UNITY_EDITOR
}