﻿namespace Drek

open System
open System.Diagnostics
open System.IO

type ExecutionContext = string

[<AbstractClassAttribute>]
type Task(name: string) =
    member this.name = name
    abstract member Execute: ExecutionContext -> string

type Target = | Executable | ClassLibrary
type BuildTask(target: Target, sources: List<string>, references: List<string>, output: string) = 
    inherit Task("build")
    new (target: Target) = BuildTask(target, [], [], "./target/")

    member this.AddFile(file: string) = this.AddFiles [file]
    member this.AddFiles(files: List<string>) = BuildTask(target, sources @ files, references, output)

    member this.AddReference(ref: string) = this.AddReferences [ref]
    member this.AddReferences(refs: List<string>) = BuildTask(target, sources, references @ refs, output)

    member this.WithOutput(output: string) = BuildTask(target, sources, references, output)

    static member (<+) (task: BuildTask, file: string) = task.AddFile file 
    static member (<++) (task: BuildTask, files: List<string>) = task.AddFiles files

    static member (<?) (task: BuildTask, ref: string) = task.AddReference ref

    static member (=>) (task: BuildTask, output: string) = task.WithOutput output

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
        "OK"

type CleanTask(cleanDirs: List<string>) =
    inherit Task("clean")
    new () = CleanTask([])
    member this.AddDir(path: string) = CleanTask (path :: cleanDirs)
    static member (<+) (task: CleanTask, path: string) = task.AddDir path
    override this.Execute(context) =
        for path in cleanDirs do Directory.Delete(path, true)
        "OK"