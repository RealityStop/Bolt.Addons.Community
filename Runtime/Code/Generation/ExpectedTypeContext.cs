using System;
using System.Collections.Generic;

namespace Unity.VisualScripting.Community
{
    public sealed class ExpectedTypeContext
    {
        private readonly Stack<Frame> stack = new Stack<Frame>();

        public Type Current
        {
            get { return stack.Count > 0 ? stack.Peek().ExpectedType : null; }
        }

        public bool IsSatisfied
        {
            get { return stack.Count > 0 && stack.Peek().Satisfied; }
        }

        public ExpectedTypeScope Expect(Type type, out ExpectedTypeResult result)
        {
            if (type == null)
            {
                result = new ExpectedTypeResult(false, null);
                return ExpectedTypeScope.Empty;
            }

            var frame = new Frame(type);
            stack.Push(frame);
            result = new ExpectedTypeResult(false, type);
            return new ExpectedTypeScope(this, frame, result);
        }

        public void MarkSatisfied(Type resolvedAs = null)
        {
            if (stack.Count == 0)
                return;

            var current = stack.Peek();

            var resolved = resolvedAs ?? current.ExpectedType;

            current.Satisfied = true;
            current.ResolvedType = resolved;
        }

        public void Clear()
        {
            stack.Clear();
        }

        private void Pop(Frame frame)
        {
            if (stack.Count == 0)
                return;

            if (ReferenceEquals(stack.Peek(), frame))
                stack.Pop();
        }

        public sealed class ExpectedTypeScope : IDisposable
        {
            public static readonly ExpectedTypeScope Empty = new ExpectedTypeScope();

            private readonly ExpectedTypeContext context;
            private readonly Frame frame;
            private bool disposed;

            private ExpectedTypeResult result;

            private ExpectedTypeScope()
            {
                result = default;
            }


            internal ExpectedTypeScope(ExpectedTypeContext context, Frame frame, ExpectedTypeResult result)
            {
                this.context = context;
                this.frame = frame;
                this.result = result;
            }

            public void Dispose()
            {
                if (disposed)
                    return;

                disposed = true;

                if (frame != null)
                {
                    result.IsSatisfied = frame.Satisfied;
                    result.ResolvedType = frame.ResolvedType ?? frame.ExpectedType;

                    context.Pop(frame);
                }
            }
        }

        internal sealed class Frame
        {
            public Type ExpectedType;
            public Type ResolvedType;
            public bool Satisfied;

            public Frame(Type ExpectedType)
            {
                this.ExpectedType = ExpectedType;
            }
        }
    }

    public class ExpectedTypeResult
    {
        public bool IsSatisfied { get; internal set; }
        public Type ResolvedType { get; internal set; }

        public ExpectedTypeResult(bool isSatisfied, Type resolvedType)
        {
            IsSatisfied = isSatisfied;
            ResolvedType = resolvedType;
        }
    }
}
