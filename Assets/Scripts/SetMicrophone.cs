using Photon.Pun;
using Photon.Voice;
using Photon.Voice.Unity;
using UnityEngine;

/// <summary>
/// 设置Photon Voice 的Microphone
/// </summary>
public class SetMicrophone : MonoBehaviourPun
{
    private void Start()
    {
        if (!photonView.IsMine) return;
        var devices = Microphone.devices;
        if (devices.Length > 0)
        {
            Debug.Log(devices);
            GetComponent<Recorder>().MicrophoneDevice = new DeviceInfo(devices[0]);
        }
    }
}