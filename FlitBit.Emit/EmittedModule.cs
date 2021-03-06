﻿#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Reflection.Emit;

namespace FlitBit.Emit
{
	/// <summary>
	///   Helper class for working with a modules in the IL stream.
	/// </summary>
	public class EmittedModule
	{
		private readonly Dictionary<string, EmittedClass> _classes = new Dictionary<string, EmittedClass>();

		/// <summary>
		///   Creates a new instance.
		/// </summary>
		/// <param name="assembly">the assembly, owner</param>
		/// <param name="name">the module's name</param>
		/// <param name="namespace">a namespace name for the classes in the module (base)</param>
		public EmittedModule(EmittedAssembly assembly, string name, string @namespace)
		{
			Contract.Requires<ArgumentNullException>(assembly != null);
			Contract.Requires<ArgumentNullException>(name != null);
			Contract.Requires<ArgumentNullException>(name.Length > 0);

			Assembly = assembly;
			Name = name;
			Namespace = @namespace ?? String.Empty;
#if DEBUG
			Builder = Assembly.Builder.DefineDynamicModule(name, name + ".dll", true);
#else
			this.Builder = this.Assembly.Builder.DefineDynamicModule(name, name + ".dll", false);
#endif
		}

		/// <summary>
		///   Gets the assembly within which the module resides.
		/// </summary>
		public EmittedAssembly Assembly { get; private set; }

		/// <summary>
		///   Gets the underlying ModuleBuilder for the module.
		/// </summary>
		public ModuleBuilder Builder { get; private set; }

		/// <summary>
		///   Indicates whether the module has been compiled.
		/// </summary>
		public bool IsCompiled { get; private set; }

		/// <summary>
		///   Gets the module's name.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		///   Gets the default namespace for the module.
		/// </summary>
		public string Namespace { get; private set; }

		/// <summary>
		///   Compiles the module.
		/// </summary>
		/// <returns></returns>
		public Module Compile()
		{
			Contract.Requires<ArgumentNullException>(!IsCompiled, "module already compiled");

			foreach (EmittedClass c in _classes.Values)
			{
				// Classes may have been individually compiled.
				if (!c.IsCompiled)
				{
					c.Compile();
				}
			}
			IsCompiled = true;
			return Builder;
		}

		/// <summary>
		///   Defines a class.
		/// </summary>
		/// <param name="name">the class' name</param>
		/// <returns>the emitted class</returns>
		public EmittedClass DefineClass(string name)
		{
			Contract.Requires<ArgumentNullException>(name != null);
			Contract.Requires<ArgumentNullException>(name.Length > 0);
			Contract.Requires(!IsCompiled, "module already compiled");

			CheckClassName(name);

			var cls = new EmittedClass(Builder, name);
			_classes.Add(name, cls);
			return cls;
		}

		/// <summary>
		///   Defines a class
		/// </summary>
		/// <param name="name">the class' name</param>
		/// <param name="attributes">the class' attributes</param>
		/// <param name="supertype">the class' supertype</param>
		/// <param name="interfaces">list of interfaces the class will implement</param>
		/// <returns>the emitted class</returns>
		public EmittedClass DefineClass(string name, TypeAttributes attributes
			, Type supertype, Type[] interfaces)
		{
			Contract.Requires<ArgumentNullException>(name != null);
			Contract.Requires<ArgumentNullException>(name.Length > 0);

			CheckClassName(name);

			var cls = new EmittedClass(Builder, name, attributes, supertype, interfaces);
			_classes.Add(name, cls);
			return cls;
		}

		/// <summary>
		///   Saves the module.
		/// </summary>
		internal void Save()
		{
			Contract.Requires(Assembly != null);
			if (!Assembly.IsCompiled)
			{
				Assembly.Compile();
			}
			Assembly.Save(this);
		}

		private void CheckClassName(string name)
		{
			Contract.Requires<ArgumentNullException>(name != null);
			Contract.Requires<ArgumentNullException>(name.Length > 0);

			if (_classes.ContainsKey(name))
			{
				throw new InvalidOperationException(String.Concat(
					"Unable to generate duplicate class. The class name is already in use: module = ",
					Name, ", class = ", name)
					);
			}
		}
	}
}