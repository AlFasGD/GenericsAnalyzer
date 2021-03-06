﻿using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenericsAnalyzer.Test.PermittedTypeArguments.CodeFixes
{
    [TestClass]
    public class GA0005_CodeFixTests : RedundantAttributeArgumentRemoverCodeFixTests
    {
        public override DiagnosticDescriptor TestedDiagnosticRule => DiagnosticDescriptors.GA0005_Rule;

        [TestMethod]
        public void ConstrainedTypeArgumentCodeFix()
        {
            var testCode =
@"
class C
<
    [PermittedTypes({|GA0005:typeof(string)|})]
    [PermittedTypes(typeof(IList<int>), typeof(ISet<int>))]
    [OnlyPermitSpecifiedTypes]
    T
>
    where T : IEnumerable<int>
{
}
";

            var fixedCode =
@"
class C
<
    [PermittedTypes(typeof(IList<int>), typeof(ISet<int>))]
    [OnlyPermitSpecifiedTypes]
    T
>
    where T : IEnumerable<int>
{
}
";

            TestCodeFixWithUsings(testCode, fixedCode);
        }
        [TestMethod]
        public void ConstrainedTypeArgumentMultipleTypeRulesCodeFix()
        {
            var testCode =
@"
class C
<
    [PermittedTypes({|GA0005:typeof(string)|}, typeof(IList<int>), typeof(ISet<int>))]
    [OnlyPermitSpecifiedTypes]
    T
>
    where T : IEnumerable<int>
{
}
";

            var fixedCode =
@"
class C
<
    [PermittedTypes(typeof(IList<int>), typeof(ISet<int>))]
    [OnlyPermitSpecifiedTypes]
    T
>
    where T : IEnumerable<int>
{
}
";

            TestCodeFixWithUsings(testCode, fixedCode);
        }
    }
}
