using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _DroppyTower
{
    public class CloudController : MonoBehaviour {

        // Use this for initialization
        void Start() {
            Debug.Log(gameObject.name);
        }

        // Update is called once per frame
        void Update() {
            Debug.Log(gameObject.name);
            if (PlayerController.life <=0)
            {
                gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
                float distance = gameObject.transform.position.y - GameManager.Instance.playerController.transform.position.y;
                if (distance > 3 * PlayerController.height)
                    Destroy(gameObject);
            }
    }
    }
}