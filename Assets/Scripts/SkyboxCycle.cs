using UnityEngine;

public class SkyboxCycle : MonoBehaviour
{
    [System.Serializable]
    public class TimeOfDay
    {
        public string name;
        public Material skybox;
        public Color ambientColor = Color.white;
        public Color sunColor = Color.white;
        public float sunIntensity = 1f;
        public Vector3 sunRotation;
        
        [HideInInspector]
        public Material runtimeSkybox;
    }

    [Header("Pory Dnia (od Poranka po Noc)")]
    public TimeOfDay[] timesOfDay;
    
    [Header("Oświetlenie (Słońce)")]
    public Light sunLight;
    
    [Header("Ustawienia Czasu")]
    [Tooltip("Ile sekund trwa stabilna pora dnia")]
    public float timePerState = 1.5f;
    [Tooltip("Ile sekund trwa płynne przenikanie (ściemnianie starego i rozjaśnianie nowego nieba)")]
    public float transitionDuration = 1.5f;
    public bool isCycling = true;

    private int currentIndex = 0;
    private int nextIndex = 1;
    private float timer = 0f;
    private bool isTransitioning = false;

    // default unity skybox tint color
    private Color defaultTint = new Color(0.5f, 0.5f, 0.5f, 0.5f);

    void Start()
    {
        if (timesOfDay == null || timesOfDay.Length == 0) return;

        if (sunLight == null)
        {
            Light[] lights = FindObjectsOfType<Light>();
            foreach (var l in lights) {
                if (l.type == LightType.Directional) {
                    sunLight = l;
                    break;
                }
            }
        }

        // clone materials to avoid mutating assets
        for (int i = 0; i < timesOfDay.Length; i++)
        {
            if (timesOfDay[i].skybox != null)
            {
                timesOfDay[i].runtimeSkybox = new Material(timesOfDay[i].skybox);
                if (timesOfDay[i].runtimeSkybox.HasProperty("_Tint")) {
                    if (i == 0) defaultTint = timesOfDay[i].runtimeSkybox.GetColor("_Tint");
                }
            }
        }

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        ApplyState(currentIndex);
    }

    void Update()
    {
        if (!isCycling || timesOfDay == null || timesOfDay.Length == 0) return;

        timer += Time.deltaTime;

        if (!isTransitioning)
        {
            if (timer >= timePerState)
            {
                isTransitioning = true;
                timer = 0f;
                nextIndex = (currentIndex + 1) % timesOfDay.Length;
            }
        }
        else
        {
            float t = Mathf.Clamp01(timer / transitionDuration);
            
            TimeOfDay current = timesOfDay[currentIndex];
            TimeOfDay next = timesOfDay[nextIndex];

            // interpolate lighting between times of day
            RenderSettings.ambientLight = Color.Lerp(current.ambientColor, next.ambientColor, t);
            
            if (sunLight != null)
            {
                sunLight.color = Color.Lerp(current.sunColor, next.sunColor, t);
                sunLight.intensity = Mathf.Lerp(current.sunIntensity, next.sunIntensity, t);
                sunLight.transform.rotation = Quaternion.Slerp(Quaternion.Euler(current.sunRotation), Quaternion.Euler(next.sunRotation), t);
            }

            // crossfade skybox tint
            if (t < 0.5f)
            {
                float fade = 1f - (t * 2f);
                if (current.runtimeSkybox != null && current.runtimeSkybox.HasProperty("_Tint"))
                {
                    RenderSettings.skybox = current.runtimeSkybox;
                    current.runtimeSkybox.SetColor("_Tint", Color.Lerp(Color.black, defaultTint, fade));
                }
            }
            else
            {
                float fade = (t - 0.5f) * 2f;
                if (next.runtimeSkybox != null && next.runtimeSkybox.HasProperty("_Tint"))
                {
                    RenderSettings.skybox = next.runtimeSkybox;
                    next.runtimeSkybox.SetColor("_Tint", Color.Lerp(Color.black, defaultTint, fade));
                }
            }

            if (t >= 1f)
            {
                isTransitioning = false;
                timer = 0f;
                currentIndex = nextIndex;
                ApplyState(currentIndex);
            }
        }
    }

    private void ApplyState(int index)
    {
        TimeOfDay tod = timesOfDay[index];
        if (tod.runtimeSkybox != null) {
            RenderSettings.skybox = tod.runtimeSkybox;
            if (tod.runtimeSkybox.HasProperty("_Tint")) tod.runtimeSkybox.SetColor("_Tint", defaultTint);
        }
        
        RenderSettings.ambientLight = tod.ambientColor;
        if (sunLight != null)
        {
            sunLight.color = tod.sunColor;
            sunLight.intensity = tod.sunIntensity;
            sunLight.transform.rotation = Quaternion.Euler(tod.sunRotation);
        }
    }
}
