﻿using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenericsAnalyzer.Test.PermittedTypeArguments.CodeFixes
{
    [TestClass]
    public class GA0004_CodeFixTests : RedundantAttributeArgumentRemoverCodeFixTests
    {
        public override DiagnosticDescriptor TestedDiagnosticRule => DiagnosticDescriptors.GA0004_Rule;

        [TestMethod]
        public void InvalidTypeArgumentCodeFix()
        {
            var testCode =
@"
class C
<
    [PermittedTypes({|GA0004:typeof(int*)|})]
    [ProhibitedBaseTypes(typeof(object))]
    T
>
{
}
";

            var fixedCode =
@"
class C
<
    [ProhibitedBaseTypes(typeof(object))]
    T
>
{
}
";

            TestCodeFixWithUsings(testCode, fixedCode);
        }
        [TestMethod]
        public void InvalidTypeArgumentMultipleTypeRulesCodeFix()
        {
            var testCode =
@"
class C
<
    [PermittedTypes({|GA0004:typeof(int*)|}, typeof(List<>))]
    [OnlyPermitSpecifiedTypes]
    T
>
{
}
";

            var fixedCode =
@"
class C
<
    [PermittedTypes(typeof(List<>))]
    [OnlyPermitSpecifiedTypes]
    T
>
{
}
";

            TestCodeFixWithUsings(testCode, fixedCode);
        }
    }
}
