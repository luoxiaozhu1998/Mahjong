using System.IO;
using UnityEngine;

public class LogFPS : MonoBehaviour
{
    // 在类成员变量区域:
    string filepath;
    float timeCount;

    void Start()
    {
        // 获取文件路径
        filepath = Path.Combine(Application.persistentDataPath , "fps.txt");

        // 清空文件
        File.WriteAllText(filepath, string.Empty);

        // 初始化时间计数
        timeCount = 0f;
    }

    void Update()
    {
        timeCount += Time.deltaTime;

        if (timeCount >= 1f)
        {
            // 计算当前帧率
            float currentFPS = 1.0f / Time.deltaTime;

            // 格式化成字符串
            string fps = currentFPS.ToString("f2");

            // 输出到文件
            File.AppendAllText(filepath, fps + "\n");

            // 重置时间计数
            timeCount = 0f;
        }
    }
}