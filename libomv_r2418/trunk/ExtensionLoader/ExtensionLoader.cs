/*
 * Copyright (c) 2008 John Hurliman
 * 
 * Permission is hereby granted, free of charge, to any person
 * obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use,
 * copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following
 * conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.Reflection;
using System.CodeDom.Compiler;
using System.IO;
using System.Text;

namespace ExtensionLoader
{
    /// <summary>
    /// Exception thrown when there is a problem with an extension
    /// </summary>
    public class ExtensionException : Exception
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Exception message</param>
        public ExtensionException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Exception that triggered this exception</param>
        public ExtensionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// The ExtensionLoader class. This static class defines functions for
    /// finding and loading extensions (plugins) from running code, dynamic
    /// link libraries, and source code. The owning class is used as the type;
    /// for example if your application is MyApp, use
    /// ExtensionLoader&lt;MyApp&gt;
    /// </summary>
    /// <typeparam name="TOwner"></typeparam>
    public static class ExtensionLoader<TOwner>
    {
        /// <summary>Currently loaded extensions</summary>
        public static List<IExtension<TOwner>> Extensions;
        
        static CodeDomProvider CSCompiler;
        static CompilerParameters CSCompilerParams;

        static ExtensionLoader()
        {
            Extensions = new List<IExtension<TOwner>>();

            CSCompiler = CodeDomProvider.CreateProvider("C#");

            CSCompilerParams = new CompilerParameters();
            CSCompilerParams.GenerateExecutable = false;
            CSCompilerParams.GenerateInMemory = true;
            if (System.Diagnostics.Debugger.IsAttached)
                CSCompilerParams.IncludeDebugInformation = true;
            else
                CSCompilerParams.IncludeDebugInformation = false;
        }

        /// <summary>
        /// Load extensions within the given assembly, from assembly files in
        /// a given directory, and from source code files in a given directory
        /// </summary>
        /// <param name="assembly">Main assembly to load extensions from</param>
        /// <param name="path">Directory to load assembly and source code
        /// extensions from</param>
        /// <param name="extensionList">An optional whitelist of extension names
        /// to load. Only the class name is needed for each extension, not the
        /// full name including namespaces</param>
        /// <param name="referencedAssemblies">List of assemblies the
        /// extensions need references to. For example: System, System.Xml, 
        /// ExtensionLoader</param>
        /// <param name="assemblySearchPattern">Search pattern for extension
        /// dlls, for example MyApp.Extension.*.dll</param>
        /// <param name="sourceSearchPattern">Search pattern for extension
        /// source code files, for example MyApp.Extension.*.cs</param>
        /// <param name="assignablesParent">The object containing the 
        /// assignable interfaces</param>
        /// <param name="assignableInterfaces">A list of interface references
        /// to assign extensions to</param>
        public static void LoadAllExtensions(Assembly assembly, string path, List<string> extensionList,
            List<string> referencedAssemblies, string assemblySearchPattern, string sourceSearchPattern,
            object assignablesParent, List<FieldInfo> assignableInterfaces)
        {
            // Add referenced assemblies to the C# compiler
            CSCompilerParams.ReferencedAssemblies.Clear();
            if (referencedAssemblies != null)
            {
                for (int i = 0; i < referencedAssemblies.Count; i++)
                    CSCompilerParams.ReferencedAssemblies.Add(referencedAssemblies[i]);
            }

            // Load internal extensions
            LoadAssemblyExtensions(assembly, extensionList);

            // Load extensions from external assemblies
            List<string> extensionNames = ListExtensionAssemblies(path, assemblySearchPattern);
            foreach (string name in extensionNames)
                LoadAssemblyExtensions(Assembly.LoadFile(name), extensionList);

            // Load extensions from external code files
            extensionNames = ListExtensionSourceFiles(path, sourceSearchPattern);
            foreach (string name in extensionNames)
            {
                CompilerResults results = CSCompiler.CompileAssemblyFromFile(CSCompilerParams, name);
                if (results.Errors.Count == 0)
                {
                    LoadAssemblyExtensions(results.CompiledAssembly, extensionList);
                }
                else
                {
                    StringBuilder errors = new StringBuilder();
                    errors.AppendLine("Error(s) compiling " + name);
                    foreach (CompilerError error in results.Errors)
                        errors.AppendFormat(" Line {0}: {1}{2}", error.Line, error.ErrorText, Environment.NewLine);
                    throw new ExtensionException(errors.ToString());
                }
            }

            if (extensionList != null)
            {
                // Sort interfaces according to the ordering in the whitelist
                SortedList<int, IExtension<TOwner>> sorted = new SortedList<int,IExtension<TOwner>>(Extensions.Count);
                for (int i = 0; i < Extensions.Count; i++)
                {
                    // Find this extension in the whitelist
                    for (int j = 0; j < extensionList.Count; j++)
                    {
                        if (extensionList[j].Equals(Extensions[i].GetType().Name, StringComparison.InvariantCultureIgnoreCase))
                        {
                            sorted.Add(j, Extensions[i]);
                            break;
                        }
                    }
                }

                // Copy the sorted list back
                Extensions = new List<IExtension<TOwner>>(sorted.Values);
            }

            if (assignableInterfaces != null)
            {
                // Assign extensions to interfaces
                foreach (FieldInfo assignable in assignableInterfaces)
                {
                    Type type = assignable.FieldType;

                    for (int i = Extensions.Count - 1; i >= 0; i--)
                    {
                        IExtension<TOwner> extension = Extensions[i];

                        if (extension.GetType().GetInterface(type.Name) != null)
                        {
                            assignable.SetValue(assignablesParent, extension);
                            break;
                        }
                    }
                }

                // Check for unassigned interfaces
                foreach (FieldInfo assignable in assignableInterfaces)
                {
                    if (assignable.GetValue(assignablesParent) == null)
                        throw new ExtensionException("Unassigned interface " + assignable.FieldType.Name);
                }
            }
        }

        /// <summary>
        /// List all of the dlls containing IExtension&lt;TOwner&gt; classes in
        /// a given path and matching a given search pattern
        /// </summary>
        /// <param name="path">File path to search for assemblies</param>
        /// <param name="searchPattern">Search pattern, for example MyApp.*.dll</param>
        /// <returns>A list of file names for assemblies</returns>
        public static List<string> ListExtensionAssemblies(string path, string searchPattern)
        {
            List<string> plugins = new List<string>();
            string[] files = Directory.GetFiles(path, searchPattern);

            foreach (string f in files)
            {
                try
                {
                    Assembly a = Assembly.LoadFrom(f);
                    System.Type[] types = a.GetTypes();
                    foreach (System.Type type in types)
                    {
                        if (type.GetInterface(typeof(IExtension<TOwner>).Name) != null)
                        {
                            plugins.Add(f);
                            break;
                        }
                    }
                }
                catch (Exception) { }
            }

            return plugins;
        }

        /// <summary>
        /// List all of the C# source code containing IExtension classes in a
        /// given path and matching a given search pattern
        /// </summary>
        /// <param name="path">File path to search for source code</param>
        /// <param name="searchPattern">Search pattern, for example MyApp.*.cs</param>
        /// <returns>A list of file names for source code</returns>
        public static List<string> ListExtensionSourceFiles(string path, string searchPattern)
        {
            List<string> plugins = new List<string>();
            string[] files = Directory.GetFiles(path, searchPattern);

            foreach (string f in files)
            {
                if (File.ReadAllText(f).Contains("IExtension"))
                    plugins.Add(f);
            }

            return plugins;
        }

        /// <summary>
        /// Instantiate a copy of all of the IExtension&lt;TOwner&gt; classes
        /// in a given assembly. If the whitelist parameter is not null, only
        /// classes with a name found in the whitelist will be loaded
        /// </summary>
        /// <param name="assembly">Assembly to load extensions from</param>
        /// <param name="whitelist">An optional whitelist of extension names to
        /// load. Pass null to disable whitelist checking</param>
        public static void LoadAssemblyExtensions(Assembly assembly, List<string> whitelist)
        {
            Type[] constructorParams = new Type[] { };
            object[] parameters = new object[] { };

            foreach (Type t in assembly.GetTypes())
            {
                try
                {
                    if (t.GetInterface(typeof(IExtension<TOwner>).Name) != null && 
                        (whitelist == null || whitelist.Contains(t.Name)))
                    {
                        ConstructorInfo info = t.GetConstructor(constructorParams);
                        IExtension<TOwner> extension = (IExtension<TOwner>)info.Invoke(parameters);
                        Extensions.Add(extension);
                    }
                }
                catch (Exception e)
                {
                    throw new ExtensionException(String.Format(
                        "Failed to load IExtension {0} from assembly {1}", t.FullName, assembly.FullName), e);
                }
            }
        }

        /// <summary>
        /// Get FieldInfo data for a member of a static class. This is a helper
        /// function in case your assignable interface(s) reside in a static 
        /// class
        /// </summary>
        /// <param name="ownerType">typeof() for static class</param>
        /// <param name="memberName">Name of the static class member to get
        /// FieldInfo for</param>
        /// <returns>FieldInfo for the static class member if the member was
        /// found, otherwise null</returns>
        public static FieldInfo GetInterface(Type ownerType, string memberName)
        {
            FieldInfo fieldInfo = ownerType.GetField(memberName);
            if (fieldInfo.FieldType.IsInterface)
                return fieldInfo;
            else
                return null;
        }

        /// <summary>
        /// Get a list of all of the interfaces in a given class. This is a
        /// helper function in case all of your assignable interfaces reside in
        /// the same parent class, with no other interface members
        /// </summary>
        /// <param name="ownerObject">Object containing the assignable
        /// interfaces</param>
        /// <returns>List of FieldInfo objects for each of the interfaces</returns>
        public static List<FieldInfo> GetInterfaces(object ownerObject)
        {
            List<FieldInfo> interfaces = new List<FieldInfo>();

            foreach (FieldInfo field in ownerObject.GetType().GetFields())
            {
                if (field.FieldType.IsInterface)
                    interfaces.Add(field);
            }

            return interfaces;
        }
    }
}
