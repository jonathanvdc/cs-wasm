using System;
using System.Collections.Generic;

namespace Wasm.Text
{
    /// <summary>
    /// An S-expression: a data structure that is either a single token or a keyword
    /// token followed by a tail.
    /// </summary>
    public struct SExpression
    {
        internal static SExpression Create(Lexer.Token head, IReadOnlyList<SExpression> tail)
        {
            return new SExpression
            {
                HasTail = true,
                Head = head,
                Tail = tail
            };
        }

        internal static SExpression Create(Lexer.Token head)
        {
            return new SExpression
            {
                HasTail = false,
                Head = head,
                Tail = Array.Empty<SExpression>()
            };
        }

        /// <summary>
        /// Tests if this S-expression has a tail.
        /// </summary>
        public bool HasTail { get; private set; }

        /// <summary>
        /// Gets the keyword token that is the head of this S-expression if the S-expression has a tail;
        /// otherwise, the token that corresponds to the S-expression itself.
        /// </summary>
        public Lexer.Token Head { get; private set; }

        /// <summary>
        /// Gets the S-expression's tail: a sequence of S-expressions that trail the S-expression's head.
        /// Note that this tail may be empty even for S-expressions that have a tail.
        /// </summary>
        public IReadOnlyList<SExpression> Tail { get; private set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return HasTail ? Head.Span.Text : $"({Head.Span.Text} {string.Join(" ", Tail)})";
        }
    }
}
