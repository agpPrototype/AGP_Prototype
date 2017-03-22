using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [RequireComponent(typeof(Image))]
    public class ImageGlowEffect : MonoBehaviour
    {

        private Image m_Image;

        [SerializeField]
        private Color m_StartColor;

        [SerializeField]
        private Color m_EndColor;

        [SerializeField]
        private float m_TransitionSpeed;

        private Vector4 m_StartColAsVect;
        private Vector4 m_EndColAsVect;
        private float m_fractionLerped;
        private bool m_IsTargetEndColor;

        // Use this for initialization
        void Start()
        {
            m_Image = GetComponent<Image>();
            if (m_Image == null)
                Debug.LogError("Image null in image effect.");
            else
                m_Image.color = m_StartColor;

            m_fractionLerped = 0.0f;
            m_StartColAsVect = new Vector4(m_StartColor.r, m_StartColor.g, m_StartColor.b, m_StartColor.a);
            m_EndColAsVect = new Vector4(m_EndColor.r, m_EndColor.g, m_EndColor.b, m_EndColor.a);
        }

        // Update is called once per frame
        void Update()
        {
            // lerp the color toward end or start color depending on what the current target is.
            Vector4 resColAsVect;
            Vector4 currColAsVect = new Color(m_Image.color.r, m_Image.color.g, m_Image.color.b, m_Image.color.a);
            m_fractionLerped += m_TransitionSpeed * Time.deltaTime;
            if (m_IsTargetEndColor)
            {
                resColAsVect = Vector4.Lerp(currColAsVect, m_EndColAsVect, m_fractionLerped);
            }
            else
            {
                resColAsVect = Vector4.Lerp(currColAsVect, m_StartColAsVect, m_fractionLerped);
            }
            m_Image.color = new Color(resColAsVect.x, resColAsVect.y, resColAsVect.z, resColAsVect.w);

            // if lerp reaches one of the colors then lerp toward the other color.
            if (m_fractionLerped >= 1.0f)
            {
                m_IsTargetEndColor = m_IsTargetEndColor ? false : true;
                m_fractionLerped = 0.0f;
            }
        }
    }
}
