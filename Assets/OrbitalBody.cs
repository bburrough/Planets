using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitalBody : MonoBehaviour
{
    public GameObject orbitFocalPoint; // A GameObject representing the focal point of the orbit. May leave epty.
    private OrbitalBody orbitFocalPointOB; // Internal reference to the OrbitalBody associated with the orbitalFocalPoint.
    public float semiMajorAxis = 1f; //http://www.met.rdg.ac.uk/~ross/Astronomy/Planets.html and http://www.unit-conversion.info/astronomical.html
    public float eccentricity = 0.0f;
    public float inclination = 0f; // degrees
    public float longitudeOfPerihelion = 0f; // degrees
    public float longitudeOfAscendingNode = 0f; // degrees
    public float orbitalPeriod = 60f; // orbitalPeriod in seconds. e.g. an orbitalPeriod of 10 will cause the body to go through a full year cycle in ten seconds.
    public float meanLongitude = 0f; // degrees
    public bool drawDebugLines = false;
    public Color debugColor = Color.white;
    public float enableAtDay = 0f; // If you want an orbital body to be created during the simulation (e.g. with a satellite launch from Earth), set this to the number of days after J2000 where you want the object to appear.
    private float argumentOfPeriapsis = 0f;
    private float timeElapsed = 0f;
    private const float interval = 1f / 40f;
    private Vector3 previousPosition;
    private bool previousPositionIsSet = false;
    private TubeRenderer tubeRenderer;
    public GameObject tubeRendererPrefab;
    private GameObject keplerianEllipseGo;
    public Material keplerianEllipseMaterial;

    public enum DrawMode
    {
        None,
        Velocity,
        Arrow,
        Orbit,
        KeplerianEllipse
    }
    public DrawMode drawMode = DrawMode.None;


    void Start()
    {
        orbitFocalPointOB = orbitFocalPoint.GetComponent<OrbitalBody>();

        // argument of periapsis actually isn't argument of periapsis. It's longitude of periapsis. To get actual argument of periapsis, we do:
        //actualArgumentOfPeriapsis = argumentOfPeriapsis;
        argumentOfPeriapsis =  longitudeOfPerihelion - longitudeOfAscendingNode + 180f;
    }


    void Update()
    {
        float days = Time.time / 5f * Clock.earthDaysPerYear;
        if (days < enableAtDay)
            return;

        //float clockDegrees = ((Time.time + 5f * 0f) / orbitalPeriod * 360f + meanLongitude - longitudeOfPerihelion) % 360f;
        //float eccentricAnomaly = EccentricAnomalyGivenMeanAnomaly(clockDegrees, eccentricity);

        transform.position = CalculateCurrentPosition();

        switch (drawMode)
        {
            case DrawMode.Velocity:
                {
                    if (!previousPositionIsSet)
                    {
                        previousPosition = transform.position;
                        previousPositionIsSet = true;
                    }
                    else
                    {
                        timeElapsed += Time.deltaTime;
                        if (timeElapsed > interval)
                        {
                            Debug.DrawLine(transform.position, transform.position + Vector3.right * 40f, debugColor, interval);
                            previousPosition = transform.position;
                            timeElapsed = 0f;
                        }
                    }
                }
                break;
            case DrawMode.Arrow:
                {
                    DrawArrowAt(transform.position);
                }
                break;
            case DrawMode.Orbit:
                {
                    if (tubeRenderer == null)
                    {
                        tubeRenderer = GameObject.Instantiate(tubeRendererPrefab).GetComponent<TubeRenderer>();
                    }

                    const int numSegments = 128;
                    Vector3 currentPosition = transform.position;
                    float currentTime = Time.time;
                    const float tubeRadius = 1.5f;

                    tubeRenderer.Reset();
                    tubeRenderer.AppendPoint(currentPosition, tubeRadius, debugColor); // first vert gets full (non-transparent) color

                    for (int i = 1; i < numSegments; i++)
                    {
                        float colorLerpValue = (numSegments - i - 1f) / numSegments;
                        Color color = Color.Lerp(new Color(debugColor.r, debugColor.g, debugColor.b, 0f), new Color(debugColor.r, debugColor.g, debugColor.b, 1f), colorLerpValue);

                        float nextTime = (Time.time + 5f * 0f) - i * (orbitalPeriod / numSegments);
                        float nextMeanAnomaly = (nextTime / orbitalPeriod * 360f + meanLongitude - longitudeOfPerihelion) % 360f;
                        float nextEccentricAnomaly = EccentricAnomalyGivenMeanAnomaly(nextMeanAnomaly, eccentricity);

                        Vector3 nextFocalPointPosition = Vector3.zero;
                        if (orbitFocalPointOB)
                            nextFocalPointPosition = orbitFocalPointOB.CalculatePositionAt(nextTime);

                        Vector3 nextPosition = nextFocalPointPosition + KeplerianPosition(nextEccentricAnomaly, semiMajorAxis, eccentricity, argumentOfPeriapsis, inclination, longitudeOfAscendingNode);
                        const float maxRange = 5e4f;
                        if (currentPosition.x < maxRange && currentPosition.y < maxRange && currentPosition.z < maxRange &&
                           currentPosition.x > -maxRange && currentPosition.y > -maxRange && currentPosition.z > -maxRange &&
                           nextPosition.x < maxRange && nextPosition.y < maxRange && nextPosition.z < maxRange &&
                           nextPosition.x > -maxRange && nextPosition.y > -maxRange && nextPosition.z > -maxRange)
                        {
                            if (drawDebugLines)
                                Debug.DrawLine(currentPosition, nextPosition, color);

                            tubeRenderer.AppendPoint(nextPosition, tubeRadius, color);
                        }
                        currentPosition = nextPosition;
                    }
                    tubeRenderer.AppendPoint(transform.position, tubeRadius, new Color(debugColor.r, debugColor.g, debugColor.b, 0f)); // close the line
                    tubeRenderer.Rebuild();
                }
                break;
            case DrawMode.KeplerianEllipse:
                {
                    /* The desire is to have the orbital body arc out an ellipse of equal areas, with the arcs expanding
                       as the orbital body moves, until the full ellipse completes and the process starts over. 

                       The ellipse should be divided in to n segmnets, where n is preferably an even number, and the
                       colors of the arc segments alternate (e.g. black/white or transparent black/ opaque white).                   

                       The ellipse should start at a common point -- likely the "slow" side of the orbit -- aka the apoapsis.
                    */
                    MeshFilter mf;
                    MeshRenderer mr;
                    if (keplerianEllipseGo == null)
                    {
                        keplerianEllipseGo = new GameObject();
                        mf = keplerianEllipseGo.AddComponent<MeshFilter>();
                        mr = keplerianEllipseGo.AddComponent<MeshRenderer>();
                        mr.sharedMaterial = keplerianEllipseMaterial;
                    }
                    else
                    {
                        mf = keplerianEllipseGo.GetComponent<MeshFilter>();
                        mr = keplerianEllipseGo.GetComponent<MeshRenderer>();
                    }

                    const int numArcSections = 16;
                    const int numTrisPerSection = 10;
                    Color blackTransparent = new Color(0f, 0f, 0f, 0f);
                    Color whiteOpaque = new Color(1f, 1f, 1f, 1f);

                    float timeAtApoapsis = Mathf.Floor(Time.time / orbitalPeriod) * orbitalPeriod + orbitalPeriod / 2f - (meanLongitude - longitudeOfPerihelion) / 360f * orbitalPeriod;  // half of an orbital period, minus the time required to reach meanLongitude
                    Vector3 previousPosition = CalculatePositionAt(timeAtApoapsis);
                    DrawArrowAt(previousPosition);

                    Vector3[] vertices = new Vector3[3 * numArcSections * numTrisPerSection];
                    int[] triangles = new int[3 * numArcSections * numTrisPerSection];
                    Color[] colors = new Color[3 * numArcSections * numTrisPerSection];
                    
                    for (int i = 0; i < numArcSections; i++)
                    {
                        for (int j = 0; j < numTrisPerSection; j++)
                        {
                            float currentSectorTime = timeAtApoapsis + orbitalPeriod / (numArcSections * numTrisPerSection) * (i * numTrisPerSection + j + 1);
                            Vector3 currentPosition = CalculatePositionAt(currentSectorTime);

                            vertices[i * numTrisPerSection * 3 + j * 3 + 0] = currentPosition;
                            vertices[i * numTrisPerSection * 3 + j * 3 + 1] = previousPosition;
                            vertices[i * numTrisPerSection * 3 + j * 3 + 2] = orbitFocalPoint == null ? Vector3.zero : orbitFocalPoint.transform.position;

                            triangles[i * numTrisPerSection * 3 + j * 3 + 0] = i * numTrisPerSection * 3 + j * 3 + 0;
                            triangles[i * numTrisPerSection * 3 + j * 3 + 1] = i * numTrisPerSection * 3 + j * 3 + 1;
                            triangles[i * numTrisPerSection * 3 + j * 3 + 2] = i * numTrisPerSection * 3 + j * 3 + 2;

                            Color arcColor = i % 2 == 0 ? whiteOpaque : blackTransparent;
                            colors[i * numTrisPerSection * 3 + j * 3 + 0] = arcColor;
                            colors[i * numTrisPerSection * 3 + j * 3 + 1] = arcColor;
                            colors[i * numTrisPerSection * 3 + j * 3 + 2] = arcColor;

                            previousPosition = currentPosition;
                        }
                    }

                    Mesh mesh = mf.mesh;
                    mesh.vertices = vertices;
                    mesh.triangles = triangles;
                    mesh.colors = colors;
                    mesh.RecalculateNormals();
                }
                break;
            case DrawMode.None:
                {
                    if (tubeRenderer != null)
                    {
                        Destroy(tubeRenderer.gameObject);
                        tubeRenderer = null;
                    }
                }
                break;
            default:
                break;
        }
    }


    // General purpose routine for drawing a debug arrow.
    private void DrawArrowAt(Vector3 position)
    {
        Debug.DrawLine(position, position + Vector3.right * 40f, debugColor);
        Debug.DrawLine(position, position + Vector3.right * 4f + Vector3.up * 4f, debugColor);
        Debug.DrawLine(position, position + Vector3.right * 4f + Vector3.down * 4f, debugColor);
    }


    // Calculate the position of the orbital body at the current time.
    private Vector3 CalculateCurrentPosition()
    {
        return CalculatePositionAt(Time.time + 5f * 0f);
    }


    // Calcualte the position of the orbital body at the provided time.
    private Vector3 CalculatePositionAt(float time)
    {
        float meanAnomaly = ((time / orbitalPeriod) * 360f + meanLongitude - longitudeOfPerihelion) % 360f;
        Vector3 pos = Vector3.zero;
        if (orbitFocalPointOB)
        {
            pos = orbitFocalPointOB.CalculatePositionAt(time);
        }
        float eccentricAnomaly = EccentricAnomalyGivenMeanAnomaly(meanAnomaly, eccentricity);
        pos += KeplerianPosition(eccentricAnomaly, semiMajorAxis, eccentricity, argumentOfPeriapsis, inclination, longitudeOfAscendingNode);
        return pos;
    }


    //private Vector3 CalcFocalPointPosition(float meanAnomaly)
    //{

    //    Vector3 pos = Vector3.zero;
    //    if (orbitFocalPointOB)
    //    {
    //        pos = orbitFocalPointOB.CalcFocalPointPosition(meanAnomaly);
    //        float eccentricAnomaly = orbitFocalPointOB.EccentricAnomalyGivenMeanAnomaly(meanAnomaly, orbitFocalPointOB.eccentricity);
    //        pos += orbitFocalPointOB.KeplerianPosition2D(eccentricAnomaly, orbitFocalPointOB.semiMajorAxis, orbitFocalPointOB.eccentricity);
    //    }
    //    return pos;
    //}


    // Convert from eccentric anomaly to true anomaly.
    // Taken from https://en.wikipedia.org/wiki/True_anomaly
    public static float TrueAnomalyGivenEccentricAnomaly(float eccentricAnomaly, float eccentricity)
    {
        //float eccentricity = 0.5f;

        //return Mathf.Atan2(Mathf.Sqrt((1f - eccentricity * eccentricity)) * Mathf.Sin(eccentricAnomaly * Mathf.Deg2Rad), Mathf.Cos(eccentricAnomaly * Mathf.Deg2Rad) - eccentricity);
        //return Mathf.Atan2(Mathf.Cos(eccentricAnomaly * Mathf.Deg2Rad) - eccentricity, Mathf.Sqrt((1f - eccentricity * eccentricity)) * Mathf.Sin(eccentricAnomaly * Mathf.Deg2Rad));
        return 2f * Mathf.Atan(Mathf.Sqrt((1f + eccentricity) / (1f - eccentricity)) * Mathf.Tan(eccentricAnomaly / 2f * Mathf.Deg2Rad)) * Mathf.Rad2Deg;
    }


    // Convert from true anomaly to eccentric anomaly.
    public static float EccentricAnomalyGivenTrueAnomaly(float trueAnomaly, float eccentricity)
    {
        /*
            v = 2 arctan( sqrt( (1+e) / (1-e) ) * tan(E/2)  )

            tan (v/2) = sqrt( (1+e) / (1-e) ) * tan(E/2) 

            tan(v/2) / sqrt( (1+e) / (1-e) )  = tan(E/2)

            E = 2 arctan( tan(v/2) / sqrt((1+e)/(1-e)) )
        */

        return 2f * Mathf.Atan(Mathf.Tan(trueAnomaly / 2f * Mathf.Deg2Rad) / Mathf.Sqrt((1f + eccentricity) / (1f - eccentricity)) ) * Mathf.Rad2Deg;
    }


    // Compute the radius given eccentric anomaly.
    public static float RadiusGivenEccentricAnomaly(float eccentricAnomaly, float semiMajorAxis, float eccentricity)
    {
        //float semiMajorAxis = 6f;
        //float eccentricity = 0.5f;

        return semiMajorAxis * (1f - eccentricity * Mathf.Cos(eccentricAnomaly * Mathf.Deg2Rad));
    }


    // Compute the radius given the true anomaly.
    public static float RadiusGivenTrueAnomaly(float trueAnomaly, float semiMajorAxis, float eccentricity)
    {
        /* theta is also called true anomaly, which is the angle between
           the current position of the orbiting object and the location in
           the orbit at which it is closest to the central body (i.e. periapsis). */
        //float semiMajorAxis = 6f;
        //float eccentricity = 0.5f;

        return (semiMajorAxis * (1f - eccentricity * eccentricity)) / (1f + eccentricity * Mathf.Cos(trueAnomaly * Mathf.Deg2Rad));
    }


    // Calculate the Keplerian position of an object.
    public static Vector3 KeplerianPosition(float eccentricAnomaly, float semiMajorAxis, float eccentricity, float argumentOfPeriapsis, float inclination, float longitudeOfAscendingNode)
    {
        // +P is axis towards periapsis
        // Q is orthogonal to P
        // (basically these are X, Y coordinates in a periapsis relative coordinate system)
        //float semiMajorAxis = 6f;
        //float eccentricity = 0.5f;

        float P = semiMajorAxis * (Mathf.Cos(eccentricAnomaly * Mathf.Deg2Rad) - eccentricity);
        float Q = semiMajorAxis * Mathf.Sin(eccentricAnomaly * Mathf.Deg2Rad) * Mathf.Sqrt(1f - eccentricity * eccentricity);

        //return new Vector3(P, 0f, Q);
        // rotate by argument of periapsis
        float x = Mathf.Cos(argumentOfPeriapsis * Mathf.Deg2Rad) * P - Mathf.Sin(argumentOfPeriapsis * Mathf.Deg2Rad) * Q;
        float y = Mathf.Sin(argumentOfPeriapsis * Mathf.Deg2Rad) * P + Mathf.Cos(argumentOfPeriapsis * Mathf.Deg2Rad) * Q;
        // rotate by inclination
        float z = Mathf.Sin(inclination * Mathf.Deg2Rad) * y;
        y = Mathf.Cos(inclination * Mathf.Deg2Rad) * y;
        // rotate by longitude of ascending node
        float xtemp = x;
        x = Mathf.Cos(longitudeOfAscendingNode * Mathf.Deg2Rad) * xtemp - Mathf.Sin(longitudeOfAscendingNode * Mathf.Deg2Rad) * y;
        y = Mathf.Sin(longitudeOfAscendingNode * Mathf.Deg2Rad) * xtemp + Mathf.Cos(longitudeOfAscendingNode * Mathf.Deg2Rad) * y;
        return new Vector3(x, z, y);
    }


    // Convert mean anomaly to eccentric anomaly.
    public static float EccentricAnomalyGivenMeanAnomaly(float meanAnomaly, float eccentricity)
    {
        //float eccentricity = 0.5f;

        int iterCount = 0;
        const int maxIterCount = 500;
        float M = meanAnomaly * Mathf.Deg2Rad;
        float E = M;
        while (true)
        {
            iterCount++;
            float dE = (E - eccentricity * Mathf.Sin(E) - M) / (1f - eccentricity * Mathf.Cos(E));
            //if (E - dE < 0f)
            //    break;
            E -= dE;
            if (Mathf.Abs(dE) < 1e-6f) break;
            if (iterCount > maxIterCount)
            {
                Debug.LogWarning("iterCount > " + maxIterCount + ", dE is " + dE + " E is: " + E + " M is: " + M);
                break;
            }

        }
        //Debug.Log("iterCount: " + iterCount);
        return E * Mathf.Rad2Deg;
    }


    /*
    public static float EccentricAnomalyGivenMeanAnomaly2(float meanAnomaly)
    {
        float eccentricity = 0.5f;

        const int maxIter = 30;
        int i = 0;

        float delta = 1e-6f;

        float E, F;

        meanAnomaly /= 360f;
        meanAnomaly = 2f * Mathf.PI * (meanAnomaly - Mathf.Floor(meanAnomaly));
        if (eccentricity < 0.8f)
            E = meanAnomaly;
        else
            E = Mathf.PI;

        F = E - eccentricity * Mathf.Sin(meanAnomaly) - meanAnomaly;

        while ((Mathf.Abs(F) > delta) && i < maxIter)
        {
            E -= F / (1f - eccentricity * Mathf.Cos(E));
            F = E - eccentricity * Mathf.Sin(E) - meanAnomaly;
            i++;
        }

        E /= (Mathf.PI / 180f);
        return Mathf.Round(E * 1e6f) / 1e6f;
    }
    */


    // Calculate the velocity of the orbital body at a particular eccentric anomaly.
    public static void OrbitalVelocityGivenEccentricAnomaly(float eccentricAnomaly, float semiMajorAxis, float eccentricity)
    {
        float Ldot = 1f;
        float vP = -semiMajorAxis * Mathf.Sin(eccentricAnomaly * Mathf.Deg2Rad) * Ldot / (1f - eccentricity * Mathf.Cos(eccentricAnomaly * Mathf.Deg2Rad));
        float vQ = semiMajorAxis * Mathf.Cos(eccentricAnomaly * Mathf.Deg2Rad) * Mathf.Sqrt(1f - eccentricity * eccentricity) * Ldot / (1f - eccentricity * Mathf.Cos(eccentricAnomaly * Mathf.Deg2Rad));
        //Debug.Log("velocity: " + (new Vector2(vP, vQ)).magnitude);
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
