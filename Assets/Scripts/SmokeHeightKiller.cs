using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class SmokeHeightKiller : MonoBehaviour
{
    [Tooltip("Wysokość w świecie (World Y), powyżej której cząsteczki dymu znikną.")]
    public float maxWorldHeight = 2.0f;
    
    private ParticleSystem ps;
    private ParticleSystem.Particle[] particles;

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
    }

    void LateUpdate()
    {
        int maxParticles = ps.main.maxParticles;
        
        // resize particle buffer
        if (particles == null || particles.Length < maxParticles)
        {
            particles = new ParticleSystem.Particle[maxParticles];
        }

        int numParticlesAlive = ps.GetParticles(particles);
        bool modified = false;

        for (int i = 0; i < numParticlesAlive; i++)
        {
            // check height limit
            if (particles[i].position.y > maxWorldHeight)
            {
                particles[i].remainingLifetime = 0f; // kill particle
                modified = true;
            }
        }

        if (modified)
        {
            ps.SetParticles(particles, numParticlesAlive);
        }
    }
}
