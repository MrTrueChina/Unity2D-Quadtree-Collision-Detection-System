using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using NUnit.Framework;
using System.Text;


namespace MtC.Tools.QuadtreeCollider.Test
{
    [TestFixture]
    public class DictionaryTest
    {
        [Test]
        public void UnionTest()
        {
            // 主字典
            Dictionary<int, string> mainDictionary = new Dictionary<int, string>();
            // 副字典
            Dictionary<int, string> subDictionary = new Dictionary<int, string>();

            mainDictionary.Add(1, "One");
            mainDictionary.Add(2, "Two");

            subDictionary.Add(3, "Three");

            // 没有重复内容的 Union
            mainDictionary = mainDictionary.Union(subDictionary).ToDictionary(pair => pair.Key, pair => pair.Value);
            Debug.Log("没有重复的 Union：" + PrintDictionary(mainDictionary));

            // 有重复的 Union
            mainDictionary = mainDictionary.Union(subDictionary).ToDictionary(pair => pair.Key, pair => pair.Value);
            Debug.Log("重复合并了 3 的 Union：" + PrintDictionary(mainDictionary));

            // 合并修改了的内容
            subDictionary[3] = "NEW THREE";
            mainDictionary = mainDictionary.Union(subDictionary).ToDictionary(pair => pair.Key, pair => pair.Value);
            Debug.Log("合并了修改后的 3 的 Union：" + PrintDictionary(mainDictionary));

            // 合并时如果有相同的 Key 不同值的情况会报错，这个方法不是很好用
        }

        [Test]
        public void RepeatAdd()
        {
            Dictionary<int, string> dictionary = new Dictionary<int, string>();

            // 存入新的值
            dictionary.Add(1, "One");
            Debug.Log("存入新值：" + PrintDictionary(dictionary));

            // 存入重复值
            dictionary.Add(1, "One");
            Debug.Log("存入重复值：" + PrintDictionary(dictionary));

            // 存入重复 Key 的不同值
            dictionary.Add(1, "NEW ONE");
            Debug.Log("存入重复 Key 的不同值" + PrintDictionary(dictionary));

            // 只要存入已有的 Key 就会报错
        }

        [Test]
        public void SetByIndex()
        {
            Dictionary<int, string> dictionary = new Dictionary<int, string>();

            // 存入新的值
            dictionary[1] = "One";
            Debug.Log("存入新值：" + PrintDictionary(dictionary));

            // 存入重复值
            dictionary[1] = "One";
            Debug.Log("存入重复值：" + PrintDictionary(dictionary));

            // 存入重复 Key 的不同值
            dictionary[1] = "NEW ONE";
            Debug.Log("存入重复 Key 的不同值" + PrintDictionary(dictionary));

            // 使用索引方式存值时，如果没有这个索引会添加，有这个索引会覆盖
        }

        private string PrintDictionary<TKey, TVal>(Dictionary<TKey, TVal> dictionary)
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append("{");

            foreach (KeyValuePair<TKey, TVal> pair in dictionary)
            {
                stringBuilder.Append("(");
                stringBuilder.Append(pair.Key.ToString());
                stringBuilder.Append(", ");
                stringBuilder.Append(pair.Value.ToString());
                stringBuilder.Append("), ");
            }

            stringBuilder.Append("}");

            return stringBuilder.ToString();
        }
    }
}
