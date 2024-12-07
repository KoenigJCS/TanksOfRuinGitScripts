using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct TileSetSave
{
    public int width;
    public int height;
    [SerializeField] public Matrix<int> tiles;

    public TileSetSave(Matrix<int> n_tiles, int n_width = 11, int n_height = 11) {
        tiles = n_tiles;
        width= n_width;
        height = n_height;
    }
}

[System.Serializable]
public class Array<T>
{
    public List<T> cells = new List<T>();
    public T this[int index] => cells[index];
}

[System.Serializable]
public class Matrix<T>
{
    public List<Array<T>> arrays = new List<Array<T>>();
    public Array<T> this[int x] => arrays[x];
    public T this[int x, int y] => arrays[x][y];
}