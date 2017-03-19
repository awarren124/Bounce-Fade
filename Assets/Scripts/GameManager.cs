﻿using UnityEngine;
using UnityEngine.UI;
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
    static int numOfBallsTrigger = 3;
    public static bool spawnSpecial = true;
    double specialFreq = 0.995f;
    static bool shrinkBall = false;
    public static UIManager ui;
    static bool gameOver = false;
    static bool readyToSpawn = true;
    public enum GameMode {Lives, KeepUp, Blind};
    public static GameMode gamemode = GameMode.Blind;
    public GameObject lostBallX;
    //    public b
    // Use this for initialization
    void Start() {
        ui = new UIManager(scoreLabel, livesLabel, panel, lostBallX);
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
    }

    // Update is called once per frame
    void FixedUpdate() {
        if (!gameOver)
        {
            ballTimer += Time.fixedDeltaTime;
            levelTimer += Time.fixedDeltaTime;
            switch (gamemode) {
                case GameMode.Lives:
                    if (Random.Range(0.0F, 1.0F) > specialFreq && spawnSpecial)
                    {
                        SpecialBall specialball = Instantiate(specialBall, ballDropPoint, Quaternion.identity);
                        spawnSpecial = false;
                    }

                    if (levelTimer > 4)
                    {
                        level++;
                        if (ballTimerTrigger > 0.3)
                        {
                            ballTimerTrigger -= 0.1f;
                            //numOfBallsTrigger += 1;
                        }
                        levelTimer = 0;
                    }

                    if (ballTimer > ballTimerTrigger)
                    {//&& numOfBalls < numOfBallsTrigger) { //Every second if there are less than 3 balls

                        //Make new ball
                        Ball instantiatedBall = Instantiate(ball, ballDropPoint, Quaternion.identity);
                        if (shrinkBall)
                        {
                            instantiatedBall.GetComponent<Animation>().Play("BallShrink");
                        }
                        ballTimer = 0;

                        numOfBalls++;
                    }

                    if (lives < 0)
                        startGameOver();
                    break;
                /*case GameMode.KeepUp:
                    if (score % 10 == 0 && readyToSpawn)
                    {
                        Ball instantiatedBall = Instantiate(ball, ballDropPoint, Quaternion.identity);
                        if (shrinkBall)
                            instantiatedBall.GetComponent<Animation>().Play("BallShrink");
                        numOfBalls++;
                        readyToSpawn = false;
                    }
                    if (lives < 0)
                        startGameOver();
                    break;*/
                case GameMode.Blind:
                    if (levelTimer > 4)
                    {
                        level++;
                        if (ballTimerTrigger > 0.3)
                        {
                            ballTimerTrigger -= 0.1f;
                            //numOfBallsTrigger += 1;
                        }
                        levelTimer = 0;
                    }

                    if (ballTimer > ballTimerTrigger)
                    {//&& numOfBalls < numOfBallsTrigger) { //Every second if there are less than 3 balls

                        //Make new ball
                        Ball instantiatedBall = Instantiate(ball, ballDropPoint, Quaternion.identity);
                        ballTimer = 0;

                        numOfBalls++;
                    }

                    float lerp = Mathf.PingPong(Time.time, 1.25F);
                    float alpha = Mathf.Lerp(1.25F, 0.0F, lerp);
                    foreach (GameObject ball in GameObject.FindGameObjectsWithTag("Ball"))
                    {
                        if (!ball.GetComponent<Ball>().stopStrobing)
                        {
                            Color color = ball.GetComponent<SpriteRenderer>().material.color;
                            color.a = alpha;
                            ball.GetComponent<SpriteRenderer>().material.color = color;
                        }
                    }

                    if (lives < 0)
                        startGameOver();
                    break;
            }
        }

    }

    public static void shrinkBalls() {
        GameObject[] balls = GameObject.FindGameObjectsWithTag("Ball");
        for (int i = 0; i < balls.Length; i++) {
            balls[i].GetComponent<Animation>().Play("BallShrink");
        }
        shrinkBall = true;
    }

    public static void expandBalls() {
        GameObject[] balls = GameObject.FindGameObjectsWithTag("Ball");
        for (int i = 0; i < balls.Length; i++) {
            balls[i].GetComponent<Animation>().Play("BallGrow");
        }
        shrinkBall = false;
    }

    public static void lostBall(Vector3 pos) {
        numOfBalls--;
        lives -= 1;
        print(pos);
        ui.showLostBallX(pos);
        ui.updateLives(lives);
    }

    public static void bouncedBall() {
        score += 1;
        readyToSpawn = true;
        ui.updateScore(score);
    }

    public void startGameOver() {
        gameOver = true;
        Time.timeScale = 1.0F;
        GameObject[] balls = GameObject.FindGameObjectsWithTag("Ball");
        for (int i = 0; i < balls.Length; i++) {
            balls[i].GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
            balls[i].GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        }

        ui.showGameOverPanel();
    }

    public static void restart() {
        ui.restart();
        ballTimerTrigger = 1.0F;
        ballTimer = 0.0F;
        GameObject[] balls = GameObject.FindGameObjectsWithTag("Ball");
        for (int i = 0; i < balls.Length; i++) {
            Ball b = balls[i].GetComponent<Ball>();
            b.gameOver();
        }

        try
        {
           SpecialBall sb = GameObject.FindGameObjectWithTag("SpecialBall").GetComponent<SpecialBall>();
            if (sb.active)
            {
                sb.shrink();
            }else
            {
                sb.GetComponent<Animation>().Play("GameOver");
                Destroy(sb, sb.GetComponent<Animation>().GetClip("GameOver").length);
            }
        }
        catch (System.Exception)
        {
        }
        
        //GameObject.FindGameObjectWithTag("SpecialBall");

        score = 0;
        lives = 3;
        ui.updateScore(score);
        ui.updateLives(lives);
        Time.timeScale = 1.0F;
        gameOver = false;
    }
}
