using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AI;

namespace Misc
{
    public class AccaliaTutorialZone : TutorialZone
    {
        private CompanionAISM cAISM;

        // Use this for initialization
        void Start()
        {
            cAISM = FindObjectOfType<CompanionAISM>();
            cAISM.enabled = false;
        }

        public override void OnTutorialZoneEnter(Collider col)
        {
            base.OnTutorialZoneEnter(col);

            cAISM.enabled = true;
        }
    }
}
