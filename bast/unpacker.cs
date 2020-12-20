using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Be.IO;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace xayrga.bast
{
    class unpacker
    {
        private Stream stream;
        private BeBinaryReader reader;

        public JAudioTableType getType(BeBinaryReader reader)
        {
            return JAudioTableType.AUDIOSOUNDEFFECT;
        }

        public void doUnpack(string file, string projectDir)
        {
            cmdarg.assert(!File.Exists(file), $"File {file} does not exist");
            stream = File.Open(file, FileMode.Open);
            reader = new BeBinaryReader(stream);
            Directory.CreateDirectory(projectDir);
            Directory.CreateDirectory($"{projectDir}/includes");
            var unpackMode = getType(reader);
            switch (unpackMode)
            {
                case JAudioTableType.AUDIOSOUNDEFFECT:
                    unpackJASE(projectDir);
                    break;
            }
        }

        private void unpackJASE(string projectDir)
        {
            var wl = new JASE();
            wl.readInfo(reader, false);
            wl.loadCategories(reader, false);

            var catSorted = wl.Categories;
            // sort 
            for (int i = 0; i < 0x12; i++) // a third __fucking iteration__ on these stupid vectors.
            {
                for (int j = 0; j < 0x12; j++)
                {
                    var current = catSorted[i]; // Grab current oscillator vector, notice the for loop starts at 1
                    var cmp = catSorted[j]; // Grab the previous object
                    if (cmp.startID > current.startID) // if its time is greater than ours
                    {
                        catSorted[j] = current; // shift us down
                        catSorted[i] = cmp; // shift it up
                    }
                }
            }
            var totalID = 0;
             // load indices 
            for (int i = 0; i < catSorted.Length; i++)
            {
                var cat = catSorted[i];
                if (cat.count > 0)
                {
                    for (int q = 0; q < cat.count; q++)
                    {
                        cat.sounds[q] = new JASESound();
                        cat.sounds[q].readInfo(reader, false);
                        cat.sounds[q].id = totalID;
                        cat.sounds[q].index = q;
                        totalID++;
                    }
                }
            }
            // unsort again, lol
            for (int i = 0; i < 0x12; i++) // a third __fucking iteration__ on these stupid vectors.
            {
                for (int j = 0; j < 0x12; j++)
                {
                    var current = catSorted[i]; // Grab current oscillator vector, notice the for loop starts at 1
                    var cmp = catSorted[j]; // Grab the previous object
                    if (cmp.index > current.index) // if its time is greater than ours
                    {
                        catSorted[j] = current; // shift us down
                        catSorted[i] = cmp; // shift it up
                    }
                }
            }

            var w = new JASEProject();
            w.version = wl.u1;
            w.revision = wl.u2;
            w.includes = new string[0x12]; 

            for (int i=0; i < 0x12; i++)
            {
                var cat = catSorted[i];
                if (cat!=null && cat.count > 0 )
                {
                    var catSer = JsonConvert.SerializeObject(cat,Formatting.Indented);
                    File.WriteAllText($"{projectDir}/includes/0x{i + 1:X}.json", catSer);
                    w.includes[i] = $"includes/0x{i + 1:X}.json";
                }
            }
            var outfile = JsonConvert.SerializeObject(w, Formatting.Indented);
            File.WriteAllText($"{projectDir}/root.json",outfile);
        }
    }
}
