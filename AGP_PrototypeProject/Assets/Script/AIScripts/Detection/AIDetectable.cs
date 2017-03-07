using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AI.Detection;

/// <summary>
/// 
/// AUTHOR: rob neir
/// 
/// DESCRIPTION: Anything that can be detected by AI inherits from this base class.
/// It contains a threat level that will determine how the AI will react when they detect
/// this specific object.
/// 
/// </summary>
namespace AI
{
    namespace Detection
    {
        public abstract class AIDetectable : MonoBehaviour
        {
            #region Member Variables

            [Tooltip("How AI will react (in terms of threat level) to seeing this visible.")]
            public ThreatLevel ThreatLevel;
            
            #endregion

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