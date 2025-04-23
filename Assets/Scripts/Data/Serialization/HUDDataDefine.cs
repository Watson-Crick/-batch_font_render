using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VNWGame.Game
{
    [CreateAssetMenu(fileName = "HUDData", menuName = "HUD/创建HUD", order = 1)]
    public class HUDDataDefine : ScriptableObject
    {
        public List<HUDGroupDataDefine> hudList = new List<HUDGroupDataDefine>();
    }
}