using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InControl;
using Utility;

namespace Inputs
{
    public class UserInput : MonoBehaviour
    {

        private InputDevice m_device = InputManager.ActiveDevice;
        private Queue<InputPacket> m_InputPacketQueue;

        void Awake()
        {
            m_InputPacketQueue = new Queue<InputPacket>();
        }

        void FixedUpdate()
        {
            GetInputs();

        }

        void GetInputs()
        {
            m_InputPacketQueue.Clear();
            //left stick
            if (m_device.LeftStickX != 0)
            {
                float amount = m_device.LeftStickX;
                InputPacket packet = new InputPacket(EnumService.InputType.LeftStickX, amount);
                m_InputPacketQueue.Enqueue(packet);
            }

            if (m_device.LeftStickY != 0)
            {
                float amount = m_device.LeftStickY;
                InputPacket packet = new InputPacket(EnumService.InputType.LeftStickY, amount);
                m_InputPacketQueue.Enqueue(packet);
            }

            if (m_device.RightStickX != 0)
            {
                float amount = m_device.RightStickX;
                InputPacket packet = new InputPacket(EnumService.InputType.RightStickX, amount);
                m_InputPacketQueue.Enqueue(packet);
            }

            if (m_device.RightStickY != 0)
            {
                float amount = m_device.RightStickY;
                InputPacket packet = new InputPacket(EnumService.InputType.RightStickY, amount);
                m_InputPacketQueue.Enqueue(packet);
            }

            if (m_device.LeftBumper)
            {
                float amount = m_device.LeftBumper;
                InputPacket packet = new InputPacket(EnumService.InputType.LB, amount);
                m_InputPacketQueue.Enqueue(packet);
            }

            if (m_device.RightBumper)
            {
                float amount = m_device.RightBumper;
                InputPacket packet = new InputPacket(EnumService.InputType.RB, amount);
                m_InputPacketQueue.Enqueue(packet);
            }

            if (m_device.LeftTrigger)
            {
                float amount = m_device.LeftTrigger;
                InputPacket packet = new InputPacket(EnumService.InputType.LT, amount);
                m_InputPacketQueue.Enqueue(packet);
            }

            if (m_device.RightTrigger)
            {
                float amount = m_device.RightTrigger;
                InputPacket packet = new InputPacket(EnumService.InputType.RT, amount);
                m_InputPacketQueue.Enqueue(packet);
            }

            if (m_device.DPadUp)
            {
                float amount = m_device.DPadUp;
                InputPacket packet = new InputPacket(EnumService.InputType.DUp, amount);
                m_InputPacketQueue.Enqueue(packet);
            }

            if (m_device.DPadDown)
            {
                float amount = m_device.DPadDown;
                InputPacket packet = new InputPacket(EnumService.InputType.DDown, amount);
                m_InputPacketQueue.Enqueue(packet);
            }

            if (m_device.DPadLeft)
            {
                float amount = m_device.DPadLeft;
                InputPacket packet = new InputPacket(EnumService.InputType.DLeft, amount);
                m_InputPacketQueue.Enqueue(packet);
            }

            if (m_device.DPadRight)
            {
                float amount = m_device.DPadRight;
                InputPacket packet = new InputPacket(EnumService.InputType.DRight, amount);
                m_InputPacketQueue.Enqueue(packet);
            }

            if (m_device.Action1)
            {
                float amount = m_device.Action1;
                InputPacket packet = new InputPacket(EnumService.InputType.X, amount);
                m_InputPacketQueue.Enqueue(packet);
            }

            if (m_device.Action2)
            {
                float amount = m_device.Action2;
                InputPacket packet = new InputPacket(EnumService.InputType.O, amount);
                m_InputPacketQueue.Enqueue(packet);
            }

            if (m_device.Action3)
            {
                float amount = m_device.Action3;
                InputPacket packet = new InputPacket(EnumService.InputType.Square, amount);
                m_InputPacketQueue.Enqueue(packet);
            }

            if (m_device.Action4)
            {
                float amount = m_device.Action4;
                InputPacket packet = new InputPacket(EnumService.InputType.Triangle, amount);
                m_InputPacketQueue.Enqueue(packet);
            }
        }
    }
}
