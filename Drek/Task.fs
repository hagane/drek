namespace Drek

open System
open System.Diagnostics
open System.IO

type ExecutionContext = string
type Target = | Executable | ClassLibrary

[<AbstractClassAttribute>]
type Task(name: string) =
    member this.name = name
    abstract member Execute: ExecutionContext -> string

[<AbstractClassAttribute>]
type BuildTask(target: Target, sources: List<string>, references: List<string>, output: string) = 
    inherit Task("build")
    new (target: Target) = BuildTask(target, [], [], "./target/")
    abstract member Construct: Target -> List<string> -> List<string> -> string -> BuildTask

    member this.AddFile(file: string) = this.AddFiles [file]
    member this.AddFiles(files: List<string>) = this.Construct target (sources @ files) references output

    member this.AddReference(ref: string) = this.AddReferences [ref]
    member this.AddReferences(refs: List<string>) = this.Construct target sources (references @ refs) output

    member this.WithOutput(output: string) = this.Construct target sources references output

    static member (<+) (task: BuildTask, file: string) = task.AddFile file 
    static member (<++) (task: BuildTask, files: List<string>) = task.AddFiles files

    static member (<?) (task: BuildTask, ref: string) = task.AddReference ref

    static member (=>) (task: BuildTask, output: string) = task.WithOutput output

type CleanTask(cleanDirs: List<string>) =
    inherit Task("clean")
    new () = CleanTask([])
    member this.AddDir(path: string) = CleanTask (path :: cleanDirs)
    static member (<+) (task: CleanTask, path: string) = task.AddDir path
    override this.Execute(context) =
        for path in cleanDirs do Directory.Delete(path, true)
        "OK"