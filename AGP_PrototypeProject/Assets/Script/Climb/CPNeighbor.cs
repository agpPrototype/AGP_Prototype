using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;

namespace Climb
{
    [System.Serializable]
    public class CPNeighbor : MonoBehaviour
    {
        public Vector3 Direction;
        public ClimbPoint TargetPoint;
        public EnumService.CPNeighborType Type;
    }
}
