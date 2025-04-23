using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using VNWGame.Game;
using VNWGame;

public class GameManager : MonoBehaviour
{
    public TMP_FontAsset mainFont;
    public List<TMP_FontAsset> fallbackList;
    public Material material;
    public TextMeshProUGUI test;
    void Start()
    {
        FontTexArraySystem.Instance.Init(mainFont, fallbackList);
        HUDFontSystem.Instance.Init(material);
        int id = 0;
        TextAsset csvFile = Resources.Load<TextAsset>("测试所有玩家名字"); // 不需要文件扩展名
        if (csvFile != null)
        {
            // 将 CSV 内容按行分割
            string[] lines = csvFile.text.Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
        
            // 保存每一行到 List<string>
            List<string> rows = new List<string>(lines);
        
            // 打印每一行内容
            
            foreach (string row in rows)
            {
                HUDFontSystem.Instance.ShowFont(id++, 2, ArabicFixerTool.FixLine(row));
            }
        }
        else
        {
            Debug.LogError("CSV 文件未找到！");
        }

        test.text = ArabicFixerTool.FixLine("شلبي");
    }

    // Update is called once per frame
    void Update()
    {
        FontTexArraySystem.Instance.Update();
        HUDFontSystem.Instance.RenderFont();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            HUDFontSystem.Instance.SaveTexture();
        }
    }
}
