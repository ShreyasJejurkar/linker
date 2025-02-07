﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ILLink.Shared;
using Mono.Cecil;

namespace Mono.Linker
{
	readonly struct LinkerAttributesInformation
	{
		readonly List<(Type Type, List<Attribute> Attributes)>? _linkerAttributes;

		private LinkerAttributesInformation (List<(Type Type, List<Attribute> Attributes)>? cache)
		{
			this._linkerAttributes = cache;
		}

		private static bool TryFindAttributeList (List<(Type Type, List<Attribute> Attributes)> list, Type type, [NotNullWhen (returnValue: true)] out List<Attribute>? foundAttributes)
		{
			foreach (var item in list) {
				if (item.Type == type) {
					foundAttributes = item.Attributes;
					return true;
				}
			}
			foundAttributes = null;
			return false;
		}

		public static LinkerAttributesInformation Create (LinkContext context, ICustomAttributeProvider provider)
		{
			Debug.Assert (context.CustomAttributes.HasAny (provider));

			List<(Type Type, List<Attribute> Attributes)>? cache = null;

			foreach (var customAttribute in context.CustomAttributes.GetCustomAttributes (provider)) {
				var attributeType = customAttribute.AttributeType;

				Attribute? attributeValue;
				switch (attributeType.Name) {
				case "RequiresUnreferencedCodeAttribute" when attributeType.Namespace == "System.Diagnostics.CodeAnalysis":
					attributeValue = ProcessRequiresUnreferencedCodeAttribute (context, provider, customAttribute);
					break;
				case "DynamicDependencyAttribute" when attributeType.Namespace == "System.Diagnostics.CodeAnalysis":
					attributeValue = DynamicDependency.ProcessAttribute (context, provider, customAttribute);
					break;
				case "RemoveAttributeInstancesAttribute":
					if (provider is not TypeDefinition td)
						continue;

					// The attribute is never removed if it's explicitly preserved (e.g. via xml descriptor)
					if (context.Annotations.TryGetPreserve (td, out TypePreserve preserve) && preserve != TypePreserve.Nothing)
						continue;

					attributeValue = new RemoveAttributeInstancesAttribute (customAttribute.ConstructorArguments);
					break;
				default:
					continue;
				}

				if (attributeValue == null)
					continue;

				if (cache == null)
					cache = new List<(Type Type, List<Attribute> Attributes)> ();

				Type attributeValueType = attributeValue.GetType ();

				if (!TryFindAttributeList (cache, attributeValueType, out var attributeList)) {
					attributeList = new List<Attribute> ();
					cache.Add ((attributeValueType, attributeList));
				}

				attributeList.Add (attributeValue);
			}

			return new LinkerAttributesInformation (cache);
		}

		public bool HasAttribute<T> () where T : Attribute
		{
			return _linkerAttributes != null && TryFindAttributeList (_linkerAttributes, typeof (T), out _);
		}

		public IEnumerable<T> GetAttributes<T> () where T : Attribute
		{
			if (_linkerAttributes == null)
				return Enumerable.Empty<T> ();

			if (!TryFindAttributeList (_linkerAttributes, typeof (T), out var attributeList))
				return Enumerable.Empty<T> ();

			if (attributeList.Count == 0)
				throw new InvalidOperationException ("Unexpected list of attributes.");

			return attributeList.Cast<T> ();
		}

		static Attribute? ProcessRequiresUnreferencedCodeAttribute (LinkContext context, ICustomAttributeProvider provider, CustomAttribute customAttribute)
		{
			if (!(provider is MethodDefinition || provider is TypeDefinition))
				return null;

			if (customAttribute.HasConstructorArguments && customAttribute.ConstructorArguments[0].Value is string message) {
				var ruca = new RequiresUnreferencedCodeAttribute (message);
				if (customAttribute.HasProperties) {
					foreach (var prop in customAttribute.Properties) {
						if (prop.Name == "Url") {
							ruca.Url = prop.Argument.Value as string;
							break;
						}
					}
				}

				return ruca;
			}

			context.LogWarning ((IMemberDefinition) provider, DiagnosticId.AttributeDoesntHaveTheRequiredNumberOfParameters, typeof (RequiresUnreferencedCodeAttribute).FullName ?? "");
			return null;
		}
	}
}
