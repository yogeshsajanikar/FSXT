// Learn more about F# at http://fsharp.net

open System.Xml
open System

type ShipKey = | GenericSystemKey 
               | CoordinateSystemKey
               | RootSystemKey
               | DetailsKey
               | CatalogKey

type OID    = | OIDType of string
type Name   = | NameType of string
type IdType = { oid : OID; name : Name }

type GenericSystemReader  = | GSReader of XmlReader * GenericSystemReader
                            | GSNullReader

type GenericSystem = | GSType of IdType
                     | GSNull


type RootSystemReader     = | RSType of XmlDocument * GenericSystem

let attributeReader (xmlReader:XmlReader) = 
    if xmlReader.HasAttributes then
        let mp = Map.empty 
        while xmlReader.MoveToAttribute do
            mp = Map.add xmlReader.Name xmlReader.Value mp
        done
        mp
    else
        Map.empty

let idReader (xmlReader:XmlReader) = 
    let! mp = attributeReader xmlReader
    Map.tryFind 


let gsXmlReader (xmlReader:XmlReader) = seq  {

    let! mp = attributeReader xmlReader


    
    while xmlReader.Read() do 
        match xmlReader.NodeType with
            XmlNodeType.Element -> 
    done
    }

let seqXml (xmlFileName:string) =  seq {
            use xmlReader = XmlReader.Create(xmlFileName)
            while xmlReader.Read() do
                if 0 = System.String.Compare(xmlReader.Name, "GenericSystem") then
                    yield (GenericSystemKey, xmlReader) 

                elif 0 = System.String.Compare(xmlReader.Name, "RootSystem") then
                    yield (RootSystemKey, xmlReader)
            done
        }




type ShipValue = | GSValue of GenericSystem
                 | RSValue of RootSystem

// We will only process root nodes 

for i in seqXml @"C:\Users\Yogesh Sajanikar\Projects\FS\MapReduce\Data\Temp.xml" do
    if true <> System.String.IsNullOrEmpty i then
        Console.WriteLine i
done

