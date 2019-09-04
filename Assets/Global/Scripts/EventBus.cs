using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

//public class UserSettingsChangedEvent : UnityEvent<UserSettings> { }
//public class HandEvent : UnityEvent<VRHand> { }
public class StringEvent : UnityEvent<string> { }
public class IntEvent : UnityEvent<int> { }
public class EnvEvent : UnityEvent<EnvironmentType> { }

public class EventBus
{
    public static UnityEvent StartGameTriggered = new UnityEvent();
    //public static UserSettingsChangedEvent UserSettingsChanged = new UserSettingsChangedEvent();
    public static UnityEvent ToggleMenuVisibility = new UnityEvent();

    public static StringEvent NewSceneLoaded = new StringEvent();

    public static UnityEvent CalibrationStarted = new UnityEvent();
    public static UnityEvent CalibrationEnded = new UnityEvent();

    public static EnvEvent EnteredTranitionStartTrigger = new EnvEvent();
    public static EnvEvent EnteredTranitionEndTrigger = new EnvEvent();
    public static UnityEvent TransitionStarted = new UnityEvent();
    public static UnityEvent TransitionEnded = new UnityEvent();
}
