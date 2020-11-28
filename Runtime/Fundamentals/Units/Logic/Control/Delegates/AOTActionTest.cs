using System;
using System.Collections;
using UnityEngine;

namespace Bolt.Addons.Community.Utility
{
    public class AOTActionTest : MonoBehaviour
    {
        public Action action;
        
        public IEnumerator Start()
        {
            yield return new WaitForSeconds(5f);
            Debug.Log("Pre Invoked");
            action?.Invoke();
        }
    }
}
