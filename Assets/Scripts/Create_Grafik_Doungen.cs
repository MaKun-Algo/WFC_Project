using System.Collections;
using System.Collections.Generic;
using MySelfmadeNamespace;
using UnityEngine;
using UnityEngine.UI;

public class Create_Grafik_Doungen : MonoBehaviour
{
    private Slider speedSlid;
    List<GameObject> instantiatedDO;
    Queue<GameObject> orderOFActiveGO;
    private IEnumerator setActivCorotine;
    private GameObject dummyObj;
    public void InitCreateGrafikDoung(Slider speedSlidAtri)
    {
        setActivCorotine= ShowTilesSpawn();
        instantiatedDO = new List<GameObject>();
        speedSlid = speedSlidAtri;
        orderOFActiveGO = new Queue<GameObject>();
    }

    //Legacy Code welcher Sofort stat angepasstem Speed ausfüllt
    /*public List<GameObject> CreateGrafikDoungen(GameObject[,] lvlArray, Camera mainCam)
    {

        int yAxis = lvlArray.GetLength(1);
        int xAxis = lvlArray.GetLength(0);
        float distance;
        float sizeMulti;
        float xAxisStart;
        float yAxisStart;
        //List<GameObject> instantiatedDO = new List<GameObject>();

        if ((float)xAxis <= (mainCam.aspect * (float)yAxis))
        {
            sizeMulti = (20.0f / (float)yAxis);
            distance = (0.1f * sizeMulti);
            yAxisStart = (1.0f - (distance / 2.0f));
            xAxisStart = 0.0f - ((((float)xAxis / 2.0f) * distance) - (distance / 2.0f));
        }
        else
        {
            sizeMulti = ((20.0f * mainCam.aspect) / (float)xAxis);
            distance = (0.1f * sizeMulti);
            xAxisStart = 0.0f - ((((float)xAxis / 2.0f) * distance) - (distance / 2.0f));
            yAxisStart = 0.0f + ((((float)yAxis / 2.0f) * distance) - (distance / 2.0f));
        }

        float currentXPos = xAxisStart;
        GameObject tmpGOPointer;

        for (int yIndex = 0; yIndex < yAxis; yIndex++)
        {
            currentXPos = xAxisStart;
            for (int xIndex = 0; xIndex < xAxis; xIndex++)
            {
                tmpGOPointer = Instantiate(lvlArray[xIndex, yIndex], new Vector3(currentXPos, yAxisStart, 0.0f), lvlArray[xIndex, yIndex].transform.rotation);
                tmpGOPointer.transform.localScale = new Vector3((sizeMulti * tmpGOPointer.transform.localScale.x),
                (tmpGOPointer.transform.localScale.y * sizeMulti), 1.0f);
                instantiatedDO.Add(tmpGOPointer);
                currentXPos += distance;
            }
            yAxisStart -= distance;
        }
        return instantiatedDO;
    }*/
    //Legacy vorbei



    //test
    public void ReihenfolgeCreateGrafikDoungen(TileData[,] lvlArray, Camera mainCam, Queue<int[]> orderOFEntrys,
    int[] startRoomCor, int[] endRoomCor)
    {
        GameObject[] startSign = Resources.LoadAll<GameObject>("Start_Sign");
        GameObject[] goalSign = Resources.LoadAll<GameObject>("Goal_Sign");
        int yAxis = lvlArray.GetLength(1);
        int xAxis = lvlArray.GetLength(0);
        float distance;
        float sizeMulti;
        float xAxisStart;
        float yAxisStart;
        //List<GameObject> instantiatedDO = new List<GameObject>();
        var tmpHolderWaitT = new GameObject();

        if ((float)xAxis <= (mainCam.aspect * (float)yAxis))
        {
            sizeMulti = (20.0f / (float)yAxis);
            distance = (0.1f * sizeMulti);
            yAxisStart = (1.0f - (distance / 2.0f));
            xAxisStart = 0.0f - ((((float)xAxis / 2.0f) * distance) - (distance / 2.0f));
        }
        else
        {
            sizeMulti = ((20.0f * mainCam.aspect) / (float)xAxis);
            distance = (0.1f * sizeMulti);
            xAxisStart = 0.0f - ((((float)xAxis / 2.0f) * distance) - (distance / 2.0f));
            yAxisStart = 0.0f + ((((float)yAxis / 2.0f) * distance) - (distance / 2.0f));
        }

        GameObject tmpGOPointer;
        



        while (orderOFEntrys.Count > 0)
        {
            int[] tmpArr = orderOFEntrys.Dequeue();
            tmpGOPointer = InstantiateAtPlace(lvlArray[tmpArr[0], tmpArr[1]], (xAxisStart + (distance * (float)tmpArr[0])),
            yAxisStart - (distance * (float)tmpArr[1]), sizeMulti);
            instantiatedDO.Add(tmpGOPointer);

            tmpGOPointer.SetActive(false);
            orderOFActiveGO.Enqueue(tmpGOPointer);

        }
        //add startsign in grafik
        instantiatedDO.Add(InstantiateStartEndAtPlace(startSign[0], (xAxisStart + (distance * (float)startRoomCor[0])),
        yAxisStart - (distance * (float)startRoomCor[1]), sizeMulti));
        //add Goal Sig in Plane
        instantiatedDO.Add(InstantiateStartEndAtPlace(goalSign[0], (xAxisStart + (distance * (float)endRoomCor[0])),
        yAxisStart - (distance * (float)endRoomCor[1]), sizeMulti));

        setActivCorotine=ShowTilesSpawn();
        StartCoroutine(setActivCorotine);

    }

    private GameObject InstantiateAtPlace(TileData toInstant, float xPos, float yPos, float sizeMulti)
    {

        GameObject tmpGOPointer = Instantiate(dummyObj,
        new Vector3(xPos, yPos, 0.0f), Quaternion.Euler(new Vector3(0, 0, toInstant.rotateEulerValue)));
        tmpGOPointer.GetComponent<SpriteRenderer>().sprite = toInstant.Sprite;
        tmpGOPointer.transform.localScale = new Vector3((sizeMulti * tmpGOPointer.transform.localScale.x),
        (tmpGOPointer.transform.localScale.y * sizeMulti), 1.0f);
        return tmpGOPointer;
    }

    private GameObject InstantiateStartEndAtPlace(GameObject toInstant, float xPos, float yPos, float sizeMulti)
    {

        GameObject tmpGOPointer = Instantiate(toInstant,
        new Vector3(xPos, yPos, 0.0f), toInstant.transform.rotation);
        tmpGOPointer.transform.localScale = new Vector3((sizeMulti * tmpGOPointer.transform.localScale.x),
        (tmpGOPointer.transform.localScale.y * sizeMulti), 1.0f);

        return tmpGOPointer;
    }

    private IEnumerator ShowTilesSpawn()
    {

        while (orderOFActiveGO.Count > 0)
        {
            if (orderOFActiveGO.Count == 0)
            {
                break;
            }
            orderOFActiveGO.Dequeue().SetActive(true);
            if (speedSlid.value >= 0)
            {
                yield return new WaitForSeconds(speedSlid.value);
            }
        }
    }

    public void DeleteAllGamObjects()
    {
        StopCoroutine(setActivCorotine);
        orderOFActiveGO.Clear();
        for (int i = (instantiatedDO.Count - 1); i >= 0; i--)
        {
            Destroy(instantiatedDO[i]);
        }
        instantiatedDO.Clear();
    }

    public void SetDummyObj(GameObject dummob){

        dummyObj= dummob;
    }

}


