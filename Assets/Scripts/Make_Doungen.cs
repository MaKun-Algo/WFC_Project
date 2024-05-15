using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MySelfmadeNamespace;
using Unity.VisualScripting;

public class Make_Doungen : MonoBehaviour
{
    //List<GameObject> listOfAllGameObjects;

    List<TileData> listOfAllDoungenObjects;
    List<TileData>[,] planeForWaveFuncCollapseAlgorithm;
    private int[,] planeOfEntropys;

    DirectionsOf[,] startToEndPathPlaneGoTo;  //das 2D array in welchem der verbundene start zu end phad festgelegt ist. speichert wohin nächste tile geht
    DirectionsOf[,] startToEndPathPlaneCameFrom;   //sspeichert die herkunft( also richtung des Eingangs)

    int[] startRoomCordinates;
    int[] endRoomCordinates;
    private int[] lastPos;
    private int xMaxIndex;
    private int yMaxIndex;

    private int smallestEntroOverOne;
    private List<int[]> lowestEntropysPositionList;
    private int weightNormal, weightItem, weightEnemy, weightEliteEnemy, weightGang, weightRaum, weightWand;
    private bool cornerAllowed;
    private int modOfWFC;
    private bool lvlIsEmpty;

    //Liste der Reihenfolge in der Räume geadded wurden
    private Queue<int[]> orderOfEntrys;

    private PathTilesGroup mainPathGroup;

    private List<PathTilesGroup> nonMainPathGroups;

    private int[,] planeOfPathTileGroupIDs;
    private int phGrIDCount;

    public TileData[,] CreateLevel(int xLength, int yLength, int[] weightGangRaumWand, int[] weightDOType, int modusOfWavefunc) //weightDOType hat die gewichte für normale, item, gegenr und elite gegenerräume, mit 0 = normal, 1 = item, 2 = Gegner und 3=elite. Reihenfogle demnach wie in Selfmade Namespace. selbe regel für GangRaumWand
    {
        TileData[,] lvlArray = new TileData[xLength, yLength];   //make Array that represents the Lvl that gets filled with all GameObjects

        //listOfAllGameObjects = new List<GameObject>();
        listOfAllDoungenObjects = new List<TileData>();
        //listOfAllDoungenObjects = new List<DoungenObjectRepresentater>();
        planeForWaveFuncCollapseAlgorithm = new List<TileData>[xLength, yLength];
        planeOfEntropys = new int[xLength, yLength];
        xMaxIndex = (xLength - 1);
        yMaxIndex = (yLength - 1);
        smallestEntroOverOne = int.MaxValue;
        lowestEntropysPositionList = new List<int[]>();
        weightNormal = weightDOType[0];
        weightItem = weightDOType[1];
        weightEnemy = weightDOType[2];
        weightEliteEnemy = weightDOType[3];
        weightGang = weightGangRaumWand[0];
        weightRaum = weightGangRaumWand[1];
        weightWand = weightGangRaumWand[2];
        if (weightGang == weightRaum && weightRaum == weightWand && weightWand == 0)
        {
            weightWand = 1;
        }
        if (weightNormal == weightItem && weightItem == weightEnemy && weightEnemy == weightEliteEnemy && weightEliteEnemy == 0)
        {
            weightNormal = 1;
        }
        modOfWFC = modusOfWavefunc;
        mainPathGroup = new PathTilesGroup();
        nonMainPathGroups = new List<PathTilesGroup>();
        phGrIDCount = 0;
        lvlIsEmpty = true;
        if ((weightGang == 0) && (weightRaum == 0))
        {
            modOfWFC = 0;   //Dies ist ein einfacher quick fix da die vierte variante des WFC (mit modofWFC==3) nicht funktioniert wenn es keine phade gibt, sondern nur Wände
        }
        if (modOfWFC == 0)
        {     //modusOfWaveFUnc gibt an auf welche art die Wave func ausgeführt werden soll. Es gibt mehrere Moduse, von denne
              // manche versuchen sollen die wahrscheinlichkeit dass alle Gänge des Lvl erbunden sind zu erhöhen,
              //oder sogar zu garantieren. 0 ist der Standard modus von ganz normalem WFC
            cornerAllowed = true;     //Anders als 0 werden die Kanten erst am Ende gelöst da diese Anfangs eine niedrige entropy haben und so 
                                      //der Doungen von außen anch innen sich bilden würde, was mehr einzelne nicht verbundene Konstrukte bildet. Dies wird so verhindert
        }
        else
        {
            cornerAllowed = false;     //CornerAllowed gibt an ob erlaubt ist Corner mit als Random auswahl zu nehmen bezogen auf Modus
        }

        orderOfEntrys = new Queue<int[]>();

        //Legacy Code:CreateListOfAllGameObjects(); //Mach eine Liste in welche alle möglichen gamneobjecte gespeichert werden, von welcher aus diese dann geladen werden können.
        CreateListOfAllDoungenObjects();            //Mach eine Liste in welche alle möglichen TileObjecte gespeichert werden, von welcher aus diese dann geladen werden können.
        // legacy Code: CreatelistOfAllDoungenObjects(); //Konvertiere die Liste von Gameobjecten zu einer Liste von Structs um mit structs statt Gameobjecten aus performance Gründen zua rbeiten

        CreatePlaneForWaveFuncCollapseAlgro(xLength, yLength);    //Mach ein 2D array in welchem Listen sind, in welchen dann der Wave Function Collapse algorith 
                                                                  //durchgeführt wird, durch erstiges Einfügen aller möglichen Gameobjekte, in welchem dann das collabieren nach entropy später gemacht werden kann
        CreateFirstStartToEndPath(xLength, yLength);   //erzeugen eines ersten verbundenen Phades in der Plane

        MakeEdgesWalls();

        FillEntropyPlane();

        if (modOfWFC == 3)
        {    //dieser mod macht wie beschrieben gruppen der einzelnen pfade. dazu muss zunähst der main pfad initialisiert erden
            CreatePlaneOfPathTileGroupIDs(xLength, yLength);
            InitiateStartPathGroup();
        }
        if (weightGang != 0 || weightRaum != 0)
        {
            UpdatePlaneWithStartEndPath();   //entferne aus der Doungne Lvl Plane alle nicht möglichen Objecte basierend auf dem Start zu end Path
        }


        MakeLvlWithWaveFunctionCollapse();  //hier passiert die eigentliche Wave function Collapse. alle einträge werden nun anch Entropy collapsed
        if (modOfWFC == 2)
        {

            FillDoungenWithWalls();
        }
        if (modOfWFC == 3)
        {

            ConnectPathGroupsInto1();   //Nach ende des WFC Algos werden die einzelnen Phadkonstrukte hier verbunden zu einem
        }
        lvlArray = SetLvLArrayWithWFCPlane(lvlArray);

        //Debuging code
        //DebugTestMethod();


        if (weightGang == 0 && weightRaum == 0)
        {
            int[] outOfView = new int[] { 99999, 99999 };
            startRoomCordinates = outOfView;
            endRoomCordinates = outOfView;
        }

        return lvlArray;
    }

    private void InitiateStartPathGroup()
    {
        lvlIsEmpty = true;

        //bei modofWFC == 3 wird das verfahren ausgeführt welches alle nicht verbundenen phade 
        //in gruppen sortiert, um diese später dann zusammenzufügen


        int xIndex = startRoomCordinates[0];
        int yIndex = startRoomCordinates[1];
        List<int> toRemovedOpening = new List<int> { 0 };
        RemoveAllElementsThatDontFitAtPlace(xIndex, yIndex, startToEndPathPlaneGoTo[xIndex, yIndex], toRemovedOpening);
        UpdateNeigboursOfThis(xIndex, yIndex, 0);
        RemoveAllElementsThatDontFitAtPlace(xIndex, yIndex, startToEndPathPlaneCameFrom[xIndex, yIndex], toRemovedOpening);
        UpdateNeigboursOfThis(xIndex, yIndex, 0);
        lowestEntropysPositionList.Clear();
        lowestEntropysPositionList.Add(new int[2] { startRoomCordinates[0], startRoomCordinates[1] });


        CollapseRandomLowestEntropy();
        /*
                Debug.Log("SInd in INitiateStartPathGroup");
                Debug.Log("Low entro list siuze " + lowestEntropysPositionList.Count);
                Debug.Log("Anzahl objecte in main path list  " + mainPathGroup.groupTiles.Count);
                Debug.Log("Anzahl elemente in nonmain path list  " + nonMainPathGroups.Count);
        */
        lvlIsEmpty = false;
    }

    private void CreatePlaneOfPathTileGroupIDs(int xLength, int yLength)
    {

        planeOfPathTileGroupIDs = new int[xLength, yLength];
        for (int i = 0; i < yLength; i++)
        {
            for (int a = 0; a < xLength; a++)
            {
                planeOfPathTileGroupIDs[a, i] = 9999;
            }
        }

    }

    private void CreateListOfAllDoungenObjects()
    {

        Tile_Library vanilaDoungenObjs = Resources.Load<Tile_Library>("TileMaps/OriginalTileLibrary");


        foreach (TileData tiles in vanilaDoungenObjs.tiles)
        {

            //Test Code Start

            if ((tiles.gangRaumWandInt == GangRaumWand.gang) && (weightGang == 0))
            {
                continue;
            }
            if ((tiles.gangRaumWandInt == GangRaumWand.raum) && (weightRaum == 0))
            {
                continue;
            }
            if ((tiles.gangRaumWandInt == GangRaumWand.wand) && (weightWand == 0))
            {
                continue;
            }
            //Test Code Ende


            listOfAllDoungenObjects.Add(tiles);

            if (tiles.gangRaumWandInt == GangRaumWand.wand)
            {
                continue;
            }
            else if (CheckAllDirectionsEqual(tiles))
            {
                continue;
            }
            else if (CheckBothOppositeDirectionsEqual(tiles))
            {
                TileData tmpTile = tiles.GetCopyOfThis();

                tmpTile.Rotate90RightRound();
                listOfAllDoungenObjects.Add(tmpTile);
                continue;
            }
            else
            {
                TileData tempTileBase;
                TileData safer = tiles.GetCopyOfThis(); ;
                for (int x = 0; x < 3; x++)
                {
                    tempTileBase = safer.GetCopyOfThis();
                    tempTileBase.Rotate90RightRound();
                    listOfAllDoungenObjects.Add(tempTileBase);
                    safer = tempTileBase;
                }
            }

        }
    }

    //Legacy Code sind folgende beiden Methoden
    /*
    private void CreateListOfAllGameObjects()
    {

        GameObject[] vanilaPrefabs = Resources.LoadAll<GameObject>("Doungen_Objects");
       

        foreach (GameObject oGO in vanilaPrefabs)
        {

            GameObject gO = Instantiate(oGO);
            Doungen_Object thisScript = gO.GetComponent<Doungen_Object>();

            listOfAllGameObjects.Add(gO);
            Destroy(gO);

            if (thisScript is Wand_object)
            {
                continue;
            }
            else if (CheckAllDirectionsEqual(thisScript))
            {
                continue;
            }
            else if (CheckBothOppositeDirectionsEqual(thisScript))
            {
                GameObject tempObj = Instantiate(gO);
                tempObj.transform.Rotate(0.0f, 0.0f, -90.0f, Space.Self);
                tempObj.GetComponent<Doungen_Object>().Rotate90RightRound();
                listOfAllGameObjects.Add(tempObj);
                Destroy(tempObj);
                continue;
            }
            else
            {
                GameObject tempObjBase = Instantiate(gO);
                GameObject safer = tempObjBase;
                for (int x = 0; x < 3; x++)
                {
                    GameObject tempObj = Instantiate(tempObjBase);
                    tempObj.transform.Rotate(0.0f, 0.0f, -90.0f, Space.Self);
                    tempObj.GetComponent<Doungen_Object>().Rotate90RightRound();
                    listOfAllGameObjects.Add(tempObj);
                    tempObjBase = tempObj;
                    Destroy(tempObj);
                }
                Destroy(safer);
            }

        }
        Debug.Log(listOfAllGameObjects.Count + " ist menge aller prefab Assets");
    }

    private bool CheckAllDirectionsEqual(Doungen_Object thisScript)
    {

        if (CheckBothOppositeDirectionsEqual(thisScript) && thisScript.GetTop() == thisScript.GetRight())
            return true;
        else
            return false;
    }*/
    private bool CheckAllDirectionsEqual(TileData thisTile)
    {

        if (CheckBothOppositeDirectionsEqual(thisTile) && thisTile.GetTop() == thisTile.GetRight())
            return true;
        else
            return false;
    }

    private bool CheckBothOppositeDirectionsEqual(TileData thisTile)
    {
        if (thisTile.GetTop() == thisTile.GetDown() && thisTile.GetRight() == thisTile.GetLeft())
            return true;
        else
            return false;

    }

    /*  Legacy Code
        private void CreatelistOfAllDoungenObjects()
        {
            DoungenObjectRepresentater tmpRepresentator;

            foreach (GameObject item in listOfAllGameObjects)
            {
                tmpRepresentator = new DoungenObjectRepresentater();
                tmpRepresentator.gORepresented = item;
                tmpRepresentator.gangRaumWandInt = item.GetComponent<Doungen_Object>().GetGangRaumWand();
                tmpRepresentator.doungenObjTypeInt = item.GetComponent<Doungen_Object>().GetDoungenObjType();
                tmpRepresentator.left = item.GetComponent<Doungen_Object>().GetLeft();
                tmpRepresentator.down = item.GetComponent<Doungen_Object>().GetDown();
                tmpRepresentator.right = item.GetComponent<Doungen_Object>().GetRight();
                tmpRepresentator.top = item.GetComponent<Doungen_Object>().GetTop();

                listOfAllDoungenObjects.Add(tmpRepresentator);
            }

        }*/



    private void CreatePlaneForWaveFuncCollapseAlgro(int xLength, int yLength)
    {
        for (int i = 0; i < yLength; i++)
        {
            for (int a = 0; a < xLength; a++)
            {
                planeForWaveFuncCollapseAlgorithm[a, i] = new List<TileData>(listOfAllDoungenObjects);


            }
        }

    }

    private void CreateFirstStartToEndPath(int xLength, int yLength)
    {
        int minXDistance = (xLength / 3);         // um zu gewährleisten dass der Phad auch ein phad und nicht nur benachtbarte felder sind gibt es eine minimal distance voneinander
        int minYDistance = (yLength / 3);

        startToEndPathPlaneGoTo = new DirectionsOf[xLength, yLength];
        startToEndPathPlaneCameFrom = new DirectionsOf[xLength, yLength];
        startRoomCordinates = new int[2] { (int)Random.Range(0, (xLength)), (int)Random.Range(0, (yLength)) };
        endRoomCordinates = new int[2];
        MakeRandomEndRoom(0, minXDistance, xLength);  //lege x coordinaten von end Punkt fest
        MakeRandomEndRoom(1, minYDistance, yLength);  //lege y Coordinate von Endpunkt fest


        DirectionsOf xDirectionForPath = 0;
        DirectionsOf yDirectionForPath = 0;
        List<DirectionsOf> directionForPathList = new List<DirectionsOf>();
        directionForPathList.Add(xDirectionForPath);
        directionForPathList.Add(yDirectionForPath);

        int[] currentPos = new int[2] { startRoomCordinates[0], startRoomCordinates[1] };
        lastPos = new int[2];

        if (minXDistance > 1 && minYDistance > 1)
        {
            int[] deviationPoint = MakeRandomDeviationPoint(xLength, yLength); //erstelle einen zufälligen punkt der zwischen start udn ende angelaufne wird um den apth etwas zufälliger und ungeordneter zu machen
            InitiateDirectionForPath(currentPos, deviationPoint, directionForPathList);  //findet raus welche generelle Richtung vom relativen Startp zum relativen EndP führt
            MakePathBetweenPoints(currentPos, deviationPoint, directionForPathList, false);  //mache phad zwischen beidne punktne und füge bei den genutzen feldern in das passende 2d array ein in welche richtung gegangen wurde, um zu wissne inw elche richtung die tile eien öffnung benötigt
            InitiateDirectionForPath(currentPos, endRoomCordinates, directionForPathList);
            MakePathBetweenPoints(currentPos, endRoomCordinates, directionForPathList, true);
        }
        else
        {
            InitiateDirectionForPath(currentPos, endRoomCordinates, directionForPathList);
            MakePathBetweenPoints(currentPos, endRoomCordinates, directionForPathList, false);
        }
    }



    private void MakePathBetweenPoints(int[] currentPoi, int[] endPoi, List<DirectionsOf> direcForPath, bool secondPath)
    {
        if (secondPath)
        {
            secondPath = false;
            if (!(currentPoi[0] == endPoi[0] && currentPoi[1] == endPoi[1]))
            {
                if ((CheckIfNextPosInPathIsLastOne(direcForPath[1], currentPoi)))
                {
                    currentPoi = MoveOntoGivenPathLine(direcForPath[0], currentPoi);
                }
                else if ((CheckIfNextPosInPathIsLastOne(direcForPath[0], currentPoi)))
                {
                    currentPoi = MoveOntoGivenPathLine(direcForPath[1], currentPoi);
                }
            }
        }

        while (!(currentPoi[0] == endPoi[0] && currentPoi[1] == endPoi[1]))
        {
            //Debug.Log("Current pos ist " + currentPoi[0] + " " + currentPoi[1]);
            lastPos[0] = currentPoi[0];
            lastPos[1] = currentPoi[1];
            if (currentPoi[0] == endPoi[0])
            {
                currentPoi = MoveOntoGivenPathLine(direcForPath[1], currentPoi);
            }
            else if (currentPoi[1] == endPoi[1])
            {
                currentPoi = MoveOntoGivenPathLine(direcForPath[0], currentPoi);
            }
            else
            {
                if ((int)Random.Range(0, 2) > 0)
                {
                    currentPoi = MoveOntoGivenPathLine(direcForPath[0], currentPoi);
                }
                else
                {
                    currentPoi = MoveOntoGivenPathLine(direcForPath[1], currentPoi);
                }
            }

        }
    }

    private bool CheckIfNextPosInPathIsLastOne(DirectionsOf dire, int[] currentPoi)
    {  //überprüft ob nächster Punkt schon belegt ist mit einem Phad
        bool nextUsed = false;
        switch (dire)
        {
            case DirectionsOf.top:
                if ((currentPoi[1] - 1) == lastPos[1]) { nextUsed = true; }
                break;
            case DirectionsOf.down:
                if ((currentPoi[1] + 1) == lastPos[1]) { nextUsed = true; }
                break;
            case DirectionsOf.right:
                if ((currentPoi[0] + 1) == lastPos[0]) { nextUsed = true; }
                break;
            case DirectionsOf.left:
                if ((currentPoi[0] - 1) == lastPos[0]) { nextUsed = true; }
                break;

            default: nextUsed = false; break;
        }
        return nextUsed;
    }

    private int[] MoveOntoGivenPathLine(DirectionsOf dire, int[] currentPoi)
    {
        switch (dire)
        {
            case DirectionsOf.top:
                startToEndPathPlaneGoTo[currentPoi[0], currentPoi[1]] = DirectionsOf.top;
                currentPoi[1] -= 1;
                startToEndPathPlaneCameFrom[currentPoi[0], currentPoi[1]] = DirectionsOf.down;
                break;
            case DirectionsOf.down:
                startToEndPathPlaneGoTo[currentPoi[0], currentPoi[1]] = DirectionsOf.down;
                currentPoi[1] += 1;
                startToEndPathPlaneCameFrom[currentPoi[0], currentPoi[1]] = DirectionsOf.top;
                break;
            case DirectionsOf.right:
                startToEndPathPlaneGoTo[currentPoi[0], currentPoi[1]] = DirectionsOf.right;
                currentPoi[0] += 1;
                startToEndPathPlaneCameFrom[currentPoi[0], currentPoi[1]] = DirectionsOf.left;
                break;
            case DirectionsOf.left:
                startToEndPathPlaneGoTo[currentPoi[0], currentPoi[1]] = DirectionsOf.left;
                currentPoi[0] -= 1;
                startToEndPathPlaneCameFrom[currentPoi[0], currentPoi[1]] = DirectionsOf.right;
                break;

            default: break;
        }
        return currentPoi;
    }

    private int[] MakeRandomDeviationPoint(int xLength, int yLength)
    {   //Function erzeugt einen randum punkt zwischen start und endpunkt.WÜrde durch die begrenzungen von Start und Endpunkt ein Quadrat aufgespannt werden, so wäre dieser nur entweder auf der horizontalen, oder vertikalen außerhalb der Begrenzungen von Start und ENdpunkt, für die funktionalität des Algorithmuses

        int xValue = (int)Random.Range(0, (xLength));
        int yValue;
        int xMinBorder = System.Math.Min(startRoomCordinates[0], endRoomCordinates[0]);
        int xMaxBorder = System.Math.Max(startRoomCordinates[0], endRoomCordinates[0]);

        if (xValue < xMaxBorder && xValue > xMinBorder)
        {  //wenn das der Fall ist der Wert innerhalb des Rahmen. 1 Wert muss innerhalb des Rahmen sein
            yValue = (int)Random.Range(0, (yLength));
        }
        else
        {      //so wäre der erste wer außerhalb des Rahmen, wodurch der zweite im Rahmens ein muss
            int yMinBorder = System.Math.Min(startRoomCordinates[1], endRoomCordinates[1]);
            int yMaxBorder = System.Math.Max(startRoomCordinates[1], endRoomCordinates[1]);
            yValue = (int)Random.Range((yMinBorder + 1), (yMaxBorder - 1));
        }
        int[] deviPoi = new int[2] { xValue, yValue };

        return deviPoi;
    }

    private void InitiateDirectionForPath(int[] realtiveStartpoint, int[] realativeEndpoint, List<DirectionsOf> direList)
    {
        if (realtiveStartpoint[0] < realativeEndpoint[0])        //findet raus welche generelle Richtung vom Startp zum EndP führt
        {
            direList[0] = DirectionsOf.right;
        }
        else
        {
            direList[0] = DirectionsOf.left;
        }
        if (realtiveStartpoint[1] < realativeEndpoint[1])
        {
            direList[1] = DirectionsOf.down;
        }
        else
        {
            direList[1] = DirectionsOf.top;
        }

    }

    private void MakeRandomEndRoom(int index, int distance, int maxDistance)
    {

        if ((startRoomCordinates[index] - distance) < 0)
        {
            endRoomCordinates[index] = (int)Random.Range((startRoomCordinates[index] + distance), maxDistance);
        }
        else if ((startRoomCordinates[index] + distance) > (maxDistance - 1))
        {
            endRoomCordinates[index] = (int)Random.Range(0, (startRoomCordinates[index] - distance));
        }
        else
        {
            int decision = (int)Random.Range(0, 2);
            if (decision == 0)
            {
                endRoomCordinates[index] = (int)Random.Range((startRoomCordinates[index] + distance), maxDistance);
            }
            else
            {
                endRoomCordinates[index] = (int)Random.Range(0, (startRoomCordinates[index] - distance));
            }
        }
    }

    private void UpdatePlaneWithStartEndPath()
    {
        List<int> toRemovedOpening = new List<int> { 0 }; //0 = wand, kein durchgang, 1 = Gang, 2 = Fläche/ raum ACHTUNG, bei GangRAumWand enum ist = = error und 3= Wand, sonst gleich

        for (int yIndex = 0; yIndex < startToEndPathPlaneGoTo.GetLength(1); yIndex++)
        {
            for (int xIndex = 0; xIndex < startToEndPathPlaneGoTo.GetLength(0); xIndex++)
            {
                if (startToEndPathPlaneGoTo[xIndex, yIndex] != 0)
                {
                    RemoveAllElementsThatDontFitAtPlace(xIndex, yIndex, startToEndPathPlaneGoTo[xIndex, yIndex], toRemovedOpening);
                    UpdateNeigboursOfThis(xIndex, yIndex, 0);
                }
                if (startToEndPathPlaneCameFrom[xIndex, yIndex] != 0)
                {
                    RemoveAllElementsThatDontFitAtPlace(xIndex, yIndex, startToEndPathPlaneCameFrom[xIndex, yIndex], toRemovedOpening);
                    UpdateNeigboursOfThis(xIndex, yIndex, 0);
                }
            }
        }
    }

    private void RemoveAllElementsThatDontFitAtPlace(int xindex, int yindex, DirectionsOf place, List<int> toRemovedList)
    { // für toRemovelIst items -> 0 = wand, kein durchgang, 1 = Gang, 2 = Fläche/ raum ACHTUNG, bei GangRAumWand enum ist 0 = error und 3= Wand, sonst gleich
        if (toRemovedList.Count > 0)
        {
            foreach (int toRemoved in toRemovedList)
            {
                switch (place)
                {
                    case DirectionsOf.top:
                        {
                            for (int index = (planeForWaveFuncCollapseAlgorithm[xindex, yindex].Count - 1); index >= 0; index--)
                            {
                                if (planeForWaveFuncCollapseAlgorithm[xindex, yindex][index].top == toRemoved)
                                {
                                    planeForWaveFuncCollapseAlgorithm[xindex, yindex].RemoveAt(index);
                                }
                            }
                            /*foreach (DoungenObjectRepresentater DoRepresen in planeForWaveFuncCollapseAlgorithm[xindex,yindex].ToList())
                            {
                                MatchTargetWeightMask selbe // so ginge es auch mit forEach, udn wäre schöner anzusehen, aber ich möchte vermeiden eine weitere Liste zu erstellen aus performance Gründen
                            }*/
                            break;
                        }
                    case DirectionsOf.right:
                        {
                            for (int index = (planeForWaveFuncCollapseAlgorithm[xindex, yindex].Count - 1); index >= 0; index--)
                            {
                                if (planeForWaveFuncCollapseAlgorithm[xindex, yindex][index].right == toRemoved)
                                {
                                    planeForWaveFuncCollapseAlgorithm[xindex, yindex].RemoveAt(index);
                                }
                            }
                            break;
                        }
                    case DirectionsOf.down:
                        {
                            for (int index = (planeForWaveFuncCollapseAlgorithm[xindex, yindex].Count - 1); index >= 0; index--)
                            {
                                if (planeForWaveFuncCollapseAlgorithm[xindex, yindex][index].down == toRemoved)
                                {
                                    planeForWaveFuncCollapseAlgorithm[xindex, yindex].RemoveAt(index);
                                }
                            }
                            break;
                        }
                    case DirectionsOf.left:
                        {
                            for (int index = (planeForWaveFuncCollapseAlgorithm[xindex, yindex].Count - 1); index >= 0; index--)
                            {
                                if (planeForWaveFuncCollapseAlgorithm[xindex, yindex][index].left == toRemoved)
                                {
                                    planeForWaveFuncCollapseAlgorithm[xindex, yindex].RemoveAt(index);
                                }
                            }
                            break;
                        }
                    default: break;
                }
            }
            if (planeOfEntropys[xindex, yindex] != 1)
            {
                planeOfEntropys[xindex, yindex] = planeForWaveFuncCollapseAlgorithm[xindex, yindex].Count;
                ProcessEntropyOfPosition(xindex, yindex);

                //Versuche Reihenfolge der Einträge
                if (planeOfEntropys[xindex, yindex] == 1)
                {
                    orderOfEntrys.Enqueue(new int[2] { xindex, yindex });
                    if (modOfWFC == 3)
                    {
                        AddTileToPathGroup(xindex, yindex);
                    }
                }
            }
        }
    }

    private void UpdateNeigboursOfThis(int xindex, int yindex, DirectionsOf blockDire)
    {
        //MACH WICHTIGSTEN CODE, wie bei Wave Func Neigborus geupdated werden. DER Leistungsfresser
        List<int> topToRemove = new List<int>();
        List<int> rightToRemove = new List<int>();
        List<int> downToRemove = new List<int>();
        List<int> leftToRemove = new List<int>();
        bool topGang, rightGang, downGang, leftGang, topRaum, rightRaum, downRaum, leftRaum, topWand, rightWand, downWand, leftWand;
        topGang = rightGang = downGang = leftGang = topRaum = rightRaum = downRaum = leftRaum = topWand = rightWand = downWand = leftWand = false;

        foreach (TileData DORepresen in planeForWaveFuncCollapseAlgorithm[xindex, yindex])
        {
            if ((!(topGang && topRaum && topWand)) && blockDire != DirectionsOf.top)
            {
                switch (DORepresen.top)
                {
                    case 0: topWand = true; break;
                    case 1: topGang = true; break;
                    case 2: topRaum = true; break;
                    default: break;
                }
            }
            if ((!(rightGang && rightRaum && rightWand)) && blockDire != DirectionsOf.right)
            {
                switch (DORepresen.right)
                {
                    case 0: rightWand = true; break;
                    case 1: rightGang = true; break;
                    case 2: rightRaum = true; break;
                    default: break;
                }
            }
            if ((!(downGang && downRaum && downWand)) && blockDire != DirectionsOf.down)
            {
                switch (DORepresen.down)
                {
                    case 0: downWand = true; break;
                    case 1: downGang = true; break;
                    case 2: downRaum = true; break;
                    default: break;
                }
            }
            if ((!(leftGang && leftRaum && leftWand)) && blockDire != DirectionsOf.left)
            {
                switch (DORepresen.left)
                {
                    case 0: leftWand = true; break;
                    case 1: leftGang = true; break;
                    case 2: leftRaum = true; break;
                    default: break;
                }
            }
        }
        FillNeigbourRemoveLists(topGang, topRaum, topWand, topToRemove);
        FillNeigbourRemoveLists(rightGang, rightRaum, rightWand, rightToRemove);
        FillNeigbourRemoveLists(downGang, downRaum, downWand, downToRemove);
        FillNeigbourRemoveLists(leftGang, leftRaum, leftWand, leftToRemove);

        if (topToRemove.Count > 0 && yindex > 0 && blockDire != DirectionsOf.top)
        {

            //RemoveAllElementsThatDontFitAtPlace(xindex, (yindex - 1), DirectionsOf.down, topToRemove);
            //UpdateNeigboursOfThis(xindex, (yindex - 1), DirectionsOf.down);

            //Test
            bool[][] oldAllSLotEdges = returnSlotEdges(xindex, (yindex - 1));
            RemoveAllElementsThatDontFitAtPlace(xindex, (yindex - 1), DirectionsOf.down, topToRemove);
            bool[][] newAllSLotEdges = returnSlotEdges(xindex, (yindex - 1));
            bool[] newChangedDirects = GetChangedDirectionsOfEdges(oldAllSLotEdges, newAllSLotEdges);

            UpdateNeigboursOfThisAfterRemove(xindex, (yindex - 1), DirectionsOf.down, newChangedDirects, newAllSLotEdges);
        }
        if (rightToRemove.Count > 0 && xindex < xMaxIndex && blockDire != DirectionsOf.right)
        {
            //RemoveAllElementsThatDontFitAtPlace((xindex + 1), yindex, DirectionsOf.left, rightToRemove);
            //UpdateNeigboursOfThis((xindex + 1), yindex, DirectionsOf.left);

            //TEST
            bool[][] oldAllSLotEdges = returnSlotEdges((xindex + 1), yindex);
            RemoveAllElementsThatDontFitAtPlace((xindex + 1), yindex, DirectionsOf.left, rightToRemove);
            bool[][] newAllSLotEdges = returnSlotEdges((xindex + 1), yindex);
            bool[] newChangedDirects = GetChangedDirectionsOfEdges(oldAllSLotEdges, newAllSLotEdges);
            UpdateNeigboursOfThisAfterRemove((xindex + 1), yindex, DirectionsOf.left, newChangedDirects, newAllSLotEdges);
        }
        if (downToRemove.Count > 0 && yindex < yMaxIndex && blockDire != DirectionsOf.down)
        {
            //RemoveAllElementsThatDontFitAtPlace(xindex, (yindex + 1), DirectionsOf.top, downToRemove);
            //UpdateNeigboursOfThis(xindex, (yindex + 1), DirectionsOf.top);

            //TEST
            bool[][] oldAllSLotEdges = returnSlotEdges(xindex, (yindex + 1));
            RemoveAllElementsThatDontFitAtPlace(xindex, (yindex + 1), DirectionsOf.top, downToRemove);
            bool[][] newAllSLotEdges = returnSlotEdges(xindex, (yindex + 1));
            bool[] newChangedDirects = GetChangedDirectionsOfEdges(oldAllSLotEdges, newAllSLotEdges);
            UpdateNeigboursOfThisAfterRemove(xindex, (yindex + 1), DirectionsOf.top, newChangedDirects, newAllSLotEdges);
        }
        if (leftToRemove.Count > 0 && xindex > 0 && blockDire != DirectionsOf.left)
        {
            //RemoveAllElementsThatDontFitAtPlace((xindex - 1), yindex, DirectionsOf.right, leftToRemove);
            //UpdateNeigboursOfThis((xindex - 1), yindex, DirectionsOf.right);

            //TEST

            bool[][] oldAllSLotEdges = returnSlotEdges((xindex - 1), yindex);
            RemoveAllElementsThatDontFitAtPlace((xindex - 1), yindex, DirectionsOf.right, leftToRemove);
            bool[][] newAllSLotEdges = returnSlotEdges((xindex - 1), yindex);
            bool[] newChangedDirects = GetChangedDirectionsOfEdges(oldAllSLotEdges, newAllSLotEdges);
            UpdateNeigboursOfThisAfterRemove((xindex - 1), yindex, DirectionsOf.right, newChangedDirects, newAllSLotEdges);
        }

    }

    private void FillNeigbourRemoveLists(bool gang, bool raum, bool wand, List<int> listOfRemovers)
    {
        if (!gang)
        { //wenn es nicht vorhanden war, dann add zu der Listd er zu entfernenden Kanten / öffnungen
            listOfRemovers.Add(1);
        }
        if (!raum)
        {
            listOfRemovers.Add(2);
        }
        if (!wand)
        {
            listOfRemovers.Add(0);
        }
    }


    private void MakeLvlWithWaveFunctionCollapse()
    {

        lowestEntropysPositionList.Clear();

        while (true)
        {
            //Debug.Log("in Methode für das " + debugint + " mal. die nummer von lowest entro pos ist " + lowestEntropysPositionList.Count);
            if (lowestEntropysPositionList.Count == 0)
            {
                smallestEntroOverOne = int.MaxValue;
                if (cornerAllowed == false)
                {
                    FillEntropyLists();
                }
                if (lowestEntropysPositionList.Count == 0)
                {
                    cornerAllowed = true;
                }
                if (cornerAllowed == true)
                {
                    FillEntropyLists();
                }
            }
            //debugint++;
            //Debug.Log("in Methode für das " + debugint + " mal. die nummer von lowest entro pos ist " + lowestEntropysPositionList.Count);
            if (lowestEntropysPositionList.Count == 0)
            {
                break;
            }
            // Debug.Log("Ist for collapse lowest entro");
            CollapseRandomLowestEntropy();
            lvlIsEmpty = false;
        }
    }

    private void FillEntropyLists()
    {
        for (int y = 0; y <= yMaxIndex; y++)
        {
            for (int x = 0; x <= xMaxIndex; x++)
            {
                ProcessEntropyOfPosition(x, y);
            }
        }
    }

    private void ProcessEntropyOfPosition(int xindex, int yindex)
    {
        if (modOfWFC == 2) //Mod 2 ist der dritte modus bei welchem nur Tiles an bestehende Pfade gesetzt werden
        {        //Dieses Verfahren setzt neue tils nur an offene Gänge/ Räume um ein verbundenes Konstrukt zu garantieren.
            if (lvlIsEmpty)
            {
                if (xindex == startRoomCordinates[0] && yindex == startRoomCordinates[1])
                {
                    ExecuteEntropyPositions(xindex, yindex);
                }
            }
            else
            {
                if (PositionHasConnectedNeigbour(xindex, yindex))
                {
                    ExecuteEntropyPositions(xindex, yindex);
                }
            }
        }
        else
        {
            ExecuteEntropyPositions(xindex, yindex);
        }
    }

    private bool PositionHasConnectedNeigbour(int xindex, int yindex)
    {
        bool hasOpenNei = false;
        hasOpenNei |= ThisNeigbourIsConnected(xindex, (yindex - 1), DirectionsOf.down);     //prüfe jede Richtung ob eine Verbindung besteht
        hasOpenNei |= ThisNeigbourIsConnected(xindex, (yindex + 1), DirectionsOf.top);
        hasOpenNei |= ThisNeigbourIsConnected((xindex - 1), yindex, DirectionsOf.right);
        hasOpenNei |= ThisNeigbourIsConnected((xindex + 1), yindex, DirectionsOf.left);
        return hasOpenNei;
    }

    private bool ThisNeigbourIsConnected(int xindex, int yindex, DirectionsOf direToCheck)
    {
        if (xindex < 0 || xindex > xMaxIndex || yindex < 0 || yindex > yMaxIndex)
            return false;
        else
        {
            if (planeOfEntropys[xindex, yindex] == 1)
            {
                switch (direToCheck)
                {
                    case DirectionsOf.top:
                        {
                            int top = planeForWaveFuncCollapseAlgorithm[xindex, yindex][0].GetTop();
                            return (top == 1 || top == 2);
                        }
                    case DirectionsOf.right:
                        {
                            int right = planeForWaveFuncCollapseAlgorithm[xindex, yindex][0].GetRight();
                            return (right == 1 || right == 2);
                        }
                    case DirectionsOf.down:
                        {
                            int down = planeForWaveFuncCollapseAlgorithm[xindex, yindex][0].GetDown();
                            return (down == 1 || down == 2);
                        }
                    case DirectionsOf.left:
                        {
                            int left = planeForWaveFuncCollapseAlgorithm[xindex, yindex][0].GetLeft();
                            return (left == 1 || left == 2);
                        }
                    default: return false;
                }
            }
            return false;
        }
    }

    private void FillDoungenWithWalls()
    {
        for (int y = 0; y <= yMaxIndex; y++)
        {
            for (int x = 0; x <= xMaxIndex; x++)
            {
                if (planeOfEntropys[x, y] > 1)
                {
                    for (int index = (planeForWaveFuncCollapseAlgorithm[x, y].Count - 1); index >= 0; index--)
                    {
                        if (planeForWaveFuncCollapseAlgorithm[x, y][index].gangRaumWandInt != GangRaumWand.wand)
                        {
                            planeForWaveFuncCollapseAlgorithm[x, y].RemoveAt(index);
                        }
                    }
                    planeOfEntropys[x, y] = 1;
                    orderOfEntrys.Enqueue(new int[2] { x, y });
                }
            }
        }
    }

    private void ExecuteEntropyPositions(int xindex, int yindex)
    {

        bool canProceed = true;
        if (cornerAllowed == false)
        {
            if (xindex == 0 || xindex == xMaxIndex || yindex == 0 || yindex == yMaxIndex)
            {
                canProceed = false;
            }
        }
        if (canProceed)
        {
            if (planeOfEntropys[xindex, yindex] == smallestEntroOverOne)
            {
                lowestEntropysPositionList.Add(new int[2] { xindex, yindex });
            }
            else if (planeOfEntropys[xindex, yindex] < smallestEntroOverOne && planeOfEntropys[xindex, yindex] > 1)
            {
                lowestEntropysPositionList.Clear();
                smallestEntroOverOne = planeOfEntropys[xindex, yindex];
                lowestEntropysPositionList.Add(new int[2] { xindex, yindex });
            }
        }
    }


    //Methoden für Path Group sind für das verfahren welches den Doungen normal nach WFC generert, 
    //jedes Gangkonstrukt in Gruppen unterteilt, und diese dann am ende verbindet um ein großes 
    //verbundenes Konstrukt zus chaffen
    private void AddTileToPathGroup(int xcordi, int ycordi)
    {
        if (planeForWaveFuncCollapseAlgorithm[xcordi, ycordi][0].gangRaumWandInt != GangRaumWand.wand)
        {
            if (lvlIsEmpty)
            {
                //hier wird startpath Initialisiert
                MakeNewPathGroup(xcordi, ycordi, mainPathGroup);
                planeOfPathTileGroupIDs[xcordi, ycordi] = mainPathGroup.uniqueID;
                lvlIsEmpty = false;
                //Debug.Log("Wurde ein main phad gemacht");
            }
            else
            {
                if (PositionHasConnectedNeigbour(xcordi, ycordi))
                {
                    //Mache heir code der neue zu gruppen zufügt
                    bool hasTop, hasRight, hasDown, hasLeft, wasAdded;
                    hasTop = hasRight = hasDown = hasLeft = wasAdded = false;
                    hasTop = ThisNeigbourIsConnected(xcordi, (ycordi - 1), DirectionsOf.down);     //prüfe jede Richtung ob eine Verbindung besteht
                    hasDown = ThisNeigbourIsConnected(xcordi, (ycordi + 1), DirectionsOf.top);
                    hasLeft |= ThisNeigbourIsConnected((xcordi - 1), ycordi, DirectionsOf.right);
                    hasRight |= ThisNeigbourIsConnected((xcordi + 1), ycordi, DirectionsOf.left);

                    if (hasTop)
                    {
                        InsertTileIntoPathGroup(xcordi, ycordi, planeOfPathTileGroupIDs[xcordi, (ycordi - 1)]);
                        wasAdded = true;
                    }
                    if (hasDown)
                    {
                        if (!wasAdded)
                        {
                            InsertTileIntoPathGroup(xcordi, ycordi, planeOfPathTileGroupIDs[xcordi, (ycordi + 1)]);
                            wasAdded = true;
                        }
                        else
                        {
                            MergePathGroups(planeOfPathTileGroupIDs[xcordi, ycordi], planeOfPathTileGroupIDs[xcordi, (ycordi + 1)]);
                        }
                    }
                    if (hasLeft)
                    {
                        if (!wasAdded)
                        {
                            InsertTileIntoPathGroup(xcordi, ycordi, planeOfPathTileGroupIDs[(xcordi - 1), ycordi]);
                            wasAdded = true;
                        }
                        else
                        {
                            MergePathGroups(planeOfPathTileGroupIDs[xcordi, ycordi], planeOfPathTileGroupIDs[(xcordi - 1), ycordi]);
                        }
                    }
                    if (hasRight)
                    {
                        if (!wasAdded)
                        {
                            InsertTileIntoPathGroup(xcordi, ycordi, planeOfPathTileGroupIDs[(xcordi + 1), ycordi]);
                            wasAdded = true;
                        }
                        else
                        {
                            MergePathGroups(planeOfPathTileGroupIDs[xcordi, ycordi], planeOfPathTileGroupIDs[(xcordi + 1), ycordi]);
                        }
                    }



                }
                else
                {       //Wenn keine verbundenen Nachbarn wird neue Gruppe eröffnet
                    PathTilesGroup newPathGroup = new PathTilesGroup();
                    MakeNewPathGroup(xcordi, ycordi, newPathGroup);
                    nonMainPathGroups.Add(newPathGroup);
                    planeOfPathTileGroupIDs[xcordi, ycordi] = newPathGroup.uniqueID;
                }

            }
        }
    }

    private PathTilesGroup MakeNewPathGroup(int xcordi, int ycordi, PathTilesGroup pathgrou)
    {
        pathgrou.groupTiles = new List<int[]>();
        int[] startP = new int[] { xcordi, ycordi };
        pathgrou.groupTiles.Add(startP);
        pathgrou.topExtreme = startP;
        pathgrou.rightExtreme = startP;
        pathgrou.downExtreme = startP;
        pathgrou.leftExtreme = startP;
        pathgrou.uniqueID = phGrIDCount;
        phGrIDCount++;

        return pathgrou;
    }

    private void InsertTileIntoPathGroup(int xCordiTile, int yCordiTile, int groupToInsertID)
    {   //Diese Methode fügt neue Tile in POathTileGruppe ein und überprüft dabei pb sich Extrempunkte ändern
        int[] tileXY = new int[] { xCordiTile, yCordiTile };
        if (groupToInsertID == 0)
        {
            mainPathGroup.groupTiles.Add(tileXY);
            ControlPathGroupExtremes(tileXY, groupToInsertID);
            planeOfPathTileGroupIDs[xCordiTile, yCordiTile] = groupToInsertID;
        }
        else
        {
            foreach (PathTilesGroup ptGroup in nonMainPathGroups)
            {
                if (ptGroup.uniqueID == groupToInsertID)
                {
                    ptGroup.groupTiles.Add(tileXY);
                    ControlPathGroupExtremes(tileXY, groupToInsertID);
                    planeOfPathTileGroupIDs[xCordiTile, yCordiTile] = groupToInsertID;
                    break;
                }
            }
        }
    }

    private void ControlPathGroupExtremes(int[] tileXY, int insertGroupID)
    {
        PathTilesGroup insertGroup = new PathTilesGroup();
        if (insertGroupID == 0)
        {
            insertGroup = mainPathGroup;
        }
        else
        {
            foreach (PathTilesGroup ptGroup in nonMainPathGroups)
            {
                if (ptGroup.uniqueID == insertGroupID)
                {
                    insertGroup = ptGroup;
                    break;
                }
            }
        }
        if (tileXY[1] < insertGroup.topExtreme[1])
        {
            insertGroup.topExtreme = tileXY;
        }
        if (tileXY[1] > insertGroup.downExtreme[1])
        {
            insertGroup.downExtreme = tileXY;
        }
        if (tileXY[0] > insertGroup.rightExtreme[0])
        {
            insertGroup.rightExtreme = tileXY;
        }
        if (tileXY[0] < insertGroup.leftExtreme[0])
        {
            insertGroup.leftExtreme = tileXY;
        }
    }

    private void MergePathGroups(int group1ID, int group2ID)
    {
        if (!(group1ID == group2ID))
        {
            if (group1ID == (mainPathGroup.uniqueID))
            {
                PathGroup_1into2(group2ID, group1ID);
            }
            else if (group2ID == (mainPathGroup.uniqueID))
            {
                PathGroup_1into2(group1ID, group2ID);
            }
            else
            {
                PathGroup_1into2(group1ID, group2ID);
            }
        }
    }

    private void PathGroup_1into2(int groupDeleteID, int groupMainID)
    {
        int[] thisTile;
        PathTilesGroup groupDelete = new PathTilesGroup();
        PathTilesGroup groupMain = new PathTilesGroup();
        int deleteIndexForNonNP = 0;
        for (int i = 0; i < nonMainPathGroups.Count; i++)
        {
            if (nonMainPathGroups[i].uniqueID == groupDeleteID)
            {
                groupDelete = nonMainPathGroups[i];
                deleteIndexForNonNP = i;
            }
            if (nonMainPathGroups[i].uniqueID == groupMainID)
            {
                groupMain = nonMainPathGroups[i];
            }
        }
        if (groupMainID == 0)
        {
            groupMain = mainPathGroup;
        }

        for (int x = ((groupDelete.groupTiles.Count) - 1); x >= 0; x--)
        {
            thisTile = groupDelete.groupTiles[x];
            planeOfPathTileGroupIDs[thisTile[0], thisTile[1]] = groupMainID;
            groupMain.groupTiles.Add(thisTile);
            groupDelete.groupTiles.RemoveAt(x);
        }
        if (groupDelete.topExtreme[1] < groupMain.topExtreme[1])
        {
            groupMain.topExtreme = groupDelete.topExtreme;
        }
        if (groupDelete.downExtreme[1] > groupMain.downExtreme[1])
        {
            groupMain.downExtreme = groupDelete.downExtreme;
        }
        if (groupDelete.rightExtreme[0] > groupMain.rightExtreme[0])
        {
            groupMain.rightExtreme = groupDelete.rightExtreme;
        }
        if (groupDelete.leftExtreme[0] < groupMain.leftExtreme[0])
        {
            groupMain.leftExtreme = groupDelete.leftExtreme;
        }

        nonMainPathGroups.RemoveAt(deleteIndexForNonNP);


    }

    private void ConnectPathGroupsInto1()
    {
        int[] currentTop;
        int[] currentRight;
        int[] currentDown;
        int[] currentLeft;

        int pgIndex = 0;
        bool nothingFound = false;
        while (true)
        {

            if (!nothingFound)
            {
                pgIndex = 0;
            }
            if (nonMainPathGroups.Count == 0 || pgIndex == nonMainPathGroups.Count)
            {
                break;
            }

            currentTop = nonMainPathGroups[pgIndex].topExtreme;
            currentRight = nonMainPathGroups[pgIndex].rightExtreme;
            currentDown = nonMainPathGroups[pgIndex].downExtreme;
            currentLeft = nonMainPathGroups[pgIndex].leftExtreme;


            bool leftIsPath = false;
            bool rightIsPath = false;
            bool topIsPath = false;
            bool downIsPath = false;
            int steps = 0;
            nothingFound = false;


            while (!(topIsPath || rightIsPath || downIsPath || leftIsPath))
            {
                steps++;
                topIsPath = IsThereAnotherPath(currentTop, DirectionsOf.top, steps);
                rightIsPath = IsThereAnotherPath(currentRight, DirectionsOf.right, steps);
                downIsPath = IsThereAnotherPath(currentDown, DirectionsOf.down, steps);
                leftIsPath = IsThereAnotherPath(currentLeft, DirectionsOf.left, steps);
                if (steps > xMaxIndex && steps > yMaxIndex)
                {
                    nothingFound = true;
                    break;
                }
            }
            pgIndex++;
            if (nothingFound)
            {
                continue;
            }

            if (topIsPath)
            {
                ConnectTilesAndGroups(currentTop, DirectionsOf.top, steps);
            }
            else if (rightIsPath)
            {
                ConnectTilesAndGroups(currentRight, DirectionsOf.right, steps);
            }
            else if (downIsPath)
            {
                ConnectTilesAndGroups(currentDown, DirectionsOf.down, steps);
            }
            else
            {
                ConnectTilesAndGroups(currentLeft, DirectionsOf.left, steps);
            }

            MakeLvlWithWaveFunctionCollapse();

            if (nonMainPathGroups.Count == 0)
            {
                break;
            }
        }
    }

    private bool IsThereAnotherPath(int[] posOfpoint, DirectionsOf goToDire, int steps)
    {
        // Diese Methode guckt nach ob eine Tile existiert die Keine Wand ist, und somit teil eiens Phades
        switch (goToDire)
        {
            case DirectionsOf.top:
                {
                    if ((posOfpoint[1] - steps) >= 0)
                    {
                        if (planeForWaveFuncCollapseAlgorithm[posOfpoint[0], (posOfpoint[1] - steps)][0].gangRaumWandInt
                        == GangRaumWand.wand)
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                    else { return false; }
                }
            case DirectionsOf.right:
                {
                    if ((posOfpoint[0] + steps) <= xMaxIndex)
                    {
                        if (planeForWaveFuncCollapseAlgorithm[(posOfpoint[0] + steps), posOfpoint[1]][0].gangRaumWandInt
                        == GangRaumWand.wand)
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                    else { return false; }
                }
            case DirectionsOf.down:
                {
                    if ((posOfpoint[1] + steps) <= yMaxIndex)
                    {
                        if (planeForWaveFuncCollapseAlgorithm[posOfpoint[0], (posOfpoint[1] + steps)][0].gangRaumWandInt
                        == GangRaumWand.wand)
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                    else { return false; }
                }
            case DirectionsOf.left:
                {
                    if ((posOfpoint[0] - steps) >= 0)
                    {
                        if (planeForWaveFuncCollapseAlgorithm[(posOfpoint[0] - steps), posOfpoint[1]][0].gangRaumWandInt
                        == GangRaumWand.wand)
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                    else { return false; }
                }
            default: return false;
        }
    }

    private void ConnectTilesAndGroups(int[] posOfpoint, DirectionsOf goToDire, int steps)
    {   //Diese MEthode sol eine Verbindung zwischen 2 Paths schaffen, indem von der TIle aus wo man weiss dass es
        //einen Phad in die Richtung gibt alle Wände dazwischen durch verbindungen austauscht.
        //Diese werden später dann eingsetzt durch erneutes durchlaufen der WFC Methode um diese Tiles collabieren zu lassen
        int x = 0;
        while (x <= steps)
        {
            switch (goToDire)
            {
                case DirectionsOf.top:
                    {
                        if (x == 0)
                        {
                            TileData startTile = planeForWaveFuncCollapseAlgorithm[posOfpoint[0], posOfpoint[1]][0];
                            UpdatePossibleTilesToConnectPaths(posOfpoint, new List<int> { 1, 2 }, new List<int> { startTile.right },
                            new List<int> { startTile.down }, new List<int> { startTile.left });
                        }
                        else if (x == steps)
                        {
                            TileData endTile = planeForWaveFuncCollapseAlgorithm[posOfpoint[0], (posOfpoint[1] - x)][0];
                            UpdatePossibleTilesToConnectPaths(new int[] { posOfpoint[0], (posOfpoint[1] - x) },
                            new List<int> { endTile.top },
                            new List<int> { endTile.right }, new List<int> { 1, 2 },
                            new List<int> { endTile.left });
                        }
                        else
                        {
                            UpdatePossibleTilesToConnectPaths(new int[] { posOfpoint[0], (posOfpoint[1] - x) },
                           new List<int> { 1, 2 },
                           new List<int> { 0 }, new List<int> { 1, 2 },
                           new List<int> { 0 });
                        }
                        break;
                    }
                case DirectionsOf.right:
                    {
                        if (x == 0)
                        {
                            TileData startTile = planeForWaveFuncCollapseAlgorithm[posOfpoint[0], posOfpoint[1]][0];
                            UpdatePossibleTilesToConnectPaths(posOfpoint, new List<int> { startTile.top }, new List<int> { 1, 2 },
                            new List<int> { startTile.down }, new List<int> { startTile.left });
                        }
                        else if (x == steps)
                        {
                            TileData endTile = planeForWaveFuncCollapseAlgorithm[(posOfpoint[0] + x), (posOfpoint[1])][0];
                            UpdatePossibleTilesToConnectPaths(new int[] { (posOfpoint[0] + x), (posOfpoint[1]) },
                            new List<int> { endTile.top },
                            new List<int> { endTile.right }, new List<int> { endTile.down },
                            new List<int> { 1, 2 });
                        }
                        else
                        {
                            UpdatePossibleTilesToConnectPaths(new int[] { (posOfpoint[0] + x), (posOfpoint[1]) },
                           new List<int> { 0 },
                           new List<int> { 1, 2 }, new List<int> { 0 },
                           new List<int> { 1, 2 });
                        }
                        break;
                    }
                case DirectionsOf.down:
                    {
                        if (x == 0)
                        {
                            TileData startTile = planeForWaveFuncCollapseAlgorithm[posOfpoint[0], posOfpoint[1]][0];
                            UpdatePossibleTilesToConnectPaths(posOfpoint, new List<int> { startTile.top }, new List<int> { startTile.right },
                            new List<int> { 1, 2 }, new List<int> { startTile.left });
                        }
                        else if (x == steps)
                        {
                            TileData endTile = planeForWaveFuncCollapseAlgorithm[posOfpoint[0], (posOfpoint[1] + x)][0];
                            UpdatePossibleTilesToConnectPaths(new int[] { posOfpoint[0], (posOfpoint[1] + x) },
                            new List<int> { 1, 2 },
                            new List<int> { endTile.right }, new List<int> { endTile.down },
                            new List<int> { endTile.left });
                        }
                        else
                        {
                            UpdatePossibleTilesToConnectPaths(new int[] { posOfpoint[0], (posOfpoint[1] + x) },
                           new List<int> { 1, 2 },
                           new List<int> { 0 }, new List<int> { 1, 2 },
                           new List<int> { 0 });
                        }
                        break;
                    }
                case DirectionsOf.left:
                    {
                        if (x == 0)
                        {
                            TileData startTile = planeForWaveFuncCollapseAlgorithm[posOfpoint[0], posOfpoint[1]][0];
                            UpdatePossibleTilesToConnectPaths(posOfpoint, new List<int> { startTile.top }, new List<int> { startTile.right },
                            new List<int> { startTile.down }, new List<int> { 1, 2 });
                        }
                        else if (x == steps)
                        {
                            TileData endTile = planeForWaveFuncCollapseAlgorithm[(posOfpoint[0] - x), (posOfpoint[1])][0];
                            UpdatePossibleTilesToConnectPaths(new int[] { (posOfpoint[0] - x), (posOfpoint[1]) },
                            new List<int> { endTile.top },
                            new List<int> { 1, 2 }, new List<int> { endTile.down },
                            new List<int> { endTile.left });
                        }
                        else
                        {
                            UpdatePossibleTilesToConnectPaths(new int[] { (posOfpoint[0] - x), (posOfpoint[1]) },
                           new List<int> { 0 },
                           new List<int> { 1, 2 }, new List<int> { 0 },
                           new List<int> { 1, 2 });
                        }
                        break;
                    }
                default: break;
            }
            x++;
        }

    }

    private void UpdatePossibleTilesToConnectPaths(int[] pos, List<int> topValids, List<int> rightValids,
    List<int> downValids, List<int> leftValids)
    {

        //Hier werden die einzelnen Tiles geupdated um verbindugn zus chaffen indem entlang der Richtung mögliche tiels eingefügt werden
        planeForWaveFuncCollapseAlgorithm[pos[0], pos[1]].Clear();

        foreach (TileData DOData in listOfAllDoungenObjects)
        {
            if (topValids.Contains(DOData.top) && rightValids.Contains(DOData.right) &&
            downValids.Contains(DOData.down) && leftValids.Contains(DOData.left)
            )
            {
                planeForWaveFuncCollapseAlgorithm[pos[0], pos[1]].Add(DOData);
            }
        }

        int ID = planeOfPathTileGroupIDs[pos[0], pos[1]];
        if (ID == 0)
        {
            mainPathGroup.groupTiles.Remove(pos);
        }
        else
        {
            foreach (PathTilesGroup ptGroup in nonMainPathGroups)
            {
                if (ptGroup.uniqueID == ID)
                {
                    ptGroup.groupTiles.Remove(pos);
                    break;
                }
            }
        }
        planeOfPathTileGroupIDs[pos[0], pos[1]] = 999;
        planeOfEntropys[pos[0], pos[1]] = planeForWaveFuncCollapseAlgorithm[pos[0], pos[1]].Count;

    }


    //Hiernach wieder Code der nicht für das Path Group Prinziep ist

    private void CollapseRandomLowestEntropy()
    {
        int entroPosListIndex = (int)Random.Range(0, lowestEntropysPositionList.Count);
        int[] chosenTile = lowestEntropysPositionList[entroPosListIndex];
        int totalWeight = 0;
        int weightType = 0;
        int weightGangRaumWand = 0;
        int cornerWeight = 0;
        Dictionary<int, TileData> tmpStoredDORepre = new Dictionary<int, TileData>();
        List<int> tmpListToChoseTile = new List<int>();

        foreach (TileData DORepre in planeForWaveFuncCollapseAlgorithm[chosenTile[0], chosenTile[1]])
        {
            cornerWeight = 1;
            switch (DORepre.doungenObjTypeInt)
            {
                case DoungenObjType.normal: { weightType = weightNormal; break; }
                case DoungenObjType.item: { weightType = weightItem; break; }
                case DoungenObjType.gegner: { weightType = weightEnemy; break; }
                case DoungenObjType.starkGegner: { weightType = weightEliteEnemy; break; }
                default: break;
            }
            switch (DORepre.gangRaumWandInt)
            {
                case GangRaumWand.gang: { weightGangRaumWand = weightGang; break; }
                case GangRaumWand.raum: { weightGangRaumWand = weightRaum; break; }
                case GangRaumWand.wand: { weightGangRaumWand = weightWand; break; }
                default: break;
            }
            cornerWeight = UseWeightOfCorners(DORepre);
            if ((weightType * weightGangRaumWand) > 0)
            {
                totalWeight += (weightType * weightGangRaumWand * cornerWeight);
                tmpStoredDORepre.Add(totalWeight, DORepre);
                tmpListToChoseTile.Add(totalWeight);
            }

        }
        int randomChoice = (int)Random.Range(0, (totalWeight + 1));
        foreach (int key in tmpListToChoseTile)
        {
            if (key >= randomChoice)
            {

                planeForWaveFuncCollapseAlgorithm[chosenTile[0], chosenTile[1]].Clear();
                planeForWaveFuncCollapseAlgorithm[chosenTile[0], chosenTile[1]].Add(tmpStoredDORepre[key]);
                lowestEntropysPositionList.RemoveAt(entroPosListIndex);
                planeOfEntropys[chosenTile[0], chosenTile[1]] = 1;
                if (modOfWFC == 3)
                {
                    AddTileToPathGroup(chosenTile[0], chosenTile[1]);
                }

                //Versuche Reihenfolge der Einträge
                orderOfEntrys.Enqueue(new int[2] { chosenTile[0], chosenTile[1] });

                break;
            }
        }
        UpdateNeigboursOfThis(chosenTile[0], chosenTile[1], 0);
    }


    private int UseWeightOfCorners(TileData DORepre)
    {
        int sumWeight = 0;
        int topCoW, rightCoW, downCoW, leftCoW;
        topCoW = rightCoW = downCoW = leftCoW = 1;

        switch (DORepre.top)
        {
            case 0: topCoW = weightWand; break;
            case 1: topCoW = weightGang; break;
            case 2: topCoW = weightRaum; break;
            default: break;
        }
        switch (DORepre.right)
        {
            case 0: rightCoW = weightWand; break;
            case 1: rightCoW = weightGang; break;
            case 2: rightCoW = weightRaum; break;
            default: break;
        }
        switch (DORepre.down)
        {
            case 0: downCoW = weightWand; break;
            case 1: downCoW = weightGang; break;
            case 2: downCoW = weightRaum; break;
            default: break;
        }
        switch (DORepre.left)
        {
            case 0: leftCoW = weightWand; break;
            case 1: leftCoW = weightGang; break;
            case 2: leftCoW = weightRaum; break;
            default: break;
        }
        sumWeight = ((topCoW + rightCoW + downCoW + leftCoW) / 16);
        if (sumWeight == 0)
        {
            sumWeight = 1;
        }

        return sumWeight;

    }


    private void FillEntropyPlane()
    {

        for (int y = 0; y <= yMaxIndex; y++)
        {
            for (int x = 0; x <= xMaxIndex; x++)
            {
                planeOfEntropys[x, y] = planeForWaveFuncCollapseAlgorithm[x, y].Count;
            }
        }
    }

    private TileData[,] SetLvLArrayWithWFCPlane(TileData[,] lvlArray)
    {
        //mache hier einsetzen der values von Plane in lvl Array
        for (int y = 0; y <= yMaxIndex; y++)
        {
            for (int x = 0; x <= xMaxIndex; x++)
            {
                lvlArray[x, y] = planeForWaveFuncCollapseAlgorithm[x, y][0];
            }
        }
        return lvlArray;
    }

    private void MakeEdgesWalls()
    {

        List<int> makeWall = new List<int>();
        makeWall.Add(1);
        makeWall.Add(2);
        for (int i = 0; i <= xMaxIndex; i++)
        {
            RemoveAllElementsThatDontFitAtPlace(i, 0, DirectionsOf.top, makeWall);
            RemoveAllElementsThatDontFitAtPlace(i, yMaxIndex, DirectionsOf.down, makeWall);
        }
        for (int i = 0; i <= yMaxIndex; i++)
        {
            RemoveAllElementsThatDontFitAtPlace(xMaxIndex, i, DirectionsOf.right, makeWall);
            RemoveAllElementsThatDontFitAtPlace(0, i, DirectionsOf.left, makeWall);
        }
    }



    // Legacy Code als noch ganze Gameobjects statt structs gespeichert wurden
    private List<GameObject> GetDeepCopyGameobjectList(List<GameObject> theList)
    {
        List<GameObject> newList = new List<GameObject>();
        GameObject tmpGO;
        foreach (GameObject item in theList)
        {
            tmpGO = Instantiate(item);
            newList.Add(tmpGO);
            Destroy(tmpGO);
        }
        return newList;
    }
    //Ende Legacy



    private void DebugTestMethod()
    {   //Legacy Methode. nicht mehr verwendbar

        Debug.Log("Der Startpunkt ist bei " + (startRoomCordinates[0]) + " " + (startRoomCordinates[1]));
        Debug.Log("Der Endpunkt ist bei " + (endRoomCordinates[0]) + " " + (endRoomCordinates[1]));
        Debug.Log("Das plane Array für start zu end path ist gehe hin");
        for (int z = 0; z < startToEndPathPlaneGoTo.GetLength(1); z++)
        {
            string zeile = "";
            for (int s = 0; s < startToEndPathPlaneGoTo.GetLength(0); s++)
            {
                zeile += "  " + startToEndPathPlaneGoTo[s, z] + " ";
            }
            Debug.Log(zeile);
        }

        Debug.Log("Das plane Array für start zu end path ist komme von");
        for (int z = 0; z < startToEndPathPlaneGoTo.GetLength(1); z++)
        {
            string zeile = "";
            for (int s = 0; s < startToEndPathPlaneGoTo.GetLength(0); s++)
            {
                zeile += "  " + startToEndPathPlaneCameFrom[s, z] + " ";
            }
            Debug.Log(zeile);
        }


        for (int y = 0; y <= yMaxIndex; y++)
        {
            for (int x = 0; x <= xMaxIndex; x++)
            {
                Debug.Log("ist x Reihe num" + y + " in spalte " + x + "und dort ist anzahl objecte" + planeForWaveFuncCollapseAlgorithm[x, y].Count + "entropy nummer ist in list da" + planeOfEntropys[x, y]);
            }
        }


        /*
                float xachse = -1.8f;
                float yachse = 0.75f;
                float xsteps = 0.4f;
                float ysteps = 0.5f;

                int count = 1;
                foreach (GameObject item in listOfAllGameObjects)
                {
                    Instantiate(item, new Vector3(xachse, yachse, 0.0f), item.transform.rotation);

                    xachse += xsteps;
                    if (xachse > 2.0f)
                    {
                        xachse = -1.8f;
                        yachse -= ysteps;
                    }
                    Debug.Log("Anzahl durchläufe ist gerade bei " + count);
                    count++;

                }

                int xx = 1;
                foreach (GameObject item in listOfAllGameObjects)
                {
                    Debug.Log("The rotation of item nubmer" + xx + "is" + item.transform.rotation);
                    xx++;
                }*/

    }











    //TRY OUT ALTERNATIVES


    private void UpdateNeigboursOfThisAfterRemove(int xindex, int yindex, DirectionsOf blockDire, bool[] changedDirections, bool[][] allSLotEdges)
    {
        List<int> topToRemove;
        List<int> rightToRemove;
        List<int> downToRemove;
        List<int> leftToRemove;

        bool[] topEdges = allSLotEdges[0];
        bool[] rightEdges = allSLotEdges[1];
        bool[] downEdges = allSLotEdges[2];
        bool[] leftEdges = allSLotEdges[3];



        if (changedDirections[0])
        {
            topToRemove = new List<int>();
            FillNeigbourRemoveLists(topEdges[1], topEdges[2], topEdges[0], topToRemove);

            if (yindex > 0 && blockDire != DirectionsOf.top)
            {
                bool[][] oldAllSLotEdges = returnSlotEdges(xindex, (yindex - 1));
                RemoveAllElementsThatDontFitAtPlace(xindex, (yindex - 1), DirectionsOf.down, topToRemove);
                bool[][] newAllSLotEdges = returnSlotEdges(xindex, (yindex - 1));
                bool[] newChangedDirects = GetChangedDirectionsOfEdges(oldAllSLotEdges, newAllSLotEdges);

                UpdateNeigboursOfThisAfterRemove(xindex, (yindex - 1), DirectionsOf.down, newChangedDirects, newAllSLotEdges);
            }
        }
        if (changedDirections[1])
        {
            rightToRemove = new List<int>();
            FillNeigbourRemoveLists(rightEdges[1], rightEdges[2], rightEdges[0], rightToRemove);
            if (xindex < xMaxIndex && blockDire != DirectionsOf.right)
            {
                bool[][] oldAllSLotEdges = returnSlotEdges((xindex + 1), yindex);
                RemoveAllElementsThatDontFitAtPlace((xindex + 1), yindex, DirectionsOf.left, rightToRemove);
                bool[][] newAllSLotEdges = returnSlotEdges((xindex + 1), yindex);
                bool[] newChangedDirects = GetChangedDirectionsOfEdges(oldAllSLotEdges, newAllSLotEdges);
                UpdateNeigboursOfThisAfterRemove((xindex + 1), yindex, DirectionsOf.left, newChangedDirects, newAllSLotEdges);
            }
        }
        if (changedDirections[2])
        {
            downToRemove = new List<int>();
            FillNeigbourRemoveLists(downEdges[1], downEdges[2], downEdges[0], downToRemove);
            if (yindex < yMaxIndex && blockDire != DirectionsOf.down)
            {
                bool[][] oldAllSLotEdges = returnSlotEdges(xindex, (yindex + 1));
                RemoveAllElementsThatDontFitAtPlace(xindex, (yindex + 1), DirectionsOf.top, downToRemove);
                bool[][] newAllSLotEdges = returnSlotEdges(xindex, (yindex + 1));
                bool[] newChangedDirects = GetChangedDirectionsOfEdges(oldAllSLotEdges, newAllSLotEdges);
                UpdateNeigboursOfThisAfterRemove(xindex, (yindex + 1), DirectionsOf.top, newChangedDirects, newAllSLotEdges);
            }
        }
        if (changedDirections[3])
        {
            leftToRemove = new List<int>();
            FillNeigbourRemoveLists(leftEdges[1], leftEdges[2], leftEdges[0], leftToRemove);
            if (xindex > 0 && blockDire != DirectionsOf.left)
            {
                bool[][] oldAllSLotEdges = returnSlotEdges((xindex - 1), yindex);
                RemoveAllElementsThatDontFitAtPlace((xindex - 1), yindex, DirectionsOf.right, leftToRemove);
                bool[][] newAllSLotEdges = returnSlotEdges((xindex - 1), yindex);
                bool[] newChangedDirects = GetChangedDirectionsOfEdges(oldAllSLotEdges, newAllSLotEdges);
                UpdateNeigboursOfThisAfterRemove((xindex - 1), yindex, DirectionsOf.right, newChangedDirects, newAllSLotEdges);
            }
        }
    }

    private bool[] GetChangedDirectionsOfEdges(bool[][] oldAllEdges, bool[][] newAllEdges)
    {
        bool[] changedDirects = new bool[4] { false, false, false, false };
        for (int i = 0; i < 4; i++)
        {
            if (oldAllEdges[i][0] != newAllEdges[i][0] || oldAllEdges[i][1] != newAllEdges[i][1] || oldAllEdges[i][2] != newAllEdges[i][2])
            {
                changedDirects[i] = true;
            }
        }

        return changedDirects;
    }
    private bool[][] returnSlotEdges(int xindex, int yindex)
    {

        bool topGang, rightGang, downGang, leftGang, topRaum, rightRaum, downRaum, leftRaum, topWand, rightWand, downWand, leftWand;
        topGang = rightGang = downGang = leftGang = topRaum = rightRaum = downRaum = leftRaum = topWand = rightWand = downWand = leftWand = false;

        foreach (TileData DORepresen in planeForWaveFuncCollapseAlgorithm[xindex, yindex])
        {
            if ((!(topGang && topRaum && topWand)))
            {
                switch (DORepresen.top)
                {
                    case 0: topWand = true; break;
                    case 1: topGang = true; break;
                    case 2: topRaum = true; break;
                    default: break;
                }
            }
            if ((!(rightGang && rightRaum && rightWand)))
            {
                switch (DORepresen.right)
                {
                    case 0: rightWand = true; break;
                    case 1: rightGang = true; break;
                    case 2: rightRaum = true; break;
                    default: break;
                }
            }
            if ((!(downGang && downRaum && downWand)))
            {
                switch (DORepresen.down)
                {
                    case 0: downWand = true; break;
                    case 1: downGang = true; break;
                    case 2: downRaum = true; break;
                    default: break;
                }
            }
            if ((!(leftGang && leftRaum && leftWand)))
            {
                switch (DORepresen.left)
                {
                    case 0: leftWand = true; break;
                    case 1: leftGang = true; break;
                    case 2: leftRaum = true; break;
                    default: break;
                }
            }
        }
        bool[] topEdges = new bool[] { topWand, topGang, topRaum };
        bool[] rightEdges = new bool[] { rightWand, rightGang, rightRaum };
        bool[] downEdges = new bool[] { downWand, downGang, downRaum };
        bool[] leftEdges = new bool[] { leftWand, leftGang, leftRaum };
        return new bool[][] { topEdges, rightEdges, downEdges, leftEdges };
    }


    public Queue<int[]> GetOrderOfEntry()
    {
        return orderOfEntrys;
    }

    public int[] GetStartRoomCoordi()
    {
        return startRoomCordinates;
    }

    public int[] GetEndRoomCoordi()
    {
        return endRoomCordinates;
    }

}