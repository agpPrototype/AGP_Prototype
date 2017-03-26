using UnityEngine;
using System.Collections;
using InControl;
using Inputs;
using Player;
using Utility;

namespace CameraController
{
    [ExecuteInEditMode]
    public class CameraRig : MonoBehaviour
    {

        public Transform Target;
        public bool AutoTargetPlayer;
        public LayerMask WallLayers;

        public enum Shoulder
        {
            Right, Left
        }

        public Shoulder shoulder;

        [System.Serializable]
        public class CameraSettings
        {
            [Header("-Positioning-")]
            public Vector3 CamPositionOffsetLeft;
            public Vector3 CamPositionOffsetRight;

            [Header("-Camera Options-")]
            public float MouseXSensitivity = 5.0f;
            public float MouseYSensitivity = 5.0f;
            public float MinAngle = -30.0f;
            public float MaxAngle = 70.0f;
            public float RotationSpeed = 10.0f;
            public float MaxCheckDistance = 0.1f;

            [Header("-Zoom-")]
            public float FieldOfView = 70.0f;
            public float ZoomFiledOfView = 30.0f;
            public float ZoomSpeed = 3.0f;

            [Header("-Visual Options-")]
            public float HideMeshWhenDistance = 0.5f;

            public bool IsKillCamActive = false;
        }
        [SerializeField]
        public CameraSettings CameraSetting;

        [System.Serializable]
        public class InputSettings
        {
            public string VerticalAxis = "Mouse X";
            public string HorizontalAxis = "Mouse Y";
            public string AimButton = "Fire2";
            public string SwitchShoulderButton = "Fire4";
        }
        [SerializeField]
        public InputSettings input;

        [System.Serializable]
        public class MovementSettings
        {
            public float MovementLerpSpeed = 5.0f;
        }

        [SerializeField]
        public MovementSettings Movement;

        private Transform mPivot;
        private Camera m_mainCam;
        private float m_newX = 0.0f;
        private float m_newY = 0.0f;
        private float m_shoulderRest = 0.0f;
        private UserInput m_userInput;
        private PCActions m_PCActions;

        // Use this for initialization
        void Start()
        {
            m_mainCam = GetComponentInChildren<Camera>();         
            mPivot = transform.GetChild(0);
            m_userInput = GameObject.FindObjectOfType<UserInput>();
            m_PCActions = new PCActions();
            m_PCActions.InputPackets = new InputPacket[18];
        }

        // Update is called once per frame
        void Update()
        {
            if (Target)
            {
                

                if (Application.isPlaying && !CameraSetting.IsKillCamActive)
                {
                    InputDevice device = InputManager.ActiveDevice;
                    RotateCamera(device.LeftTrigger);
                    CheckWall();
                    CheckMeshRenderer();

                    //Zoom(Input.GetButton(input.AimButton));
                    //LEFT TRIGGER FOR AIMING
                 
                    //if (Input.GetButtonDown(input.SwitchShoulderButton))
                    //{
                    //    SwitchShoulders();
                    //}
                    m_shoulderRest += Time.deltaTime;
                }
            }
        }

        void LateUpdate()
        {
            if (!Target)
            {
                TargetPlayer();
            }
            else
            {
                Vector3 targetPosition = Target.position;
                Quaternion targetRotation = Target.rotation;

                if (Target.gameObject.GetComponent<Items.ArrowComponent>())
                {
                    CameraSetting.IsKillCamActive = true;
                    targetPosition = targetPosition - 1 * Target.forward - 1 * Target.right - 1 * Target.up;
                    transform.position = targetPosition;
                    return;
                }

                FollowTarget(targetPosition, targetRotation);
            }
        }

        //targets the player
        private void TargetPlayer()
        {
            if (AutoTargetPlayer)
            {

                //PlayerController player = GameObject.FindObjectOfType<PlayerController>();
                PlayerControl player = GameObject.FindObjectOfType<PlayerControl>();
                Transform playerT = player.transform;
                Target = playerT;
            }
            CameraSetting.IsKillCamActive = false;
            
        }

        //follow the target
        private void FollowTarget(Vector3 targetPosition, Quaternion targetRotation)
        {
            if (!Application.isPlaying)
            {
                transform.position = targetPosition;
                transform.rotation = targetRotation;
            }
            else
            {
                Vector3 newPos = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * Movement.MovementLerpSpeed);
                transform.position = newPos;
            }
        }

        //rotate the camera with input
        private void RotateCamera(bool aiming)
        {
            if (!mPivot)
            {
                return;
            }
            InputDevice device = InputManager.ActiveDevice;
            if (!aiming)
            {
                m_newX -= CameraSetting.MouseXSensitivity * device.RightStickY;
                m_newY += CameraSetting.MouseYSensitivity * device.RightStickX;
            }
            else
            {
                m_newX -= (CameraSetting.MouseXSensitivity / 2) * device.RightStickY;
                m_newY += (CameraSetting.MouseYSensitivity / 2) * device.RightStickX;
            }

            m_newY = Mathf.Repeat(m_newY, 360);
            //if (mNewY > 25.0f)
            //{
            //    mNewY = 25.0f;
            //}
            //if (mNewY < -25.0f)
            //{
            //    mNewY = -25.0f;
            //}
            //if (aiming)
            //{
            //    if (mNewY > 90.0f && mNewY < 180.0f)
            //    {
            //        mNewY = 90.0f;
            //    }
            //    if (mNewY < 270.0f && mNewY >= 180f)
            //    {
            //        mNewY = 270.0f;
            //    }
            //}

            Vector3 eulerAngleAxis = new Vector3();
            eulerAngleAxis.x = m_newX;
            eulerAngleAxis.y = m_newY;

            m_newX = Mathf.Clamp(m_newX, CameraSetting.MinAngle, CameraSetting.MaxAngle);
            Quaternion newRotation = Quaternion.identity;

            newRotation = Quaternion.Slerp(mPivot.localRotation, Quaternion.Euler(eulerAngleAxis), Time.deltaTime * CameraSetting.RotationSpeed);
            if (Vector3.Distance(mPivot.localRotation.eulerAngles, eulerAngleAxis) < 1.0f)
            {
                eulerAngleAxis = newRotation.eulerAngles;
            }

            //mPivot.localRotation = newRotation;
            mPivot.localRotation = Quaternion.Euler(eulerAngleAxis);

            //RotateWithPlayer(Target.forward);
        }

        private void CheckWall()
        {
            if (!mPivot || !m_mainCam)
            {
                return;
            }

            RaycastHit hit;

            Transform mainCamT = m_mainCam.transform;
            Vector3 mainCamPos = mainCamT.position;
            Vector3 pivotPos = mPivot.position;

            Vector3 start = pivotPos;
            Vector3 dir = mainCamPos - pivotPos;

            float dist = Mathf.Abs(shoulder == Shoulder.Left ? CameraSetting.CamPositionOffsetLeft.z : CameraSetting.CamPositionOffsetRight.z);

            if (Physics.SphereCast(start, CameraSetting.MaxCheckDistance, dir, out hit, dist, WallLayers))
            {
                MoveCamUp(hit, pivotPos, dir, mainCamT);
            }
            else
            {
                switch (shoulder)
                {
                    case Shoulder.Left:
                        PositionCamera(CameraSetting.CamPositionOffsetLeft);
                        break;
                    case Shoulder.Right:
                        PositionCamera(CameraSetting.CamPositionOffsetRight);
                        break;
                }
            }
        }

        //moves the cam forward when we hit a wall
        void MoveCamUp(RaycastHit hit, Vector3 pivotPos, Vector3 dir, Transform cameraT)
        {
            float hitDist = hit.distance;
            Vector3 sphereCastCenter = pivotPos + (dir.normalized * hitDist);
            Vector3 cameraTPose = cameraT.position;
            cameraT.position = sphereCastCenter;
        }

        //positions the camera localPosition to a given location
        void PositionCamera(Vector3 cameraPos)
        {
            if (!m_mainCam)
            {
                return;
            }

            Transform mainCamT = m_mainCam.transform;
            Vector3 mainCamPos = mainCamT.localPosition;
            Vector3 newPos = Vector3.Lerp(mainCamPos, cameraPos, Time.deltaTime * Movement.MovementLerpSpeed);
            mainCamT.localPosition = newPos;
        }

        //hide the target's meshe render when we're too close to the mesh
        void CheckMeshRenderer()
        {
            if (!m_mainCam || !Target)
            {
                return;
            }

            SkinnedMeshRenderer[] meshes = Target.GetComponentsInChildren<SkinnedMeshRenderer>();
            Transform mainCamT = m_mainCam.transform;
            Vector3 mainCamPos = mainCamT.position;
            Vector3 targetPos = Target.position;
            float dist = Vector3.Distance(mainCamPos, (targetPos + Target.up));

            if (meshes.Length > 0)
            {
                for (int i = 0; i < meshes.Length; i++)
                {
                    if (dist <= CameraSetting.HideMeshWhenDistance)
                    {
                        meshes[i].enabled = false;
                    }
                    else
                    {
                        meshes[i].enabled = true;
                    }
                }
            }

        }

        //zooms the camera in and out
        public void Zoom(bool isZooming)
        {
            if (!m_mainCam)
            {
                return;
            }

            if (isZooming)
            {
                float newFieldOfView = Mathf.Lerp(m_mainCam.fieldOfView, CameraSetting.ZoomFiledOfView, Time.deltaTime * CameraSetting.ZoomSpeed);
                m_mainCam.fieldOfView = newFieldOfView;
            }
            else
            {
                float originalFieldOfView = Mathf.Lerp(m_mainCam.fieldOfView, CameraSetting.FieldOfView, Time.deltaTime * CameraSetting.ZoomSpeed);
                m_mainCam.fieldOfView = originalFieldOfView;
            }
        }

        //switches the camera shoulder view
        public void SwitchShoulders()
        {
            switch (shoulder)
            {
                case Shoulder.Left:
                    shoulder = Shoulder.Right;
                    break;
                case Shoulder.Right:
                    shoulder = Shoulder.Left;
                    break;
            }
        }

        public void RotateWithPlayer(Vector3 charForward)
        {
            Vector3 camForwardFlat = new Vector3(transform.forward.x, 0.0f, transform.forward.z);
            Vector3 crossProduct = Vector3.Cross(camForwardFlat, charForward);

            float angleToRotate = Vector3.Angle(charForward, camForwardFlat);
            if (crossProduct.y < 0)
            {
                angleToRotate = angleToRotate * (-1.0f);
            }
            transform.RotateAround(mPivot.position, Vector3.up, (angleToRotate) * 3.5f * Time.deltaTime);
        }
    }
}
