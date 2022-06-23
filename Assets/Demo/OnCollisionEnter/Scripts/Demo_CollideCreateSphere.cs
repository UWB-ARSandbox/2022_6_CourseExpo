using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;

namespace SimpleDemos
{
    /// <summary>
    /// An example of how to use ASL_ObjectCollider OnCollisionEnter. DO NOT USE. ASL_ObjectCollider does not suuport
    /// Unity physics, use ASL_ObjectCollider.OnTriggerEnter.
    /// </summary>
    public class Demo_CollideCreateSphere : MonoBehaviour
    {
        public string PrefabName;

        ASL_ObjectCollider m_ASLObjectCollider;

        // Start is called before the first frame update
        void Start()
        {
            m_ASLObjectCollider = gameObject.GetComponent<ASL_ObjectCollider>();
            Debug.Assert(m_ASLObjectCollider != null);

            //Assigning the deligate function to the ASL_ObjectCollider
            m_ASLObjectCollider.ASL_OnCollisionEnter(createSphereOnCollitionEnter);
        }

        /// <summary>
        /// Delegate function called by OnCollitionEnter by the ASL_ObjectCollider
        /// </summary>
        void createSphereOnCollitionEnter(Collision collision)
        {
            ASL.ASLHelper.InstantiateASLObject(PrefabName, new Vector3(0, 5, 0), Quaternion.identity);
        }
    }
}