// Yogesh Sajanikar (02/2013)
#load "XmlIterator.fs"

#nowarn "40"

open System
open System.Xml
open Institis.FXml

let builder = new Xml.NodeParserBuilder() 

let rec cmpP =  Xml.NodeParserBuilder() {
                let! c  = Xml.createP1 (Xml.NodeStart("Compartment")) (function () -> "Compartment")
                let! childCmps = Xml.manyP cmpP
                let! cs = Xml.createP1 (Xml.NodeEnd("Compartment"))   (function () -> "CompartmentE")
                return String.Concat("{", c,":",cs, "}")
            }

let shipP = Xml.NodeParserBuilder() { 
                let! s  = Xml.createP1 (Xml.NodeStart("Ship")) (function () -> "Ship")
                let! cs = Xml.manyP cmpP
                return String.Concat(s,"{", cs.ToString(), "}")
            }

let xml = "<Ship><Compartment/><Compartment><Compartment/></Compartment><Compartment/></Ship>"

let srdr = new System.IO.StringReader(xml)
let xrdr = XmlReader.Create(srdr)
let ns   = Xml.nodeStream xrdr

let (rs,nd) = Xml.runNodeP shipP ns
