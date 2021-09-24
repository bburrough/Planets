using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlanetaryLongitudeReadout : MonoBehaviour
{
    public TextMeshProUGUI text;
    public GameObject sun;
    public GameObject mercury;
    public GameObject venus;
    public GameObject earth;
    public GameObject mars;
    public GameObject jupiter;
    public GameObject saturn;
    public GameObject uranus;
    public GameObject neptune;
    public GameObject pluto;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        float mercuryAngle = Vector3.SignedAngle(Vector3.right, mercury.transform.position.normalized, Vector3.down);
        float venusAngle = Vector3.SignedAngle(Vector3.right, venus.transform.position.normalized, Vector3.down);
        float earthAngle = Vector3.SignedAngle(Vector3.right, earth.transform.position.normalized, Vector3.down);
        float marsAngle = Vector3.SignedAngle(Vector3.right, mars.transform.position.normalized, Vector3.down);
        float jupiterAngle = Vector3.SignedAngle(Vector3.right, jupiter.transform.position.normalized, Vector3.down);
        float saturnAngle = Vector3.SignedAngle(Vector3.right, saturn.transform.position.normalized, Vector3.down);
        float uranusAngle = Vector3.SignedAngle(Vector3.right, uranus.transform.position.normalized, Vector3.down);
        float neptuneAngle = Vector3.SignedAngle(Vector3.right, neptune.transform.position.normalized, Vector3.down);
        float plutoAngle = Vector3.SignedAngle(Vector3.right, pluto.transform.position.normalized, Vector3.down);

        if (mercuryAngle < 0f)
            mercuryAngle += 360f;
        if (venusAngle < 0f)
            venusAngle += 360f;
        if (earthAngle < 0f)
            earthAngle += 360f;
        if (marsAngle < 0f)
            marsAngle += 360f;
        if (jupiterAngle < 0f)
            jupiterAngle += 360f;
        if (saturnAngle < 0f)
            saturnAngle += 360f;
        if (uranusAngle < 0f)
            uranusAngle += 360f;
        if (neptuneAngle < 0f)
            neptuneAngle += 360f;
        if (plutoAngle < 0f)
            plutoAngle += 360f;

        text.text = "Mercury: " + (mercuryAngle).ToString("F1") + "\n" +
                    "Venus: " + (venusAngle).ToString("F1") + "\n" +
                    "Earth: " + (earthAngle).ToString("F1") + "\n" +
                    "Mars: " + (marsAngle).ToString("F1") + "\n" +
                    "Jupiter: " + (jupiterAngle).ToString("F1") + "\n" +
                    "Saturn: " + (saturnAngle).ToString("F1") + "\n" +
                    "Uranus: " + (uranusAngle).ToString("F1") + "\n" +
                    "Neptune: " + (neptuneAngle).ToString("F1") + "\n" +
                    "Pluto: " + (plutoAngle).ToString("F1");
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
