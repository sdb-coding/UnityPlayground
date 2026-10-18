using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class WaitFor
{
    static Dictionary<float, WaitForSeconds> cachedSeconds = new Dictionary<float, WaitForSeconds>(100);
    static Dictionary<float, WaitForSecondsRealtime> cachedSecondsRealtime = new Dictionary<float, WaitForSecondsRealtime>(100);

    static WaitForEndOfFrame endOfFrame = new WaitForEndOfFrame();
    public static WaitForEndOfFrame EndOfFrame
    {
        get { return endOfFrame; }
    }

    static WaitForFixedUpdate fixedUpdate = new WaitForFixedUpdate();
    public static WaitForFixedUpdate FixedUpdate
    {
        get { return fixedUpdate; }
    }

    /// <summary>
    /// Avoid using with randomly generated float numbers.
    /// </summary>
    /// <param name="seconds"></param>
    /// <returns></returns>
    public static WaitForSeconds Seconds(float seconds)
    {
        if (!cachedSeconds.ContainsKey(seconds))
            cachedSeconds.Add(seconds, new WaitForSeconds(seconds));

        return cachedSeconds[seconds];
    }

    /// <summary>
    /// Avoid using with randomly generated float numbers.
    /// </summary>
    /// <param name="seconds"></param>
    /// <returns></returns>
    public static WaitForSecondsRealtime SecondsRealtime(float seconds)
    {
        if (!cachedSecondsRealtime.ContainsKey(seconds))
            cachedSecondsRealtime.Add(seconds, new WaitForSecondsRealtime(seconds));
        return cachedSecondsRealtime[seconds];
    }
}
