

using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif

[assembly: AssemblyTitle("Monkeyspeak")]
[assembly: AssemblyDescription("Scripting language that is very user friendly.")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("Monkeyspeak")]
[assembly: AssemblyCopyright("Copyright ©  2017")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("bedba5f0-e0cf-4ec2-b4bc-bcddcdaaca42")]

[assembly: AssemblyVersion("7.0.10.28723")]
[assembly: AssemblyFileVersion("7.0.10.28723")]

[assembly: AssemblyDelaySign(false)]
[assembly: AssemblyKeyFile("Monkeyspeak.snk")]
[assembly: AssemblyKeyName("")]