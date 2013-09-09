FXmlNodeStream
==============

FXmlNodeStream is a combinator library that maps over XmlReader to create an top
down XML iterator.  


One can use parser combinators for parsing a set of input elements. For XML
generic parsers exist to parse XML, and hence there is no need to parse XML
grammar as such.

However, to create complex hierarchy of the objects, there are lots of
challenges faced. Many utilities generate the Serialization/Deserialization code
to create the object hierarchy. This is not desirable since the object
representation in the application may differ greatly from the XML schema.

Also these utilities tend to be DOM parsers that consume lot of memory. One may
wish to use push-pull parsers (E.g. XmlReader) that give the performance as well
as speed. However to write a code to create user's objects from the Xml using
XmlReader is a tedius job. And supposing one is working with a team (which I
was, unfortunately) which changed schema often (it was under design, and we were
forced to use it parallelly), this job becomes challenging. Even to say that to
incorporate the next version of schema is like pointing at many code changes. 

The idea that is presented here is simple. Use XmlReader as a basic XML parser,
and define XML as a stream of Xml elements such as Xml Start Node, Xml End Node,
Xml Comment, Attribute etc. 

Now define the Xml Schema as a combinator over the set of nodes, along with
actions to be peformed. Each library also makes the container element available
to the children, making it possible to add cross-referencing functionality while
creating user defined object hierarchy.

The goal is to 

1. Decouple the XML and the user defined objects/classes.
2. Define the coupling as a combinator using this library.

There are limitations to be overcome. The library is not a strict one and does
not have full functionality to check number of children and sequence etc.
However, one can use validating reader to overcome this. But in future, I'd like
to add these validating combinators as well. 




