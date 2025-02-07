﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Mono.Linker.Tests.Cases.Expectations.Assertions;
using Mono.Linker.Tests.Cases.Expectations.Helpers;

namespace Mono.Linker.Tests.Cases.DataFlow
{
	[SkipKeptItemsValidation]
	[ExpectedNoWarnings]
	class GetTypeInfoDataFlow
	{
		public static void Main ()
		{
			TestNoAnnotations (typeof (TestType));
			TestWithAnnotations (typeof (TestType));
			TestWithNull ();
			TestWithNoValue ();
		}

		[ExpectedWarning ("IL2067", nameof (DataFlowTypeExtensions.RequiresPublicMethods))]
		static void TestNoAnnotations (Type t)
		{
			t.GetTypeInfo ().RequiresPublicMethods ();
			t.GetTypeInfo ().RequiresNone ();
		}

		[ExpectedWarning ("IL2067", nameof (DataFlowTypeExtensions.RequiresPublicFields))]
		static void TestWithAnnotations ([DynamicallyAccessedMembers (DynamicallyAccessedMemberTypes.PublicMethods)] Type t)
		{
			t.GetTypeInfo ().RequiresPublicMethods ();
			t.GetTypeInfo ().RequiresPublicFields ();
			t.GetTypeInfo ().RequiresNone ();
		}

		static void TestWithNull ()
		{
			Type t = null;
			t.GetTypeInfo ().RequiresPublicMethods ();
		}

		static void TestWithNoValue ()
		{
			Type t = null;
			Type noValue = Type.GetTypeFromHandle (t.TypeHandle);
			noValue.GetTypeInfo ().RequiresPublicMethods ();
		}

		class TestType { }
	}
}
