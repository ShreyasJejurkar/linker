﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Mono.Linker;
using Mono.Linker.Steps;

namespace CustomStep
{
	public class CustomStep : IStep
	{
		public void Process (LinkContext context)
		{
			var warningMessage = MessageContainer.CreateCustomWarningMessage (
				context: context,
				text: "Warning",
				code: 6001,
				origin: new MessageOrigin (fileName: "CustomStep.cs"),
				version: WarnVersion.Latest);

			context.LogMessage (warningMessage);
		}
	}

	public class CustomStepWithInvalidWarningCode : IStep
	{
		public void Process (LinkContext context)
		{
			// All codes in the range [1000-6000] are reserved for the linker
			// and should not be used by external parties.
			var invalidWarningMessage = MessageContainer.CreateCustomWarningMessage (
				context: context,
				text: "Warning",
				code: 2500,
				origin: new MessageOrigin (fileName: "CustomStep.cs"),
				version: WarnVersion.Latest);

			context.LogMessage (invalidWarningMessage);
		}
	}
}
