//  -----------------------------------------------------------------------
//   Copyright (c) 2014 Tom Bulatewicz, Kansas State University
//   
//   Permission is hereby granted, free of charge, to any person obtaining a copy
//   of this software and associated documentation files (the "Software"), to deal
//   in the Software without restriction, including without limitation the rights
//   to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//   copies of the Software, and to permit persons to whom the Software is
//   furnished to do so, subject to the following conditions:
//   
//   The above copyright notice and this permission notice shall be included in all
//   copies or substantial portions of the Software.
//   
//   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//   IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//   FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//   AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//   LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//   OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//   SOFTWARE.
//  -----------------------------------------------------------------------

using System;
using System.IO;
using System.Reflection;

namespace KansasState.Ssw.MatlabCore
{
    /// <summary>
    ///     Provides the MLApp.MLAppClass as a dynamically-loaded assembly so that
    ///     it will work with any version of MATLAB installed on the client
    ///     machine.
    /// </summary>
    public class DynamicMLAppClass
    {
        private readonly object instance;
        private readonly Type type;

        public DynamicMLAppClass()
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var a = Assembly.LoadFile(Path.Combine(path, "Interop.MLApp.dll"));
            var t1 = a.GetType("MLApp.MLAppClass");
            instance = Activator.CreateInstance(t1);
            type = instance.GetType();
        }

        public void Execute(string cmd)
        {
            type.InvokeMember("Execute", BindingFlags.Default | BindingFlags.InvokeMethod, null, instance, new object[] {cmd});
        }

        public void Quit()
        {
            type.InvokeMember("Quit", BindingFlags.Default | BindingFlags.InvokeMethod, null, instance, null);
        }

        public void PutFullMatrix(string name, string workspace, Array reals, Array imaginary)
        {
            object[] args = {name, workspace, reals, imaginary};
            type.InvokeMember("PutFullMatrix", BindingFlags.Default | BindingFlags.InvokeMethod, null, instance, args);
        }

        public void GetFullMatrix(string name, string workspace, ref Array reals, ref Array imaginary)
        {
            Array a1 = new double[reals.Length];
            Array a2 = new double[imaginary.Length];
            object[] args = {name, workspace, a1, a2};
            var p = new ParameterModifier(4);
            p[0] = false;
            p[1] = false;
            p[2] = true;
            p[3] = true;
            ParameterModifier[] mods = {p};

            type.InvokeMember("GetFullMatrix", BindingFlags.InvokeMethod, null, instance, args, mods, null, null);

            reals = (Array)args[2];
        }
    }
}