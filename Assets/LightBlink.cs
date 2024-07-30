using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightBlink : MonoBehaviour
{
    public List<Light> lights = new List<Light>(); 

    public float light_duration; // How long the light is on full brightness
    public float blink_duration; // How long the blinking takes

    private float start_time;

    // considering them uniform
    private float color_hue;
    private float color_saturation;
    private float color_value;
    private bool blinking;

    void Start()
    {
        start_time = Time.time;
        if (lights.Count != 0)
        {
            Color.RGBToHSV(lights[0].color, out color_hue, out color_saturation, out color_value);
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (!blinking)
        {

            StartCoroutine(LightControl());
        }
     
    }

    IEnumerator LightControl()
    {
        blinking = true;
        float current_light_duration = 0;
        float current_blink_duration = 0;


        while (current_light_duration < light_duration)
        {
            foreach(Light light in lights)
            {
                color_value = Mathf.Max(color_value - Time.deltaTime / light_duration, 0);
                light.color = Color.HSVToRGB(color_hue, color_saturation, color_value);
            }
            
            current_light_duration += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(blink_duration);

        current_light_duration = 0;
        current_blink_duration = 0;

        while (current_light_duration < light_duration)
        {
            foreach (Light light in lights)
            {
                color_value = Mathf.Min(color_value + Time.deltaTime / light_duration, 1);
                light.color = Color.HSVToRGB(color_hue, color_saturation, color_value);
            }

            current_light_duration += Time.deltaTime;
            yield return null;
        }

  
        yield return new WaitForSeconds(blink_duration);
        blinking = false;
    }
}
