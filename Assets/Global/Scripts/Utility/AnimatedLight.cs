using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatedLight : MonoBehaviour
{
	public bool turnOffWhenStart;
        
	public Light TargetLight { get { return m_targetLight; }}
	private Light m_targetLight;
	private float originalIntensity;
	private int tweenId;

    private void Awake()
    {
        m_targetLight = GetComponent<Light>();
		originalIntensity = m_targetLight.intensity;
		if (turnOffWhenStart)
			m_targetLight.intensity = 0f;
    }

	private void Start()
	{
		if (turnOffWhenStart)
            m_targetLight.intensity = 0f;
	}

	private void OnDestroy()
	{
		LeanTween.cancel(tweenId);
	}

	public Coroutine AnimateForStart(float duration)
	{
		return Animate(m_targetLight.intensity, originalIntensity, duration);
	}

    public Coroutine Animate(float toIntensity, float duration)
    {
        return Animate(m_targetLight.intensity, toIntensity, duration);
    }

    public Coroutine Animate(float fromIntensity, float toIntensity, float duration)
    {
        return StartCoroutine(_AnimateLight(fromIntensity, toIntensity, m_targetLight.color, m_targetLight.color, duration));
    }

    private IEnumerator _AnimateLight(float fromIntensity, float toIntensity, Color fromColor, Color toColor, float duration)
    {
		tweenId = LeanTween.value(0, 1, duration).setOnUpdate((float value) =>
        {
			if(m_targetLight)
			{
				m_targetLight.intensity = fromIntensity + (toIntensity - fromIntensity) * value;
                m_targetLight.color = fromColor + (fromColor - toColor) * value;
			}
		}).id;

        yield return new WaitForSeconds(duration);
    }

	public void Toggle(bool turnOn, float intensity, float time, float delay)
    {
        if (turnOn)
        {
			if(Mathf.Approximately(time, 0f))
			{
				m_targetLight.intensity = intensity;
				return;
			}
			m_targetLight.enabled = true;
			LeanTween.value(gameObject, m_targetLight.intensity, 1f * intensity, time)
                        .setDelay(delay)
                        .setOnUpdate(CallOnIntensityUpdate);
        }
        else
        {
			if (Mathf.Approximately(time, 0f))
            {
				m_targetLight.intensity = 0f;
                return;
            }

			LeanTween.value(gameObject, m_targetLight.intensity, 0, time)
                        .setDelay(delay)
                        .setOnUpdate(CallOnIntensityUpdate)
				        .setOnComplete(()=>{ m_targetLight.enabled = false; });
        }
    }

	void CallOnIntensityUpdate(float val)
    {
		m_targetLight.intensity = val;
    }

    public void TurnOff()
	{
		m_targetLight.intensity = 0f;
		m_targetLight.enabled = false;
	}

	public void TurnOff(float time)
	{
		Toggle(false, 0, time, 0);
	}
      
	public void TurnOn()
    {
		m_targetLight.intensity = originalIntensity;
		m_targetLight.enabled = true;
    }

	public void TurnOn(float time)
	{
		Toggle(true, originalIntensity, time, 0);
	}
}