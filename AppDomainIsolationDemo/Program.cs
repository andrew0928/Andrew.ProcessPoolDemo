using AppDomainIsolationLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppDomainIsolationDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            AppDomainProgram.InitCount = 543;   // 模擬汙染 static fields 的狀況
            AppDomainProgram.Main(null);        // 不透過獨立的 AppDomain 執行 code

            var iso = AppDomain.CreateDomain("demo");
            iso.ExecuteAssemblyByName(typeof(AppDomainProgram).Assembly.FullName);        
        }
    }

}
