﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Roslynator.CSharp
{
    public struct IfStatementOrElseClause : IEquatable<IfStatementOrElseClause>
    {
        internal IfStatementOrElseClause(SyntaxNode node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            SyntaxKind kind = node.Kind();

            if (kind != SyntaxKind.IfStatement
                && kind != SyntaxKind.ElseClause)
            {
                throw new ArgumentException("Node must be if statement or else clause.", nameof(node));
            }

            Kind = kind;
            Node = node;
        }

        internal IfStatementOrElseClause(IfStatementSyntax ifStatement)
        {
            Node = ifStatement ?? throw new ArgumentNullException(nameof(ifStatement));
            Kind = SyntaxKind.IfStatement;
        }

        internal IfStatementOrElseClause(ElseClauseSyntax elseClause)
        {
            Node = elseClause ?? throw new ArgumentNullException(nameof(elseClause));
            Kind = SyntaxKind.ElseClause;
        }

        internal SyntaxNode Node { get; }

        public SyntaxKind Kind { get; }

        public bool IsIf
        {
            get { return Kind == SyntaxKind.IfStatement; }
        }

        public bool IsElse
        {
            get { return Kind == SyntaxKind.ElseClause; }
        }

        public StatementSyntax Statement
        {
            get
            {
                var self = this;

                return (self.IsIf) ? self.AsIf().Statement : self.AsElse().Statement;
            }
        }

        public IfStatementSyntax AsIf()
        {
            return (IfStatementSyntax)Node;
        }

        public ElseClauseSyntax AsElse()
        {
            return (ElseClauseSyntax)Node;
        }

        public override string ToString()
        {
            return Node?.ToString() ?? base.ToString();
        }

        public bool Equals(IfStatementOrElseClause other)
        {
            return Node == other.Node;
        }

        public override bool Equals(object obj)
        {
            return obj is IfStatementOrElseClause
                && Equals((IfStatementOrElseClause)obj);
        }

        public override int GetHashCode()
        {
            return Node?.GetHashCode() ?? 0;
        }

        public static bool operator ==(IfStatementOrElseClause left, IfStatementOrElseClause right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(IfStatementOrElseClause left, IfStatementOrElseClause right)
        {
            return !left.Equals(right);
        }

        public static implicit operator IfStatementOrElseClause(IfStatementSyntax ifStatement)
        {
            return new IfStatementOrElseClause(ifStatement);
        }

        public static implicit operator IfStatementSyntax(IfStatementOrElseClause ifOrElse)
        {
            return ifOrElse.AsIf();
        }

        public static implicit operator IfStatementOrElseClause(ElseClauseSyntax elseClause)
        {
            return new IfStatementOrElseClause(elseClause);
        }

        public static implicit operator ElseClauseSyntax(IfStatementOrElseClause ifOrElse)
        {
            return ifOrElse.AsElse();
        }
    }
}
