namespace Drek

open System;
open System.Diagnostics;
open System.IO;

type FSharpBuild(target: Target, sources: List<string>, references: List<string>, output: string) =
    inherit BuildTask(target, sources, references, output)

    override this.Construct(target) (sources) (references) (output) =
        FSharpBuild(target, sources, references, output) :> BuildTask

    override this.Execute(context) =
        let targetStr = 
            match target with
            | ClassLibrary -> "library"
            | Executable -> "exe"
        let extension =
            match target with
            | ClassLibrary -> "dll"
            | Executable -> "exe"
        let outFile = output + "/" + context + "." + extension

        let fsharpHome = Environment.GetEnvironmentVariable("FSHARP_HOME")
        let command = sprintf "\"%s/fsc.exe\"" fsharpHome
        let args = (sprintf " --target:%s" targetStr) + (sprintf " -o:%s" outFile)
        let sourceFiles = String.concat " " sources
        ignore (Directory.CreateDirectory output)
        let proc = Process.Start(command, sprintf "%s %s" args sourceFiles)
        proc.WaitForExit();
        match proc.ExitCode with
        | 0 -> "OK"
        | _ -> "ERROR"

