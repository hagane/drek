namespace Drek

type Project(name: string, tasks: List<Task>) = 
    new(name) = Project(name, [])

    member this.AddTask(task) = Project(name, task :: tasks)
    static member (<<+) (project: Project, task: Task) = project.AddTask(task)

    member public this.RunTask(taskName: string) = 
        let result = 
            match List.tryFind (fun (task:Task) -> task.name.Equals(taskName)) tasks with
            | Some task -> task.Execute(name)
            | None -> "NOT FOUND"
        printfn "[%s] %s > %s" name taskName result
