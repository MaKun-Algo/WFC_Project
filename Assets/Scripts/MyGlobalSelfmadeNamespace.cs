using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MySelfmadeNamespace
{
    public enum DoungenObjType
    {
        error = 0,
        normal = 1,
        item = 2,
        gegner = 3,
        starkGegner = 4
    }

    public enum GangRaumWand
    {

        error = 0,
        gang = 1,
        raum = 2,
        wand = 3
    }

    public enum DirectionsOf
    {

        top = 1,
        right = 2,
        down = 3,
        left = 4
    }
    /* legacy Code
        public struct DoungenObjectRepresentater
        {
            public int top;  // gibt an ob eine Richtung einen Gang, Mauer oder offenen Raum hat
            public int right; // 0 steht füür kein Durchgang(wand), 1 für gang, und 2 für offene Fläche (Raum) in dieser Richtung
            public int down;
            public int left;
            public DoungenObjType doungenObjTypeInt; //1 = normal, 2 = item, 3 = gegenr, 4 = starker Gegner
            public GangRaumWand gangRaumWandInt;  //1 Gang, 2 Raum, 3 Wand
            public GameObject gORepresented;
        }*/
    public class PathTilesGroup
    {
        public List<int[]> groupTiles;
        public int uniqueID;
        public int []topExtreme;
        public int []rightExtreme;
        public int []downExtreme;
        public int []leftExtreme;
    }
    [System.Serializable]
    public struct TileData
    {
        public Sprite Sprite;
        public int top;    // gibt an ob eine Richtung einen Gang, Mauer oder offenen Raum hat
        public int right;               // 0 steht füür kein Durchgang(wand), 1 für gang, und 2 für offene Fläche (Raum) in dieser Richtung
        public int down;
        public int left;
        public DoungenObjType doungenObjTypeInt;
        public GangRaumWand gangRaumWandInt;
        public int rotateEulerValue;

        public void Rotate90RightRound()
        {
            int tmpT, tmpR, tmpD, tmpL;
            tmpR = top;
            tmpD = right;
            tmpL = down;
            tmpT = left;
            top = tmpT;
            right = tmpR;
            down = tmpD;
            left = tmpL;
            rotateEulerValue -= 90;
        }
        public int GetTop()
        {
            return top;
        }
        public int GetRight()
        {
            return right;
        }
        public int GetDown()
        {
            return down;
        }
        public int GetLeft()
        {
            return left;
        }
        public void Rotate180RightRound()
        {
            Rotate90RightRound();
            Rotate90RightRound();
        }
        public void Rotate270RightRound()
        {
            Rotate90RightRound();
            Rotate90RightRound();
            Rotate90RightRound();
        }
        public TileData GetCopyOfThis()
        {
            TileData tmpTile = new TileData();
            tmpTile.Sprite = Sprite;
            tmpTile.top = top;
            tmpTile.right = right;
            tmpTile.down = down;
            tmpTile.left = left;
            tmpTile.doungenObjTypeInt = doungenObjTypeInt;
            tmpTile.gangRaumWandInt = gangRaumWandInt;
            tmpTile.rotateEulerValue = rotateEulerValue;

            return tmpTile;
        }


    }


}
