using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RandomTowerDefense.Scene;

namespace RandomTowerDefense.Managers.System
{
    /// <summary>
    /// オーディオ管理システム - 背景音楽と効果音の管理
    ///
    /// 主な機能:
    /// - ゲーム状態に基づく動的BGM切り替え
    /// - プーリングによる効果音管理
    /// - 音量制御とミュート機能
    /// - シーン間でのオーディオ持続性
    /// - オーディオ設定のUI統合
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        #region Serialized Fields
        [Header("🎵 Audio Sources")]
        [SerializeField] private List<AudioClip> bgmSource;
        [SerializeField] private List<AudioClip> seSource;

        [Header("🎮 UI Controls")]
        [SerializeField] private List<Toggle> bgmUI;
        [SerializeField] private List<Toggle> seUI;

        [Header("📱 Scene References")]
        [SerializeField] private TitleOperation titleOperation;
        [SerializeField] private StageSelectOperation stageSelectOperation;
        [SerializeField] private InGameOperation inGameOperation;
        #endregion

        #region Private Fields
        private AudioSource[] _audioSource;
        private Dictionary<string, AudioClip> _bgmList;
        private Dictionary<string, AudioClip> _seList;
        private float _pausedPos = 0f;
        #endregion

        #region Public Properties
        [HideInInspector]
        public bool enabledBGM;
        [HideInInspector]
        public bool enabledSE;
        #endregion

        #region Public API
        public void EnableBGM(bool enable)
        {
            enabledBGM = enable;
            PlayerPrefs.SetInt("BGM", enabledBGM ? 1 : 0);
            foreach (Toggle i in bgmUI)
                i.isOn = enabledBGM;

            if (enabledBGM)
            {
                //audioSource[0].Play();
                //audioSource[0].UnPause();
                if (titleOperation) PlayAudio("bgm_Opening");
                else if (stageSelectOperation) PlayAudio("bgm_Title");
                else if (inGameOperation) PlayAudio("bgm_Battle");
            }
            else
            {
                _pausedPos = _audioSource[0].time;
                _audioSource[0].Pause();
                //audioSource[0].Stop();
            }
        }

        public void EnableSE(bool enable)
        {
            enabledSE = enable;
            PlayerPrefs.SetInt("SE", enabledSE ? 1 : 0);
            foreach (Toggle i in seUI)
                i.isOn = enabledSE;


            if (enabledSE)
            {
                _audioSource[1].Play();
            }
            else
            {
                _audioSource[1].Stop();
            }
        }

        private void OnEnable()
        {
            enabledBGM = PlayerPrefs.GetInt("BGM", 1) == 1;
            enabledSE = PlayerPrefs.GetInt("SE", 1) == 1;
        }

        private void OnDisable()
        {
            PlayerPrefs.SetInt("BGM", enabledBGM ? 1 : 0);
            PlayerPrefs.SetInt("SE", enabledSE ? 1 : 0);
        }

        public void StopBGM()
        {
            _audioSource[0].Stop();
        }

        private void Awake()
        {
            enabledBGM = PlayerPrefs.GetInt("BGM", 1) == 1;
            enabledSE = PlayerPrefs.GetInt("SE", 1) == 1;

            _audioSource = GetComponents<AudioSource>();

            _bgmList = new Dictionary<string, AudioClip>();
            _seList = new Dictionary<string, AudioClip>();

            int i = 0;
            _bgmList.Add("bgm_Battle", bgmSource[i++]);
            _bgmList.Add("bgm_Opening", bgmSource[i++]);
            _bgmList.Add("bgm_Title", bgmSource[i++]);

            i = 0;
            _seList.Add("se_Lighting", seSource[i++]);
            _seList.Add("se_Shot", seSource[i++]);
            _seList.Add("se_Snail", seSource[i++]);

            _seList.Add("se_MagicFire", seSource[i++]);
            _seList.Add("se_MagicBlizzard", seSource[i++]);
            _seList.Add("se_MagicPetrification", seSource[i++]);
            _seList.Add("se_MagicSummon", seSource[i++]);

            _seList.Add("se_Button", seSource[i++]);
            _seList.Add("se_Clear", seSource[i++]);
            _seList.Add("se_Lose", seSource[i++]);

            _seList.Add("se_Hitted", seSource[i++]);

            _seList.Add("se_Flame", seSource[i++]);

            _pausedPos = 0f;
        }

        private void Start()
        {
            foreach (Toggle j in bgmUI)
                j.isOn = enabledBGM;
            foreach (Toggle j in seUI)
                j.isOn = enabledSE;

            if (titleOperation) PlayAudio("bgm_Opening");
            else if (stageSelectOperation) PlayAudio("bgm_Title");
            else if (inGameOperation) PlayAudio("bgm_Battle");
        }
        void Record()
        {
            //if (audioSource == null) return;
            //audioSource[1].clip = Microphone.Start("Built-in Microphone", true, 10, 44100);
            //audioSource[1].Play();
        }

        void Release()
        {
            //foreach (var device in Microphone.devices)
            //{
            //    if (Microphone.IsRecording(device))
            //       Microphone.End(device);
            //}
        }

        public float[] GetClipWaveform(string clipname)
        {
            float[] data = new float[_bgmList[clipname].samples * _bgmList[clipname].channels];
            if (_bgmList[clipname].GetData(data, 0) == false)
            {
                _seList[clipname].GetData(data, 0);
            }
            //for (int i = 0; i*0.25f * 44100 < data.Length; i++)
            //    Debug.Log(data[(int)(i * 0.25f * 44100)]);
            return data;
        }

        float[] GetWaveform(string clipname)
        {
            //float[] data = new float[Microphone.GetPosition("Built-in Microphone")];
            //if (bgmList[clipname].GetData(data, 0) == false)
            //{
            //    seList[clipname].GetData(data, 0);
            //}
            //return data;
            return null;
        }

        public void PlayAudio(string clipname, bool isLoop = false)
        {
            if (_bgmList.ContainsKey(clipname))
            {
                if (enabledBGM == false) return;
                _audioSource[0].pitch = 1;
                _audioSource[0].loop = isLoop;
                _audioSource[0].clip = _bgmList[clipname];
                _audioSource[0].Play();

                if (_pausedPos != 0f)
                {
                    _audioSource[0].time = _pausedPos;
                }
            }
            else
            {
                if (enabledSE)
                {
                    _audioSource[1].PlayOneShot(_seList[clipname]);
                }
            }
        }

        public AudioClip GetAudio(string clipname)
        {
            if (_bgmList.ContainsKey(clipname))
            {
                return _bgmList[clipname];
            }
            else
            {
                return _seList[clipname];
            }
        }

        public void PlayReverseAudio(string clipname, bool isLoop = false)
        {
            if (_bgmList.ContainsKey(clipname))
            {
                if (enabledBGM == false) return;
                _audioSource[0].pitch = -1;
                _audioSource[0].clip = _bgmList[clipname];
                _audioSource[0].loop = isLoop;
                _audioSource[0].Play();
                StartCoroutine(StopLoop(isLoop, 0));
            }
            else
            {
                if (enabledSE == false) return;
                _audioSource[1].pitch = -1;
                _audioSource[1].clip = _seList[clipname];
                _audioSource[1].loop = isLoop;
                _audioSource[1].Play();
                StartCoroutine(StopLoop(isLoop, 1));
            }

        }

        public IEnumerator StopLoop(bool isLoop, int SourceID)
        {
            yield return new WaitForSeconds(1f);
            _audioSource[SourceID].loop = isLoop;
        }

        /// <summary>
        /// Set audio pitch for specified audio source
        /// </summary>
        /// <param name="pitch">Pitch value to set</param>
        /// <param name="SourceID">Audio source identifier</param>
        public void SetAudioPitch(float pitch, int SourceID)
        {
            _audioSource[SourceID].pitch = pitch;
        }
        #endregion
    }
}
