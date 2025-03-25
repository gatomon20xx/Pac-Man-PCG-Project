using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using PCC.CurationMethod;
using UnityEditor.PackageManager.UI;

public class GameManager : MonoBehaviour
{
    //--------------------------------------------------------
    // Game variables
    public Ghost[] ghosts;
    public Pacman pacman;
    public Transform pelletsTileMapTransform;

    public TMP_Text startLevelOverText;
    public TMP_Text gameOverText;
    public TMP_Text scoreText;
    public TMP_Text livesText;

    public GameObject ask4Pref_UI;

    public MapManager mapManager;
    
    // Player Preference
    private PlayerPrefs playerPrefs = new PlayerPrefs();
    private PCC.ContentRepresentation.Sample.Sample m_sample;
    // Player Preference

    public KeyCode keyCode_Love = KeyCode.G;
    public KeyCode keyCode_Hate = KeyCode.B;


    public int ghostMultiplier { get; private set; } = 1;
    public int score { get; private set; } = 0;
    //static public int score;
    public int lives { get; private set; } = 0;
    //public static int lives = 3;

    public int maxLives = 1;

    public static float time2Respawn = 1.5f;
    public static float time2LoadNewRound = 2.0f;

    public static int Level = 0;

    private void Awake()
    {
        // Load your pre-trained data here, if desired
    }

    private void Start()
    {
        //NewGame();

        ask4Pref_UI.SetActive(false);
        gameOverText.enabled = false;
        for (int i = 0; i < this.ghosts.Length; i++)
        {
            this.ghosts[i].gameObject.SetActive(false);
        }

        WaitForInput2StartNewLevel();
    }

    private void WaitForInput2StartNewLevel()
    {
        //StartCoroutine(nameof(Check2StartNewLevelCoRoutine));
        StartCoroutine(WaitForInput2StartNewLevelCoroutine());
    }

    private IEnumerator WaitForInput2StartNewLevelCoroutine()
    {
        bool startNewLevel = false;
        startLevelOverText.enabled = true;

        while (!startNewLevel)
        {
            //if (Input.GetKeyDown(KeyCode.R))
            //{
            //    //@Nancy
            //    //Want to reload scene
            //}
            //if (this.lives <= 0 && Input.anyKeyDown)
            //if (this.lives <= 0 && Input.GetKeyDown(KeyCode.Return))
            if (Input.GetKeyDown(KeyCode.Return))
            {
                startNewLevel = true;
                NewLevel();
            }
            yield return null;
        }

        startLevelOverText.enabled = false;
        yield return null;
    }


    //private void Update()
    //{
    //    Check2StartNewLevel();
    //}

    // Could add a function to draw the map! :D
    private void NewLevel()
    {
        if (playerPrefs.Lessons < 5)
        {
            m_sample = playerPrefs.GenerateASample(SampleGenerationMethod.RANDOM);
        }
        else
        {
            m_sample = playerPrefs.GenerateASample(SampleGenerationMethod.RANDOM_FROM_KNOWNS);
        }

        mapManager.GetNextLevel(m_sample);

        SetScoreUI(0);
        SetLivesUI(maxLives);
        
        NewRound();
    }

    //PCG: this function sets the pellets active
    // Could add to draw the pellets
    private void NewRound()
    {
        startLevelOverText.enabled = false;
        gameOverText.enabled = false;
        
        foreach (Transform pellet in this.pelletsTileMapTransform) 
        {
            pellet.gameObject.SetActive(true);
        }

        ResetState();
    }

    // For PLAYER/NPC objects
    private void ResetState()
    {
        ResetGhostMultiplier();
        for (int i = 0; i < this.ghosts.Length; i++)
        {
            this.ghosts[i].ResetState();
        }

        this.pacman.ResetState();
    }

    
    private void GameOver()
    {
        Debug.Log("GameOver");
        gameOverText.enabled = true;
        //startLevelOverText.enabled = true;

        for (int i = 0; i < this.ghosts.Length; i++)
        {
            this.ghosts[i].gameObject.SetActive(false);
        }

        this.pacman.gameObject.SetActive(false);

        TimeForNewLevel();
    }

    private void SetScoreUI(int score)
    {
        this.score = score;
        scoreText.text = score.ToString().PadLeft(2, '0');
    }

    private void SetLivesUI(int lives)
    {
        this.lives = lives;
        livesText.text = "x" + lives.ToString();
    }

    public void GhostEaten(Ghost ghost)
    {
        int points = ghost.points * ghostMultiplier;
        SetScoreUI(this.score + points);

        ghostMultiplier++;
    }


    public void PacmanEaten()
    {
        pacman.DeathSequence();
        //this.pacman.gameObject.SetActive(false);

        SetLivesUI(this.lives - 1);

        if (this.lives > 0)
        {
            Invoke(nameof(ResetState), time2Respawn);
            //ResetState();
        }
        else 
        {
            GameOver();
        }
        //SetScore(this.score + ghost.points);
    }

    //PATTERN, UPDATE: use events
    public void PelletEaten(Pellet pellet) 
    {
        pellet.gameObject.SetActive(false);

        SetScoreUI(this.score + pellet.points);

        if (!HasRemainingPellets()) 
        {
            this.pacman.gameObject.SetActive(false);

            TimeForNewLevel();
            //Invoke(nameof(NewRound), time2LoadNewRound);
        }
    }

    private void TimeForNewLevel()
    {
        StartCoroutine(nameof(TimeForNewLevelCoroutine));
    }

    IEnumerator TimeForNewLevelCoroutine()
    {
        ask4Pref_UI.SetActive(true);

        bool isPrefSet = false;

        float newPlayerPrefValue = 0;

        while (!isPrefSet)
        {
            Debug.Log("in");

            // Using if, else if on purpose --- can't make more than one selection!
            if (Input.GetKeyDown(keyCode_Love))
            {
                Debug.Log("like");
                newPlayerPrefValue = Ask4PrefValue.GetPrefValueFromKey(KeyCode.Alpha1);
                isPrefSet = true;
            }
            else if (Input.GetKeyDown(keyCode_Hate))
            {
                Debug.Log("dislike");
                newPlayerPrefValue = Ask4PrefValue.GetPrefValueFromKey(KeyCode.Alpha6);
                isPrefSet = true;
            }

            yield return null;
        }

        Debug.Log("out");

        playerPrefs.AssignPlayerPrefs(m_sample, newPlayerPrefValue);
        
        ask4Pref_UI.SetActive(false);
        gameOverText.enabled = false;
        mapManager.GetNextLevel(m_sample);
        WaitForInput2StartNewLevel();
    }

    //PATTERN, UPDATE: use events
    public void PowerPelletEaten(PowerPellet pellet)
    {
        for (int i = 0; i < ghosts.Length; i++)
        {
            ghosts[i].scared.Enable(pellet.duration);
        }

        PelletEaten(pellet);

        // In case eat another power pellet before the first timer is up.
        CancelInvoke();
        Invoke(nameof(ResetGhostMultiplier), pellet.duration);
    }

    private bool HasRemainingPellets()
    {
        foreach (Transform pellet in this.pelletsTileMapTransform) {
            if (pellet.gameObject.activeSelf)
                return true;
        }
        return false;
    }

    private void ResetGhostMultiplier()
    {
        this.ghostMultiplier = 1;
    }

}
