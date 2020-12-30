using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Be.IO;

namespace xayrga.bast
{
    class Program
    {

        static void Main(string[] args)
        {

            cmdarg.cmdargs = args; // load command line argument system
            var operation = cmdarg.assertArg(0, "Operation");
            switch (operation)
            {
                case "unpack":
                    {
                        var inFile = cmdarg.assertArg(1, "Input File");
                        var projDir = cmdarg.assertArg(2, "Project Folder");
                        var unpk = new unpacker();

                        unpk.doUnpack(inFile, projDir);
                    }
                    break;
                case "pack":
                    {
                        var outFile = cmdarg.assertArg(2, "Output File");
                        var projDir = cmdarg.assertArg(1, "Project Folder");
                        var packOrder = cmdarg.tryArg(3,"! PackOrder is not specified and is highly recommended.");
                        if (packOrder == null)
                            packOrder = "guess";
                        var pck = new packer();
                        pck.doPack(projDir, outFile,packOrder);
                    }
                    break;
                case "help":
                    printHelp();
                    break;
                default:
                    Console.WriteLine("Unknown operation, try 'bast help'");
                    break;
            }
        
        }

        static void printHelp()
        {
            Console.WriteLine("bast");
            Console.WriteLine("syntax:");
            Console.WriteLine();
            Console.WriteLine("bast unpack <bst file> <output folder>");
            Console.WriteLine("bast pack <project folder> <bst file>");
        }
    }
}
