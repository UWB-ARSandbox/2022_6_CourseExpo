using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;

namespace SimpleDemos
{
    /// <summary>
    /// An example of how to use ASL_ObjectCollider OnCollisionExit. DO NOT USE. ASL_ObjectCollider does not suuport
    /// Unity physics, use ASL_ObjectCollider.OnTriggerExit.
    /// </summary>
    public class Demo_ChangeColorOnCollision : MonoBehaviour
    {
        [Tooltip("The color the player cube with turn while touching this object.")]
        public Material Color;
        [Tooltip("The color the player cube's default color.")]
        public Material PlayerOriginalColor;

        ASL_ObjectCollider m_ASLObjectCollider;

        // Start is called before the first frame update
        void Start()
        {
            Debug.Assert(Color != null);
            Debug.Assert(PlayerOriginalColor != null);
            m_ASLObjectCollider = gameObject.GetComponent<ASL_ObjectCollider>();
            Debug.Assert(m_ASLObjectCollider != null);

            //Assigning the deligate functions to the ASL_ObjectCollider
            m_ASLObjectCollider.ASL_OnCollisionEnter(ChangeColorOnCollisionEnter);
            m_ASLObjectCollider.ASL_OnCollisionExit(ChangeColorOnCollisionExit);
        }

        /// <summary>
        /// Delegate function called by OnCollitionEnter by the ASL_ObjectCollider
        /// </summary>
        void ChangeColorOnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.GetComponent<Demo_PlayerCube>() != null &&
                collision.gameObject.GetComponent<MeshRenderer>() != null)
            {
                ASLObject aSLObject = collision.gameObject.GetComponent<ASLObject>();
                aSLObject.SendAndSetClaim(() =>
                {
                    aSLObject.SendAndSetObjectColor(Color.color, Color.color);
                });
            }
        }

        /// <summary>
        /// Delegate function called by OnCollitionExit by the ASL_ObjectCollider
        /// </summary>
        void ChangeColorOnCollisionExit(Collision collision)
        {
            if (collision.gameObject.GetComponent<Demo_PlayerCube>() != null &&
                collision.gameObject.GetComponent<MeshRenderer>() != null)
            {
                ASLObject aSLObject = collision.gameObject.GetComponent<ASLObject>();
                aSLObject.SendAndSetClaim(() =>
                {
                    aSLObject.SendAndSetObjectColor(PlayerOriginalColor.color, PlayerOriginalColor.color);
                });
            }
        }
    }
}