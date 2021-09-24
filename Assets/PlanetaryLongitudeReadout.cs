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
