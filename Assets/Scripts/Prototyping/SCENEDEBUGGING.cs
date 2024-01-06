using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCENEDEBUGGING : MonoBehaviour
{
    public float timeScaleValue = 1.0f;





    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.RightArrow))
            timeScaleValue += 0.2f;

        if (Input.GetKeyDown(KeyCode.LeftArrow)) 
            timeScaleValue -= 0.2f;

        timeScaleValue = Mathf.Clamp01(timeScaleValue);

        Time.timeScale = timeScaleValue;    
    }
}
