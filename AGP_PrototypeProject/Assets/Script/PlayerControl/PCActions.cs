using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inputs;

namespace Player
{
    public class PCActions : MonoBehaviour
    {
        public float Horizontal;
        public float Vertical;
        public Vector3 Move;
        public Vector3 CamForward;
        public Vector3 CamRight;
        public InputPacket[] InputPackets;
        public bool Jump;
        public bool Crouch;
        public bool Running;
        public float StrafeForward;
        public float StrafeRight;
        public bool Aim;
        public bool Fire;
    }
}
