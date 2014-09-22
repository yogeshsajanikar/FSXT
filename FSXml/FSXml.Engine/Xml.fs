namespace Data.Xml.Engine

open FSharpx.Collections
open System.Xml

[<AutoOpen>]
module XmlModule = 

    type XAttrib = XAttrib of string * string
                    override xat.ToString() = 
                        match xat with
                        | XAttrib (s,v) -> s + " " + v

    type XAttribs = XAttrib list 

    type XNode = 
        | XElement of string * XAttribs * XNode LazyList
        | XEndElement of string
        | XText of string
        | XComment of string
        | XDocument 
        | XDocType of string 
        | XSkipped

        override x.ToString() = 
            match x with 
            | XElement (s,ats,xs)   -> (s,ats,xs).ToString()
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
                | XmlNodeType.Element           -> XElement (reader.LocalName, list.Empty, LazyList.empty)
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


            let isElement (x:XNode) = 
                match x with 
                | XElement _    -> true
                | _             -> false

            let rec unfolder ((reader,state) : XmlReader * bool)  = 
                if state then
                    if reader.IsEmptyElement then
                        Some (XEndElement reader.LocalName, (reader, false))
                    else
                        unfolder (reader, false)
                else
                    if reader.Read() then 
                        let x = toXNode reader
                        Some (x, (reader, isElement x))
                    else
                        None

            LazyList.unfold unfolder (reader,false)

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

    let rec count (xs:XNode LazyList) i = 
            match xs with 
            | LazyList.Nil -> i
            | LazyList.Cons(_,ys) -> count ys (i+1)
