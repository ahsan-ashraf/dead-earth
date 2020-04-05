using UnityEngine;
using System.Collections.Generic;

public enum PathDisplayMode { None, Connections, Paths }
public class AIWayPointNetwork : MonoBehaviour {

    #region Public Variables

    [HideInInspector] public PathDisplayMode  DisplayMode     = PathDisplayMode.Connections;
    [HideInInspector] public int              UIStart         = 0;
    [HideInInspector] public int              UIEnd           = 0;
                      public List<Transform>  WayPoints       = null;
    
    #endregion

}
