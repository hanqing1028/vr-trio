using UnityEngine;
using System.Collections;

public class soundOnCollide : MonoBehaviour {
    public AudioSource audio;
    public Color color;
    public static float maxIntensity = 1.8f, fadeTime = 2.0f, particleMaxSpeed = 15.0f, intervalDecay = 0.85f;

    // Visualization objects and components
    GameObject light;
    GameObject particles;
    Light lightComp;
    ParticleSystem particleSys;
    float timeLastKeyPress = 0.0f;

    // Use this for initialization
    void Start () {
        audio = GetComponent<AudioSource>();
        audio.Play();
        audio.Play(44100);


        // Create the visual objects to modify when a note is pressed
        light = new GameObject();
        light.transform.parent = transform;
        light.transform.localPosition = Vector3.zero;
        light.transform.localRotation = Quaternion.AngleAxis(90.0f, Vector3.right);
        lightComp = light.AddComponent<Light>();
        lightComp.type = LightType.Directional;
        lightComp.intensity = 0.0f;

        particles = new GameObject();
        particles.transform.parent = transform;
        particles.transform.position = this.transform.position;
        particles.transform.localRotation = Quaternion.AngleAxis(-90.0f, Vector3.right);
        particleSys = particles.AddComponent<ParticleSystem>();
        particleSys.startSpeed = particleMaxSpeed;
        particleSys.gravityModifier = 0.2f;
        particleSys.startSize = 0.1f;
        var emission = particleSys.emission;
        emission.enabled = false;
        emission.rate = 70;
        var shape = particleSys.shape;
        shape.shapeType = ParticleSystemShapeType.Hemisphere;
        shape.radius = 2.2f;
    }
	
	// Update is called once per frame
	void Update () {
        var emission = particleSys.emission;

        if (timeLastKeyPress == 0.0f)
        {
            // Dark at start of game
            lightComp.intensity = 0.0f;
            emission.enabled = false;
            return;
        }

        // Make the light fade away if notes stop being played
        lightComp.intensity = maxIntensity * (Mathf.Max(0.0f, fadeTime - Time.time + timeLastKeyPress) / fadeTime);
        // No particles if completely faded
        emission.enabled = lightComp.intensity != 0.0f;
        particleSys.startSpeed = particleMaxSpeed * lightComp.intensity / maxIntensity;
    }

    void OnTriggerEnter(Collider other)
    {
        audio.Play();
        vizSound();
        GameObject o = other.transform.parent.gameObject;
        SteamVR_TrackedObject to = (SteamVR_TrackedObject)o.GetComponent<SteamVR_TrackedObject>();
        int i = (int)to.index;
        SteamVR_Controller.Input(i).TriggerHapticPulse(1500);
    }

    void OnTriggerExit(Collider other)
    {
        Debug.Log("Do nothing in this instance.");
    }

    void vizSound() {
        timeLastKeyPress = Time.time;
        lightComp.color = color;
        particleSys.startColor = Color.Lerp(Color.white, color, 0.35f);
    }
}
