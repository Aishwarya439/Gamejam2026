using System.IO;
using UnityEngine;

public enum VisibilityState
{
    WithBottomLayer,
    WithoutBottomLayer
}

public enum EvaluationLogic
{
    A,
    B,
    C,
    D
}

[System.Serializable]
public class ComponentData
{
    public string label;
}

[System.Serializable]
public class VariantConfig
{
    public string visibilityState;
    public string evaluationLogic;
    public ComponentData[] components;

    public VisibilityState VisibilityState =>
        System.Enum.TryParse(visibilityState, out VisibilityState v) ? v : VisibilityState.WithBottomLayer;

    public EvaluationLogic EvaluationLogic =>
        System.Enum.TryParse(evaluationLogic, out EvaluationLogic e) ? e : EvaluationLogic.A;

    public static VariantConfig[] LoadAll(string fileName)
    {
        string path = Path.Combine(Application.streamingAssetsPath, fileName);
        if (!File.Exists(path))
        {
            Debug.LogError($"VariantConfig not found at: {path}");
            return null;
        }
        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<VariantConfigList>("{\"items\":" + json + "}").items;
    }

    [System.Serializable]
    private class VariantConfigList
    {
        public VariantConfig[] items;
    }
}
