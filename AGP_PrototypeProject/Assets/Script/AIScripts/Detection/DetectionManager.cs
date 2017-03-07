using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 
/// AUTHOR: rob neir
/// 
/// DESCRIPTION: Detection manager is very important for any kind of AI detection.
/// It stores all possible things that any AI in the game can detect. This has a list
/// of visible and audible things. It is stored here so that each AI is not required
/// to store the data themselves and can come to one centralized hub to get necessary
/// detection data.
/// 
/// </summary>
namespace AI
{
    namespace Detection
    {
        public enum ThreatLevel
        {
            NONE,       // no threat
            LOW,        // low threat
            GUARDED,    // guarded threat
            ELEVATED,   // elevated threat
            HIGH,       // high threat
            SEVERE,     // severe threat 
        };

        public class DetectionManager : MonoBehaviour
        {
            #region Member Variables

            public static DetectionManager Instance = null;

            private List<AIAudible> m_Audibles;
            public List<AIAudible> Audibles
            {
                get
                {
                    return m_Audibles;
                }
                private set { }
            }
            private List<AIVisible> m_Visibles;
            public List<AIVisible> Visibles
            {
                get
                {
                    return m_Visibles;
                }
                private set { }
            }

            /* event to be sent to all AIAudioDetection components */
            public delegate void Audible_Updated_EventHandler();
            public static event Audible_Updated_EventHandler AudibleUpdatedForwarderEvt;
            
            /* event to be sent to all AILineOfSightDetection components */
            public delegate void Visibles_Updated_EventHandler();
            public static event Visibles_Updated_EventHandler VisibleUpdatedForwarderEvt;

            #endregion

            private void Awake()
            {
                if (Instance == null)
                {
                    Instance = this;
                }
                else
                {
                    if (Instance != this)
                    {
                        Destroy(this.gameObject);
                    }
                }
                m_Audibles = new List<AIAudible>();
                m_Visibles = new List<AIVisible>();

                // subscribe to events received from AIDetectables.
                AIAudible.AudibleSpawnEvt += this.AddToAudibles;
                AIAudible.AudibleDestroyEvt += this.RemoveFromAudibles;

                AIVisible.VisibleSpawnEvt += this.AddToVisibles;
                AIVisible.VisibleDestroyEvt += this.RemoveFromVisibles;
            }

            public AIDetectable GetHighestThreat(AIAudioDetection audDetect, AILineOfSightDetection losDetect)
            {
                AIDetectable greatestThreatDetected = null;

                // Look for things.
                if (losDetect != null)
                {
                    AIVisible visible = losDetect.GetHighestThreat();
                    if (visible != null)
                    {
                        if(greatestThreatDetected != null)
                        {
                            if(visible.ThreatLevel > greatestThreatDetected.ThreatLevel)
                                greatestThreatDetected = visible;
                        }
                        else
                        {
                            greatestThreatDetected = visible;
                        }
                    }
                }

                // Listen for things.
                if (audDetect != null)
                {
                    AIAudible audible = audDetect.GetHighestThreat();
                    if(audible != null)
                    {
                        if (greatestThreatDetected != null)
                        {
                            if (audible.ThreatLevel > greatestThreatDetected.ThreatLevel)
                                greatestThreatDetected = audible;
                        }
                        else
                        {
                            greatestThreatDetected = audible;
                        }
                    }
                }

                return greatestThreatDetected;
            }

            /* sends event to all AIAudioDetection components after adding to audibles list. */
            private void AddToAudibles(AIAudible audible)
            {
                if(audible == null)
                {
                    Debug.Log("Cannot add null AIAudible to list of audibles!");
                    return;
                }
                m_Audibles.Add(audible);
            }

            /* sends event to all AIAudioDetection components after removing from audibles list. */
            private void RemoveFromAudibles(AIAudible audible)
            {
                if (audible == null)
                {
                    Debug.Log("Cannot remove null AIAudible to list of audibles!");
                    return;
                }
                m_Audibles.Remove(audible);
            }
            
            /* sends event to all AILineOfSightDetection components after adding to visibles list. */
            private void AddToVisibles(AIVisible visible)
            {
                if (visible == null)
                {
                    Debug.Log("Cannot add null AIVisible to list of visibles!");
                    return;
                }
                m_Visibles.Add(visible);
            }

            /* sends event to all AILineOfSightDetection components after removing from visibles list.*/
            private void RemoveFromVisibles(AIVisible visible)
            {
                if (visible == null)
                {
                    Debug.Log("Cannot remove null AIVisible to list of visibles!");
                    return;
                }
                m_Visibles.Remove(visible);
            }
        }; // Detection Manager class
    }; // Detection namespace
}; // AI namespace
