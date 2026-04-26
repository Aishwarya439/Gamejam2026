using UnityEngine;

public interface IGameplayLogic
{
    void Init(GameSceneController controller, int configIndex, int assetId, int totalComponents);
    bool OnDrop(int index, GameObject droppedOn, DraggableItem draggable);
    void OnEnd();
}
