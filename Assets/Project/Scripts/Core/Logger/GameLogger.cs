using UnityEngine;

public static class GameLogger
{
    public static bool EnableLog = true;

    public static void Log(string message)
    {
        if (!EnableLog) return;
        Debug.Log(message);
    }

    public static void Warning(string message)
    {
        if (!EnableLog) return;
        Debug.LogWarning(message);
    }

    public static void Error(string message)
    {
        if (!EnableLog) return;
        Debug.LogError(message);
    }
}