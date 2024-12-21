using UnityEngine;

public static class Bootstrap
{
    [RuntimeInitializeOnLoadMethod]
    public static void Run()
    {
        Application.targetFrameRate = 60;
        //Academy.Instance.AutomaticSteppingEnabled = false;
    }
}