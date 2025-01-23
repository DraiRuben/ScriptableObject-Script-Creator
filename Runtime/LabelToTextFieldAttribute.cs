using System;
using UnityEngine;

namespace Ruben.SOCreator
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true)] public class LabelToTextFieldAttribute : PropertyAttribute { }
}