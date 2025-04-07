using UnityEngine;

public static class Ask4PrefValue
{
    //6-pt likert
    //private static readonly float[] prefValues = new float[] { 0.9f, 0.45f, 0.2f, -0.2f, -0.45f, -0.9f };
    private static readonly float[] prefValues = new float[] { 1.0f, 0.8f, 0.5f, 0.15f, 0.05f, -0.05f, -0.15f, -0.5f, -0.8f, -1.0f };

    public static float GetPrefValueFromKey(KeyCode i_keyCode)
    {
        return i_keyCode switch
        {
            KeyCode.Alpha9 => prefValues[0],
            KeyCode.Alpha8 => prefValues[1],
            KeyCode.Alpha7 => prefValues[2],
            KeyCode.Alpha6 => prefValues[3],
            KeyCode.Alpha5 => prefValues[4],
            KeyCode.Alpha4 => prefValues[5],
            KeyCode.Alpha3 => prefValues[6],
            KeyCode.Alpha2 => prefValues[7],
            KeyCode.Alpha1 => prefValues[8],
            KeyCode.Alpha0 => prefValues[9],
            _ => float.NaN
        };
    }
}
