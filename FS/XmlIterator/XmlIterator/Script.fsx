// This file is a script that can be executed with the F# Interactive.  
// It can be used to explore and test the library project.
// Note that script files will not be part of the project build.

#load "XmlIterator.fs"

open System
open System.Xml
open Org.Eagle

let xml = "<Ship><Compartment/><Compartment></Compartment><Compartment/></Ship>"

let srdr = new System.IO.StringReader(xml)
let xrdr = XmlReader.Create(srdr)
let ns   = Xml.nodeStream xrdr

//let hs   = Xml.head ns
//let ts   = Xml.tail ns
//let hhs  = Xml.head (Xml.cons hs ts)




type Structure   = StructureT 

type Ship = | EmptyS

type Compartment = | CompartmentT of Ship

let shipP = Xml.createP "Ship"    (function () -> EmptyS) (function _ -> ())
let compP = Xml.createP "Compartment" (function (s:Ship) -> CompartmentT(s))

//let result = Xml.runNodeP ap ns

let parseShipP = Xml.seqP shipP (function s -> compP (function () -> s))

let parseShipP1 = Xml.seqP shipP (function s -> Xml.manyP (compP (function () -> s)))

type Vessel =   | VesselStart 
                | VesselEnd
                | CompartmentStart of Vessel
                | CompartmentEnd of Vessel
                | Compartments of Vessel * Vessel list

let vsP = Xml.createP1 (Xml.NodeStart("Ship")) (function () -> VesselStart)
let veP = Xml.createP1 (Xml.NodeEnd("Ship"))   (function () -> VesselEnd)
let cmsP= Xml.createP1 (Xml.NodeStart("Compartment")) (function () -> CompartmentStart VesselStart)
let cmeP= Xml.createP1 (Xml.NodeEnd("Compartment"))   

let cmP = Xml.seqP cmsP (function x -> cmeP (function () -> CompartmentEnd x))

let cmPs= Xml.manyP cmP

let vsC = Xml.seqP vsP (function v -> Xml.NodeP ( function nds -> match Xml.runNodeP cmPs nds with
                                                                        | (Some(xs),nd)     ->  (Some(Compartments(v,xs)),nd)
                                                                        | (None,nd)         ->  (None,nd) ) )