using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InControl;
using Utility;

namespace Inputs
{
    public class UserInput : MonoBehaviour
    {
        private InputPacket[] m_inputArray;
        private InputDevice m_device;
        private Queue<InputPacket> m_InputPacketQueue;
        
        public InputPacket[] InputPackets
        {
            get
            {
                return m_inputArray;
            }
        }

        void Awake()
        {
            m_device = InputManager.ActiveDevice; 
            m_inputArray = new InputPacket[16];
            m_InputPacketQueue = new Queue<InputPacket>();
        }

        void Update()
        {
            GetInputs();

        }

        void GetInputs()
        {
            //m_InputPacketQueue.Clear();
            m_device = InputManager.ActiveDevice;
            {
                float amount = m_device.LeftStickX;
                InputPacket packet = new InputPacket(EnumService.InputType.LeftStickX, 0.3f);
                Debug.Log("LEFT X: " + amount);
                m_inputArray[(int)EnumService.InputType.LeftStickX] = packet;
            }

            {
                float amount = m_device.LeftStickY;
                if (amount > 0)
                {
                    Debug.Log("LEFT Y: " + amount);
                }
                InputPacket packet = new InputPacket(EnumService.InputType.LeftStickY, amount);
                m_inputArray[(int)EnumService.InputType.LeftStickY] = packet;
            }

            {
                float amount = m_device.RightStickX;
                InputPacket packet = new InputPacket(EnumService.InputType.RightStickX, amount);
                m_inputArray[(int)EnumService.InputType.RightStickX] = packet;
            }

            {
                float amount = m_device.RightStickY;
                InputPacket packet = new InputPacket(EnumService.InputType.RightStickY, amount);
                m_inputArray[(int)EnumService.InputType.RightStickY] = packet;
            }

            {
                float amount = m_device.LeftBumper;
                InputPacket packet = new InputPacket(EnumService.InputType.LB, amount);
                m_inputArray[(int)EnumService.InputType.LB] = packet;
            }

            {
                float amount = m_device.RightBumper;
                InputPacket packet = new InputPacket(EnumService.InputType.RB, amount);
                m_inputArray[(int)EnumService.InputType.RB] = packet;
            }

            {
                float amount = m_device.LeftTrigger;
                InputPacket packet = new InputPacket(EnumService.InputType.LT, amount);
                m_inputArray[(int)EnumService.InputType.LT] = packet;
            }

            {
                float amount = m_device.RightTrigger;
                InputPacket packet = new InputPacket(EnumService.InputType.RT, amount);
                m_inputArray[(int)EnumService.InputType.RT] = packet;
            }

            {
                float amount = m_device.DPadUp;
                InputPacket packet = new InputPacket(EnumService.InputType.DUp, amount);
                m_inputArray[(int)EnumService.InputType.DUp] = packet;
            }

            {
                float amount = m_device.DPadDown;
                InputPacket packet = new InputPacket(EnumService.InputType.DDown, amount);
                m_inputArray[(int)EnumService.InputType.DDown] = packet;
            }

            {
                float amount = m_device.DPadLeft;
                InputPacket packet = new InputPacket(EnumService.InputType.DLeft, amount);
                m_inputArray[(int)EnumService.InputType.DLeft] = packet;
            }

            {
                float amount = m_device.DPadRight;
                InputPacket packet = new InputPacket(EnumService.InputType.DRight, amount);
                m_inputArray[(int)EnumService.InputType.DRight] = packet;
            }

            {
                float amount = m_device.Action1;
                InputPacket packet = new InputPacket(EnumService.InputType.X, amount);
                m_inputArray[(int)EnumService.InputType.X] = packet;
            }

            {
                float amount = m_device.Action2;
                InputPacket packet = new InputPacket(EnumService.InputType.O, amount);
                m_inputArray[(int)EnumService.InputType.O] = packet;
            }

            {
                float amount = m_device.Action3;
                InputPacket packet = new InputPacket(EnumService.InputType.Square, amount);
                m_inputArray[(int)EnumService.InputType.Square] = packet;
            }

            {
                float amount = m_device.Action4;
                InputPacket packet = new InputPacket(EnumService.InputType.Triangle, amount);
                m_inputArray[(int)EnumService.InputType.Triangle] = packet;
            }
        }
    }
}
