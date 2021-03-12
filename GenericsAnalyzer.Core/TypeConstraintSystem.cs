﻿using GenericsAnalyzer.Core.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GenericsAnalyzer.Core
{
    // Aliases like that are great
    using RuleEqualityComparer = Func<KeyValuePair<ITypeSymbol, TypeConstraintRule>, bool>;

    /// <summary>Represents a system that contains a set of rules about type constraints.</summary>
    public class TypeConstraintSystem
    {
        private Dictionary<ITypeSymbol, TypeConstraintRule> typeConstraintRules = new Dictionary<ITypeSymbol, TypeConstraintRule>(SymbolEqualityComparer.Default);

        private HashSet<ITypeParameterSymbol> inheritedTypes = new HashSet<ITypeParameterSymbol>(SymbolEqualityComparer.Default);

        public ITypeParameterSymbol TypeParameter { get; }
        public bool OnlyPermitSpecifiedTypes { get; set; }

        public TypeConstraintSystem(ITypeParameterSymbol parameter)
        {
            TypeParameter = parameter;
        }

        public void InheritFrom(ITypeParameterSymbol baseTypeParameter, TypeConstraintSystem baseSystem)
        {
            OnlyPermitSpecifiedTypes |= baseSystem.OnlyPermitSpecifiedTypes;
            typeConstraintRules.AddOrSetRange(baseSystem.typeConstraintRules);
            inheritedTypes.Add(baseTypeParameter);
        }

        #region Rule Equality Comparer Creators
        private static RuleEqualityComparer GetRuleEqualityComparer(TypeConstraintRule rule) => kvp => kvp.Value == rule;
        private static RuleEqualityComparer GetRuleEqualityComparer(ConstraintRule rule) => kvp => kvp.Value.Rule == rule;
        private static RuleEqualityComparer GetRuleEqualityComparer(TypeConstraintReferencePoint rule) => kvp => kvp.Value.TypeReferencePoint == rule;
        #endregion

        #region Diagnostics
        public bool HasNoPermittedTypes => OnlyPermitSpecifiedTypes && !typeConstraintRules.Any(GetRuleEqualityComparer(ConstraintRule.Permit));

        public int? GetFinitePermittedTypeCount()
        {
            if (!OnlyPermitSpecifiedTypes)
                return null;

            int count = 0;

            foreach (var typeRule in typeConstraintRules)
            {
                var type = typeRule.Key;
                var rule = typeRule.Value;

                if (rule.Rule == ConstraintRule.Prohibit)
                    continue;

                // There is no need to check whether the rule is a permission

                // Only exact permitted types can make for finite permitted type count
                if (rule.TypeReferencePoint == TypeConstraintReferencePoint.ExactType)
                {
                    if (type is INamedTypeSymbol named && named.IsUnboundGenericTypeSafe())
                        return null;

                    count++;
                }
                else
                    return null;
            }

            return count;
        }
        #endregion

        public TypeConstraintSystemAddResult Add(TypeConstraintRule rule, params ITypeSymbol[] types) => Add(rule, (IEnumerable<ITypeSymbol>)types);
        public TypeConstraintSystemAddResult Add(TypeConstraintRule rule, IEnumerable<ITypeSymbol> types)
        {
            var typeDiagnostics = new TypeConstraintSystemDiagnostics();

            foreach (var t in types)
            {
                if (typeDiagnostics.ConditionallyRegisterInvalidTypeArgumentType(t))
                    continue;

                if (typeDiagnostics.ConditionallyRegisterConstrainedSubstitutionType(TypeParameter, t))
                    continue;

                if (typeConstraintRules.ContainsKey(t))
                {
                    if (typeConstraintRules[t] == rule)
                        typeDiagnostics.RegisterDuplicateType(t);
                    else
                        typeDiagnostics.RegisterConflictingType(t);

                    continue;
                }

                typeConstraintRules.Add(t, rule);
            }

            return new TypeConstraintSystemAddResult(typeDiagnostics);
        }

        public bool SupersetOf(TypeConstraintSystem other) => other.SubsetOf(this);
        public bool SubsetOf(TypeConstraintSystem other)
        {
            if (!OnlyPermitSpecifiedTypes)
                if (other.OnlyPermitSpecifiedTypes)
                    return false;

            foreach (var rule in other.typeConstraintRules)
            {
                switch (rule.Value.Rule)
                {
                    case ConstraintRule.Permit:
                        continue;
                    case ConstraintRule.Prohibit:
                        if (IsPermitted(rule.Key))
                            return false;
                        break;
                }
            }

            return true;
        }

        public bool IsPermitted(ITypeParameterSymbol typeParameter, GenericTypeConstraintInfoCollection infos)
        {
            if (inheritedTypes.Contains(typeParameter))
                return true;

            var declaringElementTypeParameterSystems = infos[typeParameter.GetDeclaringSymbol()];
            var system = declaringElementTypeParameterSystems[typeParameter];
            return SupersetOf(system);
        }

        public bool IsPermitted(ITypeSymbol type)
        {
            if (type is null)
                return false;

            var permission = IsPermittedWithUnbound(type, TypeConstraintReferencePoint.ExactType, TypeConstraintReferencePoint.BaseType);
            if (permission != PermissionResult.Unknown)
                return permission == PermissionResult.Permitted;

            var interfaceQueue = new Queue<INamedTypeSymbol>(type.Interfaces);
            while (interfaceQueue.Any())
            {
                var i = interfaceQueue.Dequeue();

                permission = IsPermittedWithUnbound(i, TypeConstraintReferencePoint.BaseType);
                if (permission != PermissionResult.Unknown)
                    return permission == PermissionResult.Permitted;

                foreach (var indirectInterface in i.Interfaces)
                    interfaceQueue.Enqueue(indirectInterface);
            }

            do
            {
                permission = IsPermittedWithUnbound(type, TypeConstraintReferencePoint.BaseType);
                if (permission != PermissionResult.Unknown)
                    return permission == PermissionResult.Permitted;

                type = type.BaseType;
            }
            while (type != null);

            return !OnlyPermitSpecifiedTypes;
        }

        private PermissionResult IsPermittedWithUnbound(ITypeSymbol type, params TypeConstraintReferencePoint[] referencePoints)
        {
            var permission = IsPermitted(type, referencePoints);
            if (permission != PermissionResult.Unknown)
                return permission;

            if (type is INamedTypeSymbol namedType)
            {
                if (namedType.IsBoundGenericTypeSafe())
                {
                    var unbound = namedType.ConstructUnboundGenericType();
                    permission = IsPermitted(unbound, referencePoints);
                    if (permission != PermissionResult.Unknown)
                        return permission;
                }
            }

            return PermissionResult.Unknown;
        }

        private PermissionResult IsPermitted(ITypeSymbol type, params TypeConstraintReferencePoint[] referencePoints)
        {
            if (!typeConstraintRules.ContainsKey(type))
                return PermissionResult.Unknown;

            var rule = typeConstraintRules[type];
            if (referencePoints.Contains(rule.TypeReferencePoint))
                return (PermissionResult)rule.Rule;

            return PermissionResult.Unknown;
        }
    }
}
