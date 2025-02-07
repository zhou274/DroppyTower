using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;

namespace _DroppyTower
{
    public enum GameState
    {
        Prepare,
        Playing,
        Paused,
        PreGameOver,
        GameOver
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public static event System.Action<GameState, GameState> GameStateChanged;

        private static bool isRestart;
        public GameObject ContinueGameBtn;
        public GameObject BackMenuBtn;
        public GameState GameState
        {
            get
            {
                return _gameState;
            }
            private set
            {
                if (value != _gameState)
                {
                    GameState oldState = _gameState;
                    _gameState = value;

                    if (GameStateChanged != null)
                        GameStateChanged(_gameState, oldState);
                }
            }
        }

        public static int GameCount
        {
            get { return _gameCount; }
            private set { _gameCount = value; }
        }

        private static int _gameCount = 0;

        [Header("Set the target frame rate for this game")]
        [Tooltip("Use 60 for games requiring smooth quick motion, set -1 to use platform default frame rate")]
        public int targetFrameRate = 30;

        [Header("Current game state")]
        [SerializeField]
        private GameState _gameState = GameState.Prepare;

        // List of public variable for gameplay tweaking
        [Header("Gameplay Config")]
        [SerializeField]
        [Range(1, 10)]
        private int life;

        public int Life
        {
            get { return life; }
            private set { life = value; }
        }

        [SerializeField]
        [Range(0.01f, 10)]
        private float minSwaying = 0.5f;
        public float MinSwaying
        {
            get { return minSwaying; }
            set { minSwaying = value; }
        }


        [SerializeField]
        [Range(0.25f, 10)]
        private float maxSwaying = 5.0f;
        public float MaxSwaying
        {
            get { return maxSwaying; }
            private set { maxSwaying = value; }
        }

        [SerializeField]
        private int increaseSwayingPoint = 5;
        public int IncreaseSwayingPoint
        {
            get { return increaseSwayingPoint; }
            private set { increaseSwayingPoint = value; }
        }

        [SerializeField]
        [Range(0.01f, 1)]
        private float increaseSwayingRatio = 0.5f;
        public float IncreaseSwayingRatio
        {
            get { return increaseSwayingRatio; }
            private set { increaseSwayingRatio = value; }
        }

        [SerializeField]
        [Range(0,1)]
        private float tiltValue = 0.15f;
        public float TiltValue
        {
            get { return tiltValue; }
            private set { tiltValue = value; }
        }

        [SerializeField]
        private Vector3 localScale = new Vector3(5, 6, 5);
        public Vector3 LocalScale
        {
            get { return localScale; }
            private set { localScale = value; }
        }

        [SerializeField]
        [Range(0, 0.5f)]
        private float deviation = 0.1f;
        public float Deviation
        {
            get { return deviation; }
            private set { deviation = value; }
        }

        [SerializeField]
        [Range(0.1f, 0.9f)]
        private float fallBias = 0.5f;
        public float FallBias
        {
            get { return fallBias; }
            private set { fallBias = value; }
        }

        [SerializeField]
        private int scoreUp = 1;
        public int ScoreUp
        {
            get { return scoreUp; }
            private set { scoreUp = value; }
        }

        [SerializeField]
        private int scoreUpPerfect = 2;
        public int ScoreUpPerfect
        {
            get { return scoreUpPerfect; }
            private set { scoreUpPerfect = value; }
        }

        [SerializeField]
        private int earnCoin = 2;
        public int EarnCoin
        {
            get { return earnCoin; }
            private set { earnCoin = value; }
        }

        [SerializeField]
        private float timeScaleUpCube = 0.15f;
        public float TimeScaleUpCube
        {
            get { return timeScaleUpCube; }
            private set { timeScaleUpCube = value; }
        }

        [SerializeField]
        private float craneSwingingRadius = 6.0f;
        public float CraneSwingingRadius
        {
            get { return craneSwingingRadius; }
            private set { craneSwingingRadius = value; }
        }

        [SerializeField]
        private float craneSwingingSpeed = 2.0f;
        public float CraneSwingingSpeed
        {
            get { return craneSwingingSpeed; }
            private set { craneSwingingSpeed = value; }
        }

        [Header("Cloud Config")]
        [SerializeField]
        private float cloudSpeedMin = 0.5f;
        public float CloudSpeedMin
        {
            get { return cloudSpeedMin; }
            private set { cloudSpeedMin = value; }
        }

        [SerializeField]
        private float cloudSpeedMax = 3.0f;
        public float CloundSpeedMax
        {
            get { return cloudSpeedMax; }
            private set { cloudSpeedMax = value; }
        }

        [SerializeField]
        private int cloudSizeMin = 3;
        public int CloudSizeMin
        {
            get { return cloudSizeMin; }
            private set { cloudSizeMin = value; }
        }

        [SerializeField]
        private int cloudSizeMax = 10;
        public int CloundSizeMax
        {
            get { return cloudSizeMax; }
            private set { cloudSizeMax = value; }
        }
        [SerializeField]
        private float cameraSpeed = 2f;
        public float CameraSpeed
        {
            get { return cameraSpeed; }
            private set { cameraSpeed = value; }
        }

        // List of public variables referencing other objects
        [Header("Object References")]
        public PlayerController playerController;
        public Transform hookTrans;

        void OnEnable()
        {
            PlayerController.PlayerDied += PlayerController_PlayerDied;
        }

        void OnDisable()
        {
            PlayerController.PlayerDied -= PlayerController_PlayerDied;
        }

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                DestroyImmediate(Instance.gameObject);
                Instance = this;
            }
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        // Use this for initialization
        void Start()
        {
            // Initial setup
            Application.targetFrameRate = targetFrameRate;
            ScoreManager.Instance.Reset();

            PrepareGame();
        }

        // Listens to the event when player dies and call GameOver
        void PlayerController_PlayerDied()
        {
            GameOver();
        }

        // Make initial setup and preparations before the game can be played
        public void PrepareGame()
        {
            GameState = GameState.Prepare;

            // Automatically start the game if this is a restart.
            if (isRestart)
            {
                isRestart = false;
                StartGame();
            }
        }

        // A new game official starts
        public void StartGame()
        {
            GameState = GameState.Playing;
            if (SoundManager.Instance.background != null)
            {
                SoundManager.Instance.PlayMusic(SoundManager.Instance.background);
            }
        }

        // Called when the player died
        public void GameOver()
        {
            if (SoundManager.Instance.background != null)
            {
                SoundManager.Instance.StopMusic();
            }

            SoundManager.Instance.PlaySound(SoundManager.Instance.gameOver);
            //GameState = GameState.GameOver;
            GameCount++;
            ContinueGameBtn.SetActive(true);
            BackMenuBtn.SetActive(true);
            Time.timeScale = 0;
            // Add other game over actions here if necessary
        }
        public void BackMenu()
        {
            Time.timeScale = 1;
            GameState = GameState.GameOver;
        }
        public void ContinueGame()
        {
            Time.timeScale = 1;
            if (SoundManager.Instance.background != null)
            {
                //SoundManager.Instance.StopMusic();
                SoundManager.Instance.ResumeMusic();
            }
            SoundManager.Instance.PlaySound(SoundManager.Instance.gameOver);
            
            
        }

        // Start a new game
        public void RestartGame(float delay = 0)
        {
            isRestart = true;
            StartCoroutine(CRRestartGame(delay));
        }

        IEnumerator CRRestartGame(float delay = 0)
        {
            yield return new WaitForSeconds(delay);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void HidePlayer()
        {
            if (playerController != null)
                playerController.gameObject.SetActive(false);
        }

        public void ShowPlayer()
        {
            if (playerController != null)
                playerController.gameObject.SetActive(true);
        }
    }
}