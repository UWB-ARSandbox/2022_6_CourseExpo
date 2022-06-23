using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;

namespace StressTesting
{
    /// <summary>
    /// Moving ASL_Objects that detect collition with other ASL_Objects.
    /// </summary>
    public class StressTest_MoveingObject : MonoBehaviour
    {
        public float MovementSpeed = 5f;
        public float MaxDistance = 5f;
        public StressTest_CollisionCounter CollisionCounter;
        public enum TestMode { OnTriggerEnter, OnTriggerExit, OnTriggerStay }
        public TestMode Mode;

        Vector3 dir = Vector3.zero;
        ASLObject m_ASLObject;
        ASL_ObjectCollider m_ASLObjectCollider;

        // Start is called before the first frame update
        void Start()
        {
            m_ASLObjectCollider = gameObject.GetComponent<ASL_ObjectCollider>();
            Debug.Assert(m_ASLObjectCollider != null);
            m_ASLObject = gameObject.GetComponent<ASLObject>();
            Debug.Assert(m_ASLObject != null);

            switch (Mode)
            {
                case TestMode.OnTriggerEnter:
                    m_ASLObjectCollider.ASL_OnTriggerEnter(UpdateCounterOnCollision);
                    break;
                case TestMode.OnTriggerExit:
                    m_ASLObjectCollider.ASL_OnTriggerExit(UpdateCounterOnCollision);
                    break;
                case TestMode.OnTriggerStay:
                    m_ASLObjectCollider.ASL_OnTriggerStay(UpdateCounterOnCollision);
                    break;
                default:
                    break;
            }

            dir = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0);

        }

        // Update is called once per frame
        void Update()
        {
            m_ASLObject.SendAndSetClaim(() =>
            {
                if (transform.position.magnitude >= MaxDistance)
                {
                    dir = -transform.position.normalized;
                }
                else
                {
                    dir = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0);
                }
                Vector3 m_AdditiveMovementAmount = dir * MovementSpeed * Time.deltaTime;
                m_ASLObject.SendAndIncrementWorldPosition(m_AdditiveMovementAmount);
            });
        }

        void UpdateCounterOnCollision(Collider other)
        {
            CollisionCounter.GetComponent<ASLObject>().SendAndSetClaim(() =>
            {
                CollisionCounter.GetComponent<ASLObject>().SendFloatArray(new float[1] { 0 });
            });
        }
    }
}
