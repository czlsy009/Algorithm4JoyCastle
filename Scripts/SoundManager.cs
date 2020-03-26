using UnityEngine;
using System.Collections;

namespace CommonManagers
{
    /// <summary>
    /// 音频音效控制，包括音乐、音效的播放、暂停及静音等设置
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        [System.Serializable]
        public class Sound
        {
            public AudioClip clip;
            [HideInInspector]
            public int simultaneousPlayCount = 0;
        }

        [Header("最多允许同时播放的声音数量")]
        public int maxSimultaneousSounds = 7;

        //音频素材
        public Sound BackgroundMusic;
        public Sound ButtonSound;
        public Sound CoinSound;
        public Sound GameOverSound;
        public Sound TickSound;
        public Sound RewardedSound;
        public Sound UnlockSound;
        public Sound getPoint;

        /// <summary>
        /// 静音状态变化时触发委托
        /// </summary>
        /// <param name="isMuted"></param>
        public delegate void OnMuteStatusChanged(bool isMuted);

        public static event OnMuteStatusChanged MuteStatusChanged;
        
        /// <summary>
        /// 背景音乐开关状态变化时触发委托
        /// </summary>
        /// <param name="isOn"></param>
        public delegate void OnMusicStatusChanged(bool isOn);

        public static event OnMusicStatusChanged MusicStatusChanged;

        /// <summary>
        /// 播放状态
        /// </summary>
        enum PlayingState
        {
            Playing,
            Paused,
            Stopped
        }
        /// <summary>
        /// 音源
        /// </summary>
        public AudioSource AudioSource
        {
            get
            {
                if (_audioSource == null)
                {
                    _audioSource = GetComponent<AudioSource>();
                }

                return _audioSource;
            }
        }

        private AudioSource _audioSource;
        private PlayingState musicState = PlayingState.Stopped;
        /// <summary>
        /// 静音设置项存储 Key
        /// </summary>
        private const string MUTE_PREF_KEY = "MutePreference";
        private const int MUTED = 1;
        private const int UN_MUTED = 0;
        /// <summary>
        /// 背景音乐设置项存储 Key
        /// </summary>
        private const string MUSIC_PREF_KEY = "MusicPreference";

        private const int MUSIC_OFF = 0;
        private const int MUSIC_ON = 1;

        void Awake()
        {
            if (Instance)
            {
                DestroyImmediate(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        void Start()
        {
            // 按照存储进行设置
            SetMute(IsMuted());
        }

        /// <summary>
        /// 播放指定音效，可根据同时播放的同一音频数量自动控制音量，防止音量过大或失真。
        /// </summary>
        /// <param name="sound">要播放的音效</param>
        /// <param name="autoScaleVolume">为<c>true</c>时，自动根据播放数量控制音量大小 </param>
        /// <param name="maxVolumeScale">允许的最大音量</param>
        public void PlaySound(Sound sound, bool autoScaleVolume = true, float maxVolumeScale = 1f)
        {
            StartCoroutine(CRPlaySound(sound, autoScaleVolume, maxVolumeScale));
        }

        IEnumerator CRPlaySound(Sound sound, bool autoScaleVolume = true, float maxVolumeScale = 1f)
        {
            if (sound.simultaneousPlayCount >= maxSimultaneousSounds)
            {
                yield break;
            }

            sound.simultaneousPlayCount++;

            float vol = maxVolumeScale;

            // 根据同播数量控制音量
            if (autoScaleVolume && sound.simultaneousPlayCount > 0)
            {
                vol = vol / (float)(sound.simultaneousPlayCount);
            }

            AudioSource.PlayOneShot(sound.clip, vol);

            // 音效接近播完时减少同播数
            float delay = sound.clip.length * 0.7f;

            yield return new WaitForSeconds(delay);

            sound.simultaneousPlayCount--;
        }

        /// <summary>
        /// 播放指定音乐
        /// </summary>
        /// <param name="music">音乐</param>
        /// <param name="loop">设为<c>true</c>则循环播放</param>
        public void PlayMusic(Sound music, bool loop = true)
        {
            if (IsMusicOff())
            {
                return;
            }

            AudioSource.clip = music.clip;
            AudioSource.loop = loop;
            AudioSource.Play();
            musicState = PlayingState.Playing;
        }

        /// <summary>
        /// 暂停音乐
        /// </summary>
        public void PauseMusic()
        {
            if (musicState == PlayingState.Playing)
            {
                AudioSource.Pause();
                musicState = PlayingState.Paused;
            }    
        }

        /// <summary>
        /// 继续播放音乐
        /// </summary>
        public void ResumeMusic()
        {
            if (musicState == PlayingState.Paused)
            {
                AudioSource.UnPause();
                musicState = PlayingState.Playing;
            }
        }

        /// <summary>
        /// 停止播放音乐
        /// </summary>
        public void StopMusic()
        {
            AudioSource.Stop();
            musicState = PlayingState.Stopped;
        }

        /// <summary>
        /// 获取是否设置为静音
        /// </summary>
        /// <returns>若为<c>true</c>则静音，<c>false</c>反之.</returns>
        public bool IsMuted()
        {
            return (PlayerPrefs.GetInt(MUTE_PREF_KEY, UN_MUTED) == MUTED);
        }

        public bool IsMusicOff()
        {
            return (PlayerPrefs.GetInt(MUSIC_PREF_KEY, MUSIC_ON) == MUSIC_OFF);
        }

        /// <summary>
        /// 切换音效开关
        /// </summary>
        public void ToggleMute()
        {
            // Toggle current mute status
            bool mute = !IsMuted();

            if (mute)
            {
                // 静音
                PlayerPrefs.SetInt(MUTE_PREF_KEY, MUTED);

                if (MuteStatusChanged != null)
                {
                    MuteStatusChanged(true);
                }
            }
            else
            {
                // 开启音效
                PlayerPrefs.SetInt(MUTE_PREF_KEY, UN_MUTED);

                if (MuteStatusChanged != null)
                {
                    MuteStatusChanged(false);
                }
            }
            PlayerPrefs.Save();
            SetMute(mute);
        }

        /// <summary>
        /// 切换音乐静音开关
        /// </summary>
        public void ToggleMusic()
        {
            if (IsMusicOff())
            {
                // 开启音乐
                PlayerPrefs.SetInt(MUSIC_PREF_KEY, MUSIC_ON);
                if (musicState == PlayingState.Paused)
                {
                    ResumeMusic();
                }

                if (MusicStatusChanged != null)
                {
                    MusicStatusChanged(true);
                }
            }
            else
            {
                // 关闭音乐
                PlayerPrefs.SetInt(MUSIC_PREF_KEY, MUSIC_OFF);
                if (musicState == PlayingState.Playing)
                {
                    PauseMusic();
                }

                if (MusicStatusChanged != null)
                {
                    MusicStatusChanged(false);
                }
            }
            PlayerPrefs.Save();
        }

        void SetMute(bool isMuted)
        {
            AudioSource.mute = isMuted;
        }
    }
}
