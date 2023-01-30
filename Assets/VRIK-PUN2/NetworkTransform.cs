using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace RootMotion.Demos
{

    /// <summary>
    /// NetworkTransforms are network snapshots of Transform position, rotation, velocity and angular velocity
    /// </summary>
    [System.Serializable]
    public class NetworkTransform
    {

        public Vector3 position;
        public Quaternion rotation = Quaternion.identity;
        public Vector3 velocity;
        public Vector3 angularVelocity;

        private Vector3 lastPosition;
        private Quaternion lastRotation = Quaternion.identity;

        public Transform receiveRelTo;
        private int lastReceiveRelToId = -1;

        public void Reset(Transform t)
        {
            position = t.position;
            rotation = t.rotation;
            lastPosition = t.position;
            lastRotation = t.rotation;
            velocity = Vector3.zero;
            angularVelocity = Vector3.zero;
        }

        public void Send(PhotonStream stream)
        {
            stream.SendNext(position);
            stream.SendNext(rotation);
            stream.SendNext(velocity);
            stream.SendNext(angularVelocity);
        }

        public void SendRelTo(PhotonStream stream, PhotonView relTo)
        {
            int relToId = relTo != null ? relTo.ViewID : -1;
            stream.SendNext(relToId);

            stream.SendNext(position);
            stream.SendNext(rotation);
            stream.SendNext(velocity);
            stream.SendNext(angularVelocity);
        }

        public void Receive(PhotonStream stream)
        {
            position = (Vector3)stream.ReceiveNext();
            rotation = (Quaternion)stream.ReceiveNext();
            velocity = (Vector3)stream.ReceiveNext();
            angularVelocity = (Vector3)stream.ReceiveNext();
        }

        public void ReceiveRelTo(PhotonStream stream)
        {
            int receiveRelToId = (int)stream.ReceiveNext();

            if (receiveRelToId != lastReceiveRelToId)
            {
                var view = receiveRelToId == -1 ? null : PhotonView.Find(receiveRelToId);

                if (view != null)
                {
                    receiveRelTo = view.transform;
                }
                else receiveRelTo = null;

                lastReceiveRelToId = receiveRelToId;
            }

            position = (Vector3)stream.ReceiveNext();
            rotation = (Quaternion)stream.ReceiveNext();
            velocity = (Vector3)stream.ReceiveNext();
            angularVelocity = (Vector3)stream.ReceiveNext();
        }

        public void ReadTransformLocal(Transform t, PhotonView relTo)
        {
            receiveRelTo = relTo != null ? relTo.transform : null;

            position = SendRelTo(t.position, receiveRelTo);
            rotation = SendRelTo(t.rotation, receiveRelTo);
        }

        public void ReadTransformLocal(Transform t)
        {
            position = t.position;
            rotation = t.rotation;
        }

        public void ReadVelocitiesLocal(Transform t)
        {
            if (t == null)
            {
                velocity = Vector3.zero;
                angularVelocity = Vector3.zero;
                return;
            }
            ReadVelocitiesLocal(t.position, t.rotation);
        }

        public void ReadVelocitiesLocal(Vector3 currentPosition, Quaternion currentRotation)
        {
            // Monitor the velocity
            Vector3 v = (currentPosition - lastPosition) / Time.deltaTime;

            // Interpolation of velocity;
            velocity = Interpolate(velocity, v);

            // Monitor the angular velocity
            Quaternion rotationDelta = currentRotation * Quaternion.Inverse(lastRotation);

            float angle = 0f;
            Vector3 aV = Vector3.zero;
            rotationDelta.ToAngleAxis(out angle, out aV);

            if (float.IsInfinity(aV.x) || float.IsInfinity(aV.y) || float.IsInfinity(aV.z))
            {
                angle = 0f;
                aV = Vector3.zero;
            }

            aV *= (angle * Mathf.Deg2Rad) / Time.deltaTime;
            aV = RootMotion.QuaTools.ToBiPolar(aV);

            // Interpolation of angular velocity
            angularVelocity = Interpolate(angularVelocity, aV);

            lastPosition = currentPosition;
            lastRotation = currentRotation;
        }

        public void ApplyToLocal(Transform t)
        {
            t.position = ReceiveRelTo(position, receiveRelTo);
            t.rotation = ReceiveRelTo(rotation, receiveRelTo);
        }

        public void ApplyRemoteInterpolated(Transform t, float interpolationSpeed, float maxErrorSqrMag)
        {
            if (receiveRelTo != null)
            {
                t.position = ReceiveRelTo(position, receiveRelTo);
                t.rotation = ReceiveRelTo(rotation, receiveRelTo);
            }
            else
            {
                float sqrMag = Vector3.SqrMagnitude(t.position - position);
                if (sqrMag > maxErrorSqrMag)
                {
                    t.position = position;
                }
                else
                {
                    t.position = Vector3.Lerp(t.position, position, Time.deltaTime * interpolationSpeed);
                }
                t.rotation = Quaternion.Slerp(t.rotation, rotation, Time.deltaTime * interpolationSpeed);
            }
        }


        // Interpolate a velocity vector
        private Vector3 Interpolate(Vector3 v, Vector3 target)
        {
            return Vector3.Lerp(v, target, Time.deltaTime * 25f);
        }

        private Vector3 SendRelTo(Vector3 v, Transform t)
        {
            if (t == null) return v;
            return t.InverseTransformPoint(v);
        }

        private Quaternion SendRelTo(Quaternion r, Transform t)
        {
            if (t == null) return r;
            return Quaternion.Inverse(t.rotation) * r;
        }

        private Vector3 ReceiveRelTo(Vector3 v, Transform t)
        {
            if (t == null) return v;
            return t.TransformPoint(v);
        }

        private Quaternion ReceiveRelTo(Quaternion r, Transform t)
        {
            if (t == null) return r;
            return t.rotation * r;
        }
    }
}