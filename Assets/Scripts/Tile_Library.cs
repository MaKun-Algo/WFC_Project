using System;
using System.Collections.Generic;
using MySelfmadeNamespace;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;


[CreateAssetMenu(menuName = "Marcel/TestObj")]
public class Tile_Library : ScriptableObject
{
    [SerializeField] public List<TileData> tiles;
  
}