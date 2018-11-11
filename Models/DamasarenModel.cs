using System;
using System.Collections.Generic;
using teamakari2018mvc.Business;

namespace teamakari2018mvc.Models
{
    public class DamasarenModel
    {
        public String resultMessage {get;set;}
        public List<DangerData>  keyPhrases {get;set;} = new List<DangerData>();
        public int displaykbn {get;set;} = 0;//zeroは表示なし

    }
}