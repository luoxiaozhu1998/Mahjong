// using System;
// using System.Collections;
// using Oculus.Avatar2;
// using Photon.Pun;
// using UnityEngine;
//
// public class StreamingAvatarEntity : SampleAvatarEntity
// {
//     private PhotonView _photonView;
//     private string _userName;
//     private byte[] _avatarBytes;
//     private readonly WaitForSeconds _waitTime = new(0.08f);
//
//     protected override void Awake()
//     {
//         _photonView = GetComponent<PhotonView>();
//         if (_photonView.IsMine)
//         {
//             SetIsLocal(true);
//             var inputManager = FindObjectOfType<SampleInputManager>();
//             SetBodyTracking(inputManager);
//             SetLipSync(FindObjectOfType<OvrAvatarLipSyncContext>());
//             SetFacePoseProvider(inputManager.GetComponent<SampleFacePoseBehavior>());
//             SetEyePoseProvider(inputManager.GetComponent<SampleEyePoseBehavior>());
//             _creationInfo.features = CAPI.ovrAvatar2EntityFeatures.Preset_Default;
//         }
//         else
//         {
//             SetIsLocal(false);
//             _creationInfo.features = CAPI.ovrAvatar2EntityFeatures.Preset_Remote;
//         }
//
//         OnUserAvatarLoadedEvent.AddListener(_ => { StartCoroutine(nameof(StreamAvatarData)); });
//         base.Awake();
//     }
//
//     protected override IEnumerator Start()
//     {
//         var instantiationData = _photonView.InstantiationData;
//         _userId = Convert.ToUInt64(instantiationData[0]);
//         _userName = instantiationData[1].ToString();
//         return base.Start();
//     }
//
//
//     private IEnumerator StreamAvatarData()
//     {
//         _avatarBytes = RecordStreamData(StreamLOD.Medium);
//         _photonView.RPC(nameof(ApplyAvatarData), RpcTarget.Others, _avatarBytes);
//         yield return _waitTime;
//         StartCoroutine(StreamAvatarData());
//     }
//
//     [PunRPC]
//     private void ApplyAvatarData(byte[] bytes)
//     {
//         _avatarBytes = bytes;
//         ApplyStreamData(_avatarBytes);
//     }
// }