﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Runtime.InteropServices;
using Mono.Linker.Tests.Cases.CppCLI.Dependencies;
using Mono.Linker.Tests.Cases.Expectations.Assertions;
using Mono.Linker.Tests.Cases.Expectations.Metadata;

namespace Mono.Linker.Tests.Cases.CppCLI
{
	[ReferenceDependency ("Dependencies/TestLibrary.dll")]
	[SetupLinkerArgument ("--skip-unresolved", "true")]

	[SetupCompileBefore ("ManagedSide.dll", new[] { "Dependencies/CallCppCLIFromManagedRef.cs" })]
	[SetupCompileAfter ("ManagedSide.dll", new[] { "Dependencies/CallCppCLIFromManaged.cs" }, references: new[] { "TestLibrary.dll" })]

	[LogContains ("Warn from C++/CLI")]
	[KeptAssembly ("TestLibrary.dll")]

	[Kept]
	public class CppCLIAssemblyIsAnalyzed
	{
		public static void Main ()
		{
			CallCppCLIFromManaged.TriggerWarning ();
		}
	}
}
