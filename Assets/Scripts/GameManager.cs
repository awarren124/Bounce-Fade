﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Advertisements;
using System.Collections;

public class GameManager : MonoBehaviour {

    public Text scoreLabel;
    public Text livesLabel;
    public GameObject panel;
    public Ball ball;
    public SpecialBall specialBall;
    public GameObject wall;
    public GameObject ballFallTriggerPrefab;
    public Vector2 ballDropPoint;
    static float ballTimer = 0.0f;
    float levelTimer = 0.0f;
    public static int numOfBalls = 0;
    public static int score = 0;
    public static int lives = 3;
    static int level = 1;
    static float ballTimerTrigger = 1.0f;
    public static bool spawnSpecial = false;
    double specialFreq = 0.995f;
    static bool shrinkBall = false;
    public static UIManager ui;
    public static bool gameOver = true;
    static bool readyToSpawn = true;
    public enum GameMode { Lives, KeepUp, Blind };
    public static GameMode gamemode = GameMode.Blind;
    public GameObject lostBallX;
    public GameObject titlePanel;
    public static bool isPaused = false;
    public static Vector2[] ballVelocities;
    public static Vector2 sbVelocity;
    public GameObject pausePanel;
    public Button pauseButton;
    public GameObject timerPanel;
    public static bool updateUiSBTimer = false;
    public Star star;
    float starFreq = 0.997F;
    public Text starsLabel;
    public GameObject shopPanel;
    public GameObject tutorial;
    public GameObject camera;
    public static int adCounter = 0;
    public GameObject otherButtons;
    public GameObject credits;
    //    public b
    // Use this for initialization
    void Start() {
        camera.GetComponent<Animation>().Play();

        if(!PlayerPrefs.HasKey("Stars"))
            PlayerPrefs.SetInt("Stars", 0);

        if(!PlayerPrefs.HasKey("ActiveSkin"))
            PlayerPrefs.SetInt("ActiveSkin", 0);

        if(!PlayerPrefs.HasKey("BlindHighScore"))
            PlayerPrefs.SetInt("BlindHighScore", 0);

        if(!PlayerPrefs.HasKey("LivesHighScore"))
            PlayerPrefs.SetInt("LivesHighScore", 0);

        if(!PlayerPrefs.HasKey("ShowTutorial"))
            PlayerPrefsX.SetBool("ShowTutorial", true);

        PlayerPrefs.Save();

        ui = new UIManager(scoreLabel, livesLabel, panel, lostBallX, titlePanel, pausePanel, pauseButton, timerPanel, starsLabel, shopPanel, tutorial, otherButtons, credits);
        Application.targetFrameRate = 60;

        //Level setup

        //Walls
        GameObject leftWall = Instantiate(wall,
                                          Camera.main.ScreenToWorldPoint(new Vector2(0, Screen.height / 2)), //Center at edge of screen
                                          Quaternion.identity) as GameObject;
        leftWall.transform.position += new Vector3(0, 0, 1); //Bring to front
        leftWall.transform.localScale = new Vector2(1, Camera.main.orthographicSize * 2); //Makes wall stretch to screen height

        GameObject rightWall = Instantiate(wall,
                                           Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height / 2)),
                                           Quaternion.identity) as GameObject;
        rightWall.transform.position += new Vector3(0, 0, 1);
        rightWall.transform.localScale = new Vector2(1, Camera.main.orthographicSize * 2);

        //Ball fall trigger
        GameObject fallTrigger = Instantiate(ballFallTriggerPrefab,
                                             new Vector2(Camera.main.ScreenToWorldPoint(new Vector2(Screen.width / 2, 0)).x, Camera.main.ScreenToWorldPoint(new Vector2(0, 0)).y - 1.5f),
                                             Quaternion.identity) as GameObject;
        fallTrigger.transform.localScale = new Vector2(Camera.main.orthographicSize * 2 * Screen.width / Screen.height,
                                                       1);

        if(PlayerPrefsX.GetBool("ShowTutorial")){
            showTutorial();
        }
    }

    // Update is called once per frame
    void FixedUpdate() {

        ui.updateStarsLabel();
        if(!gameOver && !isPaused) {
            if(updateUiSBTimer)
                ui.updateSpecialBallTimer(Time.fixedDeltaTime);
            ballTimer += Time.fixedDeltaTime;
            levelTimer += Time.fixedDeltaTime;
            switch(gamemode) {
                case GameMode.Lives:
                    if(Random.Range(0.0F, 1.0F) > specialFreq && spawnSpecial) {
                        SpecialBall specialball = Instantiate(specialBall, ballDropPoint, Quaternion.identity);
                        spawnSpecial = false;
                    }

                    if(Random.Range(0.0F, 1.0F) > starFreq){
                        Star instStar = Instantiate(star, new Vector2(Random.Range(-3, 3), ballDropPoint.y), Quaternion.identity);
                        instStar.GetComponent<Rigidbody2D>().AddTorque(Random.Range(-5.0F, 5.0F));
                    }

                    if(levelTimer > 0.4) {
                        level++;
                        if(ballTimerTrigger > 0.23) {
                            ballTimerTrigger -= 0.01f;
                            //numOfBallsTrigger += 1;
                        }
                        levelTimer = 0;
                    }

                    if(ballTimer > ballTimerTrigger) {//&& numOfBalls < numOfBallsTrigger) { //Every second if there are less than 3 balls

                        //Make new ball
                        Ball instantiatedBall = Instantiate(ball, ballDropPoint, Quaternion.identity);
                        if(shrinkBall) {
                            instantiatedBall.GetComponent<Animation>().Play("BallShrink");
                        }
                        ballTimer = 0;

                        numOfBalls++;
                    }

                    if(lives < 0)
                        startGameOver();
                    break;
                case GameMode.Blind:
                    if(levelTimer > 4) {
                        level++;
                        if(ballTimerTrigger > 0.3) {
                            ballTimerTrigger -= 0.1f;
                            //numOfBallsTrigger += 1;
                        }
                        levelTimer = 0;
                    }

                    if(Random.Range(0.0F, 1.0F) > starFreq){
                        Star instStar = Instantiate(star, new Vector2(Random.Range(-3, 3), ballDropPoint.y), Quaternion.identity);
                        instStar.GetComponent<Rigidbody2D>().AddTorque(Random.Range(-25.0F, 25.0F));
                    }

                    if(ballTimer > ballTimerTrigger) {//&& numOfBalls < numOfBallsTrigger) { //Every second if there are less than 3 balls

                        //Make new ball
                        Ball instantiatedBall = Instantiate(ball, ballDropPoint, Quaternion.identity);
                        ballTimer = 0;

                        numOfBalls++;
                    }

                    float fadeTime = 1.20F;

                    float lerp = Mathf.PingPong(Time.time, fadeTime);
                    float alpha = Mathf.Lerp(fadeTime, 0.0F, lerp);
                    foreach(GameObject ball in GameObject.FindGameObjectsWithTag("Ball")) {
                        if(!ball.GetComponent<Ball>().stopStrobing) {
                            Color color = ball.GetComponent<SpriteRenderer>().material.color;
                            color.a = alpha;
                            ball.GetComponent<SpriteRenderer>().material.color = color;
                        }
                    }

                    if(lives < 0)
                        startGameOver();
                    break;
            }
        }
    }

    public static void shrinkBalls() {
        GameObject[] balls = GameObject.FindGameObjectsWithTag("Ball");
        for(int i = 0; i < balls.Length; i++) {
            balls[i].GetComponent<Animation>().Play("BallShrink");
        }
        shrinkBall = true;
    }

    public static void expandBalls() {
        GameObject[] balls = GameObject.FindGameObjectsWithTag("Ball");
        for(int i = 0; i < balls.Length; i++) {
            balls[i].GetComponent<Animation>().Play("BallGrow");
        }
        shrinkBall = false;
    }

    public static void lostBall(Vector3 pos) {
        numOfBalls--;
        lives -= 1;
        ui.showLostBallX(pos);
        if(lives >= 0) {
            ui.updateLives(lives);
        }else {
            ui.updateLives(0);
        }
    }

    public static void bouncedBall() {
        score += 1;
        if(score == 25){
            spawnSpecial = true;
        }
        readyToSpawn = true;
        ui.updateScore(score);
    }

    public void startGameOver() {
        adCounter++;

        gameOver = true;
        string stringGm = "";
        if(gamemode == GameMode.Lives) {
            stringGm = "LivesHighScore";
            if(score > PlayerPrefs.GetInt("LivesHighScore")) {
                PlayerPrefs.SetInt("LivesHighScore", score);
                PlayerPrefs.Save();
            }
        }else if(gamemode == GameMode.Blind) {
            stringGm = "BlindHighScore";
            if(score > PlayerPrefs.GetInt("BlindHighScore")) {
                PlayerPrefs.SetInt("BlindHighScore", score);
                PlayerPrefs.Save();
            }
        }

        Time.timeScale = 1.0F;
        Time.fixedDeltaTime = Time.timeScale * 0.02F;
        GameObject[] balls = GameObject.FindGameObjectsWithTag("Ball");
        for(int i = 0; i < balls.Length; i++) {
            balls[i].GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
            balls[i].GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        }

        GameObject sb = GameObject.FindGameObjectWithTag("SpecialBall");
        if(sb != null) {
            print("NOT NULL");
            if(sb.GetComponent<SpecialBall>().expanded) {
                sb.GetComponent<SpecialBall>().shrink();
            } else if(sb.GetComponent<SpecialBall>().expanding) {
                Destroy(sb);
            } else {
                sb.gameObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
                sb.gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            }
        }
    
        GameObject[] stars = GameObject.FindGameObjectsWithTag("Star");

        if(stars.Length != 0) {
            foreach(GameObject star in stars) {
                star.GetComponent<Animation>().Play("StarGameOver");
                Destroy(star, star.GetComponent<Animation>()["StarGameOver"].length);
            }
        }

        ui.showGameOverPanel(stringGm);
    }

    public static void restart(bool notMenu, bool isPauseMenu, bool actualRestart) {
        print("hereee" + isPauseMenu);
        ui.restart(isPauseMenu, actualRestart);
        spawnSpecial = false;
        if(isPauseMenu)
            play(actualRestart);
        ballTimerTrigger = 1.0F;
        ballTimer = 0.0F;
        GameObject[] balls = GameObject.FindGameObjectsWithTag("Ball");
        for(int i = 0; i < balls.Length; i++) {
            Ball b = balls[i].GetComponent<Ball>();
            b.gameOver();
        }

        try {
            GameObject sb = GameObject.FindGameObjectWithTag("SpecialBall");
            if(sb.GetComponent<SpecialBall>().expanded) {
                sb.GetComponent<SpecialBall>().shrink();
            } else if(sb.GetComponent<SpecialBall>().expanding) {
                Destroy(sb);
            } else {
                sb.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
                sb.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                sb.GetComponent<Animation>().Play("GameOver");
                Destroy(sb, sb.GetComponent<Animation>().GetClip("GameOver").length);
            }
        } catch(System.Exception) {
        }

        GameObject[] stars = GameObject.FindGameObjectsWithTag("Star");

        if(stars.Length != 0) {
            foreach(GameObject star in stars) {
                star.GetComponent<Animation>().Play("StarGameOver");
                Destroy(star, star.GetComponent<Animation>()["StarGameOver"].length);
            }
        }
        //GameObject.FindGameObjectWithTag("SpecialBall");

        score = 0;
        lives = 3;
        ui.updateScore(score);
        ui.updateLives(lives);
        Time.timeScale = 1.0F;
        Time.fixedDeltaTime = Time.timeScale * 0.02F;
        gameOver = notMenu;
    }

    public static void startGame(GameMode gm) {
        ui.fadeOutTitle(true);
        Debug.Log("StartGAme");
        ui.showPauseButton();
        gamemode = gm;
        gameOver = false;
        ui.updateLives(3);
        ui.updateScore(0);
    }

    public static void goToMenu(bool isPause) {
        ui.menuIn(isPause, true);
        restart(true, isPause, false);
    }

    public static void pause() {
        ui.showPauseMenu();
        isPaused = true;
        GameObject[] balls = GameObject.FindGameObjectsWithTag("Ball");
        ballVelocities = new Vector2[balls.Length];
        for(int i = 0; i < balls.Length; i++) {
            balls[i].GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
            ballVelocities[i] = balls[i].GetComponent<Rigidbody2D>().velocity;
            if(ballVelocities[i].y > 0) {
                ballVelocities[i].y = -ballVelocities[i].y;
            }
            balls[i].GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        }
        GameObject sb = GameObject.FindGameObjectWithTag("SpecialBall");
        if(sb != null) {
            sb.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
            sbVelocity = sb.GetComponent<Rigidbody2D>().velocity;
            sb.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        }
        GameObject[] stars = GameObject.FindGameObjectsWithTag("Star");
        if(stars.Length != 0){
            foreach(GameObject star in stars){
                star.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
                star.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            }
        }
    }

    public static void play(bool isPauseRestart) {
        isPaused = false;
        updateUiSBTimer = false;
        //if(ui.pauseInFrame)
        //ui.hidePauseMenu(isPauseRestart);
        if(isPauseRestart) {
            Debug.Log("play(" + isPauseRestart + ")");
            ui.showPauseButton();
        }
            
        GameObject[] balls = GameObject.FindGameObjectsWithTag("Ball");
        for(int i = 0; i < balls.Length; i++) {
            if(!isPauseRestart) {
                balls[i].GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
                balls[i].GetComponent<Rigidbody2D>().velocity = ballVelocities[i];
            }
        }
        try {
            GameObject sb = GameObject.FindGameObjectWithTag("SpecialBall");
            if(sb != null && !sb.GetComponent<SpecialBall>().expanded) {
                sb.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
                sb.GetComponent<Rigidbody2D>().velocity = sbVelocity;
            }
            GameObject[] stars = GameObject.FindGameObjectsWithTag("Star");
            if(stars.Length != 0){
                foreach(GameObject star in stars){
                    star.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
                }
            }
        }catch(System.Exception){
        }
    }

    public static void showShop(){
        ui.fadeOutTitle(false);
        ui.showShop();
    }

    public static void showTutorial(){
        ui.tutorialIn();
        PlayerPrefsX.SetBool("ShowTutorial", false);
    }

    public static void tutorialEnd(){
        ui.tutorialOut();

    }

    public static void showAd() {
        if(Advertisement.IsReady()) {
            Advertisement.Show();
        }
    }

    public static void showRewardAd() {
        if(Advertisement.IsReady("rewardedVideo")) {
            var options = new ShowOptions { resultCallback = handleShowResult };
            Advertisement.Show("rewardedVideo", options);
        }
    }

    private static void handleShowResult(ShowResult result) {
        switch(result) {
            case ShowResult.Finished:
                Debug.Log("The ad was successfully shown.");
                PlayerPrefs.SetInt("Stars", PlayerPrefs.GetInt("Stars") + 20);
                break;
            case ShowResult.Skipped:
                Debug.Log("The ad was skipped before reaching the end.");
                break;
            case ShowResult.Failed:
                Debug.LogError("The ad failed to be shown.");
                break;
        }
    }

    public static void rate(){
        #if UNITY_IPHONE
        Application.OpenURL("itms-apps://itunes.apple.com/app/idXXXXXXXX");
        #elif UNITY_ANDROID
        Application.OpenURL("market://details?id=XXXXXXXX");
        #endif
    }

    public static void showCredits() {
        ui.showCredits();
    }

    public static void hideCredits() {
        ui.hideCredits();
    }
}
