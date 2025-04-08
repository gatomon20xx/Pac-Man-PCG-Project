using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;
using PCC.CurationMethod;
// using UnityEditor.PackageManager.UI;
using System;

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
    public GameObject ask4PowPref_UI;
    public GameObject ask4MapPref_UI;
    public GameObject ask4FruitPref_UI;

    bool isRandom = true;

    public MapManager mapManager;
    
    // Player Preference
    private PlayerPrefs playerPrefs = new PlayerPrefs();
    private PCC.ContentRepresentation.Sample.Sample m_sample;
    private PCC.ContentRepresentation.Sample.Sample p_sample;
    private PCC.ContentRepresentation.Sample.Sample f_sample;
    private PCC.ContentRepresentation.Sample.Sample pel_sample;
    // Player Preference

    private float fruitChance = 0.2f;

    public KeyCode keyCode_9 = KeyCode.Alpha9;
    public KeyCode keyCode_8 = KeyCode.Alpha8;
    public KeyCode keyCode_7 = KeyCode.Alpha7;
    public KeyCode keyCode_6 = KeyCode.Alpha6;
    public KeyCode keyCode_5 = KeyCode.Alpha5;
    public KeyCode keyCode_4 = KeyCode.Alpha4;
    public KeyCode keyCode_3 = KeyCode.Alpha3;
    public KeyCode keyCode_2 = KeyCode.Alpha2;
    public KeyCode keyCode_1 = KeyCode.Alpha1;
    public KeyCode keyCode_0 = KeyCode.Alpha0;


    public int ghostMultiplier { get; private set; } = 1;
    public int score { get; private set; } = 0;
    //static public int score;
    public int lives { get; private set; } = 0;
    //public static int lives = 3;

    public int maxLives = 1;

    bool firstDone = false;

    public static float time2Respawn = 1.5f;
    public static float time2LoadNewRound = 2.0f;

    public static int Level = 0;

    string filename = "RecordedResponses.txt";

    private void Awake()
    {
        // Load your pre-trained data here, if desired
        File.AppendAllText(filename, "This is " + isRandom.ToString() + Environment.NewLine);
    }

    private void Start()
    {
        //NewGame();

        ask4Pref_UI.SetActive(false);
        ask4PowPref_UI.SetActive(false);
        ask4MapPref_UI.SetActive(false);
        ask4FruitPref_UI.SetActive(false);
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
                if (firstDone == false)
                {
                    NewLevel();
                    firstDone = true;
                }
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                m_sample = playerPrefs.GenerateASample(SampleGenerationMethod.RANDOM);
                pel_sample = playerPrefs.GenerateASample(SampleGenerationMethod.RANDOM);
                p_sample = playerPrefs.GenerateASample(SampleGenerationMethod.RANDOM);
                f_sample = playerPrefs.GenerateASample(SampleGenerationMethod.RANDOM);
                TimeForNewLevel();
                firstDone = true;
            }
            yield return null;
        }

        NewRound();
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
        // Separate Samples used for maps.
        if (playerPrefs.Lessons < 5)
        {
            m_sample = playerPrefs.GenerateASample(SampleGenerationMethod.RANDOM);
            pel_sample = playerPrefs.GenerateASample(SampleGenerationMethod.RANDOM);
            p_sample = playerPrefs.GenerateASample(SampleGenerationMethod.RANDOM);
            f_sample = playerPrefs.GenerateASample(SampleGenerationMethod.RANDOM);
        }
        else
        {
            m_sample = playerPrefs.GenerateASample(SampleGenerationMethod.RANDOM_FROM_KNOWNS);
            pel_sample = playerPrefs.GenerateASample(SampleGenerationMethod.RANDOM_FROM_KNOWNS);
            p_sample = playerPrefs.GenerateASample(SampleGenerationMethod.RANDOM_FROM_KNOWNS);
            f_sample = playerPrefs.GenerateASample(SampleGenerationMethod.RANDOM_FROM_KNOWNS);
        }

        mapManager.GetNextLevel(m_sample, pel_sample, p_sample, isRandom);

        if (isRandom)
        {
            fruitChance = UnityEngine.Random.Range(0.01f, 0.15f);
        }
        else
        {
            fruitChance = f_sample.GetSampleValue("fruit_chance").Item2.floatVal;
        }

        SetScoreUI(0);
        SetLivesUI(maxLives);
        
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
        else
        {
            if(UnityEngine.Random.Range(0f, 1f) < fruitChance)
            {
                mapManager.addFruit();
                Debug.Log("fruit!");
            }
        }
    }

    //PATTERN, UPDATE: use events
    public void FruitEaten(Fruit fruit)
    {
        fruit.gameObject.SetActive(false);
        Debug.Log("eat!");

        SetScoreUI(this.score + fruit.points);
    }

    private void TimeForNewLevel()
    {
        StartCoroutine(nameof(TimeForNewLevelCoroutine));
    }

    IEnumerator TimeForNewLevelCoroutine()
    {
        ask4Pref_UI.SetActive(true);

        StreamWriter sr = null;

        // Create or open text file.
        if (!File.Exists(filename))
        {
            sr = File.CreateText(filename);
        }

        bool isPrefSet = false;
        bool isPelSet = false;
        bool isNextPrefSet = false;
        bool isFruitSet = false;

        float newPlayerPrefValue = 0;
        float newPowPrefValue = 0;
        float newMapPrefValue = 0;
        float newFruitPrefValue = 0.01f;

        while (!isPrefSet)
        {
            // Debug.Log("in");

            // Using if, else if on purpose --- can't make more than one selection!
            if (Input.GetKeyDown(keyCode_9))
            {
                Debug.Log("love");
                newPlayerPrefValue = Ask4PrefValue.GetPrefValueFromKey(KeyCode.Alpha9);
                isPrefSet = true;
            }
            else if (Input.GetKeyDown(keyCode_8))
            {
                Debug.Log("really love");
                newPlayerPrefValue = Ask4PrefValue.GetPrefValueFromKey(KeyCode.Alpha8);
                isPrefSet = true;
            }
            else if (Input.GetKeyDown(keyCode_7))
            {
                Debug.Log("like");
                newPlayerPrefValue = Ask4PrefValue.GetPrefValueFromKey(KeyCode.Alpha7);
                isPrefSet = true;
            }
            else if (Input.GetKeyDown(keyCode_6))
            {
                Debug.Log("alright");
                newPlayerPrefValue = Ask4PrefValue.GetPrefValueFromKey(KeyCode.Alpha6);
                isPrefSet = true;
            }
            else if (Input.GetKeyDown(keyCode_5))
            {
                Debug.Log("dislike");
                newPlayerPrefValue = Ask4PrefValue.GetPrefValueFromKey(KeyCode.Alpha5);
                isPrefSet = true;
            }
            else if (Input.GetKeyDown(keyCode_4))
            {
                Debug.Log("really love");
                newPlayerPrefValue = Ask4PrefValue.GetPrefValueFromKey(KeyCode.Alpha4);
                isPrefSet = true;
            }
            else if (Input.GetKeyDown(keyCode_3))
            {
                Debug.Log("like");
                newPlayerPrefValue = Ask4PrefValue.GetPrefValueFromKey(KeyCode.Alpha3);
                isPrefSet = true;
            }
            else if (Input.GetKeyDown(keyCode_2))
            {
                Debug.Log("alright");
                newPlayerPrefValue = Ask4PrefValue.GetPrefValueFromKey(KeyCode.Alpha2);
                isPrefSet = true;
            }
            else if (Input.GetKeyDown(keyCode_1))
            {
                Debug.Log("dislike");
                newPlayerPrefValue = Ask4PrefValue.GetPrefValueFromKey(KeyCode.Alpha1);
                isPrefSet = true;
            }
            else if (Input.GetKeyDown(keyCode_0))
            {
                Debug.Log("hate");
                newPlayerPrefValue = Ask4PrefValue.GetPrefValueFromKey(KeyCode.Alpha0);
                isPrefSet = true;
            }

            yield return null;
        }
        File.AppendAllText(filename, "Pellet Arrangement: " + newPlayerPrefValue.ToString() + Environment.NewLine);
        ask4Pref_UI.SetActive(false);
        ask4PowPref_UI.SetActive(true);
        while (!isPelSet)
        {
            Debug.Log("in");

            // Using if, else if on purpose --- can't make more than one selection!
            if (Input.GetKeyDown(keyCode_9))
            {
                Debug.Log("love");
                newPowPrefValue = Ask4PrefValue.GetPrefValueFromKey(KeyCode.Alpha9);
                isPelSet = true;
            }
            else if (Input.GetKeyDown(keyCode_8))
            {
                Debug.Log("really love");
                newPowPrefValue = Ask4PrefValue.GetPrefValueFromKey(KeyCode.Alpha8);
                isPelSet = true;
            }
            else if (Input.GetKeyDown(keyCode_7))
            {
                Debug.Log("like");
                newPowPrefValue = Ask4PrefValue.GetPrefValueFromKey(KeyCode.Alpha7);
                isPelSet = true;
            }
            else if (Input.GetKeyDown(keyCode_6))
            {
                Debug.Log("alright");
                newPowPrefValue = Ask4PrefValue.GetPrefValueFromKey(KeyCode.Alpha6);
                isPelSet = true;
            }
            else if (Input.GetKeyDown(keyCode_5))
            {
                Debug.Log("dislike");
                newPowPrefValue = Ask4PrefValue.GetPrefValueFromKey(KeyCode.Alpha5);
                isPelSet = true;
            }
            else if (Input.GetKeyDown(keyCode_4))
            {
                Debug.Log("really love");
                newPowPrefValue = Ask4PrefValue.GetPrefValueFromKey(KeyCode.Alpha4);
                isPelSet = true;
            }
            else if (Input.GetKeyDown(keyCode_3))
            {
                Debug.Log("like");
                newPowPrefValue = Ask4PrefValue.GetPrefValueFromKey(KeyCode.Alpha3);
                isPelSet = true;
            }
            else if (Input.GetKeyDown(keyCode_2))
            {
                Debug.Log("alright");
                newPowPrefValue = Ask4PrefValue.GetPrefValueFromKey(KeyCode.Alpha2);
                isPelSet = true;
            }
            else if (Input.GetKeyDown(keyCode_1))
            {
                Debug.Log("dislike");
                newPowPrefValue = Ask4PrefValue.GetPrefValueFromKey(KeyCode.Alpha1);
                isPelSet = true;
            }
            else if (Input.GetKeyDown(keyCode_0))
            {
                Debug.Log("hate");
                newPowPrefValue = Ask4PrefValue.GetPrefValueFromKey(KeyCode.Alpha0);
                isPelSet = true;
            }

            yield return null;
        }
        File.AppendAllText(filename, "Power Density: " + newPlayerPrefValue.ToString() + Environment.NewLine);
        ask4PowPref_UI.SetActive(false);
        ask4MapPref_UI.SetActive(true);
        while (!isNextPrefSet)
        {
            // Debug.Log("in");

            // Using if, else if on purpose --- can't make more than one selection!
            if (Input.GetKeyDown(keyCode_9))
            {
                Debug.Log("love");
                newMapPrefValue = Ask4PrefValue.GetPrefValueFromKey(KeyCode.Alpha9);
                isNextPrefSet = true;
            }
            else if (Input.GetKeyDown(keyCode_8))
            {
                Debug.Log("really love");
                newMapPrefValue = Ask4PrefValue.GetPrefValueFromKey(KeyCode.Alpha8);
                isNextPrefSet = true;
            }
            else if (Input.GetKeyDown(keyCode_7))
            {
                Debug.Log("like");
                newMapPrefValue = Ask4PrefValue.GetPrefValueFromKey(KeyCode.Alpha7);
                isNextPrefSet = true;
            }
            else if (Input.GetKeyDown(keyCode_6))
            {
                Debug.Log("alright");
                newMapPrefValue = Ask4PrefValue.GetPrefValueFromKey(KeyCode.Alpha6);
                isNextPrefSet = true;
            }
            else if (Input.GetKeyDown(keyCode_5))
            {
                Debug.Log("dislike");
                newMapPrefValue = Ask4PrefValue.GetPrefValueFromKey(KeyCode.Alpha5);
                isNextPrefSet = true;
            }
            else if (Input.GetKeyDown(keyCode_4))
            {
                Debug.Log("really love");
                newMapPrefValue = Ask4PrefValue.GetPrefValueFromKey(KeyCode.Alpha4);
                isNextPrefSet = true;
            }
            else if (Input.GetKeyDown(keyCode_3))
            {
                Debug.Log("like");
                newMapPrefValue = Ask4PrefValue.GetPrefValueFromKey(KeyCode.Alpha3);
                isNextPrefSet = true;
            }
            else if (Input.GetKeyDown(keyCode_2))
            {
                Debug.Log("alright");
                newMapPrefValue = Ask4PrefValue.GetPrefValueFromKey(KeyCode.Alpha2);
                isNextPrefSet = true;
            }
            else if (Input.GetKeyDown(keyCode_1))
            {
                Debug.Log("dislike");
                newMapPrefValue = Ask4PrefValue.GetPrefValueFromKey(KeyCode.Alpha1);
                isNextPrefSet = true;
            }
            else if (Input.GetKeyDown(keyCode_0))
            {
                Debug.Log("hate");
                newMapPrefValue = Ask4PrefValue.GetPrefValueFromKey(KeyCode.Alpha0);
                isNextPrefSet = true;
            }

            yield return null;
        }
        File.AppendAllText(filename, "Map Layout: " + newPlayerPrefValue.ToString() + Environment.NewLine);
        ask4MapPref_UI.SetActive(false);
        ask4FruitPref_UI.SetActive(true);
        while (!isFruitSet)
        {
            Debug.Log("in");

            // Using if, else if on purpose --- can't make more than one selection!
            if (Input.GetKeyDown(keyCode_9))
            {
                Debug.Log("love");
                newFruitPrefValue = Ask4PrefValue.GetPrefValueFromKey(KeyCode.Alpha9);
                isFruitSet = true;
            }
            else if (Input.GetKeyDown(keyCode_8))
            {
                Debug.Log("really love");
                newFruitPrefValue = Ask4PrefValue.GetPrefValueFromKey(KeyCode.Alpha8);
                isFruitSet = true;
            }
            else if (Input.GetKeyDown(keyCode_7))
            {
                Debug.Log("like");
                newFruitPrefValue = Ask4PrefValue.GetPrefValueFromKey(KeyCode.Alpha7);
                isFruitSet = true;
            }
            else if (Input.GetKeyDown(keyCode_6))
            {
                Debug.Log("alright");
                newFruitPrefValue = Ask4PrefValue.GetPrefValueFromKey(KeyCode.Alpha6);
                isFruitSet = true;
            }
            else if (Input.GetKeyDown(keyCode_5))
            {
                Debug.Log("dislike");
                newFruitPrefValue = Ask4PrefValue.GetPrefValueFromKey(KeyCode.Alpha5);
                isFruitSet = true;
            }
            else if (Input.GetKeyDown(keyCode_4))
            {
                Debug.Log("really love");
                newFruitPrefValue = Ask4PrefValue.GetPrefValueFromKey(KeyCode.Alpha4);
                isFruitSet = true;
            }
            else if (Input.GetKeyDown(keyCode_3))
            {
                Debug.Log("like");
                newFruitPrefValue = Ask4PrefValue.GetPrefValueFromKey(KeyCode.Alpha3);
                isFruitSet = true;
            }
            else if (Input.GetKeyDown(keyCode_2))
            {
                Debug.Log("alright");
                newFruitPrefValue = Ask4PrefValue.GetPrefValueFromKey(KeyCode.Alpha2);
                isFruitSet = true;
            }
            else if (Input.GetKeyDown(keyCode_1))
            {
                Debug.Log("dislike");
                newFruitPrefValue = Ask4PrefValue.GetPrefValueFromKey(KeyCode.Alpha1);
                isFruitSet = true;
            }
            else if (Input.GetKeyDown(keyCode_0))
            {
                Debug.Log("hate");
                newFruitPrefValue = Ask4PrefValue.GetPrefValueFromKey(KeyCode.Alpha0);
                isFruitSet = true;
            }

            yield return null;
        }

        File.AppendAllText(filename, "Fruit Frequency: " + newMapPrefValue.ToString() + Environment.NewLine);

        Debug.Log("out");

        playerPrefs.AssignPlayerPrefs(m_sample, newPlayerPrefValue);
        playerPrefs.AssignPlayerPrefs(pel_sample, newPowPrefValue);
        playerPrefs.AssignPlayerPrefs(p_sample, newMapPrefValue);
        playerPrefs.AssignPlayerPrefs(f_sample, newFruitPrefValue);
        
        ask4FruitPref_UI.SetActive(false);
        gameOverText.enabled = false;
        NewLevel();
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
