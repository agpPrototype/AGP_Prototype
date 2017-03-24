using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Audio;

/// <summary>
/// 
/// AUTHOR: rob neir
/// 
/// DESCRIPTION: This component gives the game object it is attached to access
/// to functions that enable the detection of "AIAudibles" spawned in the game.
/// Essentially audio detection.
/// 
/// </summary>
namespace AI
{
    namespace Detection
    {
        public class AIAudioDetection : MonoBehaviour
        {
            #region Member Variables

            [Tooltip("transform that will be used for AI ear source.")]
            [SerializeField]
            private Transform m_Ears;
            public Transform Ears { get { return m_Ears; } }

            #endregion

            /* gets a list of AIAudibles that can be heard. */
            public List<AIAudible> GetAudibles()
            {
                List<AIAudible> possibleAudibles = DetectionManager.Instance.Audibles;
                List<AIAudible> actualAudibles = new List<AIAudible>();
                for (int i = 0; i < possibleAudibles.Count; i++)
                {
                    AIAudible audible = possibleAudibles[i];
                    // Check to see if we can see the target.
                    if (IsAudible(audible))
                    { 
                        actualAudibles.Add(audible);
                    }
                }
                return actualAudibles;
            }

            /* gets highest priority audible sound based on AIAudioDetection settings */
            public AIAudible GetHighestThreat(GameObject detector)
            {
                List<AIAudible> actualAudibles = GetAudibles();
                AIAudible highestThreat = null;
                if (actualAudibles.Count > 0)
                {
                    for (int i = 0; i < actualAudibles.Count; i++)
                    {
                        AIAudible currAudible = actualAudibles[i];
                        if (highestThreat)
                        {
                            if (currAudible.ThreatLevel > highestThreat.ThreatLevel)
                            {
                                highestThreat = currAudible;
                            }
                            else if (currAudible.ThreatLevel == highestThreat.ThreatLevel) // deal with ties based on detector's distance from threat.
                            {
                                if (detector != null)
                                {
                                    // check to see if new visible target is closer.
                                    if ((detector.transform.position - currAudible.transform.position).sqrMagnitude <
                                        (detector.transform.position - highestThreat.transform.position).sqrMagnitude)
                                    {
                                        highestThreat = currAudible;
                                    }
                                }
                            }
                        }
                        else
                        {
                            highestThreat = currAudible;
                        }
                    }
                }

                return highestThreat;
            }

            /* will return true if AIAudible is audible. function to determin if an AIAudible is audible
             is within the AIAudible class.*/
            public bool IsAudible(AIAudible audible)
            {
                // check to see if audio is within range.
                float range = audible.Range;
                Vector3 thisPos = transform.position;
                Vector3 audioPos = audible.transform.position;
                float distSquared = (Mathf.Pow(thisPos.x - audioPos.x, 2) + Mathf.Pow(thisPos.y - audioPos.y, 2) + Mathf.Pow(thisPos.z - audioPos.z, 2));
                if (distSquared <= range * range)
                {
                    return audible.RangeCheckToGameObject(gameObject, audible.gameObject, m_Ears.position, audible.transform.position, audible.Range);
                }
                return false;
            }
        }; // AIDetection class
    }; // Detection namespace
}; // AI namespace
 