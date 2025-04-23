using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VNWGame.Game
{
    public enum GroupType
    {
        Font = 1,
        Icon = 2,
        HeadFrame = 3,
        ProgressBar = 4,
        HorizontalLayoutGroup = 5,
    }
    public enum HUDAlign
    {
        Left = 0,
        Center = 1,
        Right = 2,
    }

    public enum HUDType
    {
        NearNormalShip = 0,
        NearRedPacketShip = 1,
        NearNPCRedPacketShip = 2,
        FarNormalShip = 10,
        FarNPCShip = 11,
        FarUnionShip = 12,
        None = 999
    }
}