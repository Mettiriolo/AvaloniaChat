using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace AvaloniaChat.Models
{
    [method: JsonConstructor]
    public struct Sender(string label) : IEquatable<Sender>
    {
        public static Sender Assistant { get; } = new Sender("AI助手");

        public static Sender User { get; } = new Sender("用户");
        public static Sender System { get; } = new Sender("系统");

        public string Label { get; } = label;

        public static bool operator ==(Sender left, Sender right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Sender left, Sender right)
        {
            return !(left == right);
        }

        public override readonly bool Equals([NotNullWhen(true)] object? obj)
        {
            if (obj is Sender Sender)
            {
                return this == Sender;
            }

            return false;
        }

        public readonly bool Equals(Sender other)
        {
            return string.Equals(Label, other.Label, StringComparison.OrdinalIgnoreCase);
        }

        public override readonly int GetHashCode()
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(Label);
        }

        public override readonly string ToString() => Label;
    }
}
