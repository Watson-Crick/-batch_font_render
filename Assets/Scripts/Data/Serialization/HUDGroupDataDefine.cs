using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VNWGame.Game
{
    [CreateAssetMenu(fileName = "HUDGroup", menuName = "HUD/创建HUDGroup", order = 1)]
    public class HUDGroupDataDefine : ScriptableObject
    {
        public HUDType hudType;
        public List<HUDGroup> hudGroups;
        public List<int> childGroupIndexList;
    }

    [Serializable]
    public class HUDGroup
    {
        public GroupType type;
        public string name;
        public string content;
        public int colorType;
        public float spacing;
        public HUDAlign align;
        public Vector2 size;
        public Vector3 offset;
        public float maxLength = -1;
        
        public Vector2 realSize;
        public string key;
        public bool alive;

        public List<int> childGroupIndexList;
    };
}