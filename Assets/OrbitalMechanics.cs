using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitalMechanics : MonoBehaviour
{
    public GameObject sun;
    public GameObject earth;
    public GameObject comet;
    private float earthOrbitalRadius;
    private float cometPeriapsis = 1f;
    private float earthTimeElapsed = 0f;
    private float cometTimeElapsed = 0f;
    private const float interval = 1f / 40f;
    private Vector3 previousCometPosition;
    private bool previousCometPositionIsSet = false;
    private Vector3 previousEarthPosition;
    private bool previousEarthPositionIsSet = false;
    private float previousTrueAnomaly = 0f;


    void Start()
    {
        earthOrbitalRadius = (sun.transform.position - earth.transform.position).magnitude;
    }
  

    void Update()
    {
        float earthAngleDegrees = (Time.time * 40f) % 360f;
        float cometAngleDegrees = (Time.time * 4f) % 360f;

        //earth.transform.position = new Vector3(Mathf.Sin(earthAngleDegrees * Mathf.Deg2Rad) * earthOrbitalRadius, 0f, Mathf.Cos(earthAngleDegrees * Mathf.Deg2Rad) * earthOrbitalRadius);

        float earthEccentricity = 0f;
        float earthSemiMajorAxis = 149.6f;
        float cometEccentricity = 0.5f;
        float cometSemiMajorAxis = earthSemiMajorAxis * 3f;
        float argumentOfPeriapsis = 0f;
        float inclination = 0f;
        float longitudeOfAscendingNode = 0f;

        float eccentricAnomaly = OrbitalBody.EccentricAnomalyGivenMeanAnomaly(earthAngleDegrees * -1f, earthEccentricity);
        earth.transform.position = OrbitalBody.KeplerianPosition2D(eccentricAnomaly, earthSemiMajorAxis, earthEccentricity, argumentOfPeriapsis, inclination, longitudeOfAscendingNode);

        eccentricAnomaly = OrbitalBody.EccentricAnomalyGivenMeanAnomaly(cometAngleDegrees * -1f, cometEccentricity);
        comet.transform.position = OrbitalBody.KeplerianPosition2D(eccentricAnomaly, cometSemiMajorAxis, cometEccentricity, argumentOfPeriapsis, inclination, longitudeOfAscendingNode);

        //float trueAnomaly = OrbitalBody.TrueAnomalyGivenEccentricAnomaly(eccentricAnomaly);

        //Debug.Log("clk: " + (earthAngleDegrees % 360f) + " ecc: " + eccentricAnomaly + " tru: " + trueAnomaly);

        //Debug.Log("delta M: " + (trueAnomaly - previousTrueAnomaly));
        //previousTrueAnomaly = trueAnomaly;

        // alt calculation
        //Vector3 newPosition = Quaternion.Euler(0f, trueAnomaly, 0f) * Vector3.forward * OrbitalBody.RadiusGivenEccentricAnomaly(eccentricAnomaly);
        //comet.transform.position = newPosition;

        //comet.transform.position = KeplerianPosition2D(eccentricAnomaly);

        //OrbitalVelocityGivenEccentricAnomaly(eccentricAnomaly);

        if (!previousEarthPositionIsSet)
        {
            previousEarthPosition = earth.transform.position;
            previousEarthPositionIsSet = true;
        }
        else
        {
            earthTimeElapsed += Time.deltaTime;
            if (earthTimeElapsed > interval)
            {
                Debug.DrawLine(earth.transform.position, previousEarthPosition, Color.blue, interval*4f);
                previousEarthPosition = earth.transform.position;
                earthTimeElapsed = 0f;
            }
        }

        if (!previousCometPositionIsSet)
        {
            previousCometPosition = comet.transform.position;
            previousCometPositionIsSet = true;
        }
        else
        {
            cometTimeElapsed += Time.deltaTime;
            if (cometTimeElapsed > interval)
            {
                Debug.DrawLine(comet.transform.position, previousCometPosition, Color.white, interval*4f);
                previousCometPosition = comet.transform.position;
                cometTimeElapsed = 0f;
            }
        }
    }
}


/*
------------------------------------------------------------------------------
This software is available under 2 licenses -- choose whichever you prefer.
------------------------------------------------------------------------------
ALTERNATIVE A - MIT License
Copyright (c) 2021 Bobby G. Burrough
Permission is hereby granted, free of charge, to any person obtaining a copy of 
this software and associated documentation files (the "Software"), to deal in 
the Software without restriction, including without limitation the rights to 
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies 
of the Software, and to permit persons to whom the Software is furnished to do 
so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all 
copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE 
SOFTWARE.
------------------------------------------------------------------------------
ALTERNATIVE B - Public Domain (www.unlicense.org)
This is free and unencumbered software released into the public domain.
Anyone is free to copy, modify, publish, use, compile, sell, or distribute this 
software, either in source code form or as a compiled binary, for any purpose, 
commercial or non-commercial, and by any means.
In jurisdictions that recognize copyright laws, the author or authors of this 
software dedicate any and all copyright interest in the software to the public 
domain. We make this dedication for the benefit of the public at large and to 
the detriment of our heirs and successors. We intend this dedication to be an 
overt act of relinquishment in perpetuity of all present and future rights to 
this software under copyright law.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN 
ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION 
WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
------------------------------------------------------------------------------
*/
