using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tetris
{
    class SaveHistory
    {
        string path = @"C:\Program Files (x86)\实用工具箱\game";
        public void Write(string text)
        {
            Create();
            FileStream fs = new FileStream(path + "\\tetris.txt", FileMode.Append);
            StreamWriter sw = new StreamWriter(fs, Encoding.Unicode);
            sw.Write(DateTime.Now.ToString()+"\t"+text + "\r\n");
            sw.Close();
            fs.Close();
        }

        public void Read()
        {
            Create();
            Process.Start(path + "\\tetris.txt");
        }
        public void Create()
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            if (!File.Exists(path + "\\tetris.txt"))
            {
                FileStream file = new FileStream(path + "\\tetris.txt", FileMode.Create);
                StreamWriter sw = new StreamWriter(file, Encoding.Unicode);
                sw.Write("添加方法支持方向键，和W、A、S、D操作");
                file.Close();
            }
        }
    }
}
