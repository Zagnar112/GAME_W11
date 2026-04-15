using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DamageNumber : MonoBehaviour
{
    [SerializeField]
    float moveSpeed = 1;
    [SerializeField]
    TMP_Text text;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;
    }

    public void SetNumber(float num)
    {
        text.text = num.ToString();
    }
}
