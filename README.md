XML Processing
==============

Many XML processing alternatives are possible with C#/F#. There are
Type Providers, Push/Pull readers, Document based readers. Linq
provides an alternative way of processing XML data.

Push/Pull readers provide an incremental way of reading XML files. It
is also cumbersome to write de/serialization. Linq can make use of
reader, however, in the end it ends up loading the document as
well. This can be a problem for large XML documents (> 100 Mb).

This project explores options to

* Have a approach based on XMLReader. We try to convert XMLReader to
  lazy list. The lazy list will also cache the states (unlike
  Sequence). It might be possible to create an approach where lazy
  list is used in a such a way to reduce memory (or stack) overhead,
  and at the same time provide a fast/incremental approach to
  deserialize Xml.
* The same framework should also be used to write Xml, thus providing
  bi-directional Xml processors.


Initially, this project will concentrate only on serialization and
deserialization of XML documents.



# Note #
This is based on previous implementation (still part of this repository but will be removed in future). The idea is to have seamless integration between user defined data type (can be in c#) and XML serialization and deserialization. The future plan includes generation of parsers from xsd, and filters.


