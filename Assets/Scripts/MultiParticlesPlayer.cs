using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

/// <summary>
/// Playing multiple particles
/// </summary>
public class MultiParticlesPlayer : MonoBehaviour
{
    /// <summary>
    /// Duration of all particles
    /// </summary>
    public float Duration => _duration;

    [SerializeField]
    private List<ParticleSystem> _particles = new List<ParticleSystem>();

    [SerializeField]
    private float _duration = default;

    /// <summary>
    /// Start :3
    /// </summary>
    [ContextMenu("Play")]
    [Button("Play")]
    public void Play()
    {
        foreach(ParticleSystem particle in _particles)
        {
            particle.Play();
        }
    }
}
