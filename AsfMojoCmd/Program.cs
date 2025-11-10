using System;
using System.Collections.Generic;

using AsfMojo.File;
using AsfMojo.Parsing;

namespace AsfMojoCmd
{
    class Program
    {
        static void Main(string[] args)
        {
            //example usage:
            //-i test.wmv -l  
            //-i test.wmv -t  -start 144.5074 -o D:\samples\test.jpg
            //-i test.wmv -a -start 5.0 -end 12.5 -o D:\samples\audio.wav

            Dictionary<string, object> switches = new Dictionary<string, object>();

            for (int i = 0; i < args.Length; i++)
            {
                if(args[i] == "-l")
                    switches.Add("PrintDuration", "");

                if (args[i] == "-u")
                    switches.Add("UpdateProperties", "");

                if (args[i] == "-tracks")
                    switches.Add("PrintTracks", "");

                if (args[i] == "-author")
                    switches.Add("Author", args[++i].Trim('\"'));

                if (args[i] == "-description")
                    switches.Add("Description", args[++i].Trim('\"'));

                if (args[i] == "-title")
                    switches.Add("Title", args[++i].Trim('\"'));

                if (args[i] == "-copyright")
                    switches.Add("Copyright", args[++i].Trim('\"'));

                if (args[i] == "-start")
                    switches.Add("StartOffset", Convert.ToDouble(args[++i]));

                if (args[i] == "-end")
                    switches.Add("EndOffset", Convert.ToDouble(args[++i]));

                if (args[i] == "-w")
                    switches.Add("Width", Convert.ToInt32(args[++i]));

                if (args[i] == "-i")
                    switches.Add("InputFile", args[++i]);

                if (args[i] == "-o")
                    switches.Add("OutputFile", args[++i]);

                if (args[i] == "-?")
                    switches.Add("ShowHelp", args[++i]);
            }

            if (switches.ContainsKey("InputFile") && !switches.ContainsKey("ShowHelp"))
            {
                string fileName = (string)switches["InputFile"];
                ExecuteCommands(fileName, switches);
            }
            else
                PrintUsage();
        }

        public static void ExecuteCommands(string fileName, Dictionary<string, object> switches)
        {
            try
            {
                if (switches.ContainsKey("PrintDuration")) // print file duration
                {
                    AsfFile asfFile = new AsfFile(fileName);
                    AsfFileProperties fileProperties = asfFile.GetAsfObject<AsfFileProperties>();
                    Console.WriteLine(string.Format("File {0} has a duration of {1}", fileName, fileProperties.Duration.ToString("mm':'ss\\.fff")));
                }
                else if (switches.ContainsKey("PrintTracks")) // print tracks and their codecs
                {
                    AsfFile asfFile = new AsfFile(fileName);

                    var streamObjects = asfFile.GetAsfObjects<AsfStreamPropertiesObject>();
                    var codecList = asfFile.GetAsfObject<AsfCodecListObject>();

                    // Separate codec entries by type for simple sequential mapping
                    List<Dictionary<string, object>> audioCodecEntries = new List<Dictionary<string, object>>();
                    List<Dictionary<string, object>> videoCodecEntries = new List<Dictionary<string, object>>();
                    if (codecList != null)
                    {
                        foreach (var cp in codecList.CodecProperties)
                        {
                            string type = cp.ContainsKey("Type") ? cp["Type"].ToString() : string.Empty;
                            if (type == "Audio Codec") audioCodecEntries.Add(cp);
                            else if (type == "Video Codec") videoCodecEntries.Add(cp);
                        }
                    }

                    int audioCodecIndex = 0;
                    int videoCodecIndex = 0;

                    Console.WriteLine("Tracks:");
                    foreach (var so in streamObjects)
                    {
                        bool isAudio = so.StreamType == AsfGuid.ASF_Audio_Media;
                        bool isVideo = so.StreamType == AsfGuid.ASF_Video_Media;
                        if (!isAudio && !isVideo)
                            continue; // skip unsupported types

                        if (isAudio)
                        {
                            var audioProps = (AsfMojoAudioStreamProperties)so.StreamProperties["AudioStreamProperties"];
                            Dictionary<string, object> mappedCodec = audioCodecIndex < audioCodecEntries.Count ? audioCodecEntries[audioCodecIndex++] : null;
                            string codecName = mappedCodec != null && mappedCodec.ContainsKey("Name") ? mappedCodec["Name"].ToString() : "Unknown";
                            string codecDesc = mappedCodec != null && mappedCodec.ContainsKey("Description") ? mappedCodec["Description"].ToString() : "";

                            Console.WriteLine(string.Format("  Stream {0} (Audio): Codec={1} {2} FormatTag={3} Channels={4} SampleRate={5} BitsPerSample={6} AvgBitrate={7}bps", 
                                so.StreamNumber,
                                codecName,
                                string.IsNullOrEmpty(codecDesc) ? "" : "- " + codecDesc,
                                audioProps.format_tag,
                                audioProps.number_channels,
                                audioProps.samples_per_second,
                                audioProps.bits_per_sample,
                                8 * audioProps.average_bytes_per_second));
                        }
                        else if (isVideo)
                        {
                            var videoProps = (AsfMojoVideoStreamProperties)so.StreamProperties["VideoStreamProperties"];
                            var formatData = (AsfMojoVideoStreamFormatData)so.StreamProperties["VideoStreamFormatData"];
                            Dictionary<string, object> mappedCodec = videoCodecIndex < videoCodecEntries.Count ? videoCodecEntries[videoCodecIndex++] : null;
                            string codecName = mappedCodec != null && mappedCodec.ContainsKey("Name") ? mappedCodec["Name"].ToString() : "Unknown";
                            string codecDesc = mappedCodec != null && mappedCodec.ContainsKey("Description") ? mappedCodec["Description"].ToString() : "";
                            string fourCC = System.Text.ASCIIEncoding.ASCII.GetString(formatData.compression_id).Trim('\0');

                            // Frame rate if available (AvgTimePerFrame from extended stream props object) not directly here; skip unless we add lookup.
                            Console.WriteLine(string.Format("  Stream {0} (Video): Codec={1} {2} FourCC={3} Width={4} Height={5} BitsPerPixel={6}",
                                so.StreamNumber,
                                codecName,
                                string.IsNullOrEmpty(codecDesc) ? "" : "- " + codecDesc,
                                fourCC,
                                formatData.image_width,
                                formatData.image_height,
                                formatData.bits_per_pixel_count));
                        }
                    }

                    if (codecList == null)
                    {
                        Console.WriteLine("No codec list object found; codec names may be unavailable.");
                    }
                }
                else if (switches.ContainsKey("UpdateProperties")) //update content description properties
                {
                    AsfFile asfFile = new AsfFile(fileName);
                    AsfContentDescriptionObject contentDescription = asfFile.GetAsfObject<AsfContentDescriptionObject>();

                    if (contentDescription != null)
                    {

                        string author = switches.ContainsKey("Author") ? (string)switches["Author"] : contentDescription.ContentProperties["Author"];
                        string copyright = switches.ContainsKey("Copyright") ? (string)switches["Copyright"] : contentDescription.ContentProperties["Copyright"];
                        string title = switches.ContainsKey("Title") ? (string)switches["Title"] : contentDescription.ContentProperties["Title"];
                        string description = switches.ContainsKey("Description") ? (string)switches["Description"] : contentDescription.ContentProperties["Description"];

                        AsfFile.From(fileName)
                               .WithAuthor(author)
                               .WithDescription(description)
                               .WithCopyright(copyright)
                               .WithTitle(title)
                               .Update();

                        Console.WriteLine(string.Format("Content description properties updated."));
                    }
                    else
                        Console.WriteLine(string.Format("No content description properties available."));
                }

                
            }
            catch (Exception)
            {
                PrintUsage();
            }
        }

        public static void PrintUsage()
        {
            Console.WriteLine("AsfMojoCmd options:");
            Console.WriteLine("Displaying the media file playback duration:");
            Console.WriteLine("  AsfMojoCmd -i <filename> -l");
            Console.WriteLine("Example:");
            Console.WriteLine("  AsfMojoCmd -i test.wmv -l");

            Console.WriteLine("---------------------------");
            Console.WriteLine("Listing tracks and codecs:");
            Console.WriteLine("  AsfMojoCmd -i <filename> -tracks");
            Console.WriteLine("Example:");
            Console.WriteLine("  AsfMojoCmd -i test.wmv -tracks");

            Console.WriteLine("---------------------------");
            Console.WriteLine("Extracting a still frame from an offset:");
            Console.WriteLine("  AsfMojoCmd -i <filename> -t -start <start offset> [-w <pixel width>] -o <image output file>");
            Console.WriteLine("Example:");
            Console.WriteLine("  -i test.wmv -t  -start 52.3 -o test.jpg");

            Console.WriteLine("---------------------------");
            Console.WriteLine("Extracting a WAVE audio segment from an offset:");
            Console.WriteLine("  AsfMojoCmd -i <filename> -t -start <start offset> -end <end offset> -o <wav output file>");
            Console.WriteLine("Example:");
            Console.WriteLine("  -i test.wmv -a -start 5.0 -end 12.5 -o audio.wav");
            Console.WriteLine("---------------------------");
            Console.WriteLine("Displaying help:");
            Console.WriteLine("  AsfMojoCmd -?");
        }
    }
}
