﻿namespace Data.Xml.Engine

open FSharpx.Collections
open System.Xml

[<AutoOpen>]
module XmlModule = 

    // Represents a single attribute
    type XAttrib = XAttrib of string * string
                    override xat.ToString() = 
                        match xat with
                        | XAttrib (s,v) -> s + " " + v

    // List of attributes
    type XAttribs = XAttrib list 

    type XNode = 
        // Elment with its name and attributes
        | XElement of string * XAttribs 
        // End of the element 
        | XEndElement of string
        // Contents of the the element, can be CDATA or text
        | XText of string
        // Comment (skipped)
        | XComment of string
        // Document, containing element
        | XDocument 
        // Type of document, skipped
        | XDocType of string 
        // All other elements are skipped
        | XSkipped

        // Convert element to string
        override x.ToString() = 
            match x with 
            | XElement (s,ats)      -> (s,ats).ToString()
            | XEndElement s         -> ""
            | XText s               -> "Text:" + s
            | XComment s            -> "Comment:" + s
            | XDocument             -> "Document:"
            | XDocType _            -> ""
            | XSkipped              -> ""

        static member ofReader (reader:XmlReader) =
            let toXNode (reader:XmlReader) =
                match reader.NodeType with 
                | XmlNodeType.CDATA             -> XText reader.Value
                | XmlNodeType.Comment           -> XComment reader.Value
                | XmlNodeType.Document          -> XDocument 
                | XmlNodeType.DocumentType      -> XDocType reader.Value
                | XmlNodeType.Element           -> XElement (reader.LocalName, list.Empty)
                | XmlNodeType.EndElement        -> XEndElement reader.LocalName
                | XmlNodeType.Text              -> XText reader.Value
                | XmlNodeType.Whitespace        -> XSkipped
                | XmlNodeType.SignificantWhitespace -> XSkipped
                | XmlNodeType.ProcessingInstruction -> XSkipped
                | XmlNodeType.XmlDeclaration        -> XSkipped
                | XmlNodeType.EndEntity         -> failwith <| "Does not handle XmlNodeType.EndEntity"
                | XmlNodeType.Entity            -> failwith <| "Does not handle XmlNodeType.Entity"
                | XmlNodeType.EntityReference   -> failwith <| "Does not handle XmlNodeType.EntityReference"
                | XmlNodeType.Notation          -> failwith <| "Does not handle XmlNodeType.Notation"
                | XmlNodeType.None              -> failwith <| "XmlReader.Read is not called"
                | XmlNodeType.Attribute         -> failwith <| "Attributes should be handled by collect attributes"
                | XmlNodeType.DocumentFragment  -> failwith <| "Does not handle document fragment"
                | _                                 -> XSkipped

            let unfolder (reader:XmlReader) = 
                if reader.Read() then 
                    Some (toXNode reader, reader)
                else
                    None

            LazyList.unfold unfolder reader

            static member count (xs:XNode LazyList) = 
                    let rec count' xs i = 
                        match xs with
                        | LazyList.Nil -> i
                        | LazyList.Cons(y,ys) -> count' ys (i+1)
                    count' xs 0                    

    open System

    let rec debug (xs:XNode LazyList) = 

        match xs with 
        | LazyList.Nil  -> ()
        | LazyList.Cons(y,ys) -> Console.WriteLine("{0}", y)
                                 debug ys

    let count (xs:XNode LazyList) = 
        let rec count' xs i =
            match xs with 
            | LazyList.Nil -> i
            | LazyList.Cons(y,ys) -> count' ys (i+1)
        count' xs 0


    open FSharpx.Collections.Experimental
 
    type XmlTree = XmlTree of XNode RoseTree 