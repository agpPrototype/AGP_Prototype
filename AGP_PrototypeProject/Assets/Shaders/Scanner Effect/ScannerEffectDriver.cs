using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Player;

[ExecuteInEditMode]
public class ScannerEffectDriver : MonoBehaviour {

    public Transform ScannerOrigin;
    public Material EffectMaterial;
    public float ScanDistance;
    public float m_MaxScanDistance;
    public float m_ScanSpeed;
    public float m_ScanSpeedSlowFactor;
    private float m_CurrScanSpeed;

    private Camera _camera;

    // Demo Code
    bool _scanning;

    void Start()
    {
        _scanning = false;

        // set origin to player position.
        PlayerControl playerControl = FindObjectOfType<PlayerControl>();
        if (playerControl != null)
        {
            ScannerOrigin = playerControl.transform;
        }

        m_CurrScanSpeed = m_ScanSpeed;
    }

    void Update()
    {
        if (_scanning)
        {
            if(ScanDistance <= m_MaxScanDistance)
            {
                ScanDistance += Time.deltaTime * m_CurrScanSpeed;
                m_CurrScanSpeed *= m_ScanSpeedSlowFactor;
            }
            else
            {
                Stop();
            }
        }
    }

    public void Play()
    {
        _scanning = true;
    }

    public void Stop()
    {
        _scanning = false;
        ScanDistance = 0; // reset scan.
        m_CurrScanSpeed = m_ScanSpeed;
    }

    void OnEnable()
    {
        _camera = GetComponent<Camera>();
        _camera.depthTextureMode = DepthTextureMode.Depth;
    }

    [ImageEffectOpaque]
    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        EffectMaterial.SetVector("_WorldSpaceScannerPos", ScannerOrigin.position);
        EffectMaterial.SetFloat("_ScanDistance", ScanDistance);
        RaycastCornerBlit(src, dst, EffectMaterial);
    }

    void RaycastCornerBlit(RenderTexture source, RenderTexture dest, Material mat)
    {
        // Compute Frustum Corners
        float camFar = _camera.farClipPlane;
        float camFov = _camera.fieldOfView;
        float camAspect = _camera.aspect;

        float fovWHalf = camFov * 0.5f;

        Vector3 toRight = _camera.transform.right * Mathf.Tan(fovWHalf * Mathf.Deg2Rad) * camAspect;
        Vector3 toTop = _camera.transform.up * Mathf.Tan(fovWHalf * Mathf.Deg2Rad);

        Vector3 topLeft = (_camera.transform.forward - toRight + toTop);
        float camScale = topLeft.magnitude * camFar;

        topLeft.Normalize();
        topLeft *= camScale;

        Vector3 topRight = (_camera.transform.forward + toRight + toTop);
        topRight.Normalize();
        topRight *= camScale;

        Vector3 bottomRight = (_camera.transform.forward + toRight - toTop);
        bottomRight.Normalize();
        bottomRight *= camScale;

        Vector3 bottomLeft = (_camera.transform.forward - toRight - toTop);
        bottomLeft.Normalize();
        bottomLeft *= camScale;

        // Custom Blit, encoding Frustum Corners as additional Texture Coordinates
        RenderTexture.active = dest;

        mat.SetTexture("_MainTex", source);

        GL.PushMatrix();
        GL.LoadOrtho();

        mat.SetPass(0);

        GL.Begin(GL.QUADS);

        GL.MultiTexCoord2(0, 0.0f, 0.0f);
        GL.MultiTexCoord(1, bottomLeft);
        GL.Vertex3(0.0f, 0.0f, 0.0f);

        GL.MultiTexCoord2(0, 1.0f, 0.0f);
        GL.MultiTexCoord(1, bottomRight);
        GL.Vertex3(1.0f, 0.0f, 0.0f);

        GL.MultiTexCoord2(0, 1.0f, 1.0f);
        GL.MultiTexCoord(1, topRight);
        GL.Vertex3(1.0f, 1.0f, 0.0f);

        GL.MultiTexCoord2(0, 0.0f, 1.0f);
        GL.MultiTexCoord(1, topLeft);
        GL.Vertex3(0.0f, 1.0f, 0.0f);

        GL.End();
        GL.PopMatrix();
    }
}
