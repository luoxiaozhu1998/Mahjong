using UnityEngine;

namespace PlayerAttr
{
    public abstract class PlayTileStrategy
    {
        public abstract void MahjongPut(Transform transform);
        public abstract void MahjongRotate(Transform transform);
        public abstract void BackTrace();
        public abstract void PongStrategy();
        public abstract void KongStrategy(Transform transform);
        public abstract void GrabMahjong();
    }
}