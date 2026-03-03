using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WooTimer;

public class NewBehaviourScript : MonoBehaviour
{
    // Start is called before the first frame update
    async void Start()
    {
        Debug.Log("0");

        var ctx = TimeEx.DoWhile((_, _) => Time.time < 2f);
        ctx.OnComplete(_ => Debug.LogError("One_end"));
        Debug.Log("2");
        ctx.Stop();
        await TimeEx.DoWhile((_, _) => Time.time < 4f);
        Debug.Log("4");


    }

    // Update is called once per frame
    void Update()
    {

    }
}
