using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ASL;


namespace SimpleDemos
{
    /// <summary>
    /// An example of how ASL_ObjectColliders interact with Unity Colliders. To set up an ASL_ObjectCollider, assign 
    /// a delegate function to the appropreate function within the ASL_ObjectCollider, ie. ASL_OnTriggerEnter, ASL_OnTriggerExit,
    /// ASL_OnTriggerStay. Note, ASL_ObjectCollider does not support physcis based Unity collisions between non Kinematic Rigidbodies.
    /// </summary>
    public class Demo_CollisionTests : MonoBehaviour
    {
        /// <summary>
        /// UI Element
        /// </summary>
        public Text OnCollisionEnterText;
        /// <summary>
        /// UI Element
        /// </summary>
        public Text OnCollisionExitText;
        /// <summary>
        /// UI Element
        /// </summary>
        public Text OnTriggerEnterText;
        /// <summary>
        /// UI Element
        /// </summary>
        public Text OnTriggerExitText;
        /// <summary>
        /// UI Element
        /// </summary>
        public Text OnOnTriggerStayText;

        ASLObject m_ASLObject;
        ASL_ObjectCollider m_ASLObjectCollider;

        // Start is called before the first frame update
        void Start()
        {
            Debug.Assert(OnCollisionEnterText != null);
            Debug.Assert(OnCollisionExitText != null);
            Debug.Assert(OnTriggerEnterText != null);
            Debug.Assert(OnTriggerExitText != null);
            Debug.Assert(OnOnTriggerStayText != null);
            m_ASLObjectCollider = gameObject.GetComponent<ASL_ObjectCollider>();
            Debug.Assert(m_ASLObjectCollider != null);
            m_ASLObjectCollider.ASL_OnCollisionEnter(OnCollisionEnterTest);
            m_ASLObjectCollider.ASL_OnCollisionExit(OnCollisionExitTest);
            m_ASLObjectCollider.ASL_OnTriggerEnter(OnTriggerEnterTest);
            m_ASLObjectCollider.ASL_OnTriggerExit(OnTriggerExitTest);
            m_ASLObjectCollider.ASL_OnTriggerStay(OnTriggerStayTest);
            m_ASLObject = gameObject.GetComponent<ASL.ASLObject>();
            Debug.Assert(m_ASLObject != null);
            m_ASLObject._LocallySetFloatCallback(DisplayCollision);
        }

        /// <summary>
        /// Delegate function for OnCollisionEnter
        /// </summary>
        void OnCollisionEnterTest(Collision collision)
        {
            m_ASLObject.SendAndSetClaim(() =>
            {
                float[] _myFloat = new float[1] { 0 };
                m_ASLObject.SendFloatArray(_myFloat);
            });
        }

        /// <summary>
        /// Delegate function for OnCollisionExt
        /// </summary>
        void OnCollisionExitTest(Collision collision)
        {
            m_ASLObject.SendAndSetClaim(() =>
            {
                float[] _myFloat = new float[1] { 1 };
                m_ASLObject.SendFloatArray(_myFloat);
            });
        }

        /// <summary>
        /// Delegate function for OnTriggerEnter
        /// </summary>
        /// <param name="other">Collider of the other object int the collision</param>
        void OnTriggerEnterTest(Collider other)
        {
            m_ASLObject.SendAndSetClaim(() =>
            {
                float[] _myFloat = new float[1] { 2 };
                m_ASLObject.SendFloatArray(_myFloat);
            });
        }

        /// <summary>
        /// Delegate function for OnTriggerExit
        /// </summary>
        /// <param name="other">Collider of the other object int the collision</param>
        void OnTriggerExitTest(Collider other)
        {
            m_ASLObject.SendAndSetClaim(() =>
            {
                float[] _myFloat = new float[1] { 3 };
                m_ASLObject.SendFloatArray(_myFloat);
            });
        }

        /// <summary>
        /// Delegate function for OnTriggerStay
        /// </summary>
        /// <param name="other">Collider of the other object int the collision</param>
        void OnTriggerStayTest(Collider other)
        {
            m_ASLObject.SendAndSetClaim(() =>
            {
                float[] _myFloat = new float[1] { 4 };
                m_ASLObject.SendFloatArray(_myFloat);
            });
        }

        /// <summary>
        /// Float function to update the UI across all players
        /// </summary>
        public static void DisplayCollision(string _id, float[] _myFloats)
        {
            ASL.ASLHelper.m_ASLObjects.TryGetValue(_id, out ASL.ASLObject _myObject);
            switch (_myFloats[0])
            {
                case 0:
                    //OnCollisionEnter
                    _myObject.GetComponent<Demo_CollisionTests>().OnCollisionEnterText.text = "OnCollisionEnter called with " + _myObject.gameObject.name + " at " + Time.time;
                    break;
                case 1:
                    //OnCollisionExit
                    _myObject.GetComponent<Demo_CollisionTests>().OnCollisionExitText.text = "OnCollisionExit called with " + _myObject.gameObject.name + " at " + Time.time;
                    break;
                case 2:
                    //OnTriggerEnter
                    _myObject.GetComponent<Demo_CollisionTests>().OnTriggerEnterText.text = "OnTriggerEnter called with " + _myObject.gameObject.name + " at " + Time.time;
                    break;
                case 3:
                    //OnTriggerExit
                    _myObject.GetComponent<Demo_CollisionTests>().OnTriggerExitText.text = "OnTriggerExit called with " + _myObject.gameObject.name + " at " + Time.time;
                    break;
                case 4:
                    //OnTriggerStay
                    _myObject.GetComponent<Demo_CollisionTests>().OnOnTriggerStayText.text = "OnTriggerStay called with " + _myObject.gameObject.name + " at " + Time.time;
                    break;
                default:
                    Debug.LogError("float passed to DisplayCollision was out of bounds");
                    break;
            }
        }
    }
}