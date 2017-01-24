using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Inputs;
using Utility;

/*
 * author: rob neir
 * date: 1/20/2017 
 * 
 * */

namespace UI
{
    public class CommandPanel : MonoBehaviour
    {

        [SerializeField]
        [Tooltip("command that will be located left in command panel")]
        private Text CommandLeft;
        [SerializeField]
        [Tooltip("command that will be located right in command panel")]
        private Text CommandRight;
        [SerializeField]
        [Tooltip("command that will be located up in command panel")]
        private Text CommandUp;
        [SerializeField]
        [Tooltip("command that will be located down in command panel")]
        private Text CommandDown;

        private Text m_HighlightedCommand;      // currently highlighted command.
        private float m_Alpha;                  // alpha value used for lerping.

        [SerializeField]
        [Tooltip("highlighted color for commands.")]
        private Color HighlightedColor;
        [SerializeField]
        [Tooltip("unhighlighted color for commands")]
        private Color UnhighlightedColor;

        // Use this for initialization
        void Start()
        {
            this.gameObject.SetActive(false);
            m_Alpha = 0.0f;
        }

        // Update is called once per frame
        void Update()
        {

        }

        // Lerp alpha to show the command panel nicely.
        private void LerpShow()
        {
            m_Alpha = Mathf.Lerp(0, 1.0f, m_Alpha);
            CommandLeft.color = new Color(CommandLeft.color.r, CommandLeft.color.g, CommandLeft.color.b, m_Alpha);
            CommandRight.color = new Color(CommandRight.color.r, CommandRight.color.g, CommandRight.color.b, m_Alpha);
            CommandUp.color = new Color(CommandUp.color.r, CommandUp.color.g, CommandUp.color.b, m_Alpha);
            CommandDown.color = new Color(CommandDown.color.r, CommandDown.color.g, CommandDown.color.b, m_Alpha);
        }

        // Shows the command panel when called. duh ;)
        public void Show()
        {
            this.gameObject.SetActive(true);
        }

        // Hides the command panel when called. duh ;)
        public void Hide()
        {
            HighlightCommand(null);
            this.gameObject.SetActive(false);
        }

        // Hides if is visible and shows if not visible.
        public void Toggle()
        {
            if(this.gameObject.activeSelf)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }

        private void HighlightCommand(Text commandToHighlight)
        {
            // If there was a previous command unhighlight that color.
            if (m_HighlightedCommand != null)
            {
                m_HighlightedCommand.color = UnhighlightedColor;
            }

            // Highlight the new command if there is one.
            if (commandToHighlight != null)
            {
                m_HighlightedCommand = commandToHighlight;
                m_HighlightedCommand.color = HighlightedColor;
            }
        }

        public void SelectCommand(InputPacket inputPacket)
        {
            // Only allow selecting command if command panel is active.
            if(!this.gameObject.activeSelf)
            {
                return;
            }

            // Figure out what command was issued and show that visually/execute the command.
            EnumService.InputType inputType = inputPacket.InputType;
            switch (inputType)
            {
                case EnumService.InputType.DDown:
                    Debug.Log("Chose down command");
                    HighlightCommand(CommandDown);
                    break;
                case EnumService.InputType.DUp:
                    Debug.Log("Chose up command");
                    HighlightCommand(CommandUp);
                    break;
                case EnumService.InputType.DLeft:
                    Debug.Log("Chose left command");
                    HighlightCommand(CommandLeft);
                    break;
                case EnumService.InputType.DRight:
                    Debug.Log("Chose right command");
                    HighlightCommand(CommandRight);
                    break;
            }
        }
    }
}