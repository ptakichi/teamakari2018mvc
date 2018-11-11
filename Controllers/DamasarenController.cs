using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using teamakari2018mvc.Models;
using teamakari2018mvc.Business;
using Microsoft.AspNetCore.Http;
using System.IO;



namespace teamakari2018mvc.Controllers
{
    public class DamasarenController : Controller
    {
        public IActionResult Index()
        {
            DamasarenModel model = new DamasarenModel();
            return View(model);
        }

        [HttpPost]
        [RequestSizeLimit(100000000)]
        public IActionResult OnPost(IFormFile uploadFile){
            
            DamasarenModel model = new DamasarenModel();

            if (uploadFile != null) {
                Console.Write("\n\n\n\n\n\n\n==================\n");
                Console.Write("FileName: {0}",uploadFile.FileName);//Gets the file name from the Content-Disposition header.
                Console.Write("\nHeader: {0}",uploadFile.Headers);//Gets the header dictionary of the uploaded file
                Console.Write("\nLength: {0}",uploadFile.Length);//Gets the file length in bytes.
                Console.Write("\nName: {0}",uploadFile.Name);//Gets the form field name from the Content-Disposition header.
                Console.Write("\nContent-type: {0}",uploadFile.ContentType);//Gets the raw Content-Type header of the uploaded file.
                Console.Write("\n==================\n\n\n\n\n\n");
                // full path to file in temp location
                var uploadfilePath = Path.GetTempPath() + uploadFile.FileName;

                using (var stream = new FileStream(uploadfilePath, FileMode.Create))
                {
                    uploadFile.CopyToAsync(stream).Wait();
                }

                Anaryze an = new Anaryze();
                //音声からテキスト
                Task<String> task = an.TranslationWithFileAsync(uploadfilePath);
                model.resultMessage = task.Result;

                //テキストからキーフレーズ
                Task<Document> result = an.Translate(task.Result);
                Document d = result.Result;
                // model.keyPhrases = d.documents[0].keyPhrases;

                //フレーズ一致かどうか
                List<DangerData> danger = an.JudgeData( d.documents[0].keyPhrases);
                model.keyPhrases = danger;

                int dangercount =  danger.Count(x => x.dangerRitsu >= 80);
                if(dangercount > 3){
                    model.displaykbn = 3;//警告

                }else if(dangercount > 1){
                    model.displaykbn = 2;//注意

                }else
                    model.displaykbn = 1;//安全
                }

                return View("Index",model);

        }
    }
}
