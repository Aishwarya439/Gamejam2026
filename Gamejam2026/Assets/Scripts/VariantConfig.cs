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
public class VariantConfig
{
    public string gameType;
    public string visibilityState;
    public string evaluationLogic;
    public int components;
    public int rows;
    public int columns;
    public int slots;
    public int asset_id;
    public DialogueEntry[] dialogue;

    public VisibilityState VisibilityState =>
        System.Enum.TryParse(visibilityState, out VisibilityState v) ? v : VisibilityState.WithBottomLayer;

    public EvaluationLogic EvaluationLogic =>
        System.Enum.TryParse(evaluationLogic, out EvaluationLogic e) ? e : EvaluationLogic.A;

}
