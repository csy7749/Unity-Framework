using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityFramework;

namespace GameLogic
{
    public class TestClose : MonoBehaviour
    {
        public void Close()
        {
            transform.parent.gameObject.SetActive(false);
            Log.Warning($"click close wid");
        }
    }
}
