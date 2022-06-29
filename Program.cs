using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Speech.Synthesis;
using System.Speech.AudioFormat;
using NAudio.Lame;
using NAudio.Wave;

namespace SpeechToMP3
{
    internal class Program
    {
        static void Main(string[] args)
        {
            PrintCulturesOnConsole();
            Console.Read();

            using (SpeechSynthesizer reader = new SpeechSynthesizer())
            {
                //set some settings
                reader.Volume = 100;
                reader.Rate = 0; //medium

                //save to memory stream
                var ms = new MemoryStream();
                reader.SetOutputToWaveStream(ms);

                var builder = new PromptBuilder(new System.Globalization.CultureInfo("en-US"));//ru-RU
                builder.AppendText("Read text example");

                //do speaking
                reader.Speak(builder);

                //now convert to mp3 using LameEncoder or shell out to audiograbber
                ConvertWavStreamToMp3File(ref ms, "mytest.mp3");
            }
        }

        public static void ConvertWavStreamToMp3File(ref MemoryStream ms, string savetofilename)
        {
            //rewind to beginning of stream
            ms.Seek(0, SeekOrigin.Begin);

            using (var retMs = new MemoryStream())
            using (var rdr = new WaveFileReader(ms))
            using (var wtr = new LameMP3FileWriter(savetofilename, rdr.WaveFormat, LAMEPreset.VBR_90))
            {
                rdr.CopyTo(wtr);
            }
        }

        public static void PrintCulturesOnConsole()
        {
            // Display the header.
            Console.WriteLine("{0,-53}{1}", "CULTURE", "SPECIFIC CULTURE");

            // Get each neutral culture in the .NET Framework.
            CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.NeutralCultures);
            // Sort the returned array by name.
            Array.Sort<CultureInfo>(cultures, new NamePropertyComparer<CultureInfo>());

            // Determine the specific culture associated with each neutral culture.
            foreach (var culture in cultures)
            {
                Console.Write("{0,-12} {1,-40}", culture.Name, culture.EnglishName);
                try
                {
                    Console.WriteLine("{0}", CultureInfo.CreateSpecificCulture(culture.Name).Name);
                }
                catch (ArgumentException)
                {
                    Console.WriteLine("(no associated specific culture)");
                }
            }
        }

    }
    public class NamePropertyComparer<T> : IComparer<T>
    {
        public int Compare(T x, T y)
        {
            if (x == null)
                if (y == null)
                    return 0;
                else
                    return -1;

            PropertyInfo pX = x.GetType().GetProperty("Name");
            PropertyInfo pY = y.GetType().GetProperty("Name");
            return String.Compare((string)pX.GetValue(x, null), (string)pY.GetValue(y, null));
        }
    }
}
