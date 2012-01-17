using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using KillerApps.Emulation.Atari.Lynx;
using System.Diagnostics;
using KillerApps.Emulation.Atari.Lynx.Tooling;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("AtariLynx.Tooling")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Killer-Apps")]
[assembly: AssemblyProduct("AtariLynx.Tooling")]
[assembly: AssemblyCopyright("Copyright © Killer-Apps 2012")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("70baa833-1034-4862-9aaf-95bcb52eace7")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

//[assembly: DebuggerTypeProxy(typeof(SpriteEngineDebugView), Target = typeof(SpriteEngine))]
[assembly: DebuggerVisualizer(typeof(SpriteEngineDebuggerVisualizer), typeof(SpriteEngineObjectSource),
	Target = typeof(SpriteEngine), Description = "Sprite Engine Debugger Visualizer")]
