using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public static class DebugUtility
{
    private const string EditorSymbol = "UNITY_EDITOR";
    private const string DevelopmentBuildSymbol = "DEVELOPMENT_BUILD";

    [Conditional(EditorSymbol), Conditional(DevelopmentBuildSymbol)]
    public static void Log(string message)
    {
        Debug.Log(message);
    }

    [Conditional(EditorSymbol), Conditional(DevelopmentBuildSymbol)]
    public static void LogWarning(string message)
    {
        Debug.LogWarning(message);
    }

    [Conditional(EditorSymbol), Conditional(DevelopmentBuildSymbol)]
    public static void Log(string message, Object context)
    {
        Debug.Log(message, context);
    }

    [Conditional(EditorSymbol), Conditional(DevelopmentBuildSymbol)]
    public static void LogWarning(string message, Object context)
    {
        Debug.LogWarning(message, context);
    }

    public static void LogError(string message)
    {
        Debug.LogError(message);
    }

    public static void LogError(string message, Object context)
    {
        Debug.LogError(message, context);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void RegisterQuitOnError()
    {
        Application.logMessageReceived -= QuitOnError;
        Application.logMessageReceived += QuitOnError;
    }

    private static void QuitOnError(string message, string stackTrace, LogType type)
    {
        if (type != LogType.Error && type != LogType.Exception && type != LogType.Assert)
        {
            return;
        }

        ApplicationUtility.Quit();
    }
}