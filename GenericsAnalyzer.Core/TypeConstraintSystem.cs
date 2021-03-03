﻿using GenericsAnalyzer.Core.Utilities;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace GenericsAnalyzer.Core
{
    /// <summary>Represents a system that contains a set of rules about type constraints.</summary>
    public class TypeConstraintSystem
    {
        private Dictionary<ITypeSymbol, TypeConstraintRule> typeConstraintRules = new Dictionary<ITypeSymbol, TypeConstraintRule>(SymbolEqualityComparer.Default);

        public bool OnlyPermitSpecifiedTypes { get; set; }

        public void InheritFrom(TypeConstraintSystem other)
        {
            typeConstraintRules.AddOrSetRange(other.typeConstraintRules);
        }

        public void Add(TypeConstraintRule rule, params ITypeSymbol[] types) => Add(rule, (IEnumerable<ITypeSymbol>)types);
        public void Add(TypeConstraintRule rule, IEnumerable<ITypeSymbol> types)
        {
            foreach (var t in types)
            {
                if (typeConstraintRules.ContainsKey(t))
                    typeConstraintRules[t] = rule;
                else
                    typeConstraintRules.Add(t, rule);
            }
        }

        public bool IsPermitted(ITypeSymbol type)
        {
            if (type is null)
                return false;

            var permission = IsPermitted(type, TypeConstraintReferencePoint.ExactType);
            if (permission != null)
                return permission.Value;

            var interfaceQueue = new Queue<INamedTypeSymbol>(type.Interfaces);
            while (interfaceQueue.Any())
            {
                var i = interfaceQueue.Dequeue();

                permission = IsPermitted(i, TypeConstraintReferencePoint.BaseType);
                if (permission != null)
                    return permission.Value;

                if (i.IsGenericType && !i.IsUnboundGenericType)
                {
                    var unbound = i.ConstructUnboundGenericType();
                    permission = IsPermitted(unbound, TypeConstraintReferencePoint.BaseType);
                    if (permission != null)
                        return permission.Value;
                }

                foreach (var indirectInterface in i.Interfaces)
                    interfaceQueue.Enqueue(indirectInterface);
            }

            do
            {
                permission = IsPermitted(type, TypeConstraintReferencePoint.BaseType);
                if (permission != null)
                    return permission.Value;

                type = type.BaseType;
            }
            while (!(type is null));

            return !OnlyPermitSpecifiedTypes;
        }

        private bool? IsPermitted(ITypeSymbol type, TypeConstraintReferencePoint referencePoint)
        {
            if (!typeConstraintRules.ContainsKey(type))
                return null;

            var rule = typeConstraintRules[type];
            if (rule.TypeReferencePoint == referencePoint)
                return rule.Rule == ConstraintRule.Permit;

            return null;
        }
    }
}
