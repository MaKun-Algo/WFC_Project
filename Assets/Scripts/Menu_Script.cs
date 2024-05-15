using System.Collections;
using System.Collections.Generic;
using MySelfmadeNamespace;
using UnityEngine;
using UnityEngine.UI;

public class Menu_Script : MonoBehaviour
{
    [SerializeField] private Slider gaengeSlid;
    [SerializeField] private Slider raeumeSlid;
    [SerializeField] private Slider waendeSlid;
    [SerializeField] private Slider normalSlid;
    [SerializeField] private Slider itemSlid;
    [SerializeField] private Slider enemySlid;
    [SerializeField] private Slider eliteEnemySlid;

    [SerializeField] private Button createDoungen;
    [SerializeField] private Button help;
    [SerializeField] private Button quit;
    [SerializeField] private TMPro.TMP_InputField xAchseInput;
    [SerializeField] private TMPro.TMP_InputField yAchseInput;
    [SerializeField] private Toggle hideUIToggle;
    [SerializeField] private GameObject uIPanelMenu;
    [SerializeField] private GameObject uIPanelTutorial;
    [SerializeField] private GameObject uIPanelWFC;
    [SerializeField] private GameObject dummyObj;
    [SerializeField] private Camera mainCam;
    [SerializeField] private Slider speedSlid;
    [SerializeField] private float zoomSpeed = 0.25f;
    //private List<GameObject> instantiatedGOs;
    private Make_Doungen doungenMaker;
    private Create_Grafik_Doungen grafikMaker;
    private TileData[,] lvlArray;
    private int wFCType;
    private Vector3 cameraPos;
    // Start is called before the first frame update

    void Start()
    {
        //instantiatedGOs = new List<GameObject>();
        //doungenMaker = new Make_Doungen();
        var tmpHolderDoungenMaker = new GameObject();
        doungenMaker = tmpHolderDoungenMaker.AddComponent<Make_Doungen>();

        var tmpHolderCreateGraf = new GameObject();
        grafikMaker = tmpHolderCreateGraf.AddComponent<Create_Grafik_Doungen>();
        grafikMaker.InitCreateGrafikDoung(speedSlid);
        wFCType = 3;
        cameraPos = Camera.main.transform.position;
    }

    void Update()
    {
        Camera.main.orthographicSize -= Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
        if (Camera.main.orthographicSize < 0.003)
            Camera.main.orthographicSize = 0.003f;
        else if (Camera.main.orthographicSize > 2)
            Camera.main.orthographicSize = 2;

        if (Input.GetKey(KeyCode.W))
        {
            cameraPos.y += (zoomSpeed / 10 * Camera.main.orthographicSize);
            if (cameraPos.y > 1)
            {
                cameraPos.y = 1;
            }
            Camera.main.transform.position = cameraPos;
        }
        if (Input.GetKey(KeyCode.S))
        {
            cameraPos.y -= (zoomSpeed / 10 * Camera.main.orthographicSize);
            if (cameraPos.y < -1)
            {
                cameraPos.y = -1;
            }
            Camera.main.transform.position = cameraPos;
        }
        if (Input.GetKey(KeyCode.D))
        {
            cameraPos.x += (zoomSpeed / 10 * Camera.main.orthographicSize);
            if (cameraPos.x > 1)
            {
                cameraPos.x = 1;
            }
            Camera.main.transform.position = cameraPos;
        }
        if (Input.GetKey(KeyCode.A))
        {
            cameraPos.x -= (zoomSpeed / 10 * Camera.main.orthographicSize);
            if (cameraPos.x < -1)
            {
                cameraPos.x = -1;
            }
            Camera.main.transform.position = cameraPos;
        }


    }

    //Legacy Methode die das lvl augenblicklisch eingsetzt hat statt mit bestimmbarem Speed
    /* public void CreateDoungenButton()
     {
         int xValue = int.Parse(xAchseInput.text);
         int yValue = int.Parse(yAchseInput.text);

         if (xValue < 2)
             xValue = 2;
         if (yValue < 2)
             yValue = 2;

         if (instantiatedGOs.Count > 0)
         {
             foreach (GameObject gOtoDelete in instantiatedGOs)
             {
                 Destroy(gOtoDelete);
             }
             instantiatedGOs.Clear();
         }

         lvlArray = doungenMaker.CreateLevel(xValue, yValue,
         new int[] { (int)gaengeSlid.value, (int)raeumeSlid.value, (int)waendeSlid.value },
         new int[] { (int)normalSlid.value, (int)itemSlid.value, (int)enemySlid.value, (int)eliteEnemySlid.value });
         instantiatedGOs = grafikMaker.CreateGrafikDoungen(lvlArray, mainCam);
         hideUIToggle.isOn = !hideUIToggle.isOn;


         Debug.Log("Alles Super");
     }*/
    //Legacy vorbei

    public void CreateDoungenInOrderButton()
    {
        cameraPos.y = 0;
        cameraPos.x = 0;
        Camera.main.transform.position = cameraPos;
        Camera.main.orthographicSize = 1;
        int xValue = int.Parse(xAchseInput.text);
        int yValue = int.Parse(yAchseInput.text);

        if (xValue < 2)
            xValue = 2;
        if (yValue < 2)
            yValue = 2;

        /*if (instantiatedGOs.Count > 0)
        {
            foreach (GameObject gOtoDelete in instantiatedGOs)
            {
                Destroy(gOtoDelete);
            }
            instantiatedGOs.Clear();
        }*/
        grafikMaker.DeleteAllGamObjects();
        grafikMaker.SetDummyObj(dummyObj);
        lvlArray = doungenMaker.CreateLevel(xValue, yValue,
        new int[] { (int)gaengeSlid.value, (int)raeumeSlid.value, (int)waendeSlid.value },
        new int[] { (int)normalSlid.value, (int)itemSlid.value, (int)enemySlid.value, (int)eliteEnemySlid.value }, wFCType);
        hideUIToggle.isOn = !hideUIToggle.isOn;
        grafikMaker.ReihenfolgeCreateGrafikDoungen(lvlArray, mainCam,
        doungenMaker.GetOrderOfEntry(), doungenMaker.GetStartRoomCoordi(), doungenMaker.GetEndRoomCoordi());

    }


    public void ToggleHideUI(bool value)
    {
        uIPanelMenu.SetActive(!value);
    }

    public void OpenTutorialPanel()
    {
        uIPanelMenu.SetActive(false);
        uIPanelTutorial.SetActive(true);
    }

    public void CloseTutorialPanel()
    {
        uIPanelMenu.SetActive(true);
        uIPanelTutorial.SetActive(false);
    }

    public void OpenWFCPanel()
    {
        uIPanelMenu.SetActive(false);
        uIPanelWFC.SetActive(true);
    }

    public void CloseWFCPanel()
    {
        uIPanelMenu.SetActive(true);
        uIPanelWFC.SetActive(false);
    }
    public void ToggleWFCNormal(bool value)
    {
        wFCType = 0;
    }
    public void ToggleWFCNoEdge(bool value)
    {
        wFCType = 1;
    }
    public void ToggleWFCPathVirus(bool value)
    {
        wFCType = 2;

    }
    public void ToggleWFCGroupCollapse(bool value)
    {
        wFCType = 3;
    }

    public void QuitProgram()
    {
        Application.Quit();
    }


}
