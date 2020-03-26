using System;
using System.Collections.Generic;
using UnityEngine;

namespace CommonManagers
{
    /// <summary>
    /// 时间管理器，处理定时事务、周期性事务
    /// </summary>
    public class TimerManager : BaseManager
    {
        public static TimerManager Instance;
        public delegate void TimerCallback(object param);

        /// <summary>
        /// 时间间隔
        /// </summary>
        private float m_interval = 0;
        /// <summary>
        /// 时间信息队列
        /// </summary>
        private readonly List<TimerInfo> m_timerInfos = new List<TimerInfo>();

        //private static TimerManager _instance;
        //public static TimerManager Instance
        //{
        //    get
        //    {
        //        if (_instance == null)
        //        {
        //            _instance = new TimerManager();
        //        }
        //        return _instance;
        //    }
        //}
        private void Awake()
        {
            Instance = this;
            Instance.m_lastTime = Time.time;
            StartTimer(1);
        }

        public TimerManager()
        {
            m_items = new Dictionary<TimerCallback, TimerRepeatInfo>();
            m_toRemove = new List<TimerRepeatInfo>();
            m_toAdd = new Dictionary<TimerCallback, TimerRepeatInfo>();
            m_pool = new List<TimerRepeatInfo>(100);
            //m_lastTime = Time.time;
        }

        /// <summary>
        /// 时间列表
        /// </summary>
        private readonly Dictionary<TimerCallback, TimerRepeatInfo> m_items;
        /// <summary>
        /// 删除列表
        /// </summary>
        private readonly List<TimerRepeatInfo> m_toRemove;
        /// <summary>
        /// 添加列表
        /// </summary>
        private readonly Dictionary<TimerCallback, TimerRepeatInfo> m_toAdd;

        /// <summary>
        /// 对象池
        /// </summary>
        private readonly List<TimerRepeatInfo> m_pool;
        private float m_time;
        private float m_lastTime;

        void Update()
        {
            m_time = Time.time;

            float elapsed = m_time - m_lastTime;

            // 判断时间缩放
            if (Time.timeScale != 0)
            {
                elapsed /= Time.timeScale;
            }
            m_lastTime = m_time;

            foreach (KeyValuePair<TimerCallback, TimerRepeatInfo> kvp in m_items)
            {
                TimerRepeatInfo item = kvp.Value;

                // 处理过期调用
                if (item.isDeleted)
                {
                    m_toRemove.Add(item);
                    continue;
                }
                // 处理间隔时间 没到需求的间隔时间
                item.Elapsed += elapsed;
                if (item.Elapsed < item.Interval)
                {
                    continue;
                }

                item.Elapsed -= item.Interval;
                if (item.Elapsed < 0 || item.Elapsed > 0.03f)
                {
                    item.Elapsed = 0;
                }

                if (item.RepeatCount > 0)
                {
                    item.RepeatCount--;
                    if (item.RepeatCount == 0)
                    {
                        item.isDeleted = true;
                        m_toRemove.Add(item);
                    }
                }

                if (item.Callback != null)
                {
                    try
                    {
                        item.Callback(item.Param);
                    }
                    catch (Exception e)
                    {
                        item.isDeleted = true;
                        Debug.LogException(e);
                    }
                }
            }

            // 移除废弃集合

            foreach (TimerRepeatInfo item in m_toRemove)
            {
                if (item.isDeleted && item.Callback != null)
                {
                    m_items.Remove(item.Callback);
                    ReturnToPool(item);
                }
            }
            m_toRemove.Clear();

            // 添加新的集合
            foreach (KeyValuePair<TimerCallback, TimerRepeatInfo> item in m_toAdd)
            {
                m_items.Add(item.Key, item.Value);
            }
            m_toAdd.Clear();
        }

        /// <summary>
        /// 随后调用。也就是即将调用。延迟调用
        /// </summary>
        /// <param name="callback"></param>
        public void CallLater(TimerCallback callback)
        {
            Add(0.001f, 1, callback);
        }
        /// <summary>
        /// 随后调用。也就是即将调用。延迟调用
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="callbackParam"></param>
        public void CallLater(TimerCallback callback, object callbackParam)
        {
            Add(0.001f, 1, callback, callbackParam);
        }

        /// <summary>
        ///  添加Update  调用
        /// </summary>
        /// <param name="callback"></param>
        public void AddUpdate(TimerCallback callback)
        {
            Add(0.001f, 0, callback);
        }
        /// <summary>
        /// 添加Update  调用
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="callbackParam"></param>
        public void AddUpdate(TimerCallback callback, object callbackParam)
        {
            Add(0.001f, 0, callback, callbackParam);
        }

        /// <summary>
        /// 检查计时器中 是否有当前调用
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public bool Exists(TimerCallback callback)
        {
            if (m_toAdd.ContainsKey(callback))
            {
                return true;
            }

            TimerRepeatInfo at;
            if (m_items.TryGetValue(callback, out at))
            {
                return !at.isDeleted;
            }

            return false;
        }
        /// <summary>
        /// 删除计时器调用
        /// </summary>
        /// <param name="callback"></param>
        public void Remove(TimerCallback callback)
        {
            TimerRepeatInfo t;
            if (m_toAdd.TryGetValue(callback, out t))
            {
                m_toAdd.Remove(callback);
                ReturnToPool(t);
            }

            if (m_items.TryGetValue(callback, out t))
            {
                t.isDeleted = true;
            }
        }


        public void Add(float interval, int repeatCount, TimerCallback callback)
        {
            this.Add(interval, repeatCount, callback, null);
        }

        /// <summary>
        /// 添加一个计时器  interval 参数 建议不少于 0.03 秒  如果需要使用更低的 循环时间 。建议自己写。用本函数会出现误差
        /// </summary>
        /// <param name="interval">interval 参数 建议不少于 0.03 秒  如果需要使用更低的 循环时间 。建议自己写。用本函数会出现误差</param>
        /// <param name="repeatCount">如果该参数为0  则无限 循环调用</param>
        /// <param name="callback">回调</param>
        /// <param name="callbackParam">回调参数</param>
        public void Add(float interval, int repeatCount, TimerCallback callback, object callbackParam)
        {
            if (callback == null)
            {
                Debug.LogError(" Timer CallBack Is NULL , " + interval + "   , " + repeatCount);
                return;
            }
            TimerRepeatInfo timerRepeatInfo;

            if (m_items.TryGetValue(callback, out timerRepeatInfo))
            {
                // 重置计时器
                timerRepeatInfo.Set(interval, repeatCount, callback, callbackParam);
                timerRepeatInfo.Elapsed = 0;
                timerRepeatInfo.isDeleted = false;
                return;
            }
            if (m_toAdd.TryGetValue(callback, out timerRepeatInfo))
            {
                timerRepeatInfo.Set(interval, repeatCount, callback, callbackParam);
                return;
            }
            // 从 对象池中获取
            timerRepeatInfo = GetFromPool();
            timerRepeatInfo.Interval = interval;
            timerRepeatInfo.RepeatCount = repeatCount;
            timerRepeatInfo.Callback = callback;
            timerRepeatInfo.Param = callbackParam;
            // 添加到 添加队列
            m_toAdd[callback] = timerRepeatInfo;
        }


        private TimerRepeatInfo GetFromPool()
        {
            TimerRepeatInfo timerRepeatInfo;
            int count = m_pool.Count;
            if (count > 0)
            {
                timerRepeatInfo = m_pool[count - 1];
                m_pool.RemoveAt(count - 1);
                timerRepeatInfo.isDeleted = false;
                timerRepeatInfo.Elapsed = 0;
            }
            else
            {
                timerRepeatInfo = new TimerRepeatInfo();
            }
            return timerRepeatInfo;
        }
        private void ReturnToPool(TimerRepeatInfo t)
        {
            t.Callback = null;
            m_pool.Add(t);
        }



        #region old motheds

        /// <summary>
        /// 添加一组计时器
        /// </summary>
        /// <param name="timerInfos"></param>
        public void AddTimerEvent(params TimerInfo[] timerInfos)
        {
            if (timerInfos != null && timerInfos.Length > 0)
            {
                foreach (TimerInfo timerInfo in timerInfos)
                {
                    AddTimerEvent(timerInfo);
                }
            }
        }
        /// <summary>
        /// 添加一个计时器
        /// </summary>
        /// <param name="timerInfo"></param>
        public void AddTimerEvent(TimerInfo timerInfo)
        {
            if (timerInfo != null)
            {
                if (m_timerInfos.Contains(timerInfo))
                {
                    return;
                }
                else
                {
                    m_timerInfos.Add(timerInfo);
                }
            }

        }
        /// <summary>
        /// 删除一个计时器
        /// </summary>
        /// <param name="timerInfo"></param>
        public void RemoveTimerEvent(TimerInfo timerInfo)
        {
            if (timerInfo != null && m_timerInfos.Contains(timerInfo))
            {
                m_timerInfos.Remove(timerInfo);
            }
        }


        /// <summary>
        /// 重新计时
        /// </summary>
        public void ReclockAll()
        {
            TimerInfo tiTemp = null;
            for (int i = 0; i < m_timerInfos.Count; i++)
            {
                tiTemp = m_timerInfos[i];
                tiTemp.Duration = 0;
            }
        }

        /// <summary>
        /// 重新计时
        /// </summary>
        /// <param name="timerInfo"></param>
        public void Reclock(TimerInfo timerInfo)
        {
            if (timerInfo != null)
            {
                if (m_timerInfos.Contains(timerInfo))
                {
                    TimerInfo tiTemp = null;
                    for (int i = 0; i < m_timerInfos.Count; i++)
                    {
                        tiTemp = m_timerInfos[i];
                        if (tiTemp.ClassName == timerInfo.ClassName)
                        {
                            tiTemp.Duration = 0;
                        }
                    }
                }
            }
        }


        /// <summary>
        /// 删除全部计时器
        /// </summary>
        public void RemoveAllTimer()
        {
            m_timerInfos.Clear();
        }

        /// <summary>
        /// 继续一个计时器
        /// </summary>
        /// <param name="timerInfo"></param>
        public void ResumeTimerEvent(TimerInfo timerInfo)
        {
            if (timerInfo != null && m_timerInfos.Contains(timerInfo))
            {
                timerInfo.Stop = false;
            }
        }
        /// <summary>
        /// 停止计时器
        /// </summary>
        /// <param name="timerInfo"></param>
        public void StopTimerEvent(TimerInfo timerInfo)
        {
            if (timerInfo != null && m_timerInfos.Contains(timerInfo))
            {
                timerInfo.Stop = true;
            }
        }




        /// <summary>
        /// 启动计时器
        /// </summary>
        /// <param name="value"></param>
        public void StartTimer(float value)
        {
            m_interval = value;
            InvokeRepeating("Run", 0, m_interval);
        }
        /// <summary>
        /// 停止计时器
        /// </summary>
        public void StopTimer()
        {
            CancelInvoke("Run");
        }
        private void Run()
        {
            if (m_timerInfos.Count > 0)
            {
                #region Start Timer
                for (int i = 0; i < m_timerInfos.Count; i++)
                {
                    TimerInfo timerInfo = m_timerInfos[i];
                    if (timerInfo.Delete || timerInfo.Stop)
                    {
                        continue;
                    }
                    else
                    {
                        ITimerBehaviour timerBehaviour = timerInfo.Target as ITimerBehaviour;
                        if (timerBehaviour != null)
                        {
                            timerBehaviour.TimerUpdate(timerInfo);
                            timerInfo.Duration = timerInfo.Duration + 1;
                        }
                    }
                }
                #endregion
                #region Remove Timer

                for (int i = 0; i < m_timerInfos.Count; i++)
                {
                    TimerInfo timerInfo = m_timerInfos[i];
                    if (timerInfo.Delete)
                    {
                        m_timerInfos.Remove(timerInfo);
                    }
                }
                #endregion

            }
        }

        #endregion

    }




    class TimerRepeatInfo
    {
        /// <summary>
        /// 间隔时间
        /// </summary>
        public float Interval;
        /// <summary>
        /// 循环调用次数
        /// </summary>
        public int RepeatCount;
        /// <summary>
        /// 回调函数
        /// </summary>
        public TimerCallback Callback;
        /// <summary>
        /// 参数列表
        /// </summary>
        public object Param;
        /// <summary>
        ///  时差
        /// </summary>
        public float Elapsed;
        public bool isDeleted;

        public void Set(float interval, int repeat, TimerCallback callback, object param)
        {
            this.Interval = interval;
            this.RepeatCount = repeat;
            this.Callback = callback;
            this.Param = param;
        }
    }


    /// <summary>
    /// 时间消息模型对象
    /// </summary>
    public class TimerInfo
    {
        /// <summary>
        /// 是否标记为删除
        /// </summary>
        public bool Delete { get; set; }

        /// <summary>
        /// 类名
        /// </summary>
        public string ClassName { get; set; }

        /// <summary>
        /// 是否标记为停止
        /// </summary>
        public bool Stop { get; set; }

        /// <summary>
        /// 执行目标
        /// </summary>
        public UnityEngine.Object Target;

        /// <summary>
        /// 时长
        /// </summary>
        public int Duration { get; set; }


        public override string ToString()
        {
            return string.Format("ClassName:{0},Duration:{1},Target:{2},Stop:{3},Delete:{4}", ClassName, Duration,
                Target, Stop, Delete);
        }
    }
}
