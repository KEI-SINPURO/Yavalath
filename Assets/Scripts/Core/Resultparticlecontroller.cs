using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResultParticleController : MonoBehaviour
{
    [Header("パーティクルシステム")]
    public ParticleSystem hexParticlesWin;
    public ParticleSystem hexParticlesLose;
    public ParticleSystem smallParticlesWin;
    public ParticleSystem smallParticlesLose;

    public void PlayWinEffect()
    {
        if (hexParticlesWin != null && hexParticlesWin.gameObject.activeInHierarchy)
            hexParticlesWin.Play();
        if (smallParticlesWin != null && smallParticlesWin.gameObject.activeInHierarchy)
            smallParticlesWin.Play();
        
        if (hexParticlesLose != null && hexParticlesLose.gameObject.activeInHierarchy)
            hexParticlesLose.Stop();
        if (smallParticlesLose != null && smallParticlesLose.gameObject.activeInHierarchy)
            smallParticlesLose.Stop();
    }

    public void PlayLoseEffect()
    {
        if (hexParticlesLose != null && hexParticlesLose.gameObject.activeInHierarchy)
            hexParticlesLose.Play();
        if (smallParticlesLose != null && smallParticlesLose.gameObject.activeInHierarchy)
            smallParticlesLose.Play();
        
        if (hexParticlesWin != null && hexParticlesWin.gameObject.activeInHierarchy)
            hexParticlesWin.Stop();
        if (smallParticlesWin != null && smallParticlesWin.gameObject.activeInHierarchy)
            smallParticlesWin.Stop();
    }

    public void StopAll()
    {
        if (hexParticlesWin != null && hexParticlesWin.gameObject.activeInHierarchy)
            hexParticlesWin.Stop();
        if (hexParticlesLose != null && hexParticlesLose.gameObject.activeInHierarchy)
            hexParticlesLose.Stop();
        if (smallParticlesWin != null && smallParticlesWin.gameObject.activeInHierarchy)
            smallParticlesWin.Stop();
        if (smallParticlesLose != null && smallParticlesLose.gameObject.activeInHierarchy)
            smallParticlesLose.Stop();
    }
}