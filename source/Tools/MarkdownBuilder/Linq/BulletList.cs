﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Pihrtsoft.Markdown.Linq
{
    public class BulletList : List
    {
        public BulletList()
        {
        }

        public BulletList(object content)
            : base(content)
        {
        }

        public BulletList(params object[] content)
            : base(content)
        {
        }

        public BulletList(BulletList other)
            : base(other)
        {
        }

        public override MarkdownKind Kind => MarkdownKind.List;

        public override MarkdownBuilder AppendTo(MarkdownBuilder builder)
        {
            if (content is string s)
            {
                return builder.AppendBulletItem(s).AppendLine();
            }
            else
            {
                return builder.AppendBulletItems(Elements());
            }
        }

        public override MarkdownWriter WriteTo(MarkdownWriter writer)
        {
            if (content is string s)
            {
                return writer.WriteBulletItem(s).WriteLine();
            }
            else
            {
                return writer.WriteBulletItems(Elements());
            }
        }

        internal override MElement Clone()
        {
            return new BulletList(this);
        }
    }
}
