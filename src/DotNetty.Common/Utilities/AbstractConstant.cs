﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace DotNetty.Common.Utilities
{
    using System;
    using System.Threading;

    public abstract class AbstractConstant : IConstant
    {
        static long s_nextUniquifier;

        long v_uniquifier;

        protected AbstractConstant(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public int Id { get; }

        public string Name { get; }

        public sealed override string ToString() => Name;

        protected long Uniquifier
        {
            get
            {
                long result;
                if ((result = Volatile.Read(ref v_uniquifier)) == SharedConstants.Zero64)
                {
                    result = Interlocked.Increment(ref s_nextUniquifier);
                    long previousUniquifier = Interlocked.CompareExchange(ref v_uniquifier, result, SharedConstants.Zero64);
                    if (previousUniquifier != SharedConstants.Zero64)
                    {
                        result = previousUniquifier;
                    }
                }

                return result;
            }
        }

        public abstract bool Equals(IConstant other);
    }

    /// <summary>Base implementation of <see cref="IConstant" />.</summary>
    public abstract class AbstractConstant<T> : AbstractConstant, IComparable<T>, IEquatable<T>
        where T : AbstractConstant<T>
    {
        /// <summary>Creates a new instance.</summary>
        protected AbstractConstant(int id, string name)
            : base(id, name)
        {
        }

        public sealed override int GetHashCode() => base.GetHashCode();

        public sealed override bool Equals(object obj) => base.Equals(obj);

        public bool Equals(T other) => ReferenceEquals(this, other);
        public override bool Equals(IConstant other) => ReferenceEquals(this, other);

        public int CompareTo(T o)
        {
            if (ReferenceEquals(this, o))
            {
                return 0;
            }

            AbstractConstant<T> other = o;

            int returnCode = GetHashCode() - other.GetHashCode();
            if (returnCode != 0)
            {
                return returnCode;
            }

            long thisUV = Uniquifier;
            long otherUV = other.Uniquifier;
            if (thisUV < otherUV)
            {
                return -1;
            }
            if (thisUV > otherUV)
            {
                return 1;
            }

            return ThrowHelper.ThrowException_CompareConstant();
        }
    }
}