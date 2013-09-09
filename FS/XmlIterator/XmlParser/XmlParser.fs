namespace Institis.Parser.Example

#nowarn "40" 
open System
open Institis.FXml


module Parser  =

    open Example.AssetSpace

    let rec addC (c:Compartment) (cmps:Compartment list) = 
        match cmps with
            | x::xs         -> c.Add(x) |> ignore
                               addC c xs
            | []            -> c

    let rec addS (c:ExampleStorage) (cmps:Compartment list) = 
        match cmps with
            | x::xs         -> c.Add(x) |> ignore
                               addS c xs
            | []            -> c

    // This defines the parser for the compartment, it contains the logic to create new compartment and also 
    // to add child compartments
    let rec cmpP =  Xml.NodeParserBuilder() {
                let! c  = Xml.createP1 (Xml.NodeStart("Compartment")) (function () -> new Compartment())
                let! childCmps = Xml.manyP cmpP
                let! cs = Xml.createP1 (Xml.NodeEnd("Compartment"))   (function () -> addC c childCmps)
                return cs
            }


    // The parser for main element. The root element may contain many children. 
    // TODO: Add ability to add 
    // 1. Restrict number children
    // 2. More importantly, sometimes even sequence of elements.
    let shipP = Xml.NodeParserBuilder() { 
                let! s  = Xml.createP1 (Xml.NodeStart("Ship")) (function () -> new ExampleStorage() )
                let! cs = Xml.manyP cmpP
                return addS s cs
            }




