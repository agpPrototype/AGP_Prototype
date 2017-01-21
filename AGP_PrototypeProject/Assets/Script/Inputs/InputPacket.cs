using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inputs;
using Utility;
using System;

namespace Inputs
{
    public class InputPacket
    {
        private EnumService.InputType m_inputType;
        public EnumService.InputType InputType
        {
            get
            {
                return m_inputType;
            }
            set
            {
                m_inputType = value;
            }
        }

        private float m_value;
        public float Value
        {
            get
            {
                return m_value;
            }
            set
            {
                m_value = Convert.ToSingle(value);
            }
        }
        

        public InputPacket(EnumService.InputType type, float value)
        {
            m_inputType = type;
            m_value = value;    
        }

        public InputPacket(EnumService.InputType type, bool value)
        {
            m_inputType = type;
            m_value = Convert.ToSingle(value);
        }
    }
}
