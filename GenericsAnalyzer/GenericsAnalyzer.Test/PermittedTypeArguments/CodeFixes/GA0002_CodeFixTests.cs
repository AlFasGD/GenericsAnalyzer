﻿using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenericsAnalyzer.Test.PermittedTypeArguments.CodeFixes
{
    [TestClass]
    public class GA0002_CodeFixTests : DuplicateAttributeArgumentRemoverCodeFixTests
    {
        public override DiagnosticDescriptor TestedDiagnosticRule => DiagnosticDescriptors.GA0002_Rule;

        // TODO: Find a way to test applying the code fix on a specified diagnostic
        [TestMethod]
        public void ConflictingConstraintsCodeFix()
        {
            var testCode =
@"
#pragma warning disable GA0010
#pragma warning disable GA0011

class C
<
    [PermittedTypes({|GA0002:typeof(int)|}, typeof(long), typeof(ulong))]
    [ProhibitedTypes({|GA0002:typeof(int)|})]
    [OnlyPermitSpecifiedTypes]
    T
>
{
}
";

            var fixedCode0 =
@"
#pragma warning disable GA0010
#pragma warning disable GA0011

class C
<
    [PermittedTypes(typeof(int), typeof(long), typeof(ulong))]
    [OnlyPermitSpecifiedTypes]
    T
>
{
}
";

            var fixedCode1 =
@"
#pragma warning disable GA0010
#pragma warning disable GA0011

class C
<
    [PermittedTypes(typeof(long), typeof(ulong))]
    [ProhibitedTypes(typeof(int))]
    [OnlyPermitSpecifiedTypes]
    T
>
{
}
";

            TestCodeFixWithUsings(testCode, fixedCode0, 0);
            //TestCodeFixWithUsings(testCode, fixedCode1, 1);
        }
        [TestMethod]
        public void MultipleConflictingConstraintsCodeFix()
        {
            var testCode =
@"
#pragma warning disable GA0010
#pragma warning disable GA0011

class C
<
    [PermittedTypes({|GA0002:typeof(List<int>)|}, typeof(long), typeof(ulong))]
    [ProhibitedTypes({|GA0002:typeof(List<int>)|}, {|GA0002:typeof(List<int>)|})]
    [ProhibitedBaseTypes({|GA0002:typeof(List<int>)|}, {|GA0002:typeof(List<int>)|})]
    [OnlyPermitSpecifiedTypes]
    T
>
{
}
";

            var fixedCode0 =
@"
#pragma warning disable GA0010
#pragma warning disable GA0011

class C
<
    [PermittedTypes(typeof(List<int>), typeof(long), typeof(ulong))]
    [OnlyPermitSpecifiedTypes]
    T
>
{
}
";

            var fixedCode1 =
@"
#pragma warning disable GA0010
#pragma warning disable GA0011

class C
<
    [PermittedTypes(typeof(long), typeof(ulong))]
    [ProhibitedTypes(typeof(List<int>))]
    [OnlyPermitSpecifiedTypes]
    T
>
{
}
";

            TestCodeFixWithUsings(testCode, fixedCode0, 0);
            //TestCodeFixWithUsings(testCode, fixedCode1, 1);
        }
        [TestMethod]
        public void MultipleClassesConflictingConstraintsCodeFix()
        {
            var testCode =
@"
#pragma warning disable GA0010
#pragma warning disable GA0011

class C
<
    [PermittedTypes({|GA0002:typeof(List<int>)|}, typeof(long), typeof(ulong))]
    [ProhibitedTypes({|GA0002:typeof(List<int>)|}, {|GA0002:typeof(List<int>)|})]
    [ProhibitedBaseTypes({|GA0002:typeof(List<int>)|}, {|GA0002:typeof(List<int>)|})]
    [OnlyPermitSpecifiedTypes]
    T
>
{
}
class D
<
    [PermittedTypes(typeof(List<int>), typeof(long), typeof(ulong))]
    [OnlyPermitSpecifiedTypes]
    T
>
{
}
";

            var fixedCode =
@"
#pragma warning disable GA0010
#pragma warning disable GA0011

class C
<
    [PermittedTypes(typeof(List<int>), typeof(long), typeof(ulong))]
    [OnlyPermitSpecifiedTypes]
    T
>
{
}
class D
<
    [PermittedTypes(typeof(List<int>), typeof(long), typeof(ulong))]
    [OnlyPermitSpecifiedTypes]
    T
>
{
}
";

            TestCodeFixWithUsings(testCode, fixedCode, 0);
        }
        [TestMethod]
        public void ExtraAttributesConflictingConstraintsCodeFix()
        {
            var testCode =
@"
#pragma warning disable GA0010
#pragma warning disable GA0011

class C
<
    [PermittedTypes({|GA0002:typeof(List<int>)|}, typeof(long), typeof(ulong))]
    [ProhibitedTypes({|GA0002:typeof(List<int>)|}, {|GA0002:typeof(List<int>)|})]
    [ProhibitedBaseTypes({|GA0002:typeof(List<int>)|}, {|GA0002:typeof(List<int>)|})]
    [Example(typeof(List<int>), 1)]
    [OnlyPermitSpecifiedTypes]
    T
>
{
}
";

            var fixedCode =
@"
#pragma warning disable GA0010
#pragma warning disable GA0011

class C
<
    [PermittedTypes(typeof(List<int>), typeof(long), typeof(ulong))]
    [Example(typeof(List<int>), 1)]
    [OnlyPermitSpecifiedTypes]
    T
>
{
}
";

            TestCodeFixWithUsings(testCode, fixedCode, 0);
        }
    }
}
