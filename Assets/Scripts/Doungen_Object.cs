using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MySelfmadeNamespace;


//Das ist eine Legacy Class. WIrd nicht benutzt
public class Doungen_Object : MonoBehaviour
{
    [SerializeField] private int top=0;  // gibt an ob eine Richtung einen Gang, Mauer oder offenen Raum hat
    [SerializeField] private int right=0; // 0 steht füür kein Durchgang(wand), 1 für gang, und 2 für offene Fläche (Raum) in dieser Richtung
    [SerializeField] private int down=0;
    [SerializeField] private int left=0;
    
    [SerializeField]public int doungenObjTypeInt;
  

    [SerializeField]public int gangRaumWandInt;
   
   
    public int GetTop(){
        return top;
    }
     public int GetRight(){
        return right;
    }
     public int GetDown(){
        return down;
    }
     public int GetLeft(){
        return left;
    }

    public void Rotate90RightRound(){
        int tmpT,tmpR,tmpD,tmpL;
        tmpR =top;
        tmpD =right;
        tmpL =down;
        tmpT =left;
        top= tmpT;
        right= tmpR;
        down = tmpD;
        left = tmpL;
    }
    public void Rotate180RightRound(){
        Rotate90RightRound();
        Rotate90RightRound();
    }
     public void Rotate270RightRound(){
        Rotate90RightRound();
        Rotate90RightRound();
        Rotate90RightRound();
    }
    public DoungenObjType GetDoungenObjType(){
        switch (doungenObjTypeInt)
        {
            case 1: return DoungenObjType.normal;  //1 = normal, 2 = item, 3 = gegenr, 4 = starker Gegner
            case 2: return DoungenObjType.item;
            case 3: return DoungenObjType.gegner;
            case 4: return DoungenObjType.starkGegner;
            default: return 0;
        }
    }
     public GangRaumWand GetGangRaumWand(){
        switch (gangRaumWandInt)
        {
            case 1: return GangRaumWand.gang;
            case 2: return GangRaumWand.raum;
            case 3: return GangRaumWand.wand;
            default: return 0;
        }
    }

}
