﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenericsAnalyzer.Test.PermittedTypeArguments
{
    [TestClass]
    public sealed class GA0015_Tests : BaseDiagnosticTests
    {
        public override DiagnosticDescriptor TestedDiagnosticRule => DiagnosticDescriptors.GA0015_Rule;

        protected override DiagnosticAnalyzer GetNewDiagnosticAnalyzerInstance() => new PermittedTypeArgumentAnalyzer();

        [TestMethod]
        public void RedundantUsageInClass()
        {
            var testCode =
@"
class Base { }
class A
<
    [↓InheritBaseTypeUsageConstraints]
    T
> : Base
{
}
class C
<
    [↓InheritBaseTypeUsageConstraints]
    T
>
{
}
class D
<
    [InheritBaseTypeUsageConstraints]
    T,
    U
> : C<T>
{
}
";

            AssertDiagnosticsWithUsings(testCode);
        }
    }
}
