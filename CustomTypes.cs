using System;
using System.Runtime.CompilerServices;


namespace NLab
{
    // All struct types implicitly inherit from the class System.ValueType (§16.4.3).
    // Function members in a struct cannot be abstract or virtual, and the override modifier is allowed only to override methods inherited from System.ValueType.
    
    // Implicit conversions should only be used when the conversion is inherently safe and does not involve potential data loss or exceptions. If a conversion might fail or lose information, an explicit operator is more appropriate.

    // A user-defined type can define a custom implicit or explicit conversion from or to another type, provided a 
    // standard conversion doesn't exist between the same two types. 

    // Implicit conversions don't require special syntax to be invoked and can occur in various situations,
    // for example, in assignments and methods invocations. Predefined C# implicit conversions always succeed and
    // never throw an exception. User-defined implicit conversions should behave in that way as well. If a custom
    // conversion can throw an exception or lose information, define it as an explicit conversion.

    // The is and as operators don't consider user-defined conversions. Use a cast expression to invoke a user-defined
    // explicit conversion.
    // Use the operator and implicit or explicit keywords to define an implicit or explicit conversion, respectively.
    // The type that defines a conversion must be either a source type or a target type of that conversion. A conversion
    // between two user-defined types can be defined in either of the two types.

    // The following example demonstrates how to define an implicit and explicit conversion:
    public readonly struct Digit
    {
        private readonly byte digit;

        public Digit(byte digit)
        {
            if (digit > 9)
            {
                throw new ArgumentOutOfRangeException("Digit cannot be greater than nine.");
            }
            this.digit = digit;
        }

        public static implicit operator byte(Digit d) => d.digit;

        public static explicit operator Digit(byte b) => new(b);

        public override string ToString() => $"{digit}";

        public static void DoIt()
        {
            var d = new Digit(7);
            byte number = d; // implicit
            Console.WriteLine($"{number}");  // output: 7
            Digit digit = (Digit)number; // explicit
            Console.WriteLine($"{digit}");  // output: 7
        }
    }


    // Non-destructive Mutation Using with
    // It’s not uncommon to want to take an instance of an object and get a copy either verbatim or with a few
    // small tweaks to the values. record types make this easy by giving you some new syntax using the with keyword:
    // var otherPerson = person with { Id = 2, FirstName = "Danny" };

    public enum ChannelDirection { Output, Input }



    // //You should create a struct that overrides the implicit conversion operator:

    // struct PackedValue
    // {
    //     private ushort _value;

    //     private PackedValue(ushort val)
    //     {
    //          if(val >= (1<<12)) throw new ArgumentException("val");
    //          this._value = val;
    //     }

    //     public static explicit operator PackedValue(ushort value)
    //     {
    //         return new PackedValue(value);
    //     }

    //     public static implicit operator ushort(PackedValue me)
    //     {
    //         return me._value;
    //     }
    // }



    // // Licensed to the .NET Foundation under one or more agreements.
    // // The .NET Foundation licenses this file to you under the MIT license.

    // using System.Runtime.CompilerServices;

    // namespace System
    // {
    //     public partial class Object
    //     {
    //         // Returns a Type object which represent this object instance.
    //         [Intrinsic]
    //         [MethodImpl(MethodImplOptions.InternalCall)]
    //         public extern Type GetType();

    //         // Returns a new object instance that is a memberwise copy of this
    //         // object.  This is always a shallow copy of the instance. The method is protected
    //         // so that other object may only call this method on themselves.  It is intended to
    //         // support the ICloneable interface.
    //         [Intrinsic]
    //         protected unsafe object MemberwiseClone()
    //         {
    //             return clone;
    //         }
    //     }
    // }


}
