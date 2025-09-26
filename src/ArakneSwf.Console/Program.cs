using System.Diagnostics;
using SwfRender.Console;

const string command = "-e dieR --full-animation 70004.swf export";
var fakeArguments = command.Split(' ');
var time = Stopwatch.StartNew();

var extractCommand = new ExtractCommand();
extractCommand.Execute(args.Length == 0 ? args : fakeArguments);
time.Stop();

Console.WriteLine($"Done in {time.ElapsedMilliseconds} ms");