using System;
using Unity.Cinemachine;
using UnityEngine;

namespace Services
{
    public class CameraController
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Initiate()
        {
            cinemachineBrain = MonoBehaviour.FindAnyObjectByType<CinemachineBrain>();
            cinemachineBrainEvents = cinemachineBrain.GetComponent<CinemachineBrainEvents>();
            mainCamera = cinemachineBrain.GetComponent<Camera>();
            if (mainCamera != null)
                ForceCameraResolution(mainCamera);
        }
        public static CinemachineBrain cinemachineBrain;
        public static CinemachineBrainEvents cinemachineBrainEvents;
        public static Camera mainCamera;

        static void ForceCameraResolution(Camera cam)
        {
            float targetAspect = 16.0f / 9.0f;
            
            float currentAspect = (float)Screen.width / (float)Screen.height;

            // Calculate scaling ratio
            float scaleHeight = currentAspect / targetAspect;

            if (scaleHeight < 1.0f)
            {
                Rect rect = cam.rect;
                rect.width = 1.0f;
                rect.height = scaleHeight;
                rect.x = 0;
                rect.y = (1.0f - scaleHeight) / 2.0f;
                cam.rect = rect;
            }
            else
            {
                float scaleWidth = 1.0f / scaleHeight;
                Rect rect = cam.rect;
                rect.width = scaleWidth;
                rect.height = 1.0f;
                rect.x = (1.0f - scaleWidth) / 2.0f;
                rect.y = 0;
                cam.rect = rect;
            }
        }
    }
}