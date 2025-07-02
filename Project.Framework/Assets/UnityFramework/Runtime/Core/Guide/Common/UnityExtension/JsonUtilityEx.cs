using UnityEngine;
using System.Collections.Generic;
using System;
namespace UnityFramework
{
    /// <summary>
    /// 拓展List和Dictionary的序列化
    /// </summary>
    public class JsonUtilityEx
    {
        /// <summary>
        /// 序列化一个List
        /// 和FromJson成对使用，序列化出来的数据头会有"target"
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static string ToJson<T>(List<T> list)
        {
            Serialization<T> wrapper = new Serialization<T>(list);
            return JsonUtility.ToJson(wrapper);
        }

        /// <summary>
        /// 和ToJson成对使用
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        public static List<T> FromJson<T>(string json)
        {
            Serialization<T> wrapper = JsonUtility.FromJson<Serialization<T>>(json);
            return wrapper.ToList();
        }

        /// <summary>
        /// 用来反序列化 不是由Serialization 序列化而来的json数据，一般是历史数据或者外部工具生成的json
        /// 需要在数据头加上target
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        public static List<T> FromJsonLegacy<T>(string json)
        {
            Serialization<T> wrapper = JsonUtility.FromJson<Serialization<T>>("{\"target\":" + json + "}");
            return wrapper.ToList();
        }


        public static string ToJson<T1, T2>(Dictionary<T1, T2> dic)
        {
            Serialization<T1, T2> wrapper = new Serialization<T1, T2>(dic);
            return JsonUtility.ToJson(wrapper);
        }

        public static Dictionary<T1, T2> FromJson<T1, T2>(string json)
        {
            Serialization<T1, T2> wrapper = JsonUtility.FromJson<Serialization<T1, T2>>(json);
            return wrapper.ToDictionary();
        }
    }

    // List<T>
    [Serializable]
    public class Serialization<T>
    {
        [SerializeField]
        List<T> target;
        public List<T> ToList() { return target; }

        public Serialization(List<T> target)
        {
            this.target = target;
        }
    }

    // Dictionary<TKey, TValue>
    [Serializable]
    public class Serialization<TKey, TValue> : ISerializationCallbackReceiver
    {
        [SerializeField]
        List<TKey> keys;
        [SerializeField]
        List<TValue> values;

        Dictionary<TKey, TValue> target;
        public Dictionary<TKey, TValue> ToDictionary() { return target; }

        public Serialization(Dictionary<TKey, TValue> target)
        {
            this.target = target;
        }

        public void OnBeforeSerialize()
        {
            keys = new List<TKey>(target.Keys);
            values = new List<TValue>(target.Values);
        }

        public void OnAfterDeserialize()
        {
            var count = Math.Min(keys.Count, values.Count);
            target = new Dictionary<TKey, TValue>(count);
            for (var i = 0; i < count; ++i)
            {
                target.Add(keys[i], values[i]);
            }
        }
    }
}