using Unity.Cinemachine;
using UnityEngine;

namespace Services
{
    public static class AudioListenerController
    {
        public const string allowWwiseCameraFollowTag = "AllowAudioListenerFollow";
        public const string ignoreWwiseCameraFollowTag = "IgnoreAudioListenerFollow";

        static AudioListenerView audioListenerView;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Init()
        {
            audioListenerView = MonoBehaviour.FindAnyObjectByType<AudioListenerView>();
            if (audioListenerView == null)
            {
                Debug.LogError("No AudioListenerView found");
            }
        }
        public static void FollowCamera(float lerpTime)
        {
            ResetSubscriptions();
            CameraController.cinemachineBrainEvents.CameraCutEvent.AddListener(NewCameraThing);
            CameraController.cinemachineBrainEvents.CameraActivatedEvent.AddListener(ActiveCameraChanged);
            audioListenerView.Freeze(false);
            audioListenerView.FollowTarget(CameraController.cinemachineBrain.transform, lerpTime);
        }
        public static void FollowTransform(Transform newTarget, float lerpTime)
        {
            ResetSubscriptions();
            audioListenerView.Freeze(false);
            audioListenerView.FollowTarget(newTarget, lerpTime);
        }
        public static void MoveToPosition(Vector3 newPosition, float lerpTime)
        {
            ResetSubscriptions();
            audioListenerView.Freeze(false);
            audioListenerView.MoveToPosition(newPosition, lerpTime);
        }
        #region CinemachineBrain subscriptions
        static void ActiveCameraChanged(ICinemachineCamera oldCamera, ICinemachineCamera newCamera)
        {
            if (newCamera != null
                && ((CinemachineCamera)newCamera).gameObject.CompareTag(ignoreWwiseCameraFollowTag))
            {
                audioListenerView.Freeze(true);
            }
            else
            {
                audioListenerView.Freeze(false);
            }
        }
        static void NewCameraThing(ICinemachineCamera oldCamera, ICinemachineCamera newCamera)
        {
            if (newCamera != null
                && ((CinemachineCamera)newCamera).gameObject.CompareTag(ignoreWwiseCameraFollowTag))
                audioListenerView.Freeze(true);
            else
                audioListenerView.Freeze(false);
        }
        static void ResetSubscriptions()
        {
            audioListenerView.ResetListener();
            CameraController.cinemachineBrainEvents.CameraCutEvent.RemoveListener(NewCameraThing);
            CameraController.cinemachineBrainEvents.CameraActivatedEvent.RemoveListener(ActiveCameraChanged);
        }

        #endregion
    }
}
