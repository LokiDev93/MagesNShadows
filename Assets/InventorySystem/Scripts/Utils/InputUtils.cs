using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FishNet.InventorySystem
{

    public static class InputUtils
    {

        public static bool IsShiftDown() => Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

    }

}
