using DG.Tweening;
using UnityEngine;

namespace PlayerAttr
{
    public class PlayerCPlayTileStrategy :PlayTileStrategy
    {
        private Vector3 _originPutPos = new(-28f, 1f, 20f);
        public override void MahjongPut(Transform transform)
        {
            transform.DOMove(_originPutPos, 1f);
            _originPutPos.z -= 3f;
            if (!(_originPutPos.z <= -20f)) return;
            _originPutPos.z = 20f;
            _originPutPos.x += 3f;
        }

        public override void MahjongRotate(Transform transform)
        {
            transform.DORotate(new Vector3(0f, -90f, 0f), 1f);
        }

        public override void BackTrace()
        {
            _originPutPos.z += 3f;
            if (!(_originPutPos.z > 20f)) return;
            _originPutPos.z = -19f;
            _originPutPos.x -= 3f;
        }

        public override void PongStrategy()
        {
            throw new System.NotImplementedException();
        }

        public override void KongStrategy(Transform transform)
        {
            transform.DORotate(new Vector3(0f, 0f, 0f), 1f);
        }

        public override void GrabMahjong()
        {
            throw new System.NotImplementedException();
        }
    }
}