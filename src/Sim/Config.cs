using System;
using System.IO;
using System.Reflection;
using Ramulator.Graphics;
using Ramulator.Mem;
using Ramulator.MemCtrl;
using Ramulator.MemSched;

namespace Ramulator.Sim
{
    public abstract class ConfigGroup
    {
        //verify member parameters; called after all options are parsed
        public abstract void finalize();

        //special switch for complex parameter initialization
        protected abstract bool set_special_param(string param, string val);

        public void set_param(string param, string val)
        {
            if (set_special_param(param, val))
                return;
            try
            {
                FieldInfo fi = GetType().GetField(param);
                Type t = fi.FieldType;

                if (t == typeof(int))
                    fi.SetValue(this, int.Parse(val));
                else if (t == typeof(uint))
                    fi.SetValue(this, uint.Parse(val));
                else if (t == typeof(long))
                    fi.SetValue(this, long.Parse(val));
                else if (t == typeof(ulong))
                    fi.SetValue(this, ulong.Parse(val));
                else if (t == typeof(double))
                    fi.SetValue(this, double.Parse(val));
                else if (t == typeof(bool))
                    fi.SetValue(this, bool.Parse(val));
                else if (t == typeof(string))
                    fi.SetValue(this, val);
                else if (t.BaseType == typeof(Enum))
                    fi.SetValue(this, Enum.Parse(t, val));
                else
                    throw new Exception(String.Format("Unhandled parameter type {0}", t));
            }
            catch (NullReferenceException e)
            {
                Console.WriteLine("Parameter {0} not found!", param);
                throw e;
            }
        }

        public void DumpJSON(TextWriter tw)
        {
            tw.WriteLine("{");

            bool first = true;
            foreach (FieldInfo fi in GetType().GetFields())
            {
                object o = fi.GetValue(this);
                if (!first) tw.WriteLine(",");
                tw.Write("\"{0}\":", fi.Name);
                DumpJSON(tw, o);
                first = false;
            }

            tw.WriteLine("\n}");
        }

        public void DumpJSON(TextWriter tw, object o)
        {
            if (o is StatObject)
            {
                ((StatObject)o).DumpJSON(tw);
            }
            else if (o is string)
            {
                tw.Write("\"{0}\"", (string)o);
            }
            else if (o is object[])
            {
                bool first = true;
                tw.Write("[");
                foreach (object elem in (object[])o)
                {
                    if (first) first = false;
                    else tw.Write(",");
                    DumpJSON(tw, elem);
                }
                tw.Write("]");
            }
            else if (o is object[,])
            {
                object[,] arr_2d = (object[,])o;
                int dim0 = arr_2d.GetLength(0);
                int dim1 = arr_2d.GetLength(1);
                object[][] arr = new object[dim0][];
                for (int i = 0; i < dim0; i++)
                {
                    arr[i] = new object[dim1];
                    for (int j = 0; j < dim1; j++)
                        arr[i][j] = arr_2d[i, j];
                }
                DumpJSON(tw, arr);
            }
            else if (o is object)
            {
                tw.Write("\"{0}\"", o.ToString());
            }
            else
                tw.Write("{0}", o.ToString());
        }
    }

    public class Config : ConfigGroup
    {
        public static ProcConfig proc = new ProcConfig();
        public static MemConfig mem = new MemConfig();
        public static MemCtrlConfig mctrl = new MemCtrlConfig();
        public static MemSchedConfig sched = new MemSchedConfig();
        public static GraphicsConfig gfx = new GraphicsConfig();

        // Simulation mode
        public enum SIM_TYPE { CYCLE, INST, COMPLETION };

        public static int N = 1;
        public static SIM_TYPE sim_type = SIM_TYPE.CYCLE;
        public static ulong sim_cycle_max = 100000000;
        public static ulong sim_inst_max = 0;
        public static ulong warmup_cycle_max = 200000000;

        public static string output = "output.txt";

        // Comma delimited list of dirs potentially containing traces
        public static string TraceDirs = "";

        // Empty means no debug dumps
        public static string debug_file_name = "";

        public static string[] traceFileNames;

        public void read(string[] args)
        {
            string[] traceArgs = null;
            int traceArgOffset = 0;
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-workload")
                {
                    string worksetFile = args[i + 1];
                    int index = int.Parse(args[i + 2]);
                    if (!File.Exists(args[i + 1]))
                        throw new Exception("Could not locate workset file " + worksetFile);
                    string[] lines = File.ReadAllLines(worksetFile);
                    if (TraceDirs == "")
                        TraceDirs = lines[0];

                    traceArgs = lines[index].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    traceArgOffset = 0;
                    i += 2;
                }
                else if (args[i] == "-traces")
                {
                    traceArgs = args;
                    traceArgOffset = i + 1;
                    break;
                }
                else
                {
                    set_module_param(args[i].Substring(1), args[i + 1]);
                    i++;
                }
            }

            if (traceArgs == null)
                throw new Exception("No traces specified");

            traceFileNames = new string[N];
            int traceCount = Math.Min(N, traceArgs.Length - traceArgOffset);
            if (traceCount < N)
                Console.Error.WriteLine("Warning: not enough trace files given (got {0}, wanted {1})", traceArgs.Length - traceArgOffset, N);
            if (traceCount > N)
                Console.Error.WriteLine("Warning: too many trace files given (got {0}, only wanted {1})", traceArgs.Length - traceArgOffset, N);

            for (int a = 0; a < traceCount; a++)
                traceFileNames[a] = traceArgs[traceArgOffset + a];
            for (int a = traceCount; a < N; a++)
                traceFileNames[a] = "null";

            //finalize
            finalize();
            proc.finalize();
            mem.finalize();
            mctrl.finalize();
            sched.finalize();
            gfx.finalize();
        }

        public void readConfig(string filename)
        {
            Char[] delims = new Char[] { '=' };
            StreamReader configReader = File.OpenText(filename);

            if (configReader == null)
                return;

            string buf;
            while (true)
            {
                buf = configReader.ReadLine();
                if (buf == null)
                    break;

                //remove comment
                int comment = buf.IndexOf("//");
                if (comment != -1)
                    buf = buf.Remove(comment).Trim();

                //skip empty line
                if (buf == "")
                    continue;

                string[] flags = buf.Split(delims, 2);
                if (flags.Length < 2) continue;
                set_module_param(flags[0].Trim(), flags[1].Trim());
            }
        }

        public void set_module_param(string param, string val)
        {
            Console.WriteLine("{0} <= {1}", param, val);

            if (!param.Contains("."))
            {
                set_param(param, val);
            }
            else
            {
                string name = param.Substring(0, param.IndexOf('.'));
                string subparam = param.Substring(param.IndexOf('.') + 1);
                FieldInfo fi = GetType().GetField(name);
                if (!(fi.GetValue(this) is ConfigGroup))
                {
                    throw new Exception(String.Format("Non-ConfigGroup indexed, of type {0}", fi.FieldType.ToString()));
                }
                ((ConfigGroup)fi.GetValue(this)).set_param(subparam, val);
            }
        }

        protected override bool set_special_param(string param, string val)
        {
            switch (param)
            {
                case "config":
                    readConfig(val); break;

                default:
                    return false;
            }
            return true;
        }

        public override void finalize()
        {
            if (File.Exists(Config.output))
            {
                Console.WriteLine("WARNING:Specified output file already exists! Aborting execution!");
                Environment.Exit(127);
            }
        }
    }
}