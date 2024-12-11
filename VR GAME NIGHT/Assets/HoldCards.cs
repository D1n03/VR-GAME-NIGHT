using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldCards : MonoBehaviour
{
    public readonly List<GameObject> pivotPoints = new();
    public float rotationSpan = 45.0f;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        for(int i = 0; i < pivotPoints.Count; i++)
        {
            var intervalCount = pivotPoints.Count + 1;
            var pivotPoint = pivotPoints[i];
            pivotPoint.transform.localEulerAngles = new Vector3(0.0f, 0.0f, (i + 1) * (rotationSpan / intervalCount) - rotationSpan * 0.5f);
            Debug.Log(i + " card");
        }
    }
}
