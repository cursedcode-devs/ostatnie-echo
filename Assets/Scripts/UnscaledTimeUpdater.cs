using UnityEngine;

/// <summary>
/// Aktualizuje globalną zmienną dla Shaderów ("_UnscaledTime"), 
/// która pozwala animować materiały (np. Shader Graph) 
/// gdy Time.timeScale = 0
/// </summary>
public class UnscaledTimeUpdater : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        GameObject updater = new GameObject("UnscaledTimeUpdater");
        updater.AddComponent<UnscaledTimeUpdater>();
        DontDestroyOnLoad(updater);
    }

    void Update()
    {
        Shader.SetGlobalFloat("_UnscaledTime", Time.unscaledTime);
    }
}
