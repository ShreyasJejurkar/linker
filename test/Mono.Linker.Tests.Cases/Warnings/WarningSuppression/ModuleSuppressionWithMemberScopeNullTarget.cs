﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Linker.Tests.Cases.Expectations.Assertions;
using Mono.Linker.Tests.Cases.Expectations.Metadata;

// A module level suppression with a 'type' or 'member' scope must also specify the target, pointing
// to the type or member where the suppression should be put.
[module: UnconditionalSuppressMessage ("Test", "IL2026", Scope = "member", Target = null)]

namespace Mono.Linker.Tests.Cases.Warnings.WarningSuppression
{
	[SkipKeptItemsValidation]
	[LogContains ("IL2026")]
	class ModuleSuppressionWithMemberScopeNullTarget
	{
		static void Main ()
		{
			TriggerWarning ();
		}

		[RequiresUnreferencedCode ("TriggerWarning")]
		public static Type TriggerWarning ()
		{
			return typeof (ModuleSuppressionWithMemberScopeNullTarget);
		}
	}
}
