using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Oculus.Avatar2;
using Oculus.Interaction.HandGrab;
using Oculus.Platform;
using Photon.Pun;
using Tools;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Manager
{
    public class ResourceManager
    {
        /// <summary>
        /// 加载的所有麻将
        /// </summary>
        private List<Mahjong> _mahjongList = new();

        /// <summary>
        /// 每个玩家的麻将
        /// </summary>
        private List<List<Mahjong>> _userMahjongLists = new();

        /// <summary>
        /// 每个玩家发牌位置
        /// </summary>
        private readonly List<Transform> _pickPoses = new();

        private List<Mesh> _mahjongs = new();

        // private readonly string _playerControllerPath =
        //     Path.Combine("PhotonPrefabs", "PlayerController");
        private const string PlayerControllerPath = "PUNPlayer";
        private const string photonVoiceSetupPrefabName = "VoiceSetting";
        private readonly List<Vector3> _bias;
        private readonly List<Vector3> _rotate;
        private readonly List<Vector3> _kongRotate;
        private readonly List<Vector3> _new;
        private readonly List<Vector3> _playerInitRotations;
        private readonly List<Vector3> _playerInitPositions;
        private readonly List<Vector3> _playerPutPositions;
        private readonly List<Vector3> _playerPutRotations;
        private readonly List<Vector3> _putMoveList;
        private readonly List<Vector3> _putRotateList;

        public ResourceManager()
        {
            _bias = new List<Vector3>
            {
                new(0f, 0f, 0.05f),
                new(-0.05f, 0f, 0f),
                new(0f, 0f, -0.05f),
                new(0.05f, 0f, 0f)
            };
            _kongRotate = new List<Vector3>
            {
                new(0f, 180f, 0f),
                new(0f, -180f, 0f)
            };
            _new = new List<Vector3>
            {
                new(35.0f, 2.0f, 21.0f),
                new(-21.0f, 2.0f, 35.0f),
                new(-35.0f, 2.0f, -21.0f),
                new(21.0f, 2.0f, -35.0f)
            };

            _rotate = new List<Vector3>
            {
                new(90f, 90f, 0f),
                new(90f, 0f, 0f),
                new(90f, -90f, 0f),
                new(90f, 180f, 0f)
            };

            _playerInitRotations = new List<Vector3>
            {
                new(0f, -90f, 0f),
                new(0f, 180f, 0f),
                new(0f, 90f, 0f),
                new(0f, 0f, 0f)
            };

            _playerInitPositions = new List<Vector3>
            {
                new(0.8f, 0f, 0f),
                new(0f, 0f, 0.8f),
                new(-0.8f, 0f, 0f),
                new(0f, 0f, -0.8f)
            };
            _playerPutPositions = new List<Vector3>
            {
                new(45f, 1f, -15f),
                new(15f, 1f, 45f),
                new(-45f, 1f, 15f),
                new(-15f, 1f, -45f)
            };
            _playerPutRotations = new List<Vector3>
            {
                new(0f, 90f, 0f),
                new(0f, 0f, 0f),
                new(0f, -90f, 0f),
                new(0f, 180f, 0f)
            };
            _putMoveList = new List<Vector3>
            {
                new(25f, 2f, -20f),
                new(20f, 2f, 25f),
                new(-25f, 2f, 20f),
                new(-20f, 2f, -25f)
            };
            _putRotateList = new List<Vector3>
            {
                new(0f, 90f, 0f),
                new(0f, 0f, 0f),
                new(0f, -90f, 0f),
                new(0f, -180f, 0f)
            };
            for (var i = 1; i <= 34; i++)
            {
                _mahjongs.Add(Resources.Load<GameObject>("mahjong_tile_" + i).GetComponent<MeshFilter>().sharedMesh);
            }
        }

        public void InitWhenStart()
        {
            for (var i = 1; i <= Constants.MaxPlayer; i++)
            {
                _pickPoses.Add(GameObject.Find("PickPos" + i).transform);
            }
        }

        public List<Vector3> GetPutMoveList()
        {
            return _putMoveList;
        }

        public List<Vector3> GetPutRotateList()
        {
            return _putRotateList;
        }


        public List<Vector3> GetNewList()
        {
            return _new;
        }

        public List<Vector3> GetRotateList()
        {
            return _rotate;
        }

        public List<Vector3> GetBias()
        {
            return _bias;
        }

        public List<Transform> GetPickPoses()
        {
            return _pickPoses;
        }

        public List<Vector3> GetPlayerPutPositions()
        {
            return _playerPutPositions;
        }

        public List<Vector3> GetPlayerPutRotations()
        {
            return _playerPutRotations;
        }

        /// <summary>
        /// 新创建房间或者房主改变，房主重新生成麻将
        /// </summary>
        public void LoadMahjong()
        {
            for (var j = 0; j < Constants.MaxPlayer; j++)
            {
                for (var i = 1; i <= Constants.MaxId; i++)
                {
                    _mahjongList.Add(
                        new Mahjong(i, "mahjong_tile_" + i));
                }
            }

            _mahjongList = _mahjongList.OrderBy(_ => Guid.NewGuid()).ToList();
        }

        public void ClearMahjong()
        {
            _mahjongList.Clear();
        }

        /// <summary>
        /// 分割麻将为count组,第一组14个,后count-1组13个
        /// </summary>
        /// <param name="count"></param>
        public void MahjongSplit(int count)
        {
            for (var i = 1; i <= count; i++)
            {
                _userMahjongLists.Add(_mahjongList.Take(i == 1 ? 14 : 13)
                    .OrderBy(a => a.ID).ToList());
                _mahjongList.RemoveRange(0, i == 1 ? 14 : 13);
            }
        }

        /// <summary>
        /// 获取当前还未发出的全部麻将的列表
        /// </summary>
        /// <returns></returns>
        public List<Mahjong> GetMahjongList()
        {
            return _mahjongList;
        }

        /// <summary>
        /// 生成编号为id的玩家
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public GameObject GeneratePlayer(int id)
        {
            object[] objects = {Convert.ToInt64(GameManager.Instance.GetUserId())};
            var rig = Object.FindObjectOfType<OVRCameraRig>().transform;
            rig.position = _playerInitPositions[id];
            rig.rotation = Quaternion.Euler(_playerInitRotations[id]);
            var go = PhotonNetwork.Instantiate(PlayerControllerPath,
                rig.position,
                rig.rotation, 0, objects);
            go.transform.SetParent(rig);
            go.transform.localRotation = Quaternion.identity;
            go.transform.localPosition = Vector3.zero;
            var centerEyeAnchor = rig.Find("TrackingSpace/CenterEyeAnchor");
            var voiceSetup = PhotonNetwork.Instantiate(photonVoiceSetupPrefabName, centerEyeAnchor.position,
                centerEyeAnchor.rotation);
            voiceSetup.transform.SetParent(centerEyeAnchor);
            voiceSetup.transform.localPosition = Vector3.zero;
            voiceSetup.transform.localRotation = Quaternion.identity;
            go.GetComponent<StreamingAvatar>().SetLipSync(voiceSetup.GetComponent<OvrAvatarLipSyncContext>());
            voiceSetup.GetComponent<OvrAvatarLipSyncContext>().CaptureAudio = true;
            return go;
        }

        /// <summary>
        /// 生成编号为id的玩家的麻将并且返回麻将id数组
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public SortedDictionary<int, List<GameObject>> GenerateMahjongAtStart(int id)
        {
            var pos = _pickPoses[id].position;
            var count = id == 0 ? 14 : 13;
            var ret = new SortedDictionary<int, List<GameObject>>();
            for (var i = 0; i < count; i++)
            {
                var go = PhotonNetwork.Instantiate(
                    _userMahjongLists[id][i].Name, pos,
                    Quaternion.Euler(_rotate[id]));
                // var script = go.GetComponent<MahjongAttr>();
                // script.id = _userMahjongLists[id][i].ID;
                var pv = go.GetComponent<PhotonView>();
                pv.RPC("SetState", RpcTarget.Others, false);
                go.GetComponent<MahjongAttr>().inHand = true;
                go.GetComponent<MahjongAttr>().id = _userMahjongLists[id][i].ID;
                pos += _bias[id];
                if (!ret.ContainsKey(_userMahjongLists[id][i].ID))
                {
                    ret[_userMahjongLists[id][i].ID] = new List<GameObject>();
                }

                ret[_userMahjongLists[id][i].ID].Add(go);
                // script.num = i + 1;
                // script.canPlay = true;
            }

            return ret;
        }

        public void SetUserMahjongLists(List<List<Mahjong>> useMahjongList)
        {
            _userMahjongLists = useMahjongList;
        }

        public void SetMahjongList(List<Mahjong> mahjongList)
        {
            _mahjongList = mahjongList;
        }

        public List<List<Mahjong>> GetUserMahjongLists()
        {
            return _userMahjongLists;
        }

        public List<List<int>> GetUserMahjongListsInt()
        {
            return _userMahjongLists.Select(list => list.Select(item => item.ID).ToList()).ToList();
        }

        public Mesh GetMahjongMesh(int id)
        {
            return _mahjongs[id - 1];
        }
    }
}