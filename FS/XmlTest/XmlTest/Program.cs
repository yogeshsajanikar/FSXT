using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Institis.FXml;
using Institis.Parser.Example;

namespace XmlTest
{
    class Program
    {
        static void Main(string[] args)
        {
            using(var reader = XmlReader.Create(args[0]))
            //var xml = "<Ship><Compartment/><Compartment><Compartment/></Compartment><Compartment/></Ship>";
            //using(var srdr = new System.IO.StringReader(xml))
            //using (var reader = XmlReader.Create(srdr))
            //{
            //    var node = Xml.first(reader);

            //    int count = 0;
            //    foreach (var n in Xml.nodeSeq(node))
            //    {
            //        System.Console.WriteLine(Xml.toString(n));
            //        count++;
            //    }

            //    Console.WriteLine("Total nodes = {0}", count);
            //}
            {
                // Create the stream and run the ship parser over the node stream. 
                var ns = Xml.nodeStream(reader);
                var p = Test.run(Parser.shipP, ns);
                Console.WriteLine(p);
            }
        }
    }
}
