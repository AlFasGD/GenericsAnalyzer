﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace GenericsAnalyzer.Core.Utilities
{
    public static class TypeParameterConstraintClauseSyntaxExtensions
    {
        /// <summary>Adds the specified type constraint to the clause and updates it accordingly to ensure its validity.</summary>
        /// <param name="constraintClause">The constraint clause to add a new type constraint into.</param>
        /// <param name="semanticModel">The <seealso cref="SemanticModel"/> that refers to the <seealso cref="SyntaxTree"/> that the given constraint clause is contained in.</param>
        /// <param name="typeConstraint">The type constraint to add to the clause.</param>
        /// <param name="typeSymbol">The <seealso cref="ITypeSymbol"/> of the type that will be added in the constraint clause. It's used to determine whether it's a class or an interface.</param>
        /// <returns>The modified constraint clause.</returns>
        public static TypeParameterConstraintClauseSyntax AddUpdateTypeConstraint
        (
            this TypeParameterConstraintClauseSyntax constraintClause,
            SemanticModel semanticModel,
            TypeConstraintSyntax typeConstraint,
            ITypeSymbol typeSymbol
        )
        {
            var segmentation = new TypeParameterConstraintClauseSegmentation(constraintClause, semanticModel);
            segmentation.AddTypeConstraint(typeConstraint, typeSymbol);
            return segmentation.WithTheseConstraints(constraintClause);
        }
        /// <summary>
        /// Adds the specified constraints to the clause and updates it accordingly to ensure its validity.
        /// This assumes that both the original constraint clause is valid and the constraints contain no invalid for the specified constraint clause combinations.
        /// <br/>
        /// The following modifications are performed:<br/>
        /// - If a keyword constraint (<see langword="class"/>, <see langword="struct"/>, <see langword="notnull"/>, <see langword="unmanaged"/>, <see langword="default"/>) is specified to be added, it will be added at the start, replacing the potentially existing keyword or class constraint.<br/>
        /// - Interface constraints are appended to the end of the interface constraint segment.<br/>
        /// - The <see langword="new"/>() constraint is added to the end, replacing the potentially existing one.<br/>
        /// </summary>
        /// <param name="constraintClause">The original constraint clause.</param>
        /// <param name="constraints">The new constraints to add to the constraint clause.</param>
        /// <returns>The resultant constraint clause with the specified added constraints.</returns>
        private static TypeParameterConstraintClauseSyntax AddUpdateConstraints
        (
            this TypeParameterConstraintClauseSyntax constraintClause,
            SemanticModel semanticModel,
            params TypeParameterConstraintSyntax[] constraints
        )
        {
            var segmentation = new TypeParameterConstraintClauseSegmentation(constraintClause, semanticModel);
            //segmentation.AddInitializationConstraint(constraints);
            return segmentation.WithTheseConstraints(constraintClause);
        }
    }
}
