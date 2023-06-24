using MoonActive.Connect4;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;
using static Consts;
using PlayMode = Consts.PlayMode;

public class MoonScript : MonoBehaviour
{
    public GameObject resultContainer;
    public GameObject menuContainer;
    public GameObject spawnContainers;
    public GameObject spawnColliderParent;

    public Button restartButton;
    public Button menuButton;

    public Image computerBtnImage;
    public Image playerBtnImage;
    public Image computerWarBtnImage;
    
    public Color baseColor;

    public Text resultTitle;

    public ConnectGameGrid grid;
    public Disk diskAPrefab;
    public Disk diskBPrefab;

    public static int maxRows = 6;
    public static int maxCol = 7; // probably not needed
    public static int maxDisks = maxCol * maxRows;

    public List<History> history = new();
    [Range(0, 2)]
    public float clickWait = 0.2f;
    public float timer = 0.0f;

    public PlayMode mode = PlayMode.PVP;
    private bool _paused = false;

    public void GenBoardMeta()
    {
        history.Sort(History.CompareByColumn);
        History.DisplayBoard(history, maxCol, maxRows, col: false, row: true);
    }

    public void Spawn(int column, PlayerDisk playerDisk)
    {
        int rows = history.FindAll(columnClicked => columnClicked.column == column).Count;
        Disk playerDiskPrefab = playerDisk == PlayerDisk.diskA ? diskAPrefab : diskBPrefab;

        if (rows < maxRows)
        {
            IDisk diskInstance = grid.Spawn(playerDiskPrefab, column, rows);

            this.history.Add(new History(column, rows, playerDisk));
            Debug.Log($"Instantiated disk at {column}x{rows}");
            GenBoardMeta();
        }
        else
        {
            Debug.Log($"Can not instantiate a disk at {column}x{rows}\n (column {column} is full)");
        }

        string currentPlayerDisk = playerDisk == PlayerDisk.diskA ? "diskA" : "diskB";
        char playerSide = playerDisk == PlayerDisk.diskA ? '1' : '2';

        if (History.CheckWin(history, maxCol, maxRows, currentPlayerDisk))
        {
            _paused = true;
            Debug.Log($"Player {playerSide} Wins!");
            resultTitle.text = $"Player {playerSide} Wins!";
            resultContainer.gameObject.SetActive(true);
        };
    }
    void HandleColumnClick(int column)
    {


        if (timer > clickWait)
        {
            timer = 0;
            PlayerDisk playerDisk = history.Count % 2 == 0 ? PlayerDisk.diskB : PlayerDisk.diskA;

            Spawn(column, playerDisk);

            if (history.Count == maxDisks)
            {
                _paused = true;
                Debug.Log("ITS A DRAW");
                resultTitle.text = "DRAW";
                resultContainer.gameObject.SetActive(true);
                return;
            }

        }

        if (history.Count > 0)
        {
            restartButton.gameObject.SetActive(true);
            menuButton.gameObject.SetActive(false);
        }

    }
    public void ResetDisks()
    {
        Transform[] spawnPoints = spawnContainers.GetComponentsInChildren<Transform>().Where(child => child.CompareTag("SpawnPoint")).ToArray();
        BoxCollider2D[] colliders = spawnColliderParent.GetComponentsInChildren<BoxCollider2D>().Where(child => child.CompareTag("BoardCollider")).ToArray();

        for(int i=0; i< colliders.Length; i++)
        {
            colliders[i].enabled = false;
        }

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            Transform[] disks = spawnPoints[i].GetComponentsInChildren<Transform>();
            disks = disks.Where(child => child.tag == "Disk").ToArray();
            for (int j = 0; j < disks.Length; j++)
            {
                Destroy(disks[j].gameObject);
            }
        }
    }

    public void RestartGame()
    {
        ResetDisks();
        history.Clear();
        restartButton.gameObject.SetActive(false);
        menuButton.gameObject.SetActive(true);

    }

    public void CloseResult()
    {
        _paused = false;
        resultContainer.gameObject.SetActive(false);

        RestartGame();
    }

    public void ToggleMenu()
    {
        if (!menuContainer.gameObject.activeSelf) _paused = true;
        else _paused = false;
        menuContainer.gameObject.SetActive(!menuContainer.gameObject.activeSelf);
    }

    public void ChoosePlayerVSPlayer()
    {
        mode = PlayMode.PVP;
        computerWarBtnImage.color = baseColor;
        computerBtnImage.color = baseColor;
        playerBtnImage.color = Color.gray;
        RestartGame();
    }
    public void ChoosePlayerVSPC()
    {
        mode = PlayMode.PC;
        computerWarBtnImage.color = baseColor;
        playerBtnImage.color = baseColor;
        computerBtnImage.color = Color.gray;
        RestartGame();
    }

    public void ChoosePCvPC()
    {

        mode = PlayMode.PCvPC;
        playerBtnImage.color = baseColor;
        computerBtnImage.color = baseColor;
        computerWarBtnImage.color = Color.gray;
        RestartGame();
    }
    public void ConfirmAndPlay()
    {
        _paused = false;
        playerBtnImage.color = baseColor;
        computerBtnImage.color = baseColor;
        menuContainer.gameObject.SetActive(false);
    }

    void Start()
    {
        grid.ColumnClicked += HandleColumnClick;
        baseColor = Color.white;
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (!_paused && history.Count % 2 == 1  && timer >= clickWait)
        {
            if (mode == PlayMode.PC || mode == PlayMode.PCvPC)
            {
                timer = 0;
                Random r = new();
                int col = r.Next(0, maxRows);
                Spawn(col, PlayerDisk.diskA);
            }
        }

        if(!_paused && history.Count % 2 == 0 && mode == PlayMode.PCvPC && timer >= clickWait)
        {
            timer = 0;
            Random r = new();
            int col = r.Next(0, maxRows);
            Spawn(col, PlayerDisk.diskB);
        }

        if (history.Count == 0) RestartGame();
    }
}
