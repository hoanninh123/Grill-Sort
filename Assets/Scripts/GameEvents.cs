using System;

public static class GameEvents
{
    public static event Action<int> OnFoodCountChanged;
    public static event Action OnGameCompleted;

    public static void RaiseFoodCountChanged(int remainFood)
    {
        OnFoodCountChanged?.Invoke(remainFood);
    }

    public static void RaiseGameCompleted()
    {
        OnGameCompleted?.Invoke();
    }
}