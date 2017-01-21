using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inputs;

/*
 * author: rob neir
 * date: 1/20/2017 
 * 
 * */
namespace UI
{
    public class UIManager : MonoBehaviour
    {
        public CommandPanel CommandPanel;

        public void SelectCommand(InputPacket inputPacket)
        {
            // Make sure the command panel is active before passing command selections to it.
            if (CommandPanel.gameObject.activeSelf)
            {
                CommandPanel.SelectCommand(inputPacket);
            }
        }
    }
}
