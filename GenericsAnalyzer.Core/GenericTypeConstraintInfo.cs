﻿using GenericsAnalyzer.Core.Utilities;
using Microsoft.CodeAnalysis;

namespace GenericsAnalyzer.Core
{
    public class GenericTypeConstraintInfo
    {
        private readonly TypeConstraintSystem[] systems;

        public GenericTypeConstraintInfo(int length)
        {
            systems = new TypeConstraintSystem[length];
        }

        public GenericTypeConstraintInfo(ISymbol symbol)
            : this(symbol.GetArity()) { }
        public GenericTypeConstraintInfo(IMethodSymbol methodSymbol)
            : this(methodSymbol.Arity) { }
        public GenericTypeConstraintInfo(INamedTypeSymbol typeSymbol)
            : this(typeSymbol.Arity) { }

        public TypeConstraintSystem this[int index]
        {
            get => systems[index];
            set => systems[index] = value;
        }
        public TypeConstraintSystem this[ITypeParameterSymbol typeParameter]
        {
            get => systems[typeParameter.Ordinal];
            set => systems[typeParameter.Ordinal] = value;
        }
    }
}
