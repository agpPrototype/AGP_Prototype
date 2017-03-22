using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inputs
{
    public abstract class UIHandler : MonoBehaviour
    {
        public abstract void DoActions(UIActions uia);
    }
}
