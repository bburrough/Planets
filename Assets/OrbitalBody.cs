using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitalBody : MonoBehaviour
{
    public GameObject orbitFocalPoint;
    private OrbitalBody orbitFocalPointOB;
    public float semiMajorAxis = 1f; //http://www.met.rdg.ac.uk/~ross/Astronomy/Planets.html and http://www.unit-conversion.info/astronomical.html
    public float eccentricity = 0.0f;
    public float inclination = 0f;
    public float longitudeOfPerihelion = 0f;
    public float longitudeOfAscendingNode = 0f;
    public float orbitalPeriod = 60f;
    public float meanLongitude = 0f;
    public bool drawDebugLines = false;
    public Color debugColor = Color.white;
    public float enableAtDay = 0f;
    private float argumentOfPeriapsis;
    private float timeElapsed = 0f;
    private const float interval = 1f / 40f;
    private Vector3 previousPosition;
    private bool previousPositionIsSet = false;
    private TubeRenderer tubeRenderer;
    public GameObject tubeRendererPrefab;


    public enum DrawMode
    {
        None,
        Velocity,
        Arrow,
        Orbit
    }
    public DrawMode drawMode = DrawMode.None;


    // Start is called before the first frame update
    void Start()
    {
        orbitFocalPointOB = orbitFocalPoint.GetComponent<OrbitalBody>();

        // argument of periapsis actually isn't argument of periapsis. It's longitude of periapsis. To get actual argument of periapsis, we do:
        //actualArgumentOfPeriapsis = argumentOfPeriapsis;
        argumentOfPeriapsis =  longitudeOfPerihelion - longitudeOfAscendingNode;
    }



    // Update is called once per frame
    void Update()
    {
        float days = Time.time / 5f * Clock.earthDaysPerYear;
        if (days < enableAtDay)
            return;

        float clockDegrees = ((Time.time + 5f * 0f) / orbitalPeriod * 360f + meanLongitude - longitudeOfPerihelion) % 360f;
        float eccentricAnomaly = EccentricAnomalyGivenMeanAnomaly(clockDegrees, eccentricity);

        transform.position = CalculateCurrentPosition();

        switch (drawMode)
        {
            case DrawMode.Velocity:
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
                break;
            case DrawMode.Arrow:
                Debug.DrawLine(transform.position, transform.position + Vector3.right * 40f, debugColor);
                Debug.DrawLine(transform.position, transform.position + Vector3.right * 4f + Vector3.up * 4f, debugColor);
                Debug.DrawLine(transform.position, transform.position + Vector3.right * 4f + Vector3.down * 4f, debugColor);
                break;
            case DrawMode.Orbit:
                if (tubeRenderer == null)
                {
                    tubeRenderer = GameObject.Instantiate(tubeRendererPrefab).GetComponent<TubeRenderer>();
                }

                const int numSegments = 128;
                Vector3 currentPosition = transform.position;
                float currentTime = Time.time;
                const float tubeRadius = 0.5f; 

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
                        nextFocalPointPosition = orbitFocalPointOB.CalculationPositionAt(nextTime);

                    Vector3 nextPosition = nextFocalPointPosition + KeplerianPosition2D(nextEccentricAnomaly, semiMajorAxis, eccentricity, argumentOfPeriapsis, inclination, longitudeOfAscendingNode);
                    const float maxRange = 5e4f;
                    if (currentPosition.x < maxRange && currentPosition.y < maxRange && currentPosition.z < maxRange &&
                       currentPosition.x > -maxRange && currentPosition.y > -maxRange && currentPosition.z > -maxRange &&
                       nextPosition.x < maxRange && nextPosition.y < maxRange && nextPosition.z < maxRange &&
                       nextPosition.x > -maxRange && nextPosition.y > -maxRange && nextPosition.z > -maxRange)
                    {
                        if(drawDebugLines)
                            Debug.DrawLine(currentPosition, nextPosition, color);

                        tubeRenderer.AppendPoint(nextPosition, tubeRadius, color);
                    }
                    currentPosition = nextPosition;
                }
                tubeRenderer.AppendPoint(transform.position, tubeRadius, new Color(debugColor.r, debugColor.g, debugColor.b, 0f)); // close the line
                tubeRenderer.Rebuild();
                break;
            case DrawMode.None:
            default:
                break;
        }
    }


    private Vector3 CalculateCurrentPosition()
    {
        return CalculationPositionAt(Time.time + 5f * 0f);
    }


    private Vector3 CalculationPositionAt(float time)
    {
        float meanAnomaly = ((time / orbitalPeriod) * 360f + meanLongitude - longitudeOfPerihelion) % 360f;
        Vector3 pos = Vector3.zero;
        if (orbitFocalPointOB)
        {
            pos = orbitFocalPointOB.CalculationPositionAt(time);
        }
        float eccentricAnomaly = EccentricAnomalyGivenMeanAnomaly(meanAnomaly, eccentricity);
        pos += KeplerianPosition2D(eccentricAnomaly, semiMajorAxis, eccentricity, argumentOfPeriapsis, inclination, longitudeOfAscendingNode);
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


    // Taken from https://en.wikipedia.org/wiki/True_anomaly
    public static float TrueAnomalyGivenEccentricAnomaly(float eccentricAnomaly, float eccentricity)
    {
        //float eccentricity = 0.5f;

        //return Mathf.Atan2(Mathf.Sqrt((1f - eccentricity * eccentricity)) * Mathf.Sin(eccentricAnomaly * Mathf.Deg2Rad), Mathf.Cos(eccentricAnomaly * Mathf.Deg2Rad) - eccentricity);
        //return Mathf.Atan2(Mathf.Cos(eccentricAnomaly * Mathf.Deg2Rad) - eccentricity, Mathf.Sqrt((1f - eccentricity * eccentricity)) * Mathf.Sin(eccentricAnomaly * Mathf.Deg2Rad));
        return 2f * Mathf.Atan(Mathf.Sqrt((1f + eccentricity) / (1f - eccentricity)) * Mathf.Tan(eccentricAnomaly / 2f * Mathf.Deg2Rad)) * Mathf.Rad2Deg;
    }


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


    public static float RadiusGivenEccentricAnomaly(float eccentricAnomaly, float semiMajorAxis, float eccentricity)
    {
        //float semiMajorAxis = 6f;
        //float eccentricity = 0.5f;

        return semiMajorAxis * (1f - eccentricity * Mathf.Cos(eccentricAnomaly * Mathf.Deg2Rad));
    }


    public static float RadiusGivenTrueAnomaly(float trueAnomaly, float semiMajorAxis, float eccentricity)
    {
        /* theta is also called true anomaly, which is the angle between
           the current position of the orbiting object and the location in
           the orbit at which it is closest to the central body (i.e. periapsis). */
        //float semiMajorAxis = 6f;
        //float eccentricity = 0.5f;

        return (semiMajorAxis * (1f - eccentricity * eccentricity)) / (1f + eccentricity * Mathf.Cos(trueAnomaly * Mathf.Deg2Rad));
    }



    public static Vector3 KeplerianPosition2D(float eccentricAnomaly, float semiMajorAxis, float eccentricity, float argumentOfPeriapsis, float inclination, float longitudeOfAscendingNode)
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


    public static void OrbitalVelocityGivenEccentricAnomaly(float eccentricAnomaly, float semiMajorAxis, float eccentricity)
    {
        float Ldot = 1f;
        float vP = -semiMajorAxis * Mathf.Sin(eccentricAnomaly * Mathf.Deg2Rad) * Ldot / (1f - eccentricity * Mathf.Cos(eccentricAnomaly * Mathf.Deg2Rad));
        float vQ = semiMajorAxis * Mathf.Cos(eccentricAnomaly * Mathf.Deg2Rad) * Mathf.Sqrt(1f - eccentricity * eccentricity) * Ldot / (1f - eccentricity * Mathf.Cos(eccentricAnomaly * Mathf.Deg2Rad));
        //Debug.Log("velocity: " + (new Vector2(vP, vQ)).magnitude);
    }
}
