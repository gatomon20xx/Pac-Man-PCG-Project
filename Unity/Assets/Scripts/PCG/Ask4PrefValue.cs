using UnityEngine;

public static class Ask4PrefValue
{
    //6-pt likert
    //private static readonly float[] prefValues = new float[] { 0.9f, 0.45f, 0.2f, -0.2f, -0.45f, -0.9f };
    private static readonly float[] prefValues = new float[] { 1.0f, 0.5f, 0.25f, -0.25f, -0.5f, -1.0f };

    public static float GetPrefValueFromKey(KeyCode i_keyCode)
    {
        return i_keyCode switch
        {
            KeyCode.Alpha1 => prefValues[0],
            KeyCode.Alpha6 => prefValues[5],
            _ => float.NaN
        };
    }
}
