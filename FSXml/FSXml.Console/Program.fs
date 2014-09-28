open System
open System.IO
open System.Xml
open Data.Xml.Engine
open Data.Parser
open Data.Parser.Xml


let test1 () = 
    let xmlstr = "<A>Text</A>"
    let strreader = new StringReader(xmlstr)
    let xmlreader = XmlReader.Create(strreader :> TextReader )
    let parser = simpleElement "A" 
    let xlist  = XNode.ofReader xmlreader
    runTransformer parser xlist (liftState [])

let test2 () = 
    let xmlstr = "<A><B>Text</B></A>"
    let strreader = new StringReader(xmlstr)
    let xmlreader = XmlReader.Create(strreader :> TextReader )
    let parser = simpleElement "B" 
    let parser1 = xbracket (xstart "A") parser (xend "A")
    let xlist  = XNode.ofReader xmlreader
    runTransformer parser1 xlist (liftState [])


[<EntryPoint>]
let main argv = 
    test1 () |> fun out -> printf "%A\n" out.Out 
    test2 () |> fun out -> printf "%A\n" out.Out 
    0
(* 
    if (argv.Length <> 1) then 
        printf "Insufficient arguments"
        -1
    else
        printfn "%A" (Array.get argv 0)
        let reader = XmlReader.Create(Array.get argv 0)
        //XNode.ofReader reader |> count |> printf "there are %d elements!" 
        XNode.ofReader reader |> debug 
        reader.Close()
        0 // return an integer exit code
        *)