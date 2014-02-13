using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace KansasState.Ssw.MatlabCore
{
    /// <summary>
    /// Provides the MLApp.MLAppClass as a dynamically-loaded assembly so that
    /// it will work with any version of MATLAB installed on the client
    /// machine.
    /// </summary>
    public class DynamicMLAppClass
    {
        object instance;
        Type type;

        public DynamicMLAppClass()
        {
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Assembly a = Assembly.LoadFile(Path.Combine(path, "Interop.MLApp.dll"));
            Type t1 = a.GetType("MLApp.MLAppClass");
            this.instance = Activator.CreateInstance(t1);
            this.type = this.instance.GetType();
        }

        public void Execute(string cmd)
        {
            this.type.InvokeMember("Execute", BindingFlags.Default | BindingFlags.InvokeMethod, null, this.instance, new object[] { cmd });
        }

        public void Quit()
        {
            this.type.InvokeMember("Quit", BindingFlags.Default | BindingFlags.InvokeMethod, null, this.instance, null);
        }

        public void PutFullMatrix(string name, string workspace, Array reals, Array imaginary)
        {
            object[] args = new object[] { name, workspace, reals, imaginary };
            this.type.InvokeMember("PutFullMatrix", BindingFlags.Default | BindingFlags.InvokeMethod, null, this.instance, args);
        }

        public void GetFullMatrix(string name, string workspace, ref Array reals, ref Array imaginary)
        {
            Array a1 = new double[reals.Length];
            Array a2 = new double[imaginary.Length];
            object[] args = { name, workspace, a1, a2 };
            ParameterModifier p = new ParameterModifier(4);
            p[0] = false;
            p[1] = false;
            p[2] = true;
            p[3] = true;
            ParameterModifier[] mods = { p };

            this.type.InvokeMember("GetFullMatrix", BindingFlags.InvokeMethod, null, this.instance, args, mods, null, null);

            reals = (Array)args[2];
        }
    }
}