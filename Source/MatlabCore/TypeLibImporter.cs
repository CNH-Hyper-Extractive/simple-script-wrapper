
using System;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Reflection.Emit;

namespace KansasState.Ssw.MatlabCore
{
	/// <summary>
	/// This enum is a .NET version of the COM REGKIND
	/// enum used in conjunction with the LoadTypeLibEx()
	/// API COM library function.
	/// </summary>
	internal enum REGKIND
	{
		REGKIND_DEFAULT         = 0,
		REGKIND_REGISTER        = 1,
		REGKIND_NONE            = 2
	}

	#region Importer Helper Class Here!
	/// <summary>
	/// When importing a COM type lib, this sink will
	/// be called in case of error(s) or unresolved type lib references.
	/// </summary>
	internal class ImporterNotiferSink : ITypeLibImporterNotifySink
	{
		public void ReportEvent(ImporterEventKind eventKind, 
			int eventCode, string eventMsg)
		{
			// We don't really care which kind of error is 
			// sent.  Just print out the information.
			Console.WriteLine("Event reported: {0}", eventMsg);
		}
		public Assembly ResolveRef(object typeLib)
		{
			// Return a new Assembly based on the 
			// incoming UCOMITypeLib interface
			// (expressed as System.Object).
			// Delegate to helper function.
			Assembly nestedRef = Assembly.Load(        
				MyTlbImpApp.GenerateAssemblyFromTypeLib
				((UCOMITypeLib)typeLib));
			return nestedRef;
		}
	}
	#endregion 
	
	/// <summary>
	/// This class takes a COM type lib
	/// and programmatically builds an 
	/// .NET interop assembly.
	/// 
	/// The name and version of the resulting assembly
	/// is created by reading the COM type information.
	/// </summary>
	public class MyTlbImpApp
	{
		// Need to leverage the LoadTypeLibEx() API to do our dirty work.
		// Param 3: UCOMITypeLib is the .NET version of ITypeLib.
		[DllImport("oleaut32.dll", CharSet = CharSet.Unicode)] 
		private static extern void LoadTypeLibEx(string strTypeLibName, 
		  REGKIND regKind, out UCOMITypeLib TypeLib);

		public static void generateAssembly(string typeLibraryFilePath)
		{
			// Check for -NOGUI switch.
			/*bool usingGUI = false;
			for(int i = 0; i < args.Length; i++)
			{
				if(args[i] == "-NOGUI")
					usingGUI = false;
			}*/

			// Gather user input.
            string pathToComServer = typeLibraryFilePath; // @"C:\Program Files\MATLAB\R2010a\bin\win64\mlapp.tlb";

			/*if(!usingGUI)
			{
				// Prompt for path.
				Console.WriteLine("Please enter path to COM type information.");
				Console.WriteLine(@"Example: C:\Stuff\Blah\Debug\MyComServer.dll");
				Console.Write("Path: ");
				pathToComServer = Console.ReadLine();				
			}
			else
			{
				Console.WriteLine("Pick a COM server...");
				OpenFileDialog d = new OpenFileDialog();
				if(d.ShowDialog() == DialogResult.OK)
					pathToComServer = d.FileName;
			}*/
			
			// Show path to COM server.
			//Console.WriteLine("Path: {0}\n", pathToComServer);

			// Generate an .NET assembly (no strong name).
			UCOMITypeLib theTypeLib = LoadCOMTypeInfo(pathToComServer);
			if(theTypeLib == null)
				return;

			GenerateAssemblyFromTypeLib(theTypeLib);
			//Console.WriteLine("All done!");

            //Console.ReadKey();
		}

		/// <summary>
		/// This method creates (and saves) a .NET assembly given a COM 
		/// type library.
		/// The name of the assembly is: "interop.[ComLibName].dll"
		/// The version of the assembly is: [ComLib.Major], [ComLib.Minor], 0, 0
		/// </summary>
		public static UCOMITypeLib LoadCOMTypeInfo(string pathToComServer)
		{
			// Load type information for COM server.
			UCOMITypeLib typLib = null;
			try
			{
				LoadTypeLibEx(pathToComServer, REGKIND.REGKIND_NONE, out typLib);
				string strName, strDoc, strHelpFile;
				int helpCtx;
				Console.WriteLine("COM Library Description:");
				typLib.GetDocumentation(-1, out strName, out strDoc, out helpCtx, out strHelpFile);
				Console.WriteLine("->Name: {0}", strName);
				Console.WriteLine("->Doc: {0}", strDoc);
				Console.WriteLine("->Help Context: {0}", helpCtx.ToString());
				Console.WriteLine("->Help File: {0}", strHelpFile);

			}
			catch
			{ 
				Console.WriteLine("ugh...can't load COM type info!");
			}
			return typLib;
		}

		
		public static string GenerateAssemblyFromTypeLib(UCOMITypeLib typLib)
		{
			// Need a sink for the TypeLibConverter.
			ImporterNotiferSink sink = new ImporterNotiferSink();
		
			#region Notes on Strong Name...
			/* Don't need a *.snk file if you are not building a primary interop asm.
			 * Just send in nulls to the ConvertTypeLibToAssembly() method.
			 * But if you have a valid *.snk file, you can use it as so:
			 *
			 * // Object representation of *.snk file.
			 * FileStream fs = File.Open(@"D:\APress Books\InteropBook\MyTypeLibImporter\bin\Debug\theKey.snk", 
			 * 							FileMode.Open);
			 * System.Reflection.StrongNameKeyPair keyPair = new StrongNameKeyPair(fs);
			 */
			#endregion 

			// This class will covert COM type info into
			// .NET metadata and vice-versa.
			TypeLibConverter tlc = new TypeLibConverter();

			// Generate name of the assembly.
			string typeLibName = Marshal.GetTypeLibName(typLib);
			string asmName = "Interop." + typeLibName + ".dll"; 

			// Now make the assembly based on COM type information.
			AssemblyBuilder asmBuilder = tlc.ConvertTypeLibToAssembly((UCOMITypeLib)typLib, 
				asmName,
				TypeLibImporterFlags.SafeArrayAsSystemArray,  
				sink, 
				null,		// If you have a strong name: keyPair.PublicKey, 
				null,		// If you have a strong name: keyPair  
				typeLibName, // Namespace name is same as file name.
				null);		 // null = (typeLibMajor.typeLibMinor.0.0)

			// Save the assembly in the app directory!
			asmBuilder.Save(asmName);

			// return Assembly ref to call.
			return asmName;
		}
 	}
}
