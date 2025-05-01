using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using static Phantom.Utils;

namespace Phantom
{
    public class Obfuscator
    {
        public static string RandomString(int length, Random random)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string GenerateXorBatchScript(string input, Random rng)
        {
            // Encode the input string to Base64
            var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(input));

            // Construct the batch script to decode and execute the Base64 string
            var base64String = $@"
@echo off
        set ""aaa={encoded}""
        for /f ""delims="" %%b in ('powershell -Command ""[Text.Encoding]::UTF8.GetString([Convert]::FromBase64String('%aaa%'))""') do (
            set ""decoded=%%b""
        )
        call %decoded%";

            return base64String;
        }

        public static (string, string) GenCodeBat(string input, Random rng, string SetVarName, int level = 5)
        {
            string ret = string.Empty;
            string[] lines = input.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

            int amount = 5;
            if (level > 1) amount -= level;
            amount *= 2;

            List<string> setlines = new List<string>();
            List<string[]> linevars = new List<string[]>();
            foreach (string line in lines)
            {
                List<string> splitted = new List<string>();
                string sc = string.Empty;
                bool invar = false;
                foreach (char c in line)
                {
                    if (c == '%')
                    {
                        invar = !invar;
                        sc += c;
                        continue;
                    }
                    if ((c == ' ' || c == '\'' || c == '.') && invar)
                    {
                        invar = false;
                        sc += c;
                        continue;
                    }
                    if (!invar && sc.Length >= amount)
                    {
                        splitted.Add(sc);
                        invar = false;
                        sc = string.Empty;
                    }
                    sc += c;
                }
                splitted.Add(sc);

                List<string> vars = new List<string>();
                foreach (string s in splitted)
                {
                    string name = RandomString(10, rng);
                    setlines.Add($"!{SetVarName}! \"{name}={s}\"");
                    vars.Add(name);
                }
                linevars.Add(vars.ToArray());
            }

            setlines = new List<string>(setlines.OrderBy(x => rng.Next()));
            for (int i = 0; i < setlines.Count; i++)
            {
                ret += setlines[i];
                int r = rng.Next(0, 2);
                ret += Environment.NewLine;
            }

            string varcalls = string.Empty;
            foreach (string[] line in linevars)
            {
                foreach (string s in line) varcalls += $"%{s}%";
                varcalls += Environment.NewLine;
            }
            return (ret.TrimEnd('\r', '\n'), varcalls.TrimEnd('\r', '\n'));
        }
    }
}