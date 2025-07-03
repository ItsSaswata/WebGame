// CameraShake.cs
using Unity.Cinemachine;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    private CinemachineImpulseSource impulseSource;

    void Awake()
    {
        Instance = this;
        impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    public void Shake(float intensity = 1f)
    {
        if (impulseSource != null)
        {
            impulseSource.GenerateImpulse(intensity);
        }
    }
}
