open System
open System.Xml
open Data.Xml.Engine

[<EntryPoint>]
let main argv = 
    if (argv.Length <> 1) then 
        printf "Insufficient arguments"
        -1
    else
        printfn "%A" (Array.get argv 0)
        let reader = XmlReader.Create(Array.get argv 0)
        XNode.ofReader reader |> count |> printf "there are %d elements!" 
        reader.Close()
        0 // return an integer exit code
