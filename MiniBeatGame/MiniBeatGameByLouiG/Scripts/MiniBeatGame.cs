using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;


namespace GameByLouiG
{
    public class MiniBeatGame : MonoBehaviour
    {
        #region Variable Field
        [SerializeField][Tooltip("譜面資訊\nMini game spectrm data")]
        private string musicSpectrum = "";

        [SerializeField][Tooltip("音樂BPM\nMusic's BPM(Beat per minture)")]
        private int musicBPM = 0;

        [SerializeField][Tooltip("節奏片段集合\nBeat part group")]
        private Transform beatGroup = null;

        [SerializeField][Tooltip("節奏片段Prefab\nBeat part prefab")]
        private Transform beatPrefab = null;

        [SerializeField][Tooltip("節奏按壓區塊顏色\nBeat color when game type are hold")]
        private Color beatHoldColor = Color.white;

        [SerializeField][Tooltip("節奏放開區塊顏色\nBeat color when game type are unhold")]
        private Color beatUnHoldColor = Color.white;

        [SerializeField][Tooltip("提示箭頭\nThe hint arrow")]
        private RectTransform arrow = null;

        [SerializeField][Tooltip("完美判定容許值(單位為毫秒)\nBeat prefect timing range(ms)")]
        private float perfectTiming = 0f;

        [SerializeField][Tooltip("普通判定容許值(單位為毫秒)\nBeat normal timing range(ms)")]
        private float normalTiming = 0f;

        [SerializeField][Tooltip("完美判定區塊顏色\nBeat perfect field color")]
        private Color perfectTimingColor = Color.white;

        [SerializeField][Tooltip("普通判定區塊顏色\nBeat normal field color")]
        private Color normalTimingColor = Color.white;

        [SerializeField][Tooltip("節拍完美/普通時間範圍顯示\n Beat's perfect or normal beat timing field show")]
        private bool beatTimingFieldShowing = false;

        /// <summary>
        /// Hint arrow angle(Rotation Z)
        /// </summary>
        private float arrowAngle = 0f;

        /// <summary>
        /// The min beat game spin time in one trun
        /// </summary>
        private float spinTime = 0f;

        /// <summary>
        /// The game time when game start
        /// Also used to check the beat state when input
        /// </summary>
        private float gameTime = 0f;

        /// <summary>
        /// Used to control the mini game spin or not
        /// </summary>
        private bool spin = false;

        /// <summary>
        /// The index of the beat timing
        /// It will add when player input or miss a beat timing
        /// </summary>
        private int beatIndex = 0;

        /// <summary>
        /// Game type
        /// Hold:
        /// In this mode player shold holding button(default-space)
        /// If up the key the system will check beat timing state 
        /// Unhold:
        /// In this mode player shold unHold button(default-space)
        /// If down the key the system will check beat timing state 
        /// </summary>
        private GameType gameType = GameType.Hold;

        private List<float> beatTiming = new List<float>();

        private bool[] beatInputStates = null;

        private int inputState = 999;

        private int perfectBeatCounter = 0;

        private int normalBeatCounter = 0;

        private int missCounter = 0;

        /// <summary>
        /// 0 Clockwise
        /// 1 Counterclockwise
        /// </summary>
        private int rotateDir = 0;

        /// <summary>
        /// Control the hint arrow rotate direction
        /// </summary>
        private int dir = 0;

        /// <summary>
        /// Contain all beat's time length
        /// </summary>
        private float[] timingList = null;

        /// <summary>
        /// Contain all beat's transform
        /// </summary>
        private Transform[] beatList = null;

        /// <summary>
        /// Contain all beat's angle
        /// </summary>
        private float[] angleList = null;

        /// <summary>
        /// When perfect timing effective, trigger perfect timing function
        /// </summary>
        private EventHandler perfectTimingTrigger;

        /// <summary>
        /// When normal timing effective, trigger normal timing function
        /// </summary>
        private EventHandler normalTimingTrigger;

        /// <summary>
        /// When miss timing effective, trigger miss timing function
        /// </summary>
        private EventHandler missTimingrigger;
        #endregion

        void Start()
        {
#if UNITY_EDITOR
            GameSetting();
#endif
        }

        void Update()
        {
            if (spin)
            {
                if (beatIndex < beatTiming.Count)
                {
                    if (gameType == GameType.Hold)
                    {
                        if (Input.GetKeyUp(KeyCode.Space))
                        {
                            inputState = TimingCheck(beatTiming[beatIndex]);
                            gameType = GameType.UnHold;
                        }
                    }
                    else if (gameType == GameType.UnHold)
                    {
                        if (Input.GetKeyDown(KeyCode.Space))
                        {
                            inputState = TimingCheck(beatTiming[beatIndex]);
                            gameType = GameType.Hold;
                        }
                    }

                    if (gameTime > beatTiming[beatIndex])
                    {
                        if (!beatInputStates[beatIndex])
                        {
                            //Debug.Log("Out Miss!"+ arrow.localRotation.eulerAngles.z+"\n"+ gameTime +" "+ beatTiming[beatIndex]);
                            missCounter++;
                            if (missTimingrigger != null)
                            {
                                missTimingrigger(this, EventArgs.Empty);
                            }

                            if (gameType == GameType.Hold)
                            { gameType = GameType.UnHold; }
                            else if (gameType == GameType.UnHold)
                            { gameType = GameType.Hold; }
                        }
                        beatIndex++;
                    }
                }


                gameTime += Time.deltaTime;

                arrowAngle += (360 / spinTime) / 60 * dir;
                arrow.localRotation = Quaternion.Euler(0, 0, arrowAngle);

                if (rotateDir == 0)
                {
                    if (arrowAngle <= -360)
                    {
                        arrow.localRotation = Quaternion.Euler(0, 0, -360);
                        spin = false;
                    }
                }
                else if (rotateDir == 1)
                {
                    if (arrowAngle >= 360)
                    {
                        arrow.localRotation = Quaternion.Euler(0, 0, 360);
                        spin = false;
                    }
                }
            }
        }

        public void GameSetting()
        {
            beatTiming.Clear();
            string[] spectrumData = musicSpectrum.Split('|');
            int singleBeatLength = 0;
            if (!int.TryParse(spectrumData[0], out singleBeatLength))
            {
                Debug.Log("Format error, please check your musicSpectrum are correct or not");
                return;
            }

            string beatData = spectrumData[1];

            float unitAngle = 0;

            float unitTime = 0;

            int beatCount = 0;
            for (int index = 0; index < beatData.Length; index++)
            {
                int data = 0;
                if (!int.TryParse(beatData[index].ToString(), out data))
                {
                    Debug.Log("Format error, please check your musicSpectrum are correct or not");
                    return;
                }
                else
                {
                    beatCount += data;
                }
            }


            beatInputStates = new bool[beatCount];
            timingList = new float[beatData.Length];
            beatList = new Transform[beatData.Length];
            angleList = new float[beatData.Length];
            for (int index = 0; index < beatInputStates.Length; ++index)
            {
                beatInputStates[index] = false;
            }

            unitAngle = 360 / beatCount;

            spinTime = ((float)60 / musicBPM) * singleBeatLength * beatCount;

            unitTime = spinTime / beatCount;

            float lastBeatAngle = 0f;
            bool hold = true;
            for (int index = 0; index < beatData.Length; ++index)
            {
                GameObject beatObj = Instantiate(beatPrefab.gameObject);
                beatObj.transform.SetParent(beatGroup);
                Image beatImg = beatObj.GetComponent<Image>();
                beatList[index] = beatObj.transform;

                int data = 0;
                if (!int.TryParse(beatData[index].ToString(), out data))
                {
                    Debug.Log("Format error, please check your musicSpectrum are correct or not");
                    return;
                }
                else
                {
                    float angle = unitAngle * data;
                    angleList[index] 
= angle;

                    if (hold)
                    {
                        beatImg.color = beatHoldColor;
                    }
                    else if (!hold)
                    {
                        beatImg.color = beatUnHoldColor;
                    }

                    hold = !hold;

                    beatImg.fillAmount = (angle / 360);
                    beatImg.rectTransform.localRotation = Quaternion.Euler(0, 0, lastBeatAngle);
                    beatImg.rectTransform.localPosition = Vector3.zero;
                    beatImg.rectTransform.localScale = Vector3.one;
                    lastBeatAngle += -angle;

                    float currentBeatTime = unitTime * data;
                    timingList[index] = currentBeatTime;
                }
            } 
        }

        #region GameControl
        #region GameStart
        public void GameStart()
        {
            spin = true;
            BeatTimingReady(beatList.Length, beatList, timingList, angleList);
        }

        public void GameStart(int _rotateDir)
        {
            spin = true;
            rotateDir = _rotateDir;
            if (_rotateDir == 0)
            {
                dir = -1;
            }
            else if (_rotateDir == 1)
            {
                dir = 1;
            }
            BeatTimingReady(beatList.Length, beatList, timingList, angleList);
        }
        #endregion

        public void ReSetGame()
        {
            for (int index = 0; index < beatGroup.childCount; ++index)
            {
                DestroyImmediate(beatGroup.GetChild(0).gameObject);
            }
            GameSetting();
            gameTime = 0;
            spin = false;
            beatIndex = 0;
            gameType = GameType.Hold;
            inputState = 999;
            arrow.localRotation = Quaternion.Euler(Vector3.zero);
            arrowAngle = 0;
            perfectBeatCounter = 0;
            normalBeatCounter = 0;
            missCounter = 0;
        }
        #endregion

        #region Data Get

        /// <summary>
        /// Get the current beat data, like perfectBeat count,
        /// normalBeat count and miss count
        /// It will return vec3Int value
        /// x : perfectBeat count
        /// y : normalBeat count
        /// z : miss count
        /// </summary>
        public Vector3Int GetBeatData()
        {
            Vector3Int beatData = new Vector3Int();
            beatData.x = perfectBeatCounter;
            beatData.y = normalBeatCounter;
            beatData.z = missCounter;

            return beatData;
        }

        public int GetBeatCount()
        {
            int beatCount = beatList.Length;
            return beatCount;
        }

        public bool GetSpinState()
        {
            return spin;
        }

        #endregion

        #region Data Set

        public void SetBeatMapData(string _mapData, int _BPM)
        {
            musicSpectrum = _mapData;
            musicBPM = _BPM;
        }

        public void SetBeatColor(Color _hold, Color _unHold
            , Color _perfectTiming, Color _normalTiming)
        {
            beatHoldColor = _hold;
            beatUnHoldColor = _unHold;
            perfectTimingColor = _perfectTiming;
            normalTimingColor = _normalTiming;
        }

        public void SetBeatColor(Color _hold, Color _unHold)
        {
            beatHoldColor = _hold;
            beatUnHoldColor = _unHold;
        }

        public void AddBeatTimingTriggerEvent(EventHandler _event, BeatTimingType beatTimingType)
        {
            switch(beatTimingType)
            {
                case BeatTimingType.Perfect:
                    perfectTimingTrigger += _event;
                    break;
                case BeatTimingType.Normal:
                    normalTimingTrigger += _event;
                    break;
                case BeatTimingType.Miss:
                    missTimingrigger += _event;
                    break;
            }
        }
        #endregion

        private int TimingCheck(float currentTiming)
        {
            if (beatInputStates[beatIndex]) { return 999; }
            beatInputStates[beatIndex] = true;
            Debug.Log("第" + (beatIndex + 1) + "組判斷時間: " + gameTime + " 當前節奏時間: " + currentTiming + "  完美容錯基準: " + (currentTiming - (perfectTiming / 1000)) + " 普通容錯基準: " + (currentTiming - (normalTiming / 1000)));
            // First, check perfect timing
            if (gameTime <= currentTiming &&
                gameTime >= (currentTiming - (perfectTiming / 1000)))
            {
                // Perfect
                Debug.Log("Perfet");
                perfectBeatCounter++;
                if (perfectTimingTrigger != null)
                {
                    perfectTimingTrigger(this, EventArgs.Empty);
                }
                return 1;
            }
            else if (gameTime < (currentTiming - (perfectTiming / 1000))
                && gameTime >= (currentTiming - (normalTiming / 1000)))
            {
                // Normal
                Debug.Log("Normal");
                normalBeatCounter++;
                if (normalTimingTrigger != null)
                {
                    normalTimingTrigger(this, EventArgs.Empty);
                }
                return 0;
            }
            else
            {
                // Miss
                Debug.Log("Miss");
                missCounter++;
                if (missTimingrigger != null)
                {
                    missTimingrigger(this, EventArgs.Empty);
                }
                return -1;
            }
        }

        /// <summary>
        /// Show the beat effective field and set the beat timing
        /// </summary>
        /// <param name="beatLength"></param>
        /// <param name="beats"></param>
        /// <param name="_timingList"></param>
        /// <param name="_angleList"></param>
        private void BeatTimingReady(int beatLength, Transform[] beats, float[] _timingList, float[] _angleList)
        {
            if (beatTimingFieldShowing)
            {
                // Show beat effective field
                for (int index = 0; index < beatLength; ++index)
                {
                    Transform beatObj = beats[index];
                    Image beatImg = beatObj.GetComponent<Image>();
                    float currentBeatTime = _timingList[index];
                    float angle = 0;
                    if (rotateDir == 0)
                    {
                        angle = _angleList[index];
                    }

                    GameObject normalRange = Instantiate(beatPrefab.gameObject);
                    normalRange.transform.SetParent(beatObj);
                    normalRange.name = "normalTiming";
                    Image normalImg = normalRange.GetComponent<Image>();
                    normalImg.color = normalTimingColor;
                    float normalScale = ((normalTiming / 1000) / currentBeatTime);
                    normalImg.fillAmount = beatImg.fillAmount * normalScale;
                    normalImg.rectTransform.localRotation = Quaternion.Euler(0, 0, -angle + (angle * normalScale));
                    normalImg.rectTransform.localPosition = Vector3.zero;
                    normalImg.rectTransform.localScale = Vector3.one;

                    GameObject perfectRange = Instantiate(beatPrefab.gameObject);
                    perfectRange.transform.SetParent(beatObj);
                    perfectRange.name = "perfectTiming";
                    Image perfectImg = perfectRange.GetComponent<Image>();
                    perfectImg.color = perfectTimingColor;
                    float perfectScale = ((perfectTiming / 1000) / currentBeatTime);
                    perfectImg.fillAmount = beatImg.fillAmount * perfectScale;
                    perfectImg.rectTransform.localRotation = Quaternion.Euler(0, 0, -angle + (angle * perfectScale));
                    perfectImg.rectTransform.localPosition = Vector3.zero;
                    perfectImg.rectTransform.localScale = Vector3.one;
                }
            }

            // Set the beat timing
            if (rotateDir == 0)
            {
                for (int index = 0; index < beatLength; ++index)
                {
                    if ((index - 1) >= 0)
                    {
                        beatTiming.Add(beatTiming[index - 1] + _timingList[index]);
                    }
                    else
                    {
                        beatTiming.Add(_timingList[index]);
                    }
                }
            }
            else if (rotateDir == 1)
            {
                for (int index = (beatLength -1); index >= 0; --index)
                {
                    if (index == (beatLength - 1))
                    {
                        beatTiming.Add(_timingList[index]);
                    }
                    else
                    {
                        beatTiming.Add(beatTiming[beatTiming.Count - 1] + _timingList[index]);
                    }
                }
            }
        }

        private void DeleteAllEventHandler()
        {
            perfectTimingTrigger = null;
            normalTimingTrigger = null;
            missTimingrigger = null;
        }

        private void OnDestroy()
        {
            DeleteAllEventHandler();
        }
    }


    public enum GameType
    {
        Hold = 0,
        UnHold
    }

    public enum BeatTimingType
    {
        Perfect = 0,
        Normal,
        Miss
    }
}
