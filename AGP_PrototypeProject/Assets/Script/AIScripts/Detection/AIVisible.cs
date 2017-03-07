using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// AUTHOR: rob neir
/// 
/// DESCRIPTION: When attached to a game object allows the game object to be noticed
/// by AI. The AI will automatically analyze this object and determine how to react.
/// 
/// </summary>
namespace AI
{
    namespace Detection
    {
        public class AIVisible : AIDetectable
        {
            #region Member Variables

            /* event for when visible is destroyed to notify DetectionManager */
            public delegate void Visible_Spawn_EventHandler(AIVisible visible);
            public static event Visible_Spawn_EventHandler VisibleSpawnEvt;
            
            /* event for when visible is destroyed to notify DetectionManager */
            public delegate void Visible_Destroy_EventHandler(AIVisible visible);
            public static event Visible_Destroy_EventHandler VisibleDestroyEvt;

            #endregion

            protected override void Start()
            {
                base.Start();
            }

            public override void RegisterToDetectionManager()
            {
                /* send event to DetectionManager about spawning this visible. */
                if (VisibleSpawnEvt != null)
                {
                    VisibleSpawnEvt(this);
                }
            }

            public override void OnDestroy()
            {
                /* send event to DetectionManager about destroying this visible. */
                if (VisibleDestroyEvt != null)
                {
                    VisibleDestroyEvt(this);
                }
            }
        }; // AIVisible class
    }; // Detection namespace
}; // AI namespace