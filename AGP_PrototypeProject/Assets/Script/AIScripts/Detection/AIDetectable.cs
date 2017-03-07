using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AI.Detection;

namespace AI
{
    namespace Detection
    {
        public abstract class AIDetectable : MonoBehaviour
        {
            [Tooltip("How AI will react (in terms of threat level) to seeing this visible.")]
            public ThreatLevel ThreatLevel;

            protected virtual void Start()
            {
                /* make sure any detectable adds itself to the DetectionManager */
                RegisterToDetectionManager();
            }
            /* tells DetectionManager to add this AIDetectable to the correct list. */
            public abstract void RegisterToDetectionManager();
            public abstract void OnDestroy();
        }; // AIDetectable class
    }; // Detection namespace
}; // AI namespace