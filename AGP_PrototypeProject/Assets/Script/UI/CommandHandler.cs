using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UI;

/*
 * author: rob neir
 * date: 1/20/2017 
 * description: THIS CLASS IS TEMPORARY AND FOR TESTING FOR INPUT SINCE INPUT ISNT SETUP YET! :)
 * */

public class CommandHandler : MonoBehaviour
{

    [SerializeField]
    private UIManager UIManager;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            CommandPanel commandPanel = UIManager.CommandPanel;
            if (commandPanel != null)
            {
                commandPanel.Toggle();
            }
        }

        // Testing command panel input handling
        if (Input.GetKeyDown(KeyCode.U))
        {
            UIManager.CommandPanel.SelectCommand(new Inputs.InputPacket(Utility.EnumService.InputType.DUp, true));
        }
        else if (Input.GetKeyDown(KeyCode.N))
        {
            UIManager.CommandPanel.SelectCommand(new Inputs.InputPacket(Utility.EnumService.InputType.DDown, true));
        }
        else if (Input.GetKeyDown(KeyCode.H))
        {
            UIManager.CommandPanel.SelectCommand(new Inputs.InputPacket(Utility.EnumService.InputType.DLeft, true));
        }
        else if (Input.GetKeyDown(KeyCode.J))
        {
            UIManager.CommandPanel.SelectCommand(new Inputs.InputPacket(Utility.EnumService.InputType.DRight, true));
        }
    }
}