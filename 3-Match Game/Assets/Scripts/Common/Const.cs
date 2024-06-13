using UnityEngine;
using System.Collections;
using System.ComponentModel;

namespace Const
{
    public enum UIOptionsKey
    {

    }

    public enum MinionState
    {
        None = 0,
        Idle,
        Move,
        Attack,
        Hurt,
        Death
    }

    public enum TileType
    {
        None = -1,

        [Description("#044F00")] //Common
        DarkGreen = 0,
        [Description("#00FF00")] //Common
        Green,
        [Description("#0000AB")] //Common
        Indigo,
        [Description("#FF0000")] //Common
        Red,
        [Description("#FF5D5D")] //Common
        Salmon,
        [Description("#008080")] //Common
        Teal,
        [Description("#FFFF00")] //Common
        Yellow,

        Count
    }

    public enum TileState
    {
        Idle,
        Moving,
    }

    public enum TileColorHex
    {
        None,

        [Description("#044F00")] //Common
        DarkGreen,
        [Description("#00FF00")] //Common
        Green,
        [Description("#0000AB")] //Common
        Indigo,
        [Description("#FF0000")] //Common
        Red,
        [Description("#FF5D5D")] //Common
        Salmon,
        [Description("#008080")] //Common
        Teal,
        [Description("#FFFF00")] //Common
        Yellow,
    }

    public enum SwapDirection
    {
        None,
        Right,
        Left,
        Up,
        Down,
    }
}
