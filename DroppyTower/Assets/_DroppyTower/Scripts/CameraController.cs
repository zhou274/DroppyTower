using UnityEngine;
using System.Collections;

namespace _DroppyTower
{
    public class CameraController : MonoBehaviour
    {
        public Transform playerTransform;
        private Vector3 velocity = Vector3.zero;
        private Vector3 originalDistance;

        [Header("Camera Follow Smooth-Time")]
        public float smoothTime = 0.1f;

        [Header("Shaking Effect")]
        // How long the camera shaking.
    public float shakeDuration = 0.1f;
        // Amplitude of the shake. A larger value shakes the camera harder.
        public float shakeAmount = 0.2f;
        public float decreaseFactor = 0.3f;
        [HideInInspector]
        public Vector3 originalPos;

        private float currentShakeDuration;
        private float currentDistance;
        bool firstPlay=true;

        void OnEnable()
        {
            PlayerController.PlayerLostLife += PlayerLostLife;
        }

        void OnDisable()
        {
            PlayerController.PlayerLostLife -= PlayerLostLife;
        }

        void Start()
        {
        }

        void Update()
        {
            if (GameManager.Instance != null)
            {
                if (GameManager.Instance.GameState == GameState.Playing)
                {
                    if (firstPlay)
                    {
                        originalDistance = transform.position - playerTransform.transform.position;
                        firstPlay = false;
                    }
                    Vector3 pos = playerTransform.position + originalDistance;
                    transform.position = Vector3.SmoothDamp(transform.position, pos, ref velocity, smoothTime);
                }
            }
        }

        void PlayerLostLife()
        {
            if (PlayerController.life > 0)
                ShakeCamera();
        }

        public void FixPosition()
        {
            transform.position = playerTransform.position + originalDistance;
        }

        public void ShakeCamera()
        {
            StartCoroutine(Shake());
        }

        IEnumerator Shake()
        {
            originalPos = transform.position;
            currentShakeDuration = shakeDuration;
            while (currentShakeDuration > 0)
            {
                transform.position = originalPos + Random.insideUnitSphere * shakeAmount;
                currentShakeDuration -= Time.deltaTime * decreaseFactor;
                yield return null;
            }
            transform.position = originalPos;
        }
    }
}