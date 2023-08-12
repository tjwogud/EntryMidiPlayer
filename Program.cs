using NAudio.Midi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace EntryMidiPlayer
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            string[] noteTypes = { "C", "Db", "D", "Eb", "E", "F", "Gb", "G", "Ab", "A", "Bb", "B" };
            int limit = 361;

            Console.Write("파일 이름을 입력하세요: ");
            string path = Console.ReadLine();
            if (!File.Exists(path))
                path += ".mid";
            MidiFile midi = new MidiFile(path, false);
            midi.Events.MidiFileType = 0;
            List<string> notes = new List<string>();
            List<MidiEvent> events = midi.Events.SelectMany(_ => _).ToList();
            long max = 0;
            List<long> realTimes = new List<long>();
            double lastRealTime = 0;
            double lastAbsoluteTime = 0;
            double currentMicroSecondsPerTick = 0;
            Console.Write("\r로딩중...");
            for (int i = 0; i < events.Count; i++)
            {
                if (events[i].AbsoluteTime > lastAbsoluteTime)
                {
                    lastRealTime += (events[i].AbsoluteTime - lastAbsoluteTime) * currentMicroSecondsPerTick;
                }
                lastAbsoluteTime = events[i].AbsoluteTime;
                if (events[i] is NoteOnEvent e1 && e1.OffEvent != null)
                {
                    int octave = e1.NoteNumber / 12 - 1;
                    string noteType = noteTypes[e1.NoteNumber % 12];
                    long time = (long)(lastRealTime / 1000);
                    realTimes.Add(time);
                    notes.Add(noteType + Math.Min(Math.Max(octave, 1), 7) + ":" + time);
                    if (max < time)
                        max = time;
                    Console.Write($"\r{Math.Round((double)(i + 1) / events.Count * 100)}% 완료...");
                }
                else if (events[i] is TempoEvent e2)
                {
                    currentMicroSecondsPerTick = (double)e2.MicrosecondsPerQuarterNote / midi.DeltaTicksPerQuarterNote;
                }
            }

            long noteLength = 2000;
            for (int i = limit; i < realTimes.Count; i++)
            {
                long l = realTimes[i - limit] + 2000 - realTimes[i] - 100;
                if (l > 0)
                    noteLength -= l;
            }

            string result = noteLength + "," + (max + 1000) + "|" + string.Join(",", notes) + ",end";

            Clipboard.SetText(result);
            Console.WriteLine("\r완료!\t\t\t");
            Console.WriteLine("복사되었습니다.");
            Console.WriteLine();
            Console.Write("엔터 키를 눌러 종료 . . .");
            Console.Read();
        }
    }
}
