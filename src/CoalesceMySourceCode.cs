using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using PlasticMetal.MobileSuit;
using PlasticMetal.MobileSuit.Core;
using PlasticMetal.MobileSuit.ObjectModel;
using PlasticMetal.MobileSuit.ObjectModel.Future;
using PlasticMetal.MobileSuit.Parsing;

namespace CoalesceMySourceCode
{
    [SuitInfo("CoalesceMySourceCode")]
    class CoalesceMySourceCode 
        : CommandLineApplication<CoalesceMySourceCode.CoalesceParameter>,
        ICommandLineApplication
            , IStartingInteractive
    {
        public class CoalesceParameter : AutoDynamicParameter
        {
            [Option("o")] public string OutputFileName { get; set; } = "";
            [Option("i")] public string InputDirectory { get; set; } = "";
            [WithDefault][Option("t")] public string CommentToken { get; set; } = "//";
            [Option("f")]
            [WithDefault]
            [AsCollection]
            public List<string> Format { get; set; } = new List<string>();
            [Option("ex")]
            [AsCollection]
            [WithDefault]
            public List<string> Excludes { get; set; } = new List<string>();
        }
        [SuitAlias("cc")]
        public int Coalesce(CoalesceParameter p)
        {
            var sb = new StringBuilder();
            CoalesceDir("", new DirectoryInfo(p.InputDirectory), sb, p);
            File.WriteAllText(p.OutputFileName, sb.ToString());
            return 0;
        }
        
        private void CoalesceDir(string prefix, DirectoryInfo dir, StringBuilder sb,  CoalesceParameter p)
        {
            foreach (var f in dir.GetFiles())
            {
                if (p.Format.Contains(f.Extension))
                {
                    var fn = $"{prefix}{dir.Name}\\{f.Name}";
                    if (p.Excludes.Any(ex=> Regex.IsMatch(fn,ex)))
                    {
                        IO.WriteLine(
                            Suit.CreateContentArray(
                                ("Skipped:",IO.ColorSetting.ErrorColor),
                                (fn,null)
                            ));
                        continue;
                    }
                    IO.WriteLine(
                        Suit.CreateContentArray(
                            ("Added:", IO.ColorSetting.AllOkColor),
                            (fn, null)
                        ));
                    sb.Append($"\n\n// {fn}\n\n");
                    sb.Append(File.ReadAllText(f.FullName));
                }
            }

            foreach (var nextDir in dir.GetDirectories())
            {
                CoalesceDir(prefix + $"{dir.Name}\\", nextDir, sb, p);
            }
        }
        static void Main(string[] args)
        {
            Suit.GetBuilder()
                .UsePrompt<PowerLineThemedPromptServer>()
                .UseLog(PlasticMetal.MobileSuit.Core.ILogger.OfFile("D:\\clc.log"))
                .Build<CoalesceMySourceCode>()
                .Run(args);
        }

        public override void SuitShowUsage()
        {
            IO.WriteLine(Suit.CreateContentArray(
                ("Usage:",IO.ColorSetting.ListTitleColor)
                
            ));
            IO.WriteLine("\tcmsc -i <InputDir> -t <CommentToken> -o <OutputFile> -f <extension>", IO.ColorSetting.CustomInformationColor);
            IO.WriteLine(Suit.CreateContentArray(
                ("Options:", IO.ColorSetting.ListTitleColor)
            ));
            IO.AppendWriteLinePrefix();
            IO.WriteLine(Suit.CreateContentArray(
                
                ("-f <extension>", IO.ColorSetting.CustomInformationColor),
                ("\tAllow Multiple, At least One; The extension of files' name that you want to coalesce.", null)
            ));
            IO.WriteLine(Suit.CreateContentArray(

                ("-ex <RegEx>", IO.ColorSetting.CustomInformationColor),
                ("\tAllow Multiple; The file/dir names' RegEx which you want to exclude.", null)
            ));
            IO.WriteLine();


            IO.SubtractWriteLinePrefix();
        }

        public override int SuitStartUp(CoalesceParameter arg)
        {
            IO.WriteLine("Called.");
            return 0;
        }
        [SuitIgnore]
        public void OnInitialized()
        {
            IO.PrintAssemblyInformation(
                "Coalesce-My-Source-Code",
                new Version("1.0.0"),
                true,
                "Ferdinand Sukhoi",
                "https://github.com/FerdinandSukhoi/Coalesce-My-Source-Code",
                false
            );
        }
    }
}
