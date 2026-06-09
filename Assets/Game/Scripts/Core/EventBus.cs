using System;

namespace PeopleFlow.Core
{
    /// <summary>
    /// Static event hub for decoupling systems.
    /// Call <see cref="Reset"/> during level transitions to clear subscribers.
    /// </summary>
    public static class EventBus
    {
        public static event Action OnGoalCompleted;
        public static event Action OnConveyorOverflow;
        public static event Action OnLevelTimeOut;
        public static event Action OnAllGoalsCleared;
        public static event Action<int, int> OnConveyorCapacityChanged;

        public static void RaiseGoalCompleted() => OnGoalCompleted?.Invoke();
        public static void RaiseConveyorOverflow() => OnConveyorOverflow?.Invoke();
        public static void RaiseLevelTimeOut() => OnLevelTimeOut?.Invoke();
        public static void RaiseAllGoalsCleared() => OnAllGoalsCleared?.Invoke();
        public static void RaiseConveyorCapacityChanged(int current, int capacity) => OnConveyorCapacityChanged?.Invoke(current, capacity);

        /// <summary>
        /// Clears all event subscriptions.
        /// </summary>
        public static void Reset()
        {
            OnGoalCompleted = null;
            OnConveyorOverflow = null;
            OnLevelTimeOut = null;
            OnAllGoalsCleared = null;
            OnConveyorCapacityChanged = null;
        }
    }
}
