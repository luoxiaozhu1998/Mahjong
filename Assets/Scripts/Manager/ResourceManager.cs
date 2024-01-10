using System;
using System.Collections.Generic;
using System.Linq;
using Controller;
using Oculus.Avatar2;
using Photon.Pun;
using Tools;
using Unity.Mathematics;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = System.Random;

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

        private readonly List<Mesh> _mahjongs = new();

        // private readonly string _playerControllerPath =
        //     Path.Combine("PhotonPrefabs", "PlayerController");
        private const string PlayerControllerPath = "PUNPlayer";
        private const string PhotonVoiceSetupPrefabName = "VoiceSetting";
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
        private static Random rng = new();

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
                new(0.56f, 0.83f, 0.45f),
                new(-0.45f, 0.83f, 0.56f),
                new(-0.56f, 0.83f, -0.45f),
                new(0.45f, 0.83f, -0.56f)
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
            _pickPoses.Clear();
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
            _mahjongList.Clear();

            //1个1万
            _mahjongList.Add(
                new Mahjong(1, "mahjong_tile_" + 1));
            //4个2万
            for (var j = 0; j < Constants.MaxPlayer; j++)
            {
                _mahjongList.Add(
                    new Mahjong(2, "mahjong_tile_" + 2));
            }

            //4个3万
            for (var j = 0; j < Constants.MaxPlayer; j++)
            {
                _mahjongList.Add(
                    new Mahjong(3, "mahjong_tile_" + 3));
            }

            //4个4万
            for (var j = 0; j < Constants.MaxPlayer; j++)
            {
                _mahjongList.Add(
                    new Mahjong(4, "mahjong_tile_" + 4));
            }

            //4个5万
            for (var j = 0; j < Constants.MaxPlayer; j++)
            {
                _mahjongList.Add(
                    new Mahjong(5, "mahjong_tile_" + 5));
            }

            //4个6万
            for (var j = 0; j < Constants.MaxPlayer; j++)
            {
                _mahjongList.Add(
                    new Mahjong(6, "mahjong_tile_" + 6));
            }

            //4个7万
            for (var j = 0; j < Constants.MaxPlayer; j++)
            {
                _mahjongList.Add(
                    new Mahjong(7, "mahjong_tile_" + 7));
            }

            //4个8万
            for (var j = 0; j < Constants.MaxPlayer; j++)
            {
                _mahjongList.Add(
                    new Mahjong(8, "mahjong_tile_" + 8));
            }

            //4个9万
            for (var j = 0; j < Constants.MaxPlayer; j++)
            {
                _mahjongList.Add(
                    new Mahjong(9, "mahjong_tile_" + 9));
            }

            //4个1条
            for (var j = 0; j < Constants.MaxPlayer; j++)
            {
                _mahjongList.Add(
                    new Mahjong(10, "mahjong_tile_" + 10));
            }

            //1个2条
            _mahjongList.Add(
                new Mahjong(11, "mahjong_tile_" + 11));
            _mahjongList.Add(
                new Mahjong(11, "mahjong_tile_" + 11));
            //4个3条
            for (var j = 0; j < Constants.MaxPlayer; j++)
            {
                _mahjongList.Add(
                    new Mahjong(12, "mahjong_tile_" + 12));
            }

            //1个4条
            _mahjongList.Add(
                new Mahjong(13, "mahjong_tile_" + 13));
            //4个5条
            for (var j = 0; j < Constants.MaxPlayer; j++)
            {
                _mahjongList.Add(
                    new Mahjong(14, "mahjong_tile_" + 14));
            }

            for (var j = 0; j < Constants.MaxPlayer; j++)
            {
                _mahjongList.Add(
                    new Mahjong(15, "mahjong_tile_" + 15));
            }

            for (var j = 0; j < Constants.MaxPlayer; j++)
            {
                _mahjongList.Add(
                    new Mahjong(16, "mahjong_tile_" + 16));
            }

            for (var j = 0; j < Constants.MaxPlayer; j++)
            {
                _mahjongList.Add(
                    new Mahjong(17, "mahjong_tile_" + 17));
            }

            for (var j = 0; j < Constants.MaxPlayer; j++)
            {
                _mahjongList.Add(
                    new Mahjong(18, "mahjong_tile_" + 18));
            }

            //1个4条
            _mahjongList.Add(
                new Mahjong(19, "mahjong_tile_" + 19));

            for (var j = 0; j < Constants.MaxPlayer; j++)
            {
                _mahjongList.Add(
                    new Mahjong(20, "mahjong_tile_" + 20));
            }

            for (var j = 0; j < 3; j++)
            {
                _mahjongList.Add(
                    new Mahjong(21, "mahjong_tile_" + 21));
            }

            for (var j = 0; j < Constants.MaxPlayer; j++)
            {
                _mahjongList.Add(
                    new Mahjong(22, "mahjong_tile_" + 22));
            }

            for (var j = 0; j < Constants.MaxPlayer; j++)
            {
                _mahjongList.Add(
                    new Mahjong(23, "mahjong_tile_" + 23));
            }

            for (var j = 0; j < Constants.MaxPlayer; j++)
            {
                _mahjongList.Add(
                    new Mahjong(24, "mahjong_tile_" + 24));
            }

            for (var j = 0; j < Constants.MaxPlayer - 1; j++)
            {
                _mahjongList.Add(
                    new Mahjong(25, "mahjong_tile_" + 25));
            }

            for (var j = 0; j < Constants.MaxPlayer; j++)
            {
                _mahjongList.Add(
                    new Mahjong(26, "mahjong_tile_" + 26));
            }

            for (var j = 0; j < Constants.MaxPlayer; j++)
            {
                _mahjongList.Add(
                    new Mahjong(27, "mahjong_tile_" + 27));
            }

            for (var j = 0; j < Constants.MaxPlayer; j++)
            {
                _mahjongList.Add(
                    new Mahjong(28, "mahjong_tile_" + 28));
            }

            for (var i = 29; i <= Constants.MaxId; i++)
            {
                for (var j = 0; j < Constants.MaxPlayer; j++)
                {
                    _mahjongList.Add(
                        new Mahjong(i, "mahjong_tile_" + i));
                }
            }

            _mahjongList = _mahjongList.OrderBy(_ => rng.Next()).ToList();
            //前面放三个1万
            for (var j = 1; j <= 3; j++)
            {
                _mahjongList.Insert(0, new Mahjong(1, "mahjong_tile_" + 1));
            }

            //前面放三个2条
            for (var j = 1; j <= 2; j++)
            {
                _mahjongList.Insert(0, new Mahjong(11, "mahjong_tile_" + 11));
            }

            //前面放三个4条
            for (var j = 1; j <= 3; j++)
            {
                _mahjongList.Insert(0, new Mahjong(13, "mahjong_tile_" + 13));
            }

            //前面放三个1饼
            for (var j = 1; j <= 3; j++)
            {
                _mahjongList.Insert(0, new Mahjong(19, "mahjong_tile_" + 19));
            }

            // _mahjongList.Insert(0, new Mahjong(1, "mahjong_tile_" + 1));
            // _mahjongList.Insert(0, new Mahjong(1, "mahjong_tile_" + 1));
            // _mahjongList.Insert(0, new Mahjong(1, "mahjong_tile_" + 1));
            // _mahjongList.Insert(0, new Mahjong(2, "mahjong_tile_" + 2));
            // _mahjongList.Insert(0, new Mahjong(3, "mahjong_tile_" + 3));
            // _mahjongList.Insert(0, new Mahjong(4, "mahjong_tile_" + 4));
            // _mahjongList.Insert(0, new Mahjong(5, "mahjong_tile_" + 5));
            // _mahjongList.Insert(0, new Mahjong(6, "mahjong_tile_" + 6));
            // _mahjongList.Insert(0, new Mahjong(7, "mahjong_tile_" + 7));
            // _mahjongList.Insert(0, new Mahjong(8, "mahjong_tile_" + 8));
            // _mahjongList.Insert(0, new Mahjong(9, "mahjong_tile_" + 9));
            // _mahjongList.Insert(0, new Mahjong(9, "mahjong_tile_" + 9));
            _mahjongList.Insert(0, new Mahjong(21, "mahjong_tile_" + 21));
            _mahjongList.Insert(0, new Mahjong(25, "mahjong_tile_" + 25));
            _mahjongList = _mahjongList.ToList();
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
            _userMahjongLists.Clear();
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
            object[] objects = {Convert.ToInt64(GameManager.Instance.GetUserId()), PhotonNetwork.LocalPlayer.NickName};
            var rig = Object.FindObjectOfType<OVRCameraRig>().transform;
            rig.position = _playerInitPositions[id];
            rig.rotation = Quaternion.Euler(_playerInitRotations[id]);
            var go = PhotonNetwork.Instantiate(PlayerControllerPath, rig.position, rig.rotation, 0, objects);
            go.transform.SetParent(rig);
            go.transform.localRotation = Quaternion.identity;
            go.transform.localPosition = Vector3.zero;
            var centerEyeAnchor = rig.Find("TrackingSpace/CenterEyeAnchor");
            var voiceSetup = PhotonNetwork.Instantiate(PhotonVoiceSetupPrefabName, centerEyeAnchor.position,
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
                go.layer = LayerMask.NameToLayer("Ignore Raycast");
                var attr = go.GetComponent<MahjongAttr>();
                var pv = attr.photonView;
                //其他人不可以抓取我的麻将
                pv.RPC(nameof(attr.SetState), RpcTarget.Others);
                attr.inMyHand = true;
                attr.inOthersHand = false;
                pv.RPC(nameof(attr.RPCSetInMyHand), RpcTarget.Others, false);
                pv.RPC(nameof(attr.RPCSetInOthersHand), RpcTarget.Others, true);
                pv.RPC(nameof(attr.RPCSetOnDesk), RpcTarget.All, false);
                pv.RPC(nameof(attr.RPCSetIsThrown), RpcTarget.All, false);
                pv.RPC(nameof(attr.RPCSetLayer), RpcTarget.Others, LayerMask.NameToLayer("Mahjong"));
                attr.ID = _userMahjongLists[id][i].ID;
                pos += _bias[id];
                if (!ret.ContainsKey(_userMahjongLists[id][i].ID))
                {
                    ret[_userMahjongLists[id][i].ID] = new List<GameObject>();
                }

                ret[_userMahjongLists[id][i].ID].Add(go);
                attr.num = i + 1;
                if (!pv.IsMine)
                {
                    pv.GetComponent<MeshFilter>().mesh = GameManager.Instance.GetMahjongMesh(34);
                }
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