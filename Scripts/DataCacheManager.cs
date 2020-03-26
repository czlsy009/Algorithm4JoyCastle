using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace CommonManagers
{
    public class DataCacheManager : MonoBehaviour
    {
        private static DataCacheManager _instance;
        public static DataCacheManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = AppBridgeLink.Instance.GetManager<DataCacheManager>(ManagerName.DATACACHE);
                }
                return _instance;
            }
        }


        #region Dictionary

        #region AddDictionary

        /// <summary>
        /// 将一个对象写入到本地缓存目录
        /// </summary>
        /// <param name="model">数据模型</param>
        /// <param name="moduleName">模块名</param>
        public void SaveDictionary<T>(T model, string moduleName) where T : IBaseItemModel<T>, new()
        {
            SaveDictionary(model, moduleName, null);
        }
        /// <summary>
        /// 将一个对象写入到本地缓存目录
        /// </summary>
        /// <param name="model">数据模型</param>
        /// <param name="moduleName">模块名</param>
        /// <param name="childModuleNameId">子模块名</param>
        public void SaveDictionary<T>(T model, string moduleName, string childModuleNameId) where T : IBaseItemModel<T>, new()
        {
            File.WriteAllText(GetCachePath(moduleName, childModuleNameId), model.ModelToJsonString(), Encoding.UTF8);
        }
        #endregion



        #region ReadDictionary

        /// <summary>
        /// 读取本地缓存文件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="moduleName"></param>
        /// <returns></returns>
        public T ReadDictionary<T>(string moduleName) where T : IBaseItemModel<T>, new()
        {
            return ReadDictionary<T>(moduleName, null);
        }

        /// <summary>
        /// 读取本地缓存文件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="moduleName"></param>
        /// <param name="childModuleNameId"></param>
        /// <returns></returns>
        public T ReadDictionary<T>(string moduleName, string childModuleNameId) where T : IBaseItemModel<T>, new()
        {
            T tempModel = default(T);
            TinyJson.Node resultNode = TinyJson.Parser.Instance.Load(File.ReadAllText(GetCachePath(moduleName, childModuleNameId)));
            if (resultNode != null)
            {
                tempModel = new T();
                tempModel.JsonToModelString(resultNode);
            }
            else
            {
                Debug.LogError(string.Format("ReadDictionary ==>  moduleName : {0} childModuleNameId : {1} Decode Error ", moduleName, childModuleNameId));
            }
            return tempModel;
        }


        #endregion

        #region UpdateDictionary

        /// <summary>
        /// 更新数据模型
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="model">数据模型</param>
        /// <param name="moduleName"></param>
        public void UpdateDictionary<T>(T model, string moduleName)
            where T : IBaseItemModel<T>, new()
        {
            UpdateDictionary(model, moduleName, null);
        }

        /// <summary>
        /// 更新数据模型
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="model">数据模型</param>
        /// <param name="moduleName"></param>
        /// <param name="childModuleNameId"></param>
        public void UpdateDictionary<T>(T model, string moduleName, string childModuleNameId)
           where T : IBaseItemModel<T>, new()
        {
            T temp = ReadDictionary<T>(moduleName, childModuleNameId);

            if (temp != null && model.ID.Equals(temp.ID))
            {
                SaveDictionary(model, moduleName, childModuleNameId);
            }
        }

        #endregion


        #region RemoveDictionary

        /// <summary>
        /// 删除字典 对象模型
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="model">数据模型</param>
        /// <param name="moduleName"></param>
        /// <returns>返回是否找到删除目标</returns>
        public bool RemoveItemDictionary<T>(T model, string moduleName)
            where T : IBaseItemModel<T>, new()
        {
            return RemoveItemDictionary(model, moduleName, null);
        }

        /// <summary>
        /// 删除字典 对象模型
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="model">数据模型</param>
        /// <param name="moduleName"></param>
        /// <param name="childModuleNameId"></param>
        /// <returns>返回是否找到删除目标</returns>
        public bool RemoveItemDictionary<T>(T model, string moduleName, string childModuleNameId)
            where T : IBaseItemModel<T>, new()
        {
            T temp = ReadDictionary<T>(moduleName, childModuleNameId);

            if (temp != null && model.ID.Equals(temp.ID))
            {
                RemoveDictionary(moduleName, childModuleNameId);
                return true;
            }
            return false;
        }


        public void RemoveDictionary(string moduleName)
        {
            RemoveDictionary(moduleName, null);
        }
        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="moduleName"></param>
        /// <param name="childModuleNameId"></param>
        public void RemoveDictionary(string moduleName, string childModuleNameId)
        {
            DeleteCachePath(moduleName, childModuleNameId);
        }

        #endregion


        #endregion

        #region List

        #region AddList

        /// <summary>
        /// 把对象集合写入到本地缓存目录
        /// </summary>
        /// <param name="models">数据模型集合</param>
        /// <param name="moduleName">模块名</param>
        public void SaveList<T>(List<T> models, string moduleName) where T : IBaseItemsModel<T>, new()
        {
            SaveList(models, moduleName, null);
        }

        /// <summary>
        /// 把对象集合写入到本地缓存目录
        /// </summary>
        /// <param name="models">数据模型集合</param>
        /// <param name="moduleName">模块名</param>
        /// <param name="childModuleNameId">子模块名</param>
        public void SaveList<T>(List<T> models, string moduleName, string childModuleNameId) where T : IBaseItemsModel<T>, new()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("[");
            foreach (T baseItemModel in models)
            {
                stringBuilder.Append(baseItemModel.ModelToJsonString() + ",");
            }
            if (models.Count > 0)
            {
                stringBuilder = stringBuilder.Remove(stringBuilder.Length - 1, 1);
            }
            stringBuilder.Append("]");
            File.WriteAllText(GetCachePath(moduleName, childModuleNameId), stringBuilder.ToString(), Encoding.UTF8);
        }

        #endregion

        #region AppendList

        /// <summary>
        /// 追加数据保存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="models">数据模型集合</param>
        /// <param name="moduleName">模块名</param>
        public void AppendList<T>(T models, string moduleName) where T : IBaseItemsModel<T>, new()
        {
            AppendList(new List<T> { models }, moduleName, null);
        }
        /// <summary>
        /// 追加数据保存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="models">数据模型集合</param>
        /// <param name="moduleName">模块名</param>
        public void AppendList<T>(List<T> models, string moduleName) where T : IBaseItemsModel<T>, new()
        {
            AppendList(models, moduleName, null);
        }

        /// <summary>
        /// 追加数据保存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="models">数据模型集合</param>
        /// <param name="moduleName">模块名</param>
        /// <param name="maxCount">最大数据长度</param>
        public void AppendList<T>(List<T> models, string moduleName, int maxCount) where T : IBaseItemsModel<T>, new()
        {
            AppendList(models, moduleName, null, maxCount);
        }

        /// <summary>
        /// 追加数据保存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="models">数据模型集合</param>
        /// <param name="moduleName">模块名</param>
        /// <param name="childModuleNameId">子模块名</param>
        /// <param name="maxCount">最大数据长度</param>
        public void AppendList<T>(List<T> models, string moduleName, string childModuleNameId, int maxCount) where T : IBaseItemsModel<T>, new()
        {
            List<T> oldList = ReadList<T>(moduleName, childModuleNameId);
            oldList.AddRange(models);

            if (oldList.Count > maxCount)
            {
                for (int i = 0; i < oldList.Count; )
                {
                    oldList.RemoveAt(0);
                    if (oldList.Count >= maxCount)
                    {
                        break;
                    }
                }
            }

            SaveList(oldList, moduleName, childModuleNameId);
        }

        /// <summary>
        /// 追加数据保存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="models">数据模型集合</param>
        /// <param name="moduleName">模块名</param>
        /// <param name="childModuleNameId">子模块名</param>
        public void AppendList<T>(List<T> models, string moduleName, string childModuleNameId) where T : IBaseItemsModel<T>, new()
        {
            List<T> oldList = ReadList<T>(moduleName, childModuleNameId);
            oldList.AddRange(models);
            SaveList(oldList, moduleName, childModuleNameId);
        }
        #endregion

        #region ReadList

        /// <summary>
        /// 读取本地缓存文件 
        /// </summary>
        /// <returns></returns>
        public List<T> ReadList<T>(string moduleName) where T : IBaseItemsModel<T>, new()
        {
            return ReadList<T>(moduleName, null);
        }
        /// <summary>
        /// 读取本地缓存文件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="moduleName"></param>
        /// <param name="childModuleNameId"></param>
        /// <returns></returns>
        public List<T> ReadList<T>(string moduleName, string childModuleNameId) where T : IBaseItemsModel<T>, new()
        {
            List<T> followItemModel = new List<T>();
            TinyJson.Node resultNode = TinyJson.Parser.Instance.Load(File.ReadAllText(GetCachePath(moduleName, childModuleNameId)));
            if (resultNode != null)
            {
                if (resultNode.IsArray())
                {
                    List<TinyJson.Node> lists = (List<TinyJson.Node>)resultNode;
                    foreach (TinyJson.Node node in lists)
                    {
                        T tempModel = new T();
                        followItemModel.Add(tempModel.JsonToModelString(node));
                    }
                }
            }
            else
            {
                Debug.LogError(string.Format(" ReadList ==> moduleName : {0} childModuleNameId : {1} Decode Error ", moduleName, childModuleNameId));
            }
            return followItemModel;
        }
        #endregion

        #region RemoveList


        /// <summary>
        /// 删除单条数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <param name="moduleName"></param>
        public void RemoveItemList<T>(T model, string moduleName) where T : IBaseItemsModel<T>, new()
        {
            RemoveItemList(model, moduleName, null);
        }
        /// <summary>
        /// 删除单条数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <param name="moduleName"></param>
        /// <param name="childModuleNameId"></param>
        public void RemoveItemList<T>(T model, string moduleName, string childModuleNameId) where T : IBaseItemsModel<T>, new()
        {
            List<T> oldList = ReadList<T>(moduleName, childModuleNameId);
            int m_index = -1;

            for (int index = 0; index < oldList.Count; index++)
            {
                T t = oldList[index];
                if (t.ID.Equals(model.ID))
                {
                    m_index = index;
                    break;
                }
            }
            if (m_index >= 0)
            {
                oldList.RemoveAt(m_index);
            }
            SaveList(oldList, moduleName, childModuleNameId);
        }

        public void RemoveList(string moduleName)
        {
            RemoveList(moduleName, null);
        }
        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="moduleName"></param>
        /// <param name="childModuleNameId"></param>
        public void RemoveList(string moduleName, string childModuleNameId)
        {
            DeleteCachePath(moduleName, childModuleNameId);
        }

        #endregion


        #region UpdateList


        /// <summary>
        /// 更新数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="model">数据实体对象</param>
        /// <param name="moduleName"></param>
        /// <returns>返回是否找到ID匹配的数据</returns>
        public bool UpdateList<T>(T model, string moduleName) where T : IBaseItemsModel<T>, new()
        {
            return UpdateList(model, moduleName, string.Empty);
        }


        /// <summary>
        /// 更新数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="model">数据实体对象</param>
        /// <param name="moduleName"></param>
        /// <param name="childModuleNameId"></param>
        /// <returns>返回是否找到ID匹配的数据</returns>
        public bool UpdateList<T>(T model, string moduleName, string childModuleNameId) where T : IBaseItemsModel<T>, new()
        {
            bool isfind = false;
            List<T> oldList = ReadList<T>(moduleName, childModuleNameId);
            List<T> newList = new List<T>();
            for (int index = 0; index < oldList.Count; index++)
            {
                T m = oldList[index];
                if (m.ID.Equals(model.ID))
                {
                    isfind = true;
                    oldList[index] = model;
                }
                newList.Add(oldList[index]);
            }

            SaveList(newList, moduleName, childModuleNameId);
            return isfind;
        }

        #endregion


        #endregion

        #region CachePath

        #region AddCachePath
        /// <summary>
        /// 获取缓存路径
        /// </summary>
        /// <param name="moduleName">模块名称</param>
        /// <returns></returns>
        public string GetCachePath(string moduleName)
        {
            return GetCachePath(moduleName, null);
        }

        /// <summary>
        /// 获取缓存路径
        /// </summary>
        /// <param name="moduleName">模块名称</param>
        /// <param name="childModuleNameId"></param>
        /// <returns></returns>
        private string GetCachePath(string moduleName, string childModuleNameId)
        {
            string rootCachePath;
            string fileName;
            moduleName = moduleName.Trim();

            if (string.IsNullOrEmpty(childModuleNameId))
            {
                fileName = Mathf.Abs(moduleName.GetHashCode()).ToString();
                // 子模块名为空
                rootCachePath = string.Format("{0}/caches/{1}/", Application.persistentDataPath, moduleName);
            }
            else
            {
                fileName = Mathf.Abs((moduleName + childModuleNameId).GetHashCode()).ToString();
                // 子模块名不为空
                rootCachePath = string.Format("{0}/caches/{1}/{2}/", Application.persistentDataPath, moduleName,
                   childModuleNameId.GetHashCode().ToString("x8"));
            }
            // 判断路径是否存在
            if (!Directory.Exists(rootCachePath))
            {
                // 创建路径
                Directory.CreateDirectory(rootCachePath);
            }
            string result = string.Format("{0}{1}.cache", rootCachePath, fileName);

            // 判断文件是否 存在
            if (!File.Exists(result))
            {
                // 创建文件
                File.Create(result).Dispose();
            }
            return result;
        }
        #endregion

        #region Delete Cache Path

        /// <summary>
        /// 删除数据缓存文件
        /// </summary>
        /// <param name="moduleName">模块名</param>
        public void DeleteCachePath(string moduleName)
        {
            DeleteCachePath(moduleName, null);
        }
        /// <summary>
        /// 删除数据缓存文件
        /// </summary>
        /// <param name="moduleName">模块名</param>
        /// <param name="childModuleNameId">子模块名称</param>
        public void DeleteCachePath(string moduleName, string childModuleNameId)
        {
            string rootCachePath;
            string fileName;
            if (string.IsNullOrEmpty(childModuleNameId))
            {
                fileName = Mathf.Abs(moduleName.GetHashCode()).ToString();
                // 子模块名为空
                rootCachePath = string.Format("{0}/caches/{1}/", Application.persistentDataPath, moduleName);
            }
            else
            {
                fileName = Mathf.Abs((moduleName + childModuleNameId).GetHashCode()).ToString();
                // 子模块名不为空
                rootCachePath = string.Format("{0}/caches/{1}/{2}/", Application.persistentDataPath, moduleName,
                   childModuleNameId);
            }
            // 判断路径是否存在
            if (!Directory.Exists(rootCachePath))
            {
                // 创建路径
                Directory.CreateDirectory(rootCachePath);
            }
            string result = string.Format("{0}{1}.cache", rootCachePath, fileName);

            // 判断文件是否 存在
            if (File.Exists(result))
            {
                // 删除文件
                File.Delete(result);
            }
        }

        #endregion

        #endregion


    }
}
