using System.Collections;
using UnityEngine;

namespace _DroppyTower
{
    public class CubeController : MonoBehaviour
    {
        private bool isCollide = true;
        private bool isNewCube = true;
        private bool hasCollided;
        bool previous = false;
        bool isFirstCube = true;
        Vector3 center;
        GameObject oldCube;
        Transform particle = null;
        float distance;
        float biasDistance;
        float failForceX;
        float failForceY;
        float leftRightDistance;
        float compareDistance;
        bool fail;
        bool breakFail;
        float reverseForce;
        float breakForce = 50000;
        int cubeNumber;
        Vector3 pivot;
        float angle;
        bool perfect;
        Vector3 oriPos;
        float originRotation;
        bool isFix;
        Rigidbody cubeRigid;

        void Awake()
        {
            cubeRigid = gameObject.transform.GetComponent<Rigidbody>();
            StartCoroutine(ScaleUp());
        }

        void OnEnable()
        {
            GameManager.GameStateChanged += OnGameStateChanged;
        }

        void OnGameStateChanged(GameState newState, GameState oldState)
        {
            if (newState == GameState.GameOver)
            {
                gameObject.GetComponent<Rigidbody>().isKinematic = true;
            }
            if(newState==GameState.Playing)
            {
                gameObject.GetComponent<Rigidbody>().isKinematic = false;
            }
        }

        IEnumerator ScaleUp()
        {
            float speedScale = 1 / GameManager.Instance.TimeScaleUpCube;
            float value = 0;
            Vector3 originalScale = new Vector3(0, GameManager.Instance.LocalScale.y, 0);
            while (value < 1)
            {
                value += Time.deltaTime * speedScale;
                transform.localScale = Vector3.Lerp(originalScale, GameManager.Instance.LocalScale, value);
                yield return null;
            }
            PlayerController.CanDrop = true;
        }

        IEnumerator ScaleDown()
        {
            float speedScale = 1 / GameManager.Instance.TimeScaleUpCube;
            float value = 0;
            while (value < 1)
            {
                value += Time.deltaTime * speedScale;
                transform.localScale = Vector3.Lerp(GameManager.Instance.LocalScale, Vector3.zero, value);
                yield return null;
            }
            Destroy(gameObject);
        }

        void OnDisable()
        {
            GameManager.GameStateChanged -= OnGameStateChanged;
        }

        float GetBiasDistance(float biasValue)
        {
            return gameObject.GetComponent<MeshFilter>().mesh.bounds.extents.x * GameManager.Instance.LocalScale.x * 2 * biasValue;
        }

        void OnCollisionEnter(Collision col)
        {
            // Play drop sound.
            SoundManager.Instance.PlaySound(SoundManager.Instance.drop);

            //Check the building is new when enter collision
            if (!isNewCube && isCollide)
            {
                biasDistance = gameObject.transform.position.x - col.gameObject.transform.position.x;
                distance = Mathf.Abs(biasDistance);
                leftRightDistance = gameObject.transform.position.x - col.gameObject.transform.position.x;

                if (col.gameObject.tag == "Player")
                {
                    //Compare distance between center old and new building to make it fall out or not
                    if (distance < GetBiasDistance(GameManager.Instance.FallBias))
                    {
                       
                        if (leftRightDistance < 0)
                        {
                            failForceX = -20 * (GetBiasDistance(GameManager.Instance.FallBias) - distance) / GetBiasDistance(GameManager.Instance.FallBias);
                            failForceX = Mathf.Clamp(failForceX, -20, -5);
                            failForceY = -Random.Range(19, 25);
                        }
                        else
                        {
                            failForceX = 20 * (GetBiasDistance(GameManager.Instance.FallBias) - distance) / GetBiasDistance(GameManager.Instance.FallBias);
                            failForceX = Mathf.Clamp(failForceX, 5, 20);
                            failForceY = -Random.Range(19, 25);
                        }

                        //Perfect when distance is smaller than deviation
                        if (distance < GetBiasDistance(GameManager.Instance.Deviation))
                        {
                            GameManager.Instance.playerController.perfectCount += 1;
                            oldCube = col.gameObject;
                            gameObject.transform.parent = oldCube.transform;
                            gameObject.transform.localPosition = new Vector3(0, gameObject.transform.localPosition.y, 0);
                            Vector3 position = new Vector3(0, -0.5f, 0);
                            GameManager.Instance.playerController.AddCoin(new Vector3(gameObject.transform.position.x + PlayerController.height, gameObject.transform.position.y + PlayerController.height * 1f, gameObject.transform.position.z));
                            GameManager.Instance.playerController.CreatePerfectEffect(position, gameObject);
                            GameManager.Instance.playerController.CreateScoreEffect(new Vector3(gameObject.transform.position.x, oldCube.transform.position.y + 0.001f, gameObject.transform.position.z), gameObject);
                            gameObject.GetComponent<Rigidbody>().mass = 10f;
                            oriPos = gameObject.transform.localPosition;
                            perfect = true;
                            hasCollided = true;
                            StartCoroutine(FixRotation());
                            //SoundManager.Instance.PlaySound(SoundManager.Instance.drop);
                            ScoreManager.Instance.AddScore(GameManager.Instance.ScoreUpPerfect);
                            SoundManager.Instance.PlaySound(SoundManager.Instance.musicNote[GameManager.Instance.playerController.perfectCount - 1]);
                            if (GameManager.Instance.playerController.perfectCount >= 7)
                            {
                                SoundManager.Instance.PlaySound(SoundManager.Instance.Cheer);
                                GameManager.Instance.playerController.perfectCount = 0;
                            }
                            CoinManager.Instance.AddCoins(GameManager.Instance.EarnCoin);
                            GameManager.Instance.playerController.SetCubeNumber(1);
                        }
                        //Or score when distance is bigger than deviation
                        else
                        {
                            GameManager.Instance.playerController.perfectCount = 0;
                            oldCube = col.gameObject;
                            gameObject.GetComponent<Rigidbody>().mass = 10f;
                            GameManager.Instance.playerController.CreateScoreEffect(new Vector3(gameObject.transform.position.x, oldCube.transform.position.y + 0.001f, gameObject.transform.position.z), gameObject);
                            gameObject.transform.parent = oldCube.transform;
                            oriPos = gameObject.transform.localPosition;
                            hasCollided = true;
                            if (distance > GetBiasDistance(GameManager.Instance.FallBias) - GetBiasDistance(GameManager.Instance.FallBias) * 0.3f)
                                StartCoroutine(RotateEffect());
                            else
                            {
                                perfect = true;
                                StartCoroutine(FixRotation());
                            }
//                            SoundManager.Instance.PlaySound(SoundManager.Instance.drop);
                            SoundManager.Instance.PlaySound(SoundManager.Instance.score);
                            ScoreManager.Instance.AddScore(GameManager.Instance.ScoreUp);
                            GameManager.Instance.playerController.SetCubeNumber(1);
                            PlayerController.LastPosXCube = PlayerController.LastPosXCube + biasDistance;
                        }
                        //if (compareDistance > (gameObject.GetComponent<MeshFilter>().mesh.bounds.extents.x) * gameObject.transform.lossyScale.x * 0.2f)
                        //{
                        //    PlayerController.swing += GameManager.Instance.IncreaseSwayingRatio;
                        //    Debug.Log("PLUS = " + PlayerController.swing);
                        //}
                        //if (compareDistance < (gameObject.GetComponent<MeshFilter>().mesh.bounds.extents.x) * gameObject.transform.lossyScale.x * 0.1f)
                        //{
                        //    PlayerController.swing -= GameManager.Instance.IncreaseSwayingRatio;
                        //    Debug.Log("SUB = " + PlayerController.swing);
                        //}
                    }
                    //When distance is so big then the building will fall out and add more force to it to make effect to older building and make older building not kinematic anymore when on collision.
                    else
                    {
                        GameManager.Instance.playerController.perfectCount = 0;
                        if (leftRightDistance < 0)
                        {
                            failForceX = -20 * (distance - GetBiasDistance(GameManager.Instance.FallBias)) / GetBiasDistance(GameManager.Instance.FallBias);
                            failForceX = Mathf.Clamp(failForceX, -23, -8);
                            failForceY = -Random.Range(19, 25);
                        }
                        else
                        {
                            failForceX = 20 * (distance - GetBiasDistance(GameManager.Instance.FallBias)) / GetBiasDistance(GameManager.Instance.FallBias);
                            failForceX = Mathf.Clamp(failForceX, 8, 23);
                            failForceY = -Random.Range(19, 25);
                        }

                        oldCube = col.gameObject;
                        cubeRigid.velocity = Vector3.zero;
                        cubeRigid.mass = 10;
                        cubeRigid.constraints = RigidbodyConstraints.None;
                        cubeRigid.AddForce(Vector3.down * 3700);
                        cubeRigid.velocity = new Vector3(failForceX, 0, -40);
                        cubeRigid.useGravity = true;
                        cubeRigid.angularDrag = 6f;
                        fail = true;
                        if (oldCube.GetComponent<CubeController>().cubeNumber != 1)
                            oldCube.GetComponent<Rigidbody>().isKinematic = false;
                        GameManager.Instance.playerController.SetCubeNumber(1);

                    }
                }
                //This is for case the building is the first one
                if (isFirstCube && isCollide)
                {
                    GameManager.Instance.playerController.CreateScoreEffect(new Vector3(gameObject.transform.position.x, gameObject.transform.position.y + 0.1f - (gameObject.GetComponent<MeshFilter>().mesh.bounds.extents.y) * gameObject.transform.lossyScale.y * 2, gameObject.transform.position.z), gameObject);
                    PlayerController.swingingPosition = gameObject.transform.position;
                    gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
                    StartCoroutine(FixRotation());
                    hasCollided = true;
//                    SoundManager.Instance.PlaySound(SoundManager.Instance.drop);
                    SoundManager.Instance.PlaySound(SoundManager.Instance.score);
                    PlayerController.RootPosXCube = gameObject.transform.localPosition.x;
                    PlayerController.LastPosXCube = gameObject.transform.localPosition.x;
                    GameManager.Instance.playerController.SetCubeNumber(1);
                }

                if (isCollide && !isNewCube && col.gameObject.tag != "Player" && !isFirstCube)
                {
                    GameManager.Instance.playerController.perfectCount = 0;
                    Destroy(gameObject);
                }
                isCollide = false;
            }

            //Destroy the building if it collided with something else
            if (col.gameObject.tag != "Player" && !isFirstCube && fail)
            {
                GameManager.Instance.playerController.perfectCount = 0;
                Destroy(gameObject);
            }
        }

        //When exit collision with the older building make it kinematic again
        private void OnCollisionExit(Collision collision)
        {
            if (oldCube != null)
            if (fail && oldCube.GetComponent<FixedJoint>() != null)
            {
                oldCube.GetComponent<Rigidbody>().isKinematic = true;
            }
        }

        //When camera can see the building it will breakable
        private void OnBecameVisible()
        {
            if (gameObject.GetComponent<FixedJoint>() != null)
            {
                gameObject.GetComponent<FixedJoint>().breakForce = Mathf.Infinity;
                gameObject.GetComponent<FixedJoint>().breakTorque = breakForce;
                cubeRigid.mass = 10;
            }
        }

        //When the building out of camera make it unbreakable
        void OnBecameInvisible()
        {
            if (gameObject.GetComponent<FixedJoint>() != null)
            {
                gameObject.GetComponent<FixedJoint>().breakForce = Mathf.Infinity;
                gameObject.GetComponent<FixedJoint>().breakTorque = Mathf.Infinity;
                cubeRigid.mass = 100;
            }
            else if (cubeNumber != 1 && (isCollide || fail))
            {
                Destroy(gameObject);
            }
        }

        //when joint is break add force and temporary remove kinematic to make effect to the building under
        private void OnJointBreak(float breakForce)
        {
            gameObject.transform.localPosition = oriPos;
            cubeRigid.velocity = Vector3.zero;
            if (!perfect)
                PlayerController.LastPosXCube = PlayerController.LastPosXCube - biasDistance;
            if (leftRightDistance < 0)
            {
                reverseForce = -20;
            }
            else
            {
                reverseForce = 20;
            }
            cubeRigid.isKinematic = false;
            if (!breakFail)
            {
                cubeRigid.mass = 10;
                cubeRigid.constraints = RigidbodyConstraints.None;
                cubeRigid.AddForce(Vector3.down * 4000);
                cubeRigid.useGravity = true;
                cubeRigid.angularDrag = 6;
                cubeRigid.velocity = new Vector3(reverseForce, 0, -20);
                if (gameObject.transform.childCount > 0)
                {
                    for (int i = 0; i < gameObject.transform.childCount; i++)
                    {
                        particle = gameObject.transform.GetChild(i);
                        particle.parent = null;
                    }
                }
                fail = true;
                if (oldCube != null && oldCube.GetComponent<CubeController>().cubeNumber != 1 && PlayerController.breakCount < 3)
                {
                    oldCube.GetComponent<Rigidbody>().isKinematic = false;
                    PlayerController.breakCount += 1;
                }
                breakFail = true;
                ScoreManager.Instance.AddScore(-1);
                GameManager.Instance.playerController.SetCubeNumber(-1);
                PlayerController.originHookPosition -= new Vector3(0, PlayerController.height, 0);
                PlayerController.originRopePosition -= new Vector3(0, PlayerController.height, 0);
                PlayerController.oriCubeOnTopPosition -= new Vector3(0, PlayerController.height, 0);
                PlayerController.countCoroutine += 1;
            }
        }

        // Use this for initialization
        void Start()
        {
            originRotation = gameObject.transform.eulerAngles.y;
            if (!PlayerController.isFirstCube)
                isFirstCube = false;
        }

        //Make the building a little tilt for more interesting
        IEnumerator RotateEffect()
        {
            if (leftRightDistance < 0)
            {
                angle = 1f;
            }
            else
            {
                angle = -1f;
            }
            oriPos = gameObject.transform.localPosition;
            pivot = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y - gameObject.GetComponent<MeshFilter>().mesh.bounds.extents.y * gameObject.transform.lossyScale.y * 2, gameObject.transform.position.z);
            Vector3 curPos = gameObject.transform.position;
            var startTime = Time.time;
            float runTime = 0.25f;
            float timePast = 0;
            while (Time.time < startTime + runTime)
            {
                timePast += Time.deltaTime;
                transform.RotateAround(pivot, Vector3.forward, angle);
                yield return null;
            }

            StartCoroutine(FixRotation());
        }

        //Fix rotation after  the building have been tilt
        IEnumerator FixRotation()
        {
            gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, PlayerController.oriCubeOnTopPosition.z);
            var oriAngle = gameObject.transform.rotation;
            Quaternion targetAngle = Quaternion.Euler(0, originRotation, 0);
            if (oldCube != null)
            {
                targetAngle = oldCube.transform.rotation;
            }
            else
            {
                isFix = true;
            }
            Vector3 curPos = gameObject.transform.localPosition;
            if(cubeNumber != 1)
                if(!isFix)
                    gameObject.transform.rotation = Quaternion.Slerp(oriAngle, targetAngle, 1);
            if (!isFirstCube && !perfect)
            {
                gameObject.transform.localPosition = new Vector3(oriPos.x, Mathf.Lerp(curPos.y, oriPos.y, 1), oriPos.z);
            }
            if (isFirstCube)
            {
                cubeNumber = 1;
                cubeRigid.isKinematic = true;
                cubeRigid.mass = 100;
                PlayerController.firstCubePosition = gameObject.transform.position;
                PlayerController.cubePivot = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y - gameObject.GetComponent<MeshFilter>().mesh.bounds.extents.y * gameObject.transform.lossyScale.y * 2, gameObject.transform.position.z);
                PlayerController.originHookPosition += new Vector3(0, PlayerController.height, 0);
                PlayerController.oriCubeOnTopPosition += new Vector3(0, PlayerController.height, 0);
                PlayerController.originRopePosition += new Vector3(0, PlayerController.height, 0);
                PlayerController.isCreateCube = true;
            }
            else
            {
                cubeRigid.freezeRotation = false;
                gameObject.AddComponent<FixedJoint>();
                if (oldCube != null)
                {
                    gameObject.GetComponent<FixedJoint>().connectedBody = oldCube.gameObject.GetComponent<Rigidbody>();
                }
                gameObject.GetComponent<FixedJoint>().anchor = new Vector3(0, -0.5f, 0);
                gameObject.GetComponent<FixedJoint>().breakTorque = breakForce;
                gameObject.GetComponent<FixedJoint>().breakForce = Mathf.Infinity;
                gameObject.GetComponent<FixedJoint>().axis = new Vector3(0, 0, 1);
                PlayerController.swinging = true;
                cubeRigid.isKinematic = true;
            }
            if (hasCollided && gameObject.GetComponent<FixedJoint>() != null && previous && oldCube != null)
            {
                StartCoroutine(MoveCameraUp());
                hasCollided = false;
                previous = false;
            }
            yield return null;
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null && GameManager.Instance.playerController != null)
            {
                if (previous)
                {
                    if (PlayerController.life > 0)
                    {
                        PlayerController.life -= 1;
                        GameManager.Instance.playerController.LostLife();
                    }
                    PlayerController.isCreateCube = true;
                }
                GameManager.Instance.playerController.perfectCount = 0;
            }
        }

        IEnumerator MoveCameraUp()
        {
            var oriPlayerPosition = GameManager.Instance.playerController.transform.position;
            var oriHookPos = PlayerController.originHookPosition;
            var oriCubeTopPos = PlayerController.oriCubeOnTopPosition;
            var oriRopePosition = PlayerController.originRopePosition;
            var playerDes = oriPlayerPosition + new Vector3(0, PlayerController.height, 0);
            var hookDes = oriHookPos + new Vector3(0, PlayerController.height, 0);
            var topDes = oriCubeTopPos + new Vector3(0, PlayerController.height, 0);
            var ropeDes = oriRopePosition + new Vector3(0, PlayerController.height, 0);
            var startTime = Time.time;
            float runTime = 0.25f;
            float timePast = 0;
            while (Time.time < startTime + runTime)
            {
                timePast += Time.deltaTime;
                float factor = timePast / runTime;
                PlayerController.originHookPosition = new Vector3(hookDes.x, Mathf.Lerp(oriHookPos.y, hookDes.y, factor), hookDes.z);
                PlayerController.oriCubeOnTopPosition = new Vector3(topDes.x, Mathf.Lerp(oriCubeTopPos.y, topDes.y, factor), topDes.z);
                PlayerController.originRopePosition = new Vector3(ropeDes.x, Mathf.Lerp(oriRopePosition.y, ropeDes.y, factor), ropeDes.z);
                GameManager.Instance.playerController.transform.position = new Vector3(playerDes.x, Mathf.Lerp(oriPlayerPosition.y, playerDes.y, factor), playerDes.z);
                yield return null;
            }
            PlayerController.isCreateCube = true;
        }

        // Update is called once per frame
        void Update()
        {
            if (isNewCube)
            {
                transform.position = GameManager.Instance.hookTrans.position;
            }

            //Define velocity for the building when it is fall out
            Vector3 limitVelocity = new Vector3(reverseForce, -20, -20);
            if (breakFail)
            {
                if (cubeRigid.velocity.magnitude > limitVelocity.magnitude)
                {
                    cubeRigid.velocity = new Vector3(reverseForce, -20, -20);
                }
            }

            if (fail && oldCube != null)
            {
                distance = Mathf.Abs(gameObject.transform.position.x - oldCube.transform.position.x);
                if (distance > (gameObject.GetComponent<MeshFilter>().mesh.bounds.extents.x) * gameObject.transform.lossyScale.x)
                {
                    cubeRigid.velocity = new Vector3(failForceX, -30, failForceY);
                }
                else
                {
                    cubeRigid.velocity = new Vector3(failForceX, -30, failForceY);
                }
            }

            if (!fail && oldCube != null && gameObject.transform.position.y > PlayerController.height * 4 && !isCollide && !isNewCube && !previous && !hasCollided && gameObject.GetComponent<FixedJoint>() != null)
            {
                if (oldCube.GetComponent<Rigidbody>().mass > 10)
                {
                    gameObject.GetComponent<FixedJoint>().breakTorque = Mathf.Infinity;
                }
                else
                {
                    gameObject.GetComponent<FixedJoint>().breakTorque = breakForce;
                }
                if (oldCube.GetComponent<FixedJoint>() == null && !isCollide && !isFirstCube && transform.position.y > PlayerController.height * 3 && !fail)
                {
                    fail = true;
                    OnJointBreak(100000);
                }
            }

            //Drop the building from hook when player touch
            if (GameManager.Instance.GameState == GameState.Playing && PlayerController.CanDrop)
            {
                if (cubeRigid.velocity.y > 0)
                    cubeRigid.velocity = new Vector3(cubeRigid.velocity.x, 0, cubeRigid.velocity.z);
                if (Input.GetMouseButtonDown(0) && isNewCube)
                {
                    if (gameObject.transform.childCount > 0)
                    {
                        for (int i = 0; i < gameObject.transform.childCount; i++)
                        {
                            var child = gameObject.transform.GetChild(i);
                            if (child.name == "Clinch")
                            {
                                child.parent = null;
                            }
                        }
                    }
                    GameManager.Instance.playerController.Clinch.SetActive(false);
                    if (!isFirstCube)
                        cubeRigid.mass = 0;
                    gameObject.transform.rotation = Quaternion.Euler(0, gameObject.transform.eulerAngles.y, 0);
                    cubeRigid.velocity = new Vector3(0, -70, 0);
                    isNewCube = false;
                    previous = true;
                    PlayerController.CanDrop = false;
                }

                if (gameObject.GetComponent<FixedJoint>() != null)
                {
                    if (!isCollide && !isFirstCube && !previous && gameObject.GetComponent<FixedJoint>().connectedBody == null && !fail)
                    {
                        OnJointBreak(10000);
                        fail = true;
                    }
                }
            }

            if ((fail || breakFail) && PlayerController.life <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}
