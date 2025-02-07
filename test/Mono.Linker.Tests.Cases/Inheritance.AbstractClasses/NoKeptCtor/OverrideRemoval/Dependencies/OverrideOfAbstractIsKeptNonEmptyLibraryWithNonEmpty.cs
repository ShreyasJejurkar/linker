﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mono.Linker.Tests.Cases.Inheritance.AbstractClasses.NoKeptCtor.OverrideRemoval.Dependencies
{
	public class OverrideOfAbstractIsKeptNonEmptyLibraryWithNonEmpty :
		OverrideOfAbstractIsKeptNonEmpty_BaseType
	{
		Dependencies.OverrideOfAbstractIsKeptNonEmpty_UnusedType _field;

		public override void Method ()
		{
			_field = null;
		}
	}
}
