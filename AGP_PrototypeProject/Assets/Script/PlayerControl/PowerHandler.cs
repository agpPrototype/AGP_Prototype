using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameCritical;
using Utility;
using System;

namespace Player
{
    public class PowerHandler : MonoBehaviour {

        // Use this for initialization
        void Start () {

        }

        // Update is called once per frame
        void Update () {

        }


        public void ProcessPowers(PCActions pca)
        {
            if (pca.InputPackets[(int)EnumService.InputType.Triangle] != null)
            {
                pca.SmellSmoke = Convert.ToBoolean(pca.InputPackets[(int)EnumService.InputType.Triangle].Value);
            }
            DoActions(pca);
        }


        private void DoActions(PCActions pca)
        {
            if (pca.SmellSmoke)
            { 
                GameController.Instance.SmellSmokeDriver.ToggleSmellSmoke();
            }
        }
    }
}

