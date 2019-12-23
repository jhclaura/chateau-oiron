using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AnimatedAudio : MonoBehaviour
{
	public bool turnOffWhenStart;
        
	public AudioSource TargetAudio { get { return m_targetAudio; }}
	private AudioSource m_targetAudio;
	public float OriginalVolumn
	{ 
		get { return m_originalVolumn; }
		set { m_originalVolumn = value; }
	}
	private float m_originalVolumn;
	private int tweenId;
    public bool IsPlaying { get { return m_targetAudio.isPlaying; } }

	private void Awake()
	{
		m_targetAudio = GetComponent<AudioSource>();
	}

	private void Start()
    {
		m_originalVolumn = m_targetAudio.volume;
		if (turnOffWhenStart)
			m_targetAudio.volume = 0f;
    }

	private void OnDestroy()
	{
		LeanTween.cancel(tweenId);
	}

	public void Toggle(bool turnOn, float volume, float time, float delay, bool doStop = false)
    {
		if (turnOn)
        {
			m_targetAudio.UnPause();

			if (!m_targetAudio.isPlaying)
				m_targetAudio.Play();
                
			LeanTween.cancel(gameObject);
			LeanTween.value(gameObject, m_targetAudio.volume, volume, time)
				        .setDelay(delay)
				        .setOnUpdate(CallOnVolumeUpdate);
        }
        else
        {
			if (m_targetAudio.isPlaying)
            {
				LeanTween.cancel(gameObject);
				LeanTween.value(gameObject, m_targetAudio.volume, 0f, time)
					        .setDelay(delay)
					        .setOnUpdate(CallOnVolumeUpdate)
					        .setOnComplete(() => {
								if (doStop)
									m_targetAudio.Stop();
								else
									m_targetAudio.Pause();
                            });                        
            }
        }
    }

	public void TweenVolumn(float targetVolumn)
	{
		if (Mathf.Approximately(targetVolumn, m_targetAudio.volume)) return;

		LeanTween.value(m_targetAudio.volume, targetVolumn, 1f)
					.setOnUpdate(CallOnVolumeUpdate);
	}

	public void ToggleOn()
	{
		m_targetAudio.volume = 0f;
		Toggle(true, m_originalVolumn, 1f, 0f);
	}

	public void ToggleOff()
    {
		Toggle(false, 0, 1f, 0f);
    }

	void CallOnVolumeUpdate(float val)
    {
		m_targetAudio.volume = val;
    }

    public void TurnOff()
	{
		m_targetAudio.volume = 0f;
		m_targetAudio.enabled = false;
	}

	public void TurnOn()
    {
		m_targetAudio.volume = m_originalVolumn;
		m_targetAudio.enabled = true;
    }

	public void Play()
	{
		m_targetAudio.UnPause();

        if (!m_targetAudio.isPlaying)
            m_targetAudio.Play();
	}
        
	public void Pause()
	{
		if (m_targetAudio.isPlaying)
			m_targetAudio.Pause();
	}

	public void Stop(bool fadeOut = false, float fadeTime = 0.3f)
	{
		if (fadeOut)
			Toggle(false, 0f, fadeTime, 0, true);
		else
    		m_targetAudio.Stop();
	}
}