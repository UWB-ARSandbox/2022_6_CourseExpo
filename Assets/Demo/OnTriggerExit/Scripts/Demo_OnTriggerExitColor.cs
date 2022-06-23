using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;

namespace SimpleDemos
{
    /// <summary>
    /// An example of how to use ASL_ObjectCollider OnTriggerExit. Pass a delegate function to 
    /// ASL_ObjectCollider.ASL_OnTriggerExit(). This function will be called when the objcet collides 
    /// with another ASLObject.
    /// </summary>
    public class Demo_OnTriggerExitColor : MonoBehaviour
    {
        [Tooltip("The color the player cube with turn when it exits this object.")]
        public Material Color;

        ASL_ObjectCollider m_ASLObjectCollider;

        // Start is called before the first frame update
        void Start()
        {
            Debug.Assert(Color != null);
            m_ASLObjectCollider = gameObject.GetComponent<ASL_ObjectCollider>();
            Debug.Assert(m_ASLObjectCollider != null);

            //Assigning the deligate function to the ASL_ObjectCollider
            m_ASLObjectCollider.ASL_OnTriggerExit(ChangeColorOnTriggerExit);
        }

        /// <summary>
        /// Delegate function called by OnCollitionExit by the ASL_ObjectCollider
        /// </summary>
        /// <param name="other">The collider of the other object in the collition</param>
        void ChangeColorOnTriggerExit(Collider other)
        {
            if (other.gameObject.GetComponent<Demo_PlayerCube>() != null &&
                other.gameObject.GetComponent<MeshRenderer>() != null)
            {
                ASLObject aSLObject = other.gameObject.GetComponent<ASLObject>();
                aSLObject.SendAndSetClaim(() =>
                {
                    aSLObject.SendAndSetObjectColor(Color.color, Color.color);
                });
            }
        }
    }
}