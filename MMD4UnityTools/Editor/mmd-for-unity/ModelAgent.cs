using UnityEngine;

namespace MMDExtensions
{

    public class ModelAgent
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name='file'>読み込むファイルパス</param>
        public ModelAgent(string file_path)
        {
            if (string.IsNullOrEmpty(file_path))
            {
                throw new System.ArgumentException();
            }
            file_path_ = file_path;
        }

        public string file_path_;
    }
}