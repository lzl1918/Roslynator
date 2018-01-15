﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Pihrtsoft.Markdown.Linq
{
    public class MOrderedList : MList
    {
        public MOrderedList()
        {
        }

        public MOrderedList(object content)
            : base(content)
        {
        }

        public MOrderedList(params object[] content)
            : base(content)
        {
        }

        public MOrderedList(MOrderedList other)
            : base(other)
        {
        }

        public override MarkdownKind Kind => MarkdownKind.OrderedList;

        public override MarkdownWriter WriteTo(MarkdownWriter writer)
        {
            if (content is string s)
            {
                return writer.WriteOrderedItem(1, s).WriteLine();
            }
            else
            {
                return writer.WriteOrderedItems(Elements());
            }
        }

        internal override MElement Clone()
        {
            return new MOrderedList(this);
        }
    }
}