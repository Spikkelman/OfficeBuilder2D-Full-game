using System;
using System.Collections.Generic;

[System.Serializable]
public class TileSaveData
{
    public string tileType;
    public int x;
    public int y;
}

[System.Serializable]
public class TileSaveDataList
{
    public List<TileSaveData> tiles;
}