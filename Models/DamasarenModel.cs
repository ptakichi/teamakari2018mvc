using System;
using System.Collections.Generic;
using teamakari2018mvc.Business;

namespace teamakari2018mvc.Models
{
    public class DamasarenModel
    {
        public String resultMessage {get;set;} = "音声ファイルをアップロードしてください";
        public List<DangerData>  keyPhrases {get;set;} = new List<DangerData>();

    }
}