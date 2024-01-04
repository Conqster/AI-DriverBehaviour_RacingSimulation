using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class InvokeCrazy : MonoBehaviour
{
    public float counter = 0;
    public bool doCarzy;
    //public int times = 0;
    public float timer = 0;

    [SerializeField, Range(0, 10)] float playTime;
    [SerializeField, Range(0, 5)] float playTimer;
    public float modTime;
    public float useTime = 1;


    private void Update()
    {

        modTime = playTime % playTimer;

        //if(doCarzy)
        //{
        //    StartCoroutine(Do());
        //}

        if (doCarzy)
        {
            if (InvokeAt(ref timer, 1.0f))
            {
                crazy();
            }
        }
        else
            timer = 0;
    }



    private bool NewInvoke(ref float timer, float value)
    {
        if (timer != 50.0f)
            timer += Time.deltaTime;

        if (timer > value && timer != 50.0f)
            return true;

        return false;
    }



    private bool InvokeAt(ref float timer, float value)
    {

        if(timer != 50f)
        {
            timer += Time.deltaTime;
        }

        if (timer > value && timer != 50.0f)
        {
            timer = 50f;  
            return true;
        }
        return false;
    }
    private void crazy()
    {
        counter++;
    }

    private IEnumerator Do()
    {
        //Do stuffs
        //Do stuffs
        yield return new WaitForSeconds(useTime);
        crazy();
    }



}
