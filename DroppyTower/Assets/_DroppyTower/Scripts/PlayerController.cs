using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace _DroppyTower
{
    public class PlayerController : MonoBehaviour
    {
        public static event System.Action PlayerDied;
        public static event System.Action PlayerLostLife;

        public static bool CanDrop = true;
        public GameObject rope;
        public static float height;
        public static bool isCreateCube;
        float speed;
        float circleWidth;
        float circleHeight;
        float timeCounter;
        float timeCounterSwing;
        public GameObject hook;
        public GameObject cubeOnTop;
        public static Vector3 originHookPosition;
        public static Vector3 oriCubeOnTopPosition;
        public static Vector3 originRopePosition;
        public static float RootPosXCube;
        public static float LastPosXCube;
        public static bool isFirstCube=true;
        public static bool swinging = false;
        GameObject character;
        public static Vector3 swingingPosition;
        public static int life;
        public ParticleSystem scoreEffect;
        public ParticleSystem perfectEffect;
        public static float swing;
        public static Vector3 firstCubePosition;
        bool startCoroutine=true;
        public static int countCoroutine;
        public static Vector3 cubePivot;
        public GameObject Clinch;
        Quaternion oriRopeRotate;
        Quaternion oriHookRotate;
        Quaternion oriClinchRotate;
        public GameObject[] clouds;
        [SerializeField]
        int createCloudScore=0;
        //float minSwinging;
        public static int breakCount;
        Vector3 clinchScale;
        public GameObject addCoin;
        Vector3 bottomPosition;
        Vector3 topPosition;
        public bool die;
        Vector3 velocity = Vector3.zero;
        bool moveDown=true;
        public int perfectCount;
        private float cubeBoundX;
        private int cubeNumer = 0;
        public int CubeNumber
        {
            get { return cubeNumer; }
            private set { cubeNumer = value; }
        }

        public void SetCubeNumber(int number)
        {
            CubeNumber += number;
        }

        void OnEnable()
        {
            GameManager.GameStateChanged += OnGameStateChanged;
        }

        void OnDisable()
        {
            GameManager.GameStateChanged -= OnGameStateChanged;
        }

        void Start()
        {
            bottomPosition = Camera.main.transform.position;
            clinchScale = Clinch.transform.localScale;
            //minSwinging = GameManager.Instance.MinSwaying;
            oriHookRotate = hook.transform.rotation;
            oriRopeRotate = rope.transform.rotation;
            oriClinchRotate = Clinch.transform.rotation;
            originRopePosition = rope.transform.position;
            originHookPosition = hook.transform.position;
            oriCubeOnTopPosition = cubeOnTop.transform.position;
            speed = GameManager.Instance.CraneSwingingSpeed;
            circleHeight = GameManager.Instance.CraneSwingingRadius;
            circleWidth = GameManager.Instance.CraneSwingingRadius;
            // Setup
        }
        // Update is called once per frame
        void Update()
        {
            //Create cloud when player gain 7 score
            if (ScoreManager.Instance.Score > 10 && (ScoreManager.Instance.Score > createCloudScore + 2))
            {
                CreateCloud();
                createCloudScore = ScoreManager.Instance.Score;
            }

            //move down camera when the building fall out
            if (startCoroutine && countCoroutine > 0)
            {
                MoveCamera();
                startCoroutine = false;
            }

            //set max and min for swinging ratio
            if (swing > GameManager.Instance.MaxSwaying)
                swing = GameManager.Instance.MaxSwaying;
            if (swing < GameManager.Instance.MinSwaying)
                swing = GameManager.Instance.MinSwaying;

            if (GameManager.Instance.GameState != GameState.Prepare)
            {
                swing = CalculateSwing(CubeNumber);

                if (life <= 0 && GameManager.Instance.GameState == GameState.Playing)
                {
                    GameManager.Instance.GameOver();
                    //Die();
                }
                    
                timeCounter += Time.deltaTime * speed;

                //move rope and hook
            float yTop= oriCubeOnTopPosition.y+ Mathf.Sin(timeCounter) * circleHeight;
            float yRope=originRopePosition.y+ Mathf.Sin(timeCounter) * circleHeight;
                float zRotate= oriRopeRotate.z+ Mathf.Cos(timeCounter) * circleWidth;
            cubeOnTop.transform.position = new Vector3(cubeOnTop.transform.position.x, yTop, cubeOnTop.transform.position.z);
                rope.GetComponent<Rigidbody>().centerOfMass = new Vector3(0, 1, 0);
                hook.transform.rotation = oriHookRotate;
                Clinch.transform.rotation = oriClinchRotate;
            rope.transform.position = new Vector3(rope.transform.position.x, yRope, rope.transform.position.z);
            rope.transform.localRotation= Quaternion.Euler(rope.transform.localRotation.x, rope.transform.localRotation.y, zRotate*3);
                // Activities that take place every frame              
                if (isCreateCube)
                {
                    CreateNewCube();
                }
            }
            //Swinging the building
            if (swinging)
            {
                timeCounterSwing += Time.deltaTime;
                float bounusSwing = swing * Mathf.Abs(RootPosXCube - LastPosXCube) / (2 * GameManager.Instance.LocalScale.x * cubeBoundX) * GameManager.Instance.TiltValue;
                float zSwing = Mathf.Cos(timeCounterSwing) * (swing + bounusSwing) * 3f;
                character.transform.rotation = Quaternion.Euler(0, CharacterManager.Instance.characters[CharacterManager.Instance.CurrentCharacterIndex].transform.eulerAngles.y, zSwing);
            }
            if(die)
            {
                if (moveDown)
                {
                    Camera.main.transform.position = Vector3.SmoothDamp(Camera.main.transform.position, bottomPosition, ref velocity, GameManager.Instance.CameraSpeed);
                    if (Mathf.Abs(Camera.main.transform.position.y - bottomPosition.y) < 1f)
                        moveDown = !moveDown;
                }
                else
                {
                    Camera.main.transform.position = Vector3.SmoothDamp(Camera.main.transform.position, topPosition, ref velocity, GameManager.Instance.CameraSpeed);
                    if (Mathf.Abs(Camera.main.transform.position.y - topPosition.y) < 1f)
                        moveDown = !moveDown;
                }
            }
        }

        public void LostLife()
        {
            SoundManager.Instance.PlaySound(SoundManager.Instance.wrong);
            if (PlayerLostLife != null)
                PlayerLostLife();

        }

        // Listens to changes in game state
        void OnGameStateChanged(GameState newState, GameState oldState)
        {
            if (newState == GameState.Playing)
            {
                CanDrop = true;
                cubeOnTop.transform.position += new Vector3(0, height*3, 0);
                cubeOnTop.SetActive(true);
                gameObject.transform.position = new Vector3(gameObject.transform.position.x,0,gameObject.transform.position.y);
                swing = 0;
                life = GameManager.Instance.Life;
                LastPosXCube = RootPosXCube = 0;
                isCreateCube = false;
                isFirstCube = true;
                swinging = false;
                character = (GameObject)Instantiate(CharacterManager.Instance.characters[CharacterManager.Instance.CurrentCharacterIndex], new Vector3(hook.transform.position.x, hook.transform.position.y -0.5f- (hook.GetComponent<MeshFilter>().mesh.bounds.extents.y) * hook.transform.lossyScale.y, hook.transform.position.z), Quaternion.Euler(0, CharacterManager.Instance.characters[CharacterManager.Instance.CurrentCharacterIndex].transform.eulerAngles.y, 0));
                Clinch.transform.parent = null;
                Clinch.transform.position = character.transform.position;
                Clinch.transform.parent = character.transform;
                Clinch.transform.localScale = clinchScale;
                character.transform.localScale = GameManager.Instance.LocalScale;
                character.GetComponent<CubeController>().enabled = true;
                height = (character.GetComponent<MeshFilter>().mesh.bounds.extents.y) * character.transform.lossyScale.y*2.0f;
                //character.AddComponent<HingeJoint>();
                //character.GetComponent<HingeJoint>().anchor = new Vector3(0, 0.5f, 0);
                //character.GetComponent<HingeJoint>().connectedBody = hook.GetComponent<Rigidbody>();
                character.GetComponent<Rigidbody>().freezeRotation = true;


                // Do whatever necessary when a new game starts
            }
        }

        //Create effect when player gain score
        public void CreateScoreEffect(Vector3 Position,GameObject Starsize)
        {
            scoreEffect.transform.position = Position;
            var main = scoreEffect.main;
            main.startSize = Starsize.transform.lossyScale.x;
            scoreEffect.Play();

        }

        //create effect when player gain perfect score
        public void CreatePerfectEffect(Vector3 Position,GameObject Parent)
        {
            perfectEffect.transform.parent=Parent.transform;
            perfectEffect.transform.localPosition = Position;
            var main = scoreEffect.main;
            perfectEffect.Play();

        }

        public void AddCoin(Vector3 position)
        {
            GameObject addCoinCanvas = (GameObject)Instantiate(addCoin, position, Quaternion.Euler(Camera.main.transform.eulerAngles.x, 0, 0));
            Text Cointext = addCoinCanvas.transform.GetChild(0).GetComponent<Text>();
            if (Cointext != null)
                Cointext.text = "+" + GameManager.Instance.EarnCoin.ToString();
            StartCoroutine(MoveAndFade(addCoinCanvas));

        }

        IEnumerator MoveAndFade(GameObject canvas)
        {
            var startTime = Time.time;
            float runTime = 1.5f;
            float timePast = 0;
            var oriPos = canvas.transform.position;
            while (Time.time < startTime + runTime)
            {
                timePast += Time.deltaTime;
                float factor = timePast / runTime;
                canvas.transform.position = oriPos + new Vector3(0, factor * 5.0f, 0);
                canvas.GetComponent<CanvasGroup>().alpha = 1 - factor;
                yield return null;
            }
            Destroy(canvas);

        }

        //create cloud at random position
        void CreateCloud()
        {
            int amount = Random.Range(1, 3);

            for (int i = 1; i <= amount; i++)
            {
                float width = Random.Range(-21, 21);
                int cloudNumber = Random.Range(0, 3);
                float velocity = Random.Range(GameManager.Instance.CloudSpeedMin, GameManager.Instance.CloundSpeedMax);
                int direction = Random.Range(-1, 1);
                int rand = Random.Range(0, 2);
                Quaternion cloudQuaternion = Quaternion.Euler(Vector3.zero);
                if(rand == 0)
                {
                    direction = 1;
                    cloudQuaternion = Quaternion.Euler(Vector3.zero);
                }
                else
                {
                    direction = -1;
                    cloudQuaternion = Quaternion.Euler(0, 180, 0);
                }
                GameObject cloud = (GameObject)Instantiate(clouds[cloudNumber], new Vector3(width, gameObject.transform.position.y + height * 8, gameObject.transform.position.z), cloudQuaternion);
                int size = Random.Range(GameManager.Instance.CloudSizeMin, GameManager.Instance.CloundSizeMax);
                cloud.transform.localScale = new Vector3(size, size, size);
                cloud.AddComponent<Rigidbody>();
                cloud.GetComponent<Rigidbody>().useGravity = false;
                cloud.GetComponent<Rigidbody>().velocity = new Vector3(direction * velocity, 0, 0);
            }
        }

        public void MoveCamera()
        {
            StartCoroutine(_MoveCamera());
        }

        IEnumerator _MoveCamera()
        {
            var startTime = Time.time;
            var originPos = gameObject.transform.position;
            Vector3 targetPosition = gameObject.transform.position - new Vector3(0,height, 0);
            float runTime = 0.1f;
            float timePast = 0;

            while (Time.time < startTime + runTime)
            {
                timePast += Time.deltaTime;
                float factor = timePast / runTime;
                gameObject.transform.position = Vector3.Lerp(originPos, targetPosition, factor);
                yield return null;
            }
            countCoroutine -= 1;
            if (countCoroutine > 0)
                MoveCamera();
            else
                startCoroutine = true;
        }

        //create cube at hook position
        void CreateNewCube()
        {
            breakCount = 0;
            Clinch.SetActive(true);
            isFirstCube = false;
            GameObject cube = (GameObject)Instantiate(CharacterManager.Instance.characters[CharacterManager.Instance.CurrentCharacterIndex], new Vector3(hook.transform.position.x, hook.transform.position.y-0.5f - (hook.GetComponent<MeshFilter>().mesh.bounds.extents.y) * hook.transform.lossyScale.y, hook.transform.position.z), Quaternion.Euler(0, CharacterManager.Instance.characters[CharacterManager.Instance.CurrentCharacterIndex].transform.eulerAngles.y, 0));
            cubeBoundX = cube.GetComponent<MeshFilter>().mesh.bounds.extents.x;
            Clinch.transform.parent = null;
            Clinch.transform.position = cube.transform.position;
            Clinch.transform.parent = cube.transform;
            Clinch.transform.localScale = clinchScale;
            cube.transform.localScale = new Vector3(0, GameManager.Instance.LocalScale.y, 0);
            isCreateCube = false;
            cube.GetComponent<CubeController>().enabled = true;
            //cube.AddComponent<HingeJoint>();
            //cube.GetComponent<HingeJoint>().anchor = new Vector3(0, 0.5f, 0);
            //cube.GetComponent<HingeJoint>().axis = new Vector3(0, 0, 1);
            //cube.GetComponent<HingeJoint>().connectedBody = hook.GetComponent<Rigidbody>();
            cube.GetComponent<Rigidbody>().freezeRotation = true;
        }

        // Calls this when the player dies and game over
        public void Die()
        {
            
            isCreateCube = false;
            //cubeOnTop.SetActive(false);
            if (PlayerDied != null)
                PlayerDied();
            topPosition = Camera.main.transform.position;
            // Fire event
        }

        float CalculateSwing(int number)
        {
            float newSwing = GameManager.Instance.MinSwaying;
            if (CubeNumber <= 2)
                return newSwing;
            else
            {
                newSwing = (CubeNumber - 2 / GameManager.Instance.IncreaseSwayingPoint) * GameManager.Instance.IncreaseSwayingRatio;
                return Mathf.Clamp(newSwing, GameManager.Instance.MinSwaying, GameManager.Instance.MaxSwaying);
            }
        }
    }
}