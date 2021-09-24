using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Clock : MonoBehaviour
{
    public TextMeshProUGUI text;

    
    private const float earthOrbitalPeriod = 5f; // One simulated year takes earthOrbitalPeriod seconds in real time.
    public const float earthDaysPerYear = 365.2421875f;
    private const float julianDaysPerYear = 365.25f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        text.text = "J2000 + " + ((Time.time + earthOrbitalPeriod * 0f) / earthOrbitalPeriod * julianDaysPerYear).ToString("F2") + " days";
    }
}
