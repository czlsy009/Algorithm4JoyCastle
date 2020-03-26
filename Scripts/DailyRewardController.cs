using UnityEngine;
using System;

namespace CommonControllers
{
    
    public class DailyRewardController : MonoBehaviour
    {
        public static DailyRewardController Instance { get; private set; }
        /// <summary>
        /// 下次刷新日常奖励的时间。
        /// </summary>
        public DateTime NextRewardTime
        {
            get
            {
                return GetNextRewardTime();
            }
        }
        /// <summary>
        /// 领取日常奖励的间隔时间
        /// </summary>
        public TimeSpan TimeUntilReward
        { 
            get
            {
                return NextRewardTime.Subtract(DateTime.Now);
            }
        }

        [Header("勾选是否开启日常奖励")]
        public bool disable;

        [Header("日常奖励配置")]
        [Tooltip("两次奖励间隔时间（小时）")]
        public int rewardIntervalHours = 6;
        [Tooltip("两次奖励间隔时间（分钟）")]
        public int rewardIntervalMinutes = 0;
        [Tooltip("两次奖励间隔时间（秒）")]
        public int rewardIntervalSeconds = 0;
        /// <summary>
        /// 最小奖励系数
        /// </summary>
        public int minRewardValue = 20;
        /// <summary>
        /// 最大奖励系数
        /// </summary>
        public int maxRewardValue = 50;
        /// <summary>
        /// PlayerPrefs存储Key
        /// </summary>
        private const string NextRewardTimePPK = "SGLIB_NEXT_DAILY_REWARD_TIME";

        void Awake()
        {
            if (Instance)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        /// <summary>
        /// 检测是否到达日常奖励触发时间
        /// </summary>
        /// <returns><c>true</c> 已可以触发,反之 <c>false</c>.</returns>
        public bool CanRewardNow()
        {
            return TimeUntilReward <= TimeSpan.Zero;
        }

        /// <summary>
        /// 随机奖励系数
        /// </summary>
        /// <returns>随机得到的奖励系数</returns>
        public int GetRandomReward()
        {
            return UnityEngine.Random.Range(minRewardValue, maxRewardValue + 1);
        }

        /// <summary>
        /// 根据设定奖励间隔设置下次可触发奖励的时间。
        /// </summary>
        public void ResetNextRewardTime()
        {
            DateTime next = DateTime.Now.Add(new TimeSpan(rewardIntervalHours, rewardIntervalMinutes, rewardIntervalSeconds));
            StoreNextRewardTime(next);
        }
       
        void StoreNextRewardTime(DateTime time)
        {
            PlayerPrefs.SetString(NextRewardTimePPK, time.ToBinary().ToString());
            PlayerPrefs.Save();
        }
        /// <summary>
        /// 获取PlayerPrefs中存储的下次奖励时间（若未设定则可现在触发）。
        /// </summary>
        DateTime GetNextRewardTime()
        {
            string storedTime = PlayerPrefs.GetString(NextRewardTimePPK, string.Empty);
            //TODO:If null is now.
            if (!string.IsNullOrEmpty(storedTime))
                return DateTime.FromBinary(Convert.ToInt64(storedTime));
            else
                return DateTime.Now;
        }
    }
}
