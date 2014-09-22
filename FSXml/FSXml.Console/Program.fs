open System
open System.Xml
open Data.Xml.Engine

[<EntryPoint>]
let main argv = 
    if (argv.Length <> 1) then 
        printf "Insufficient arguments\n"
        -1
    else
        printfn "%A" (Array.get argv 0)
        let reader = XmlReader.Create(Array.get argv 0)
        let ctx = XNode.ofReader reader |> fun xs -> count xs 0
        printf "There are %d elements\n" ctx
        //XNode.ofReader reader |> debug
        reader.Close()
        0 // return an integer exit code
