using System.Collections.Generic;

namespace Game;

public class SaveData
{

  public Dictionary<string, LevelCompletionData> LevelCompletionStatus { get; set; } = new();

  public void SaveLevelCompletion(string id, bool completed)
  {
    LevelCompletionStatus.TryAdd(id, new LevelCompletionData());
    LevelCompletionStatus[id].IsCompleted = completed;
  }
}