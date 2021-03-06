﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenericsAnalyzer.Test.PermittedTypeArguments
{
    [TestClass]
    public sealed class GA0005_Tests : BaseDiagnosticTests
    {
        public override DiagnosticDescriptor TestedDiagnosticRule => DiagnosticDescriptors.GA0005_Rule;

        protected override DiagnosticAnalyzer GetNewDiagnosticAnalyzerInstance() => new PermittedTypeArgumentAnalyzer();

        [TestMethod]
        public void InvalidTypeArguments()
        {
            var testCode =
@"
class C
<
    [PermittedTypes(↓typeof(string))]
    [PermittedBaseTypes(typeof(IEnumerable<>))]
    [PermittedBaseTypes(↓typeof(List<>))]
    [PermittedTypes(typeof(IList<int>), typeof(ISet<int>))]
    [OnlyPermitSpecifiedTypes]
    T
>
    where T : IEnumerable<int>
{
}
";

            AssertDiagnosticsWithUsings(testCode);
        }
    }
}
