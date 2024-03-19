using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Autohand.Demo;

namespace Autohand {
    [DefaultExecutionOrder(10000)]
    public class OVRHandControllerSwapper : MonoBehaviour {
        [Header("Hands")]
        public Hand rightHand;
        public Hand leftHand;

        [Header("Hand Tracking Follows")]
        public Transform rightHandTrackingFollow;
        public Transform leftHandTrackingFollow;

        [Header("Controller Tracking Follows")]
        public Transform rightControllerTrackingFollow;
        public Transform leftControllerTrackingFollow;

        [Header("Hand Tracking Enable")]
        public MonoBehaviour[] enableScriptHandTracking;
        public GameObject[] enableObjectHandTracking;

        [Header("Controller Tracking Enable")]
        public MonoBehaviour[] enableScriptControllerTracking;
        public GameObject[] enableObjectControllerTracking;

        bool handTrackingActive = false;
        bool lastHandTrackingActive = false;

        private void Awake() {
            handTrackingActive = OVRInput.IsControllerConnected(OVRInput.Controller.RHand) && OVRInput.IsControllerConnected(OVRInput.Controller.LHand);
            OnSwap(handTrackingActive);
            lastHandTrackingActive = handTrackingActive;
        }

        private void FixedUpdate() {
            handTrackingActive = OVRInput.IsControllerConnected(OVRInput.Controller.RHand) && OVRInput.IsControllerConnected(OVRInput.Controller.LHand);
            if(handTrackingActive != lastHandTrackingActive)
                OnSwap(handTrackingActive);

            lastHandTrackingActive = handTrackingActive;
        }

        void OnSwap(bool handTrackingActive) {
            if(handTrackingActive) {
                for(int i = 0; i < enableScriptHandTracking.Length; i++)
                    enableScriptHandTracking[i].enabled = true;
                for(int i = 0; i < enableObjectHandTracking.Length; i++)
                    enableObjectHandTracking[i].SetActive(true);
                for(int i = 0; i < enableScriptControllerTracking.Length; i++)
                    enableScriptControllerTracking[i].enabled = false;
                for(int i = 0; i < enableObjectControllerTracking.Length; i++)
                    enableObjectControllerTracking[i].SetActive(false);

                rightHand.follow = rightHandTrackingFollow;
                leftHand.follow = leftHandTrackingFollow;
            }
            else {
                for(int i = 0; i < enableScriptHandTracking.Length; i++)
                    enableScriptHandTracking[i].enabled = false;
                for(int i = 0; i < enableObjectHandTracking.Length; i++)
                    enableObjectHandTracking[i].SetActive(false);
                for(int i = 0; i < enableScriptControllerTracking.Length; i++)
                    enableScriptControllerTracking[i].enabled = true;
                for(int i = 0; i < enableObjectControllerTracking.Length; i++)
                    enableObjectControllerTracking[i].SetActive(true);

                rightHand.follow = rightControllerTrackingFollow;
                leftHand.follow = leftControllerTrackingFollow;
            }
        }
    }

}